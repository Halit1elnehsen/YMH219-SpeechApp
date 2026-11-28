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
    private Button _listen = null!, _stopMic = null!, _wave = null!;

    public Form1()
    {
        InitializeComponent(); BuildInterface(); LoadSpeechOptions();
        _speech.TextRecognized += text => Ui(() => AddRecognition(text));
        _speech.StatusChanged += text => Ui(() => _sttStatus.Text = text);
        FormClosing += (_, _) => _speech.Dispose();
    }

    private void BuildInterface()
    {
        Text = "Ses Defteri — YMH219"; StartPosition = FormStartPosition.CenterScreen; MinimumSize = new Size(960, 660); ClientSize = new Size(1200, 800); BackColor = Color.FromArgb(234, 239, 244); Font = new Font("Segoe UI", 9.5f);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 2 }; root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 94)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); Controls.Add(root);
        var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(29, 43, 61), Padding = new Padding(20, 12, 20, 10) };
        var title = new Label { Text = "Ses Defteri", Font = new Font("Segoe UI", 21, FontStyle.Bold), ForeColor = Color.FromArgb(248, 250, 253), AutoSize = true, Location = new Point(20, 8) };
        var subtitle = new Label { Text = "Konuşmayı dinle. Metni kendi hızında okut.", Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(188, 205, 222), AutoSize = true, Location = new Point(23, 54) };
        var localBadge = new Label { Text = "WINDOWS'TA YEREL İŞLEM", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(255, 213, 153), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(760, 34) };
        header.Controls.AddRange([title, subtitle, localBadge]); header.Resize += (_, _) => localBadge.Left = header.ClientSize.Width - localBadge.Width - 20; root.Controls.Add(header); root.SetColumnSpan(header, 2);
        var tts = Card("Metni seslendir", Color.FromArgb(233, 184, 119)); root.Controls.Add(tts, 0, 1);
        var stt = Card("Konuşmayı yazıya çevir", Color.FromArgb(90, 174, 165)); stt.Margin = new Padding(12, 0, 0, 0); root.Controls.Add(stt, 1, 1);
        BuildTts(tts); BuildStt(stt);
    }

    private static Panel Card(string title, Color wash)
    {
        var panel = new SurfacePanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(253, 254, 255), Padding = new Padding(20), Margin = new Padding(0, 0, 8, 0), CornerRadius = 18, OutlineColor = Color.FromArgb(213, 222, 231) };
        panel.Tag = (title, wash);
        return panel;
    }

    private static Panel SectionHeading(Panel card)
    {
        var (title, accentColor) = ((string, Color))card.Tag!;
        var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        header.Controls.Add(new Panel { BackColor = accentColor, Size = new Size(5, 26), Location = new Point(2, 7) });
        header.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.FromArgb(36, 53, 73), AutoSize = true, Location = new Point(16, 5) });
        return header;
    }

    private void BuildTts(Panel card)
    {
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, RowCount = 9, ColumnCount = 1 };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 31)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); content.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 29)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 43)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 52)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 39)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); card.Controls.Add(content);
        content.Controls.Add(SectionHeading(card), 0, 0);
        content.Controls.Add(Caption("OKUMA SESİ"), 0, 1);
        _voices = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), AccessibleName = "Ses seçimi" }; _voices.SelectedIndexChanged += (_, _) => { if (_voices.SelectedItem is VoiceChoice voice) Try(() => _speech.SelectVoice(voice.Name), _ttsStatus); }; content.Controls.Add(_voices, 0, 2);
        _text = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(248, 250, 252), Text = "Merhaba. Bu uygulama yazdığınız metni sesli okuyabilir.", AccessibleName = "Seslendirilecek metin" }; content.Controls.Add(_text, 0, 3);
        content.Controls.Add(Caption("OKUMA HIZI  ·  YAVAŞTAN HIZLIYA"), 0, 4); _rate = new TrackBar { Dock = DockStyle.Fill, Minimum = -5, Maximum = 5, Value = 0, TickFrequency = 1, TickStyle = TickStyle.BottomRight, Height = 42, AccessibleName = "Okuma hızı" }; content.Controls.Add(_rate, 0, 5);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 5, 0, 0) };
        var start = ActionButton("Oku", Color.FromArgb(196, 119, 65)); start.Click += (_, _) => Try(() => { if (string.IsNullOrWhiteSpace(_text.Text)) { _ttsStatus.Text = "Önce okunacak bir metin yazın."; return; } _speech.SetRate(_rate.Value); _speech.Speak(_text.Text); _ttsStatus.Text = "Sesli okuma sürüyor…"; }, _ttsStatus);
        var pause = ActionButton("Duraklat", Color.FromArgb(222, 230, 236), Color.FromArgb(49, 67, 86)); pause.Click += (_, _) => { _speech.Pause(); _ttsStatus.Text = "Okuma duraklatıldı."; };
        var stop = ActionButton("Durdur", Color.FromArgb(222, 230, 236), Color.FromArgb(49, 67, 86)); stop.Click += (_, _) => { _speech.StopSpeaking(); _ttsStatus.Text = "Okuma durdu."; };
        buttons.Controls.AddRange([start, pause, stop]); content.Controls.Add(buttons, 0, 6);
        var clear = ActionButton("Metni temizle", Color.FromArgb(244, 247, 249), Color.FromArgb(74, 93, 111)); clear.Dock = DockStyle.Left; clear.Width = 145; clear.Click += (_, _) => _text.Clear(); content.Controls.Add(clear, 0, 7);
        _ttsStatus = Caption("Hazır · Metin bu cihazın sesleriyle okunur."); content.Controls.Add(_ttsStatus, 0, 8);
    }

    private void BuildStt(Panel card)
    {
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, RowCount = 7, ColumnCount = 1 };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 52)); content.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); card.Controls.Add(content);
        content.Controls.Add(SectionHeading(card), 0, 0);
        content.Controls.Add(Caption("TANIYICI DİLİ  ·  WINDOWS'TA KURULU OLANLAR"), 0, 1);
        _languages = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), AccessibleName = "Konuşma tanıma dili" }; content.Controls.Add(_languages, 0, 2);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 5, 0, 0) };
        _listen = ActionButton("Mikrofonu aç", Color.FromArgb(32, 132, 119)); _listen.Click += (_, _) => StartMic();
        _stopMic = ActionButton("Durdur", Color.FromArgb(222, 230, 236), Color.FromArgb(49, 67, 86)); _stopMic.Click += (_, _) => { _speech.StopRecognition(); _sttStatus.Text = "Tanıma durduruldu."; };
        _wave = ActionButton("WAV dosyası", Color.FromArgb(222, 230, 236), Color.FromArgb(49, 67, 86)); _wave.Click += (_, _) => OpenWave();
        actions.Controls.AddRange([_listen, _stopMic, _wave]); content.Controls.Add(actions, 0, 3);
        _recognized = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(248, 251, 252), Font = new Font("Segoe UI", 10), Padding = new Padding(12), AccessibleName = "Tanıma sonuçları" }; content.Controls.Add(_recognized, 0, 4);
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 7, 0, 0) };
        var clear = ActionButton("Sonuçları temizle", Color.FromArgb(244, 247, 249), Color.FromArgb(74, 93, 111)); clear.Width = 172; clear.Click += (_, _) => _recognized.Clear(); bottom.Controls.Add(clear); content.Controls.Add(bottom, 0, 5);
        _sttStatus = Caption("Hazır · Dil ve mikrofon aygıtı bu cihaza bağlıdır."); content.Controls.Add(_sttStatus, 0, 6);
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
            else { _sttStatus.Text = "Tanıyıcı bulunamadı. Windows konuşma dilini kurup uygulamayı yeniden açın."; _listen.Enabled = false; _wave.Enabled = false; _stopMic.Enabled = false; }
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
    private void Try(Action action, Label? status = null)
    {
        try { action(); }
        catch (Exception ex) { if (status is not null) status.Text = "İşlem başlatılamadı."; MessageBox.Show(this, $"İşlem tamamlanamadı. Mikrofon, ses aygıtı, ses dosyası veya kurulu dil ayarlarını kontrol edin.\n\nAyrıntı: {ex.Message}", "Ses hizmeti kullanılamıyor", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private void Ui(Action action) { if (IsDisposed || !IsHandleCreated) return; try { BeginInvoke(action); } catch (InvalidOperationException) { } }
    private static Label Caption(string text) => new() { Text = text, AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, ForeColor = Color.FromArgb(119, 134, 122), Font = new Font("Segoe UI", 8, FontStyle.Bold) };
    private static DepthButton ActionButton(string text, Color back, Color? fore = null) => new() { Text = text, Width = 130, Height = 42, Margin = new Padding(0, 0, 9, 0), BackColor = back, ForeColor = fore ?? Color.White, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
}
