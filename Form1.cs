using System.Speech.Recognition;

namespace YMH219_SpeechApp;

public partial class Form1 : Form
{
    private readonly SpeechService _speech = new();
    private TextBox _text = null!;
    private RichTextBox _recognized = null!;
    private ComboBox _voices = null!, _languages = null!;
    private Label _ttsStatus = null!, _sttStatus = null!;
    private TrackBar _rate = null!;
    private Button _listen = null!, _stopMic = null!;

    public Form1()
    {
        InitializeComponent(); BuildInterface(); LoadSpeechOptions();
        _speech.TextRecognized += text => Ui(() => AddRecognition(text));
        _speech.StatusChanged += text => Ui(() => _sttStatus.Text = text);
        FormClosing += (_, _) => _speech.Dispose();
    }

    private void BuildInterface()
    {
        Text = "Ses Defteri — YMH219"; StartPosition = FormStartPosition.CenterScreen; MinimumSize = new Size(960, 640); ClientSize = new Size(1180, 780); BackColor = Color.FromArgb(247, 244, 238);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 2, RowCount = 2 }; root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); Controls.Add(root);
        var header = new Panel { Dock = DockStyle.Fill }; var title = new Label { Text = "SES DEFTERİ", Font = new Font("Segoe UI", 21, FontStyle.Bold), ForeColor = Color.FromArgb(44, 61, 57), AutoSize = true, Location = new Point(0, 0) }; var subtitle = new Label { Text = "SÖZÜ YAZIYA · YAZIYI SESE", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(146, 121, 90), AutoSize = true, Location = new Point(3, 48) }; header.Controls.AddRange([title, subtitle]); root.Controls.Add(header); root.SetColumnSpan(header, 2);
        var tts = Card("01  ·  METNİ SESLENDİR", Color.FromArgb(249, 237, 218)); root.Controls.Add(tts, 0, 1);
        var stt = Card("02  ·  KONUŞMAYI YAZIYA ÇEVİR", Color.FromArgb(225, 239, 233)); root.Controls.Add(stt, 1, 1);
        BuildTts(tts); BuildStt(stt);
    }

    private static Panel Card(string title, Color wash)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20), Margin = new Padding(0, 0, 13, 0) };
        panel.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(55, 77, 67), AutoSize = true, Location = new Point(20, 18) });
        panel.Tag = wash; return panel;
    }

    private void BuildTts(Panel card)
    {
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(0, 50, 0, 0), RowCount = 8, ColumnCount = 1 };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 31)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); content.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 29)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 43)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 52)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 39)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); card.Controls.Add(content);
        content.Controls.Add(Caption("OKUMA SESİ"), 0, 0);
        _voices = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) }; _voices.SelectedIndexChanged += (_, _) => { if (_voices.SelectedItem is VoiceChoice voice) Try(() => _speech.SelectVoice(voice.Name)); }; content.Controls.Add(_voices, 0, 1);
        _text = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "Merhaba! Bu uygulama yazdığınız metni sesli okuyabilir." }; content.Controls.Add(_text, 0, 2);
        content.Controls.Add(Caption("OKUMA HIZI"), 0, 3); _rate = new TrackBar { Dock = DockStyle.Fill, Minimum = -5, Maximum = 5, Value = 0, TickFrequency = 1, Height = 42 }; content.Controls.Add(_rate, 0, 4);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 5, 0, 0) };
        var start = ActionButton("OKU", Color.FromArgb(186, 119, 68)); start.Click += (_, _) => Try(() => { _speech.SetRate(_rate.Value); _speech.Speak(_text.Text); _ttsStatus.Text = "Sesli okuma sürüyor…"; });
        var pause = ActionButton("DURAKLAT", Color.FromArgb(228, 230, 224), Color.FromArgb(58, 77, 67)); pause.Click += (_, _) => { _speech.Pause(); _ttsStatus.Text = "Okuma duraklatıldı."; };
        var stop = ActionButton("DURDUR", Color.FromArgb(228, 230, 224), Color.FromArgb(58, 77, 67)); stop.Click += (_, _) => { _speech.StopSpeaking(); _ttsStatus.Text = "Okuma durdu."; };
        buttons.Controls.AddRange([start, pause, stop]); content.Controls.Add(buttons, 0, 5);
        var clear = ActionButton("METNİ TEMİZLE", Color.FromArgb(248, 245, 239), Color.FromArgb(112, 106, 91)); clear.Dock = DockStyle.Left; clear.Width = 145; clear.Click += (_, _) => _text.Clear(); content.Controls.Add(clear, 0, 6);
        _ttsStatus = Caption("Hazır · Metin yerel Windows sesleriyle okunur."); content.Controls.Add(_ttsStatus, 0, 7);
    }

    private void BuildStt(Panel card)
    {
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(0, 50, 0, 0), RowCount = 6, ColumnCount = 1 };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 52)); content.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); card.Controls.Add(content);
        content.Controls.Add(Caption("TANIYICI DİLİ  ·  WINDOWS'TA KURULU OLANLAR"), 0, 0);
        _languages = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) }; content.Controls.Add(_languages, 0, 1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 5, 0, 0) };
        _listen = ActionButton("MİKROFONU AÇ", Color.FromArgb(51, 127, 102)); _listen.Click += (_, _) => StartMic();
        _stopMic = ActionButton("DURDUR", Color.FromArgb(228, 230, 224), Color.FromArgb(58, 77, 67)); _stopMic.Click += (_, _) => { _speech.StopRecognition(); _sttStatus.Text = "Tanıma durduruldu."; };
        var file = ActionButton("WAV DOSYASI", Color.FromArgb(228, 230, 224), Color.FromArgb(58, 77, 67)); file.Click += (_, _) => OpenWave();
        actions.Controls.AddRange([_listen, _stopMic, file]); content.Controls.Add(actions, 0, 2);
        _recognized = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(250, 252, 249), Font = new Font("Segoe UI", 10), Padding = new Padding(12) }; content.Controls.Add(_recognized, 0, 3);
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 7, 0, 0) };
        var clear = ActionButton("SONUÇLARI TEMİZLE", Color.FromArgb(242, 245, 241), Color.FromArgb(89, 112, 99)); clear.Width = 172; clear.Click += (_, _) => _recognized.Clear(); bottom.Controls.Add(clear); content.Controls.Add(bottom, 0, 4);
        _sttStatus = Caption("Hazır · Dil ve mikrofon kullanılabilirliği cihaza bağlıdır."); content.Controls.Add(_sttStatus, 0, 5);
    }

    private void LoadSpeechOptions()
    {
        try
        {
            foreach (var voice in _speech.Voices) _voices.Items.Add(new VoiceChoice($"{voice.VoiceInfo.Name} · {voice.VoiceInfo.Culture?.DisplayName ?? "Dil belirtilmedi"}", voice.VoiceInfo.Name!));
            if (_voices.Items.Count > 0) _voices.SelectedIndex = 0;
        }
        catch (Exception ex) { _ttsStatus.Text = $"Ses listesi alınamadı: {ex.Message}"; }
        try
        {
            foreach (var rec in _speech.Recognizers) _languages.Items.Add(RecognizerChoice.From(rec));
            var preferred = Enumerable.Range(0, _languages.Items.Count).FirstOrDefault(i => _languages.Items[i] is RecognizerChoice r && r.Info.Culture.Name.StartsWith("tr", StringComparison.OrdinalIgnoreCase));
            if (_languages.Items.Count > 0) _languages.SelectedIndex = preferred;
            else _sttStatus.Text = "Bu Windows kurulumunda konuşma tanıyıcısı yok. Ayarlar > Saat ve Dil > Konuşma bölümüne bakın.";
        }
        catch (Exception ex) { _sttStatus.Text = $"Tanıyıcı listesi alınamadı: {ex.Message}"; }
    }

    private void StartMic()
    {
        if (_languages.SelectedItem is not RecognizerChoice language) { _sttStatus.Text = "Tanıyıcı bulunamadı; Windows'a bir konuşma dili ekleyin."; return; }
        Try(() => { _speech.StartMicrophone(language.Info); _sttStatus.Text = $"Mikrofon dinliyor · {language.Info.Culture.Name}"; });
    }
    private void OpenWave()
    {
        if (_languages.SelectedItem is not RecognizerChoice language) { _sttStatus.Text = "Önce kullanılabilir bir tanıyıcı dili seçin."; return; }
        using var dialog = new OpenFileDialog { Filter = "WAV ses dosyası (*.wav)|*.wav", Title = "Konuşma örneği seç" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Try(() => _speech.StartWaveFile(language.Info, dialog.FileName));
    }
    private void AddRecognition(string text)
    {
        if (IsDisposed) return;
        _recognized.AppendText($"[{DateTime.Now:HH:mm:ss}]  {text}{Environment.NewLine}");
        _recognized.SelectionStart = _recognized.TextLength; _recognized.ScrollToCaret();
    }
    private void Try(Action action)
    {
        try { action(); }
        catch (Exception ex) { _sttStatus.Text = "İşlem başlatılamadı."; MessageBox.Show(this, $"İşlem tamamlanamadı. Mikrofon, ses aygıtı, ses dosyası veya kurulu dil ayarlarını kontrol edin.\n\nAyrıntı: {ex.Message}", "Ses hizmeti kullanılamıyor", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private void Ui(Action action) { if (IsDisposed || !IsHandleCreated) return; try { BeginInvoke(action); } catch (InvalidOperationException) { } }
    private static Label Caption(string text) => new() { Text = text, AutoSize = true, ForeColor = Color.FromArgb(119, 134, 122), Font = new Font("Segoe UI", 8, FontStyle.Bold) };
    private static Button ActionButton(string text, Color back, Color? fore = null) => new() { Text = text, Width = 130, Height = 39, Margin = new Padding(0, 0, 9, 0), FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = fore ?? Color.White, Font = new Font("Segoe UI", 8, FontStyle.Bold), Cursor = Cursors.Hand };
}
