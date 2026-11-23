using System.Globalization;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace YMH219_SpeechApp;

public sealed class SpeechService : IDisposable
{
    private readonly SpeechSynthesizer _synth = new();
    private SpeechRecognitionEngine? _recognizer;
    public IReadOnlyList<RecognizerInfo> Recognizers => SpeechRecognitionEngine.InstalledRecognizers().ToArray();
    public IReadOnlyList<InstalledVoice> Voices => _synth.GetInstalledVoices().Where(v => v.Enabled).ToArray();
    public event Action<string>? TextRecognized;
    public event Action<string>? StatusChanged;

    public void SelectVoice(string voiceName) { if (!string.IsNullOrWhiteSpace(voiceName)) _synth.SelectVoice(voiceName); }
    public void SetRate(int rate) => _synth.Rate = Math.Clamp(rate, -10, 10);
    public void Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Okunacak metni yazın.");
        _synth.SpeakAsyncCancelAll();
        _synth.SetOutputToDefaultAudioDevice();
        _synth.SpeakAsync(text.Trim());
        StatusChanged?.Invoke("Metin seslendiriliyor.");
    }
    public void Pause() { if (_synth.State == SynthesizerState.Speaking) _synth.Pause(); }
    public void Resume() { if (_synth.State == SynthesizerState.Paused) _synth.Resume(); }
    public void StopSpeaking() { _synth.SpeakAsyncCancelAll(); StatusChanged?.Invoke("Sesli okuma durdu."); }

    public void StartMicrophone(RecognizerInfo recognizer) => StartRecognition(recognizer, null);
    public void StartWaveFile(RecognizerInfo recognizer, string path) => StartRecognition(recognizer, path);

    private void StartRecognition(RecognizerInfo info, string? wavePath)
    {
        StopRecognition();
        var engine = new SpeechRecognitionEngine(info);
        engine.LoadGrammar(new DictationGrammar());
        engine.SpeechRecognized += (_, args) =>
        {
            if (args.Result.Confidence >= .25f) TextRecognized?.Invoke(args.Result.Text);
        };
        engine.SpeechRecognitionRejected += (_, _) => StatusChanged?.Invoke("Konuşma anlaşılmadı; daha net ve kısa bir cümle deneyin.");
        engine.RecognizeCompleted += (_, args) =>
        {
            if (args.Error is not null) StatusChanged?.Invoke($"Tanıma hatası: {args.Error.Message}");
            else if (args.Cancelled) StatusChanged?.Invoke("Konuşma tanıma durduruldu.");
            else StatusChanged?.Invoke("Ses dosyası işlendi.");
        };
        _recognizer = engine;
        if (wavePath is null) engine.SetInputToDefaultAudioDevice();
        else engine.SetInputToWaveFile(wavePath);
        engine.RecognizeAsync(RecognizeMode.Multiple);
        StatusChanged?.Invoke(wavePath is null ? $"Mikrofon açık · {info.Culture.Name}" : $"Ses dosyası tanınıyor · {Path.GetFileName(wavePath)}");
    }

    public void StopRecognition()
    {
        if (_recognizer is null) return;
        try { _recognizer.RecognizeAsyncCancel(); _recognizer.SetInputToNull(); }
        catch (InvalidOperationException) { }
        _recognizer.Dispose(); _recognizer = null;
    }

    public void Dispose() { StopRecognition(); _synth.SpeakAsyncCancelAll(); _synth.Dispose(); }
}

public sealed record RecognizerChoice(string DisplayName, RecognizerInfo Info)
{
    public override string ToString() => DisplayName;
    public static RecognizerChoice From(RecognizerInfo info) => new($"{info.Culture.DisplayName} ({info.Culture.Name})", info);
}

public sealed record VoiceChoice(string DisplayName, string Name)
{
    public override string ToString() => DisplayName;
}
