using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Media;
using System.Text;

namespace SnakeGame;

public sealed class GameForm : Form
{
    private enum GameState { Ready, Playing, Paused, GameOver }
    private enum AppView { Home, Play, Skins, Settings }
    private enum Direction { Up, Down, Left, Right }
    private enum Difficulty { Easy, Normal, Hard }
    private enum GameMode { Classic, Wrap, Rush }
    private enum ThemeSeason { Spring, Summer, Autumn, Winter }

    private enum SkinPattern
    {
        Plain, Stripes, Dots, Waves, Lightning, Scales, Flames, Frost,
        Circuit, Galaxy, Camo, Candy, Coral, Prism, Stars, Honeycomb,
        Tiger, Zebra, Neon, Royal, Legendary
    }

    private sealed record SnakeSkin(
        string Name, Color Body, Color Head, Color Outline,
        Color Highlight, Color PatternColor, SkinPattern Pattern, bool IsLegendary = false);

    private sealed record ArcadeTheme(
        string Season, string TimeOfDay, ThemeSeason SeasonType, bool IsNight,
        Color Background, Color Dark, Color Accent, Color Text,
        Color Highlight, Color Grid, Color Effect);

    private static readonly ArcadeTheme[] Themes =
    [
        new("SPRING", "DAY", ThemeSeason.Spring, false,
            Color.FromArgb(35, 80, 39), Color.FromArgb(13, 48, 22),
            Color.FromArgb(244, 198, 94), Color.FromArgb(255, 248, 220),
            Color.FromArgb(185, 231, 135), Color.FromArgb(74, 127, 65),
            Color.FromArgb(246, 166, 184)),
        new("SPRING", "NIGHT", ThemeSeason.Spring, true,
            Color.FromArgb(16, 37, 42), Color.FromArgb(7, 24, 29),
            Color.FromArgb(111, 211, 176), Color.FromArgb(227, 255, 239),
            Color.FromArgb(171, 235, 185), Color.FromArgb(42, 83, 78),
            Color.FromArgb(214, 163, 220)),
        new("SUMMER", "DAY", ThemeSeason.Summer, false,
            Color.FromArgb(25, 100, 117), Color.FromArgb(8, 51, 66),
            Color.FromArgb(255, 204, 75), Color.FromArgb(255, 247, 215),
            Color.FromArgb(133, 226, 207), Color.FromArgb(49, 130, 145),
            Color.FromArgb(255, 226, 116)),
        new("SUMMER", "NIGHT", ThemeSeason.Summer, true,
            Color.FromArgb(19, 38, 68), Color.FromArgb(8, 22, 45),
            Color.FromArgb(95, 210, 226), Color.FromArgb(230, 245, 255),
            Color.FromArgb(137, 195, 255), Color.FromArgb(48, 76, 116),
            Color.FromArgb(170, 219, 255)),
        new("AUTUMN", "DAY", ThemeSeason.Autumn, false,
            Color.FromArgb(111, 62, 29), Color.FromArgb(54, 27, 15),
            Color.FromArgb(246, 174, 68), Color.FromArgb(255, 239, 210),
            Color.FromArgb(224, 119, 48), Color.FromArgb(145, 83, 39),
            Color.FromArgb(241, 137, 55)),
        new("AUTUMN", "NIGHT", ThemeSeason.Autumn, true,
            Color.FromArgb(43, 29, 47), Color.FromArgb(25, 16, 31),
            Color.FromArgb(224, 148, 75), Color.FromArgb(255, 232, 215),
            Color.FromArgb(191, 116, 109), Color.FromArgb(83, 58, 77),
            Color.FromArgb(234, 163, 91)),
        new("WINTER", "DAY", ThemeSeason.Winter, false,
            Color.FromArgb(63, 112, 139), Color.FromArgb(20, 54, 78),
            Color.FromArgb(204, 237, 250), Color.FromArgb(245, 252, 255),
            Color.FromArgb(153, 216, 242), Color.FromArgb(91, 145, 172),
            Color.FromArgb(236, 251, 255)),
        new("WINTER", "NIGHT", ThemeSeason.Winter, true,
            Color.FromArgb(18, 29, 58), Color.FromArgb(8, 16, 37),
            Color.FromArgb(139, 190, 245), Color.FromArgb(237, 244, 255),
            Color.FromArgb(160, 177, 255), Color.FromArgb(48, 65, 104),
            Color.FromArgb(193, 231, 255))
    ];

    private static readonly SnakeSkin[] Skins =
    [
        new("Classic Lime", Color.FromArgb(104,193,91), Color.FromArgb(183,247,122), Color.FromArgb(42,104,57), Color.FromArgb(75,223,255,187), Color.FromArgb(165,239,104), SkinPattern.Plain),
        new("Ruby Fang", Color.FromArgb(191,48,66), Color.FromArgb(255,112,120), Color.FromArgb(103,24,39), Color.FromArgb(255,190,190), Color.FromArgb(255,205,210), SkinPattern.Stripes),
        new("Ocean Wave", Color.FromArgb(38,139,191), Color.FromArgb(103,211,245), Color.FromArgb(16,70,111), Color.FromArgb(180,233,255), Color.FromArgb(145,232,255), SkinPattern.Waves),
        new("Arctic Frost", Color.FromArgb(142,207,224), Color.FromArgb(218,249,255), Color.FromArgb(61,120,151), Color.FromArgb(255,255,255), Color.FromArgb(236,255,255), SkinPattern.Frost),
        new("Solar Gold", Color.FromArgb(218,157,41), Color.FromArgb(255,220,105), Color.FromArgb(117,74,18), Color.FromArgb(255,244,177), Color.FromArgb(255,231,142), SkinPattern.Stars),
        new("Cyber Neon", Color.FromArgb(32,205,178), Color.FromArgb(115,255,224), Color.FromArgb(7,91,90), Color.FromArgb(190,255,245), Color.FromArgb(77,255,226), SkinPattern.Neon),
        new("Toxic Slime", Color.FromArgb(115,190,43), Color.FromArgb(193,255,75), Color.FromArgb(45,91,22), Color.FromArgb(228,255,146), Color.FromArgb(211,255,74), SkinPattern.Dots),
        new("Purple Haze", Color.FromArgb(131,86,190), Color.FromArgb(193,139,255), Color.FromArgb(61,38,111), Color.FromArgb(230,204,255), Color.FromArgb(213,161,255), SkinPattern.Royal),
        new("Sunset Blaze", Color.FromArgb(224,91,52), Color.FromArgb(255,166,81), Color.FromArgb(117,43,38), Color.FromArgb(255,211,148), Color.FromArgb(255,194,104), SkinPattern.Flames),
        new("Tiger Rush", Color.FromArgb(221,133,37), Color.FromArgb(255,190,76), Color.FromArgb(82,48,26), Color.FromArgb(255,229,160), Color.FromArgb(39,28,26), SkinPattern.Tiger),
        new("Zebra Stripe", Color.FromArgb(218,224,229), Color.FromArgb(255,255,255), Color.FromArgb(55,62,72), Color.FromArgb(255,255,255), Color.FromArgb(54,63,76), SkinPattern.Zebra),
        new("Mint Chip", Color.FromArgb(77,177,145), Color.FromArgb(154,238,195), Color.FromArgb(26,93,75), Color.FromArgb(218,255,228), Color.FromArgb(222,255,233), SkinPattern.Scales),
        new("Candy Pop", Color.FromArgb(231,99,164), Color.FromArgb(255,174,213), Color.FromArgb(124,42,94), Color.FromArgb(255,223,241), Color.FromArgb(255,244,255), SkinPattern.Candy),
        new("Lava Core", Color.FromArgb(184,58,35), Color.FromArgb(255,118,49), Color.FromArgb(75,25,24), Color.FromArgb(255,194,104), Color.FromArgb(255,224,93), SkinPattern.Honeycomb),
        new("Galaxy Dust", Color.FromArgb(72,70,153), Color.FromArgb(154,137,255), Color.FromArgb(34,28,93), Color.FromArgb(226,217,255), Color.FromArgb(255,221,135), SkinPattern.Galaxy),
        new("Volt Strike", Color.FromArgb(62,120,217), Color.FromArgb(144,210,255), Color.FromArgb(26,52,122), Color.FromArgb(221,244,255), Color.FromArgb(255,237,99), SkinPattern.Lightning),
        new("Jungle Camo", Color.FromArgb(91,119,57), Color.FromArgb(166,187,91), Color.FromArgb(39,65,36), Color.FromArgb(202,222,143), Color.FromArgb(44,69,40), SkinPattern.Camo),
        new("Rose Quartz", Color.FromArgb(203,110,142), Color.FromArgb(255,188,206), Color.FromArgb(104,48,78), Color.FromArgb(255,229,238), Color.FromArgb(255,226,239), SkinPattern.Coral),
        new("Diamond Skin", Color.FromArgb(100,183,205), Color.FromArgb(208,251,255), Color.FromArgb(42,103,133), Color.FromArgb(255,255,255), Color.FromArgb(225,255,255), SkinPattern.Prism),
        new("Shadow Ops", Color.FromArgb(66,79,91), Color.FromArgb(142,160,173), Color.FromArgb(22,29,39), Color.FromArgb(185,205,216), Color.FromArgb(114,235,203), SkinPattern.Circuit),
        new("LEGENDARY PHOENIX", Color.FromArgb(151,74,205), Color.FromArgb(255,202,91), Color.FromArgb(74,31,130), Color.FromArgb(255,238,174), Color.FromArgb(255,178,63), SkinPattern.Legendary, true)
    ];

    private const int CellSize = 23;
    private const int Columns = 30;
    private const int Rows = 21;
    private const int BoardCells = Columns * Rows;
    private const int DesignWidth = 1160;
    private const int DesignHeight = 760;

    private static readonly Color Background = Color.FromArgb(8, 13, 17);
    private static readonly Color Panel = Color.FromArgb(16, 24, 30);
    private static readonly Color PanelLight = Color.FromArgb(22, 32, 39);
    private static readonly Color Border = Color.FromArgb(37, 53, 59);
    private static readonly Color White = Color.FromArgb(238, 245, 242);
    private static readonly Color Muted = Color.FromArgb(123, 143, 146);
    private static readonly Color Green = Color.FromArgb(165, 239, 104);
    private static readonly Color Gold = Color.FromArgb(255, 194, 83);
    private int _selectedTheme;
    private ArcadeTheme ActiveTheme => Themes[_selectedTheme];
    private Color RetroBackground => ActiveTheme.Background;
    private Color RetroGold => ActiveTheme.Accent;
    private Color RetroWhite => ActiveTheme.Text;
    private Color RetroDark => ActiveTheme.Dark;

    private readonly System.Windows.Forms.Timer _animationTimer = new() { Interval = 16 };
    private readonly System.Windows.Forms.Timer _ambientTimer = new() { Interval = 90 };
    private readonly Random _random = new();
    private readonly List<Point> _snake = [];
    private readonly Point[] _previousSnake = new Point[BoardCells];
    private int _previousSnakeCount;
    private Point _food;
    private Point? _goldenFood;

    private GameState _state = GameState.Ready;
    private AppView _view = AppView.Home;
    private Direction _direction = Direction.Right;
    private Direction _nextDirection = Direction.Right;
    private Difficulty _difficulty = Difficulty.Normal;
    private GameMode _mode = GameMode.Classic;

    private DateTime _lastStepAt;
    private DateTime _lastFrameAt;
    private DateTime? _goldenExpiresAt;
    private TimeSpan _rushTimeLeft = TimeSpan.FromSeconds(60);
    private float _frozenProgress;
    private int _lastDisplayedSecond = 60;
    private int _score;
    private int _bestScore;
    private int _foodEaten;
    private bool _soundEnabled = true;
    private bool _won;
    private bool _newRecord;
    private bool _scoreSaveFailed;
    private bool _isFullscreen;
    private int _selectedSkin;
    private Rectangle _windowedBounds;
    private float _viewScale = 1f;
    private float _viewOffsetX;
    private float _viewOffsetY;
    private Matrix _viewMatrix = new(1, 0, 0, 1, 0, 0);

    private bool _goldBurstActive;
    private DateTime _goldBurstStarted;
    private PointF _goldBurstCenter;
    private readonly GoldParticle[] _goldParticles = new GoldParticle[36];
    private int _goldParticleCount;

    private struct GoldParticle
    {
        public float X, Y, VelocityX, VelocityY, Age, Lifetime, Size;
    }

    private Bitmap? _chrome;
    private Bitmap? _appleSprite;
    private Bitmap? _goldAppleSprite;
    private readonly PointF[] _snakeCenters = new PointF[BoardCells];
    private readonly GraphicsPath _snakePath = new();
    private readonly GraphicsPath _snakeHighlightPath = new();
    private readonly GraphicsPath _overlayPath = RoundPath(new Rectangle(138, 220, 538, 334), 20);

    private readonly Rectangle _boardCard = new(32, 100, 750, 600);
    private readonly Rectangle _field = new(62, 150, Columns * CellSize, Rows * CellSize);
    private readonly Rectangle _rightPanel = new(802, 100, 326, 600);
    private readonly Rectangle _scoreArea = new(824, 124, 280, 132);
    private readonly Rectangle _modeArea = new(824, 270, 280, 105);
    private readonly Rectangle _difficultyArea = new(824, 390, 280, 105);
    private readonly Rectangle _actionArea = new(824, 510, 280, 170);
    private readonly Rectangle _skinButton = new(838, 606, 266, 34);
    private readonly Rectangle _resetBestButton = new(838, 646, 266, 26);
    private readonly Rectangle _skinPickerPanel = new(78, 164, 658, 460);
    private readonly Rectangle _skinPickerClose = new(48, 84, 112, 36);
    private readonly Rectangle _retroStartButton = new(350, 488, 460, 60);
    private readonly Rectangle _retroSkinsButton = new(350, 575, 220, 48);
    private readonly Rectangle _retroSettingsButton = new(590, 575, 220, 48);
    private readonly Rectangle _retroDifficultyLeft = new(455, 378, 44, 48);
    private readonly Rectangle _retroDifficultyText = new(500, 378, 160, 48);
    private readonly Rectangle _retroDifficultyRight = new(661, 378, 44, 48);
    private readonly Rectangle _boardStatus = new(660, 119, 94, 24);
    private readonly Rectangle _overlayButton = new(287, 462, 240, 52);
    private readonly Rectangle _mainButton = new(838, 552, 174, 48);
    private readonly Rectangle _soundButton = new(1020, 552, 38, 48);
    private readonly Rectangle _restartButton = new(1066, 552, 38, 48);
    private readonly Rectangle _classicButton = new(838, 324, 82, 36);
    private readonly Rectangle _wrapButton = new(926, 324, 82, 36);
    private readonly Rectangle _rushButton = new(1014, 324, 82, 36);
    private readonly Rectangle _easyButton = new(838, 444, 82, 36);
    private readonly Rectangle _normalButton = new(926, 444, 82, 36);
    private readonly Rectangle _hardButton = new(1014, 444, 82, 36);
    private readonly Rectangle _modeTimerRect = new(1037, 282, 58, 20);
    private readonly Rectangle _homeNavButton = new(398, 18, 76, 36);
    private readonly Rectangle _playNavButton = new(482, 18, 76, 36);
    private readonly Rectangle _skinsNavButton = new(566, 18, 82, 36);
    private readonly Rectangle _settingsNavButton = new(656, 18, 104, 36);
    private readonly Rectangle _homePlayCard = new(78, 478, 190, 112);
    private readonly Rectangle _homeSkinsCard = new(282, 478, 190, 112);
    private readonly Rectangle _homeSettingsCard = new(486, 478, 238, 112);
    private readonly Rectangle _settingsSoundButton = new(330, 222, 145, 48);
    private readonly Rectangle _settingsResetButton = new(330, 378, 145, 48);
    private readonly Rectangle _fullscreenButton = new(1030, 19, 34, 30);
    private readonly Rectangle _minimizeButton = new(1070, 19, 34, 30);
    private readonly Rectangle _closeButton = new(1110, 19, 34, 30);

    private Point _mousePoint = new(-1, -1);
    private Point _dragOffset;
    private bool _dragging;
    private int _hoverId;

    private readonly Font _smallFont = new("Segoe UI", 8, FontStyle.Regular);
    private readonly Font _labelFont = new("Segoe UI", 8, FontStyle.Bold);
    private readonly Font _scoreFont = new("Segoe UI", 26, FontStyle.Bold);
    private readonly Font _numberFont = new("Segoe UI", 25, FontStyle.Bold);
    private readonly Font _heroFont = new("Segoe UI", 25, FontStyle.Bold);
    private readonly Font _welcomeFont = new("Segoe UI", 30, FontStyle.Bold);
    private readonly Font _welcomeSmallFont = new("Segoe UI", 9, FontStyle.Regular);
    private readonly Font _effectFont = new("Segoe UI", 13, FontStyle.Bold);
    private readonly Font _buttonFont = new("Segoe UI", 8.5f, FontStyle.Bold);
    private readonly Font _iconFont = new("Segoe UI Symbol", 17, FontStyle.Regular);

    private readonly SolidBrush _whiteBrush = new(White);
    private readonly SolidBrush _darkBrush = new(Color.FromArgb(12, 23, 17));
    private readonly SolidBrush _mutedBrush = new(Muted);
    private readonly SolidBrush _greenBrush = new(Green);
    private readonly SolidBrush _goldBrush = new(Gold);
    private SolidBrush _snakeBrush = new(Color.FromArgb(104, 193, 91));
    private SolidBrush _headBrush = new(Color.FromArgb(183, 247, 122));
    private SolidBrush _headShadowBrush = new(Color.FromArgb(45, 138, 233, 104));
    private readonly SolidBrush _eyeWhiteBrush = new(Color.FromArgb(246, 255, 249, 230));
    private readonly SolidBrush _eyeBrush = new(Color.FromArgb(10, 25, 20));
    private readonly SolidBrush _foodBrush = new(Color.FromArgb(255, 103, 105));
    private readonly SolidBrush _foodGlowBrush = new(Color.FromArgb(38, 255, 103, 105));
    private readonly SolidBrush _goldGlowBrush = new(Color.FromArgb(42, 255, 194, 83));
    private readonly SolidBrush _headHighlightBrush = new(Color.FromArgb(85, 255, 255, 255));
    private readonly Pen _snakeOutlinePen = new(Color.FromArgb(42, 104, 57), CellSize - 1)
    {
        StartCap = LineCap.Round,
        EndCap = LineCap.Round,
        LineJoin = LineJoin.Round
    };
    private readonly Pen _snakeBodyPen = new(Color.FromArgb(111, 194, 91), CellSize - 6)
    {
        StartCap = LineCap.Round,
        EndCap = LineCap.Round,
        LineJoin = LineJoin.Round
    };
    private readonly Pen _snakeHighlightPen = new(Color.FromArgb(75, 223, 255, 187), 2.2f)
    {
        StartCap = LineCap.Round,
        EndCap = LineCap.Round,
        LineJoin = LineJoin.Round
    };
    private readonly Pen _headOutlinePen = new(Color.FromArgb(190, 57, 123, 64), 1.2f);
    private readonly Pen _skinPatternPen = new(Color.FromArgb(180, 255, 255, 255), 2.4f)
    {
        StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round
    };
    private readonly Pen _skinAccentPen = new(Color.FromArgb(180, 255, 255, 255), 1.4f)
    {
        StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round
    };
    private readonly Pen _skinGlowPen = new(Color.FromArgb(45, 165, 239, 104), 25f)
    {
        StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round
    };
    private readonly Pen[] _legendaryPens =
    [
        new Pen(Color.FromArgb(155, 255, 206, 91), 2.4f),
        new Pen(Color.FromArgb(115, 206, 130, 255), 1.6f),
        new Pen(Color.FromArgb(210, 255, 241, 180), 1.2f)
    ];
    private readonly SolidBrush _overlayBrush = new(Color.FromArgb(218, 5, 10, 13));
    private readonly SolidBrush _cardBrush = new(Color.FromArgb(248, 19, 29, 35));
    private readonly Pen _goldPen = new(Color.FromArgb(235, Gold), 2);
    private readonly Pen[] _goldBurstPens =
    [
        new Pen(Color.FromArgb(235, 255, 232, 151), 3.2f),
        new Pen(Color.FromArgb(170, 255, 194, 83), 2.3f),
        new Pen(Color.FromArgb(90, 255, 157, 55), 1.5f)
    ];
    private readonly SolidBrush[] _goldParticleBrushes =
    [
        new SolidBrush(Color.FromArgb(235, 255, 240, 166)),
        new SolidBrush(Color.FromArgb(205, 255, 205, 87)),
        new SolidBrush(Color.FromArgb(170, 255, 164, 66)),
        new SolidBrush(Color.FromArgb(125, 255, 246, 207))
    ];
    private readonly StringFormat _leftFormat = new()
    {
        Alignment = StringAlignment.Near,
        LineAlignment = StringAlignment.Center,
        FormatFlags = StringFormatFlags.NoWrap
    };
    private readonly StringFormat _centerFormat = new()
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center,
        FormatFlags = StringFormatFlags.NoWrap
    };
    private readonly StringFormat _rightFormat = new()
    {
        Alignment = StringAlignment.Far,
        LineAlignment = StringAlignment.Center,
        FormatFlags = StringFormatFlags.NoWrap
    };

    public GameForm()
    {
        Text = "SnakeGame";
        ClientSize = new Size(1160, 760);
        MinimumSize = new Size(900, 600);
        MaximumSize = Size.Empty;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Background;
        KeyPreview = true;
        DoubleBuffered = true;

        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);

        _bestScore = LoadBestScore();
        UpdateViewTransform();
        _selectedTheme = LoadSelectedTheme();
        _selectedSkin = LoadSelectedSkin();
        ApplySkin(_selectedSkin);
        ResetSnake();
        _appleSprite = CreateAppleSprite(false);
        _goldAppleSprite = CreateAppleSprite(true);
        BuildChrome();

        _animationTimer.Tick += (_, _) => UpdateGameFrame();
        _ambientTimer.Tick += (_, _) =>
        {
            if (_view != AppView.Play || _state != GameState.Playing)
                Invalidate();
        };
        _ambientTimer.Start();
        KeyDown += HandleKeyDown;
        MouseMove += HandleMouseMove;
        MouseDown += HandleMouseDown;
        MouseUp += (_, _) => _dragging = false;
        MouseClick += HandleMouseClick;
        MouseLeave += (_, _) => UpdateHover(new Point(-1, -1));
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_chrome is null || ClientSize.Width < 1 || ClientSize.Height < 1)
            return;

        UpdateViewTransform();
        BuildChrome();
        Invalidate();
    }

    protected override void WndProc(ref Message m)
    {
        const int WmNcHitTest = 0x84;
        const int HtClient = 1;
        const int grip = 8;

        base.WndProc(ref m);
        if (m.Msg != WmNcHitTest || _isFullscreen || (int)m.Result != HtClient)
            return;

        long packed = m.LParam.ToInt64();
        Point point = PointToClient(new Point(unchecked((short)packed),
            unchecked((short)(packed >> 16))));

        bool left = point.X < grip;
        bool right = point.X >= ClientSize.Width - grip;
        bool top = point.Y < grip;
        bool bottom = point.Y >= ClientSize.Height - grip;

        int hit = top
            ? left ? 13 : right ? 14 : 12
            : bottom
                ? left ? 16 : right ? 17 : 15
                : left ? 10 : right ? 11 : HtClient;

        if (hit != HtClient)
            m.Result = (IntPtr)hit;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // The static interface is rendered once into _chrome.
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_chrome is null)
            return;

        Graphics g = e.Graphics;
        g.Clear(Background);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;

        g.Transform = _viewMatrix;
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.None;
        g.Clear(RetroBackground);
        DrawThemeAtmosphere(g);

        switch (_view)
        {
            case AppView.Home:
                DrawHomePage(g);
                break;
            case AppView.Skins:
                DrawSkinsPage(g);
                break;
            case AppView.Settings:
                DrawSettingsPage(g);
                DrawRetroNavigation(g);
                break;
            case AppView.Play:
                DrawRetroPlayChrome(g);
                DrawBoardDynamic(g);
                DrawRetroPlayDashboard(g);
                DrawRetroNavigation(g);
                break;
        }

        DrawRetroWindowControls(g);
    }

    private void UpdateViewTransform()
    {
        _viewScale = Math.Min(ClientSize.Width / (float)DesignWidth,
            ClientSize.Height / (float)DesignHeight);
        _viewOffsetX = (ClientSize.Width - DesignWidth * _viewScale) / 2f;
        _viewOffsetY = (ClientSize.Height - DesignHeight * _viewScale) / 2f;
        _viewMatrix.Dispose();
        _viewMatrix = new Matrix(_viewScale, 0, 0, _viewScale, _viewOffsetX, _viewOffsetY);
    }

    private Point ClientToDesign(Point point) =>
        new((int)Math.Round((point.X - _viewOffsetX) / _viewScale),
            (int)Math.Round((point.Y - _viewOffsetY) / _viewScale));

    private Rectangle ClientToDesign(Rectangle rect)
    {
        int left = (int)Math.Floor((rect.Left - _viewOffsetX) / _viewScale);
        int top = (int)Math.Floor((rect.Top - _viewOffsetY) / _viewScale);
        int right = (int)Math.Ceiling((rect.Right - _viewOffsetX) / _viewScale);
        int bottom = (int)Math.Ceiling((rect.Bottom - _viewOffsetY) / _viewScale);
        return Rectangle.FromLTRB(left, top, right, bottom);
    }

    private Rectangle DesignToClient(Rectangle rect)
    {
        int left = (int)Math.Floor(rect.Left * _viewScale + _viewOffsetX);
        int top = (int)Math.Floor(rect.Top * _viewScale + _viewOffsetY);
        int right = (int)Math.Ceiling(rect.Right * _viewScale + _viewOffsetX);
        int bottom = (int)Math.Ceiling(rect.Bottom * _viewScale + _viewOffsetY);
        return Rectangle.FromLTRB(left, top, right, bottom);
    }

    private void InvalidateDesign(Rectangle rect) => Invalidate(DesignToClient(rect));

    private void ToggleFullscreen()
    {
        if (!_isFullscreen)
        {
            _windowedBounds = Bounds;
            _isFullscreen = true;
            MinimumSize = Size.Empty;
            MaximumSize = Size.Empty;
            Bounds = Screen.FromControl(this).Bounds;
        }
        else
        {
            _isFullscreen = false;
            Bounds = _windowedBounds;
            MinimumSize = new Size(900, 600);
            MaximumSize = Size.Empty;
        }

        UpdateViewTransform();
        Invalidate();
    }

    private void BuildChrome()
    {
        _chrome?.Dispose();
        _chrome = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);

        using Graphics g = Graphics.FromImage(_chrome);
        g.Transform = _viewMatrix;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using (var background = new LinearGradientBrush(
                   new Rectangle(0, 0, DesignWidth, DesignHeight),
                   Color.FromArgb(13, 21, 25), Background, 90f))
            g.FillRectangle(background, new Rectangle(0, 0, DesignWidth, DesignHeight));

        using (var ambientGlow = new SolidBrush(Color.FromArgb(15, 132, 205, 105)))
            g.FillEllipse(ambientGlow, -210, 430, 500, 500);

        using (var topLine = new Pen(Border, 1))
        {
            g.DrawLine(topLine, 0, 70, DesignWidth, 70);
            g.DrawLine(topLine, 32, 724, 1128, 724);
        }

        FillRound(g, new Rectangle(34, 18, 34, 34), 11, Green);
        StaticText(g, "S", new Rectangle(34, 17, 34, 35), 18, FontStyle.Bold,
            Background, StringAlignment.Center);

        StaticText(g, "SNAKE", new Rectangle(80, 15, 105, 23), 14, FontStyle.Bold, White);
        StaticText(g, "ARCADE", new Rectangle(81, 38, 100, 15), 7, FontStyle.Bold, Muted);

        FillRound(g, new Rectangle(864, 24, 7, 7), 4, Green);
        StaticText(g, "OFFLINE", new Rectangle(879, 17, 80, 20), 8, FontStyle.Bold, Muted);
        StaticText(g, "NO LOGIN  ·  NO ADS", new Rectangle(879, 37, 145, 15), 7, FontStyle.Regular,
            Color.FromArgb(90, 111, 115));

        DrawStaticPanel(g, _boardCard);
        DrawStaticPanel(g, _rightPanel);
        DrawStaticPanel(g, _scoreArea, PanelLight);
        using (var scoreDivider = new Pen(Border, 1))
            g.DrawLine(scoreDivider, 968, 142, 968, 238);
        DrawStaticPanel(g, _modeArea, PanelLight);
        DrawStaticPanel(g, _difficultyArea, PanelLight);
        DrawStaticPanel(g, _actionArea, PanelLight);

        StaticText(g, "PLAYFIELD", new Rectangle(62, 116, 180, 22), 8, FontStyle.Bold, Muted);
        FillRound(g, _field, 12, Color.FromArgb(8, 15, 18));
        StrokeRound(g, _field, 12, Color.FromArgb(48, 74, 66));

        using (var grid = new Pen(Color.FromArgb(19, 135, 163, 133), 1))
        {
            for (int x = 1; x < Columns; x++)
            {
                int px = _field.Left + x * CellSize;
                g.DrawLine(grid, px, _field.Top + 1, px, _field.Bottom - 1);
            }

            for (int y = 1; y < Rows; y++)
            {
                int py = _field.Top + y * CellSize;
                g.DrawLine(grid, _field.Left + 1, py, _field.Right - 1, py);
            }
        }



        StaticText(g, "GAME MODE", new Rectangle(842, 282, 180, 18), 8, FontStyle.Bold, Muted);
        StaticText(g, "Choose your run", new Rectangle(842, 302, 180, 16), 7, FontStyle.Regular,
            Color.FromArgb(105, 126, 128));

        StaticText(g, "DIFFICULTY", new Rectangle(842, 402, 180, 18), 8, FontStyle.Bold, Muted);
        StaticText(g, "Choose your pace", new Rectangle(842, 422, 180, 16), 7, FontStyle.Regular,
            Color.FromArgb(105, 126, 128));

        StaticText(g, "YOUR SESSION", new Rectangle(842, 528, 180, 18), 8, FontStyle.Bold, Muted);

        DrawStaticButton(g, _classicButton);
        DrawStaticButton(g, _wrapButton);
        DrawStaticButton(g, _rushButton);
        DrawStaticButton(g, _easyButton);
        DrawStaticButton(g, _normalButton);
        DrawStaticButton(g, _hardButton);
        DrawStaticButton(g, _mainButton, true);
        DrawStaticButton(g, _soundButton);
        DrawStaticButton(g, _restartButton);
        DrawStaticButton(g, _fullscreenButton);
        DrawStaticButton(g, _skinButton);
        DrawStaticButton(g, _resetBestButton);

        StaticText(g, "SPACE  PAUSE", new Rectangle(62, 650, 130, 20), 7, FontStyle.Bold,
            Color.FromArgb(95, 118, 118));
        StaticText(g, "WASD  /  ARROWS", new Rectangle(201, 650, 160, 20), 7, FontStyle.Bold,
            Color.FromArgb(95, 118, 118));
        StaticText(g, "GOLDEN FRUIT  +30", new Rectangle(540, 650, 210, 20), 7, FontStyle.Bold,
            Color.FromArgb(133, 115, 78));
    }

    private void NavigateTo(AppView view)
    {
        if (view != AppView.Play && _state == GameState.Playing)
            SetState(GameState.Paused);

        _view = view;
        Invalidate();
    }

    private void DrawPageBackground(Graphics g)
    {
        using var background = new LinearGradientBrush(
            new Rectangle(0, 0, DesignWidth, DesignHeight),
            Color.FromArgb(13, 22, 27), Background, 90f);
        g.FillRectangle(background, 0, 0, DesignWidth, DesignHeight);

        using var glow = new SolidBrush(Color.FromArgb(18, 112, 198, 111));
        g.FillEllipse(glow, -140, 400, 560, 480);
        using var line = new Pen(Border, 1);
        g.DrawLine(line, 0, 70, DesignWidth, 70);
        g.DrawLine(line, 32, 724, 1128, 724);
    }

    private void DrawAppHeader(Graphics g)
    {
        FillRound(g, new Rectangle(34, 18, 34, 34), 11, Green);
        StaticText(g, "S", new Rectangle(34, 17, 34, 35), 18,
            FontStyle.Bold, Background, StringAlignment.Center);
        StaticText(g, "SNAKE", new Rectangle(80, 15, 105, 23),
            14, FontStyle.Bold, White);
        StaticText(g, "ARCADE", new Rectangle(81, 38, 100, 15),
            7, FontStyle.Bold, Muted);

        DrawNavigationButton(g, _homeNavButton, "HOME", _view == AppView.Home);
        DrawNavigationButton(g, _playNavButton, "PLAY", _view == AppView.Play);
        DrawNavigationButton(g, _skinsNavButton, "SKINS", _view == AppView.Skins);
        DrawNavigationButton(g, _settingsNavButton, "SETTINGS", _view == AppView.Settings);
    }

    private void DrawNavigationButton(Graphics g, Rectangle rect, string label, bool active)
    {
        bool hover = rect.Contains(_mousePoint);
        Color fill = active ? Color.FromArgb(39, 68, 46) :
            hover ? Color.FromArgb(34, 47, 52) : Color.Transparent;
        FillRound(g, rect, 8, fill);
        if (active)
            StrokeRound(g, rect, 8, Color.FromArgb(80, Green));
        DrawLabel(g, label, _buttonFont,
            active ? _greenBrush : hover ? _whiteBrush : _mutedBrush,
            rect, _centerFormat);
    }

    private void DrawLobbyArtwork(Graphics g)
    {
        DrawSnakePreview(g, new Rectangle(420, 132, 326, 312), Skins[_selectedSkin]);
    }

    private void DrawScorePanel(Graphics g)
    {
        DrawLabel(g, "CURRENT SCORE", _labelFont, _mutedBrush,
            new Rectangle(838, 139, 122, 18), _centerFormat);
        DrawLabel(g, "PERSONAL BEST", _labelFont, _mutedBrush,
            new Rectangle(978, 139, 116, 18), _centerFormat);

        DrawLabel(g, FormatScore(_score), _scoreFont, _whiteBrush,
            new Rectangle(834, 166, 128, 52), _centerFormat);
        DrawLabel(g, FormatScore(_bestScore), _numberFont, _greenBrush,
            new Rectangle(978, 166, 116, 52), _centerFormat);
    }


    private void DrawHomePage(Graphics g)
    {
        using var trail = new SolidBrush(RetroGold);

        // Thin angular accents frame the title instead of crossing the text.
        g.FillRectangle(trail, 792, 128, 112, 10);
        g.FillRectangle(trail, 894, 128, 10, 108);
        g.FillRectangle(trail, 760, 226, 144, 10);
        g.FillRectangle(trail, 254, 275, 10, 96);
        g.FillRectangle(trail, 254, 361, 150, 10);

        DrawPixelText(g, "RETRO ARCADE", DesignWidth / 2, 76, 3, RetroGold);

        DrawPixelText(g, "SNAKE", DesignWidth / 2, 132, 12, RetroWhite);
        DrawPixelText(g, "GAME", DesignWidth / 2, 226, 12, RetroWhite);

        DrawPixelText(g, "SELECT DIFFICULTY", DesignWidth / 2, 340, 3,
            RetroWhite);

        DrawRetroMenuSnake(g, 345, 385);
        DrawPixelApple(g, 410, 378, 5);

        Color leftArrowColor = _hoverId == 26 ? ActiveTheme.Highlight : RetroWhite;
        Color difficultyColor = _hoverId == 28 ? ActiveTheme.Highlight : RetroWhite;
        Color rightArrowColor = _hoverId == 27 ? ActiveTheme.Highlight : RetroWhite;

        DrawPixelArrow(g, 470, 399, true, leftArrowColor);
        DrawPixelText(g, DifficultyName(_difficulty).ToUpperInvariant(),
            DesignWidth / 2, 394, 4, difficultyColor);
        DrawPixelArrow(g, 670, 399, false, rightArrowColor);

        DrawRetroButton(g, _retroStartButton, "PRESS ENTER TO PLAY",
            3, _retroStartButton.Contains(_mousePoint), true);
        DrawRetroButton(g, _retroSkinsButton, "SKINS", 3,
            _retroSkinsButton.Contains(_mousePoint));
        DrawRetroButton(g, _retroSettingsButton, "SETTINGS", 3,
            _retroSettingsButton.Contains(_mousePoint));

        DrawPixelText(g, "ULTEK-CREATED BY SKY CAMPBELL",
            DesignWidth / 2, 704, 2, RetroWhite);
    }

    private void DrawRetroButton(Graphics g, Rectangle rect, string label,
        int pixelSize, bool hover, bool primary = false)
    {
        using var fill = new SolidBrush(hover
            ? Color.FromArgb(255, ActiveTheme.Background)
            : primary ? Color.FromArgb(255, ActiveTheme.Grid) : RetroDark);
        g.FillRectangle(fill, rect);

        DrawPixelOutline(g, rect,
            hover ? ActiveTheme.Highlight : RetroGold,
            primary ? 4 : 3);

        int textHeight = pixelSize * 7;
        int top = rect.Y + (rect.Height - textHeight) / 2;
        DrawPixelText(g, label, rect.X + rect.Width / 2,
            top, pixelSize, RetroWhite);
    }

    private void DrawRetroMenuSnake(Graphics g, int x, int y)
    {
        SnakeSkin skin = Skins[_selectedSkin];
        DrawRetroSnakeBlock(g, x + 6, y + 11, 12, skin, 3, false);
        DrawRetroSnakeBlock(g, x + 18, y + 11, 12, skin, 2, false);
        DrawRetroSnakeBlock(g, x + 30, y + 11, 12, skin, 1, false);
        DrawRetroSnakeBlock(g, x + 42, y + 11, 12, skin, 0, true);
    }

    private void DrawPixelApple(Graphics g, int x, int y, int pixel)
    {
        using var leaf = new SolidBrush(Color.FromArgb(71, 213, 49));
        using var fruit = new SolidBrush(Color.FromArgb(255, 185, 31));
        using var light = new SolidBrush(Color.FromArgb(255, 222, 74));
        using var shade = new SolidBrush(Color.FromArgb(222, 112, 13));
        using var stem = new SolidBrush(Color.FromArgb(54, 111, 16));

        g.FillRectangle(stem, x + pixel * 3, y + pixel, pixel, pixel * 2);
        g.FillRectangle(leaf, x + pixel * 3, y, pixel * 2, pixel);
        g.FillRectangle(leaf, x + pixel * 5, y + pixel, pixel, pixel);

        g.FillRectangle(fruit, x + pixel * 2, y + pixel * 2, pixel * 5, pixel * 4);
        g.FillRectangle(fruit, x + pixel, y + pixel * 3, pixel * 7, pixel * 2);
        g.FillRectangle(light, x + pixel * 2, y + pixel * 2, pixel * 2, pixel);
        g.FillRectangle(light, x + pixel, y + pixel * 3, pixel, pixel);
        g.FillRectangle(shade, x + pixel * 5, y + pixel * 5, pixel * 2, pixel);
        g.FillRectangle(shade, x + pixel * 6, y + pixel * 4, pixel, pixel);
    }

    private static void DrawPixelText(Graphics g, string text, int centerX,
        int top, int pixelSize, Color color)
    {
        int widthUnits = text.Sum(c => c == ' ' ? 4 : 6) - 1;
        int x = centerX - widthUnits * pixelSize / 2;
        using var brush = new SolidBrush(color);

        foreach (char raw in text)
        {
            char c = char.ToUpperInvariant(raw);
            if (c == ' ')
            {
                x += pixelSize * 4;
                continue;
            }

            string[] glyph = PixelGlyph(c);
            for (int row = 0; row < glyph.Length; row++)
            for (int column = 0; column < glyph[row].Length; column++)
            {
                if (glyph[row][column] == '1')
                    g.FillRectangle(brush, x + column * pixelSize,
                        top + row * pixelSize, pixelSize, pixelSize);
            }

            x += pixelSize * 6;
        }
    }

    private static string[] PixelGlyph(char c) => c switch
    {
        'A' => ["01110","10001","10001","11111","10001","10001","10001"],
        'B' => ["11110","10001","10001","11110","10001","10001","11110"],
        'C' => ["01111","10000","10000","10000","10000","10000","01111"],
        'D' => ["11110","10001","10001","10001","10001","10001","11110"],
        'E' => ["11111","10000","10000","11110","10000","10000","11111"],
        'F' => ["11111","10000","10000","11110","10000","10000","10000"],
        'G' => ["01111","10000","10000","10111","10001","10001","01111"],
        'H' => ["10001","10001","10001","11111","10001","10001","10001"],
        'I' => ["11111","00100","00100","00100","00100","00100","11111"],
        'J' => ["00111","00010","00010","00010","10010","10010","01100"],
        'K' => ["10001","10010","10100","11000","10100","10010","10001"],
        'L' => ["10000","10000","10000","10000","10000","10000","11111"],
        'M' => ["10001","11011","10101","10101","10001","10001","10001"],
        'N' => ["10001","11001","10101","10011","10001","10001","10001"],
        'O' => ["01110","10001","10001","10001","10001","10001","01110"],
        'P' => ["11110","10001","10001","11110","10000","10000","10000"],
        'Q' => ["01110","10001","10001","10001","10101","10010","01101"],
        'R' => ["11110","10001","10001","11110","10100","10010","10001"],
        'S' => ["01111","10000","10000","01110","00001","00001","11110"],
        'T' => ["11111","00100","00100","00100","00100","00100","00100"],
        'U' => ["10001","10001","10001","10001","10001","10001","01110"],
        'V' => ["10001","10001","10001","10001","10001","01010","00100"],
        'W' => ["10001","10001","10001","10101","10101","10101","01010"],
        'X' => ["10001","10001","01010","00100","01010","10001","10001"],
        'Y' => ["10001","10001","01010","00100","00100","00100","00100"],
        'Z' => ["11111","00001","00010","00100","01000","10000","11111"],
        '0' => ["01110","10001","10011","10101","11001","10001","01110"],
        '1' => ["00100","01100","00100","00100","00100","00100","01110"],
        '2' => ["01110","10001","00001","00010","00100","01000","11111"],
        '3' => ["11110","00001","00001","01110","00001","00001","11110"],
        '4' => ["00010","00110","01010","10010","11111","00010","00010"],
        '5' => ["11111","10000","10000","11110","00001","00001","11110"],
        '6' => ["01110","10000","10000","11110","10001","10001","01110"],
        '7' => ["11111","00001","00010","00100","01000","01000","01000"],
        '8' => ["01110","10001","10001","01110","10001","10001","01110"],
        '9' => ["01110","10001","10001","01111","00001","00001","01110"],
        '-' => ["00000","00000","00000","11111","00000","00000","00000"],
        ':' => ["00000","00100","00100","00000","00100","00100","00000"],
        '.' => ["00000","00000","00000","00000","00000","00110","00110"],
        '\'' => ["00100","00100","00010","00000","00000","00000","00000"],
        _ => ["00000","00000","00000","00000","00000","00000","00000"]
    };

    private static void DrawPixelArrow(Graphics g, int x, int y,
        bool left, Color color)
    {
        using var brush = new SolidBrush(color);
        for (int row = 0; row < 5; row++)
        {
            int offset = left ? 4 - row : row;
            int px = x + offset * 4;
            g.FillRectangle(brush, px, y + row * 4, 4, 4);
        }

        int shaftX = left ? x + 12 : x;
        g.FillRectangle(brush, shaftX, y + 8, 12, 4);
    }

    private void DrawRetroWindowControls(Graphics g)
    {
        DrawPixelText(g, "-", 1087, 22, 3, RetroGold);
        DrawPixelOutline(g, new Rectangle(1039, 20, 16, 16), RetroGold, 2);
        DrawPixelText(g, "X", 1127, 20, 2, RetroGold);
    }

    private void DrawHomeOptionCard(Graphics g, Rectangle rect,
        string title, string description, string symbol, int hoverId)
    {
        bool hover = _hoverId == hoverId;
        FillRound(g, rect, 13, hover
            ? Color.FromArgb(33, 55, 52)
            : PanelLight);
        StrokeRound(g, rect, 13, hover ? Color.FromArgb(115, Green) : Border);

        FillRound(g, new Rectangle(rect.X + 15, rect.Y + 18, 44, 44), 13,
            hover ? Color.FromArgb(62, 104, 64) : Color.FromArgb(37, 58, 49));
        DrawLabel(g, symbol, _heroFont, _greenBrush,
            new Rectangle(rect.X + 15, rect.Y + 18, 44, 44), _centerFormat);
        DrawLabel(g, title, _buttonFont, _whiteBrush,
            new Rectangle(rect.X + 70, rect.Y + 16, rect.Width - 84, 24), _leftFormat);
        DrawLabel(g, description, _smallFont, _mutedBrush,
            new Rectangle(rect.X + 70, rect.Y + 42, rect.Width - 84, 30), _leftFormat);
        DrawLabel(g, "OPEN  ›", _smallFont, hover ? _greenBrush : _mutedBrush,
            new Rectangle(rect.X + 16, rect.Y + 79, rect.Width - 30, 18), _leftFormat);
    }

    private void DrawSkinsPage(Graphics g)
    {
        DrawPixelText(g, "BACK", 104, 75, 3,
            _skinPickerClose.Contains(_mousePoint) ? ActiveTheme.Highlight : RetroWhite);
        DrawPixelText(g, "SKINS", DesignWidth / 2, 48, 8, RetroWhite);
        DrawSkinPicker(g);

        SnakeSkin skin = Skins[_selectedSkin];
        Rectangle previewArea = new(790, 145, 328, 320);
        DrawPixelPanel(g, previewArea, true);
        DrawRetroSnakePreview(g, previewArea, skin);

        DrawPixelText(g, SkinShortName(_selectedSkin), 954, 490, 4,
            skin.IsLegendary ? ActiveTheme.Highlight : RetroWhite);
        DrawPixelText(g, SkinPatternName(skin.Pattern).ToUpperInvariant(),
            954, 535, 2, RetroWhite);

        DrawPixelText(g, skin.IsLegendary
                ? "SPECIAL AURA AND SPARKS"
                : "UNIQUE PIXEL DESIGN",
            954, 574, 2,
            skin.IsLegendary ? ActiveTheme.Accent : RetroWhite);

        DrawPixelText(g, "CLICK A SKIN TO EQUIP",
            954, 620, 2, RetroWhite);
    }

    private void DrawSkinPicker(Graphics g)
    {
        for (int i = 0; i < Skins.Length; i++)
        {
            Rectangle tile = SkinTileRect(i);
            SnakeSkin skin = Skins[i];
            bool selected = i == _selectedSkin;
            bool hover = tile.Contains(_mousePoint);

            using var fill = new SolidBrush(selected
                ? Color.FromArgb(255, ActiveTheme.Grid)
                : hover ? Color.FromArgb(255, ActiveTheme.Background) : RetroDark);
            g.FillRectangle(fill, tile);
            DrawPixelOutline(g, tile,
                selected ? ActiveTheme.Highlight : RetroGold,
                selected ? 3 : 2);

            DrawSkinSwatch(g, tile, skin);
            DrawPixelText(g, SkinShortName(i), tile.X + 151,
                tile.Y + 23, 2, skin.IsLegendary
                    ? ActiveTheme.Highlight
                    : selected ? ActiveTheme.Text : RetroWhite);
        }

        DrawPixelText(g, "ARROWS TO SELECT  -  ENTER TO RETURN",
            390, 628, 2, RetroWhite);
    }

    private void DrawSkinSwatch(Graphics g, Rectangle tile, SnakeSkin skin)
    {
        int cell = 13;
        int y = tile.Y + 23;
        DrawRetroSnakeBlock(g, tile.X + 22, y, cell, skin, 0, false);
        DrawRetroSnakeBlock(g, tile.X + 35, y, cell, skin, 1, false);
        DrawRetroSnakeBlock(g, tile.X + 48, y, cell, skin, 2, true);
    }

    private static string SkinShortName(int index) => index switch
    {
        0 => "CLASSIC",
        1 => "RUBY FANG",
        2 => "OCEAN WAVE",
        3 => "ARCTIC",
        4 => "SOLAR GOLD",
        5 => "CYBER NEON",
        6 => "TOXIC SLIME",
        7 => "PURPLE HAZE",
        8 => "SUNSET",
        9 => "TIGER",
        10 => "ZEBRA",
        11 => "MINT CHIP",
        12 => "CANDY POP",
        13 => "LAVA CORE",
        14 => "GALAXY",
        15 => "VOLT STRIKE",
        16 => "JUNGLE CAMO",
        17 => "ROSE QUARTZ",
        18 => "DIAMOND",
        19 => "SHADOW OPS",
        _ => "PHOENIX"
    };

    private void DrawRetroSnakePreview(Graphics g, Rectangle area, SnakeSkin skin)
    {
        const int cellSize = 26;
        int size = cellSize;
        int firstColumn = area.X + 18 + cellSize / 2;
        int secondColumn = firstColumn + cellSize;
        int thirdColumn = secondColumn + cellSize;
        int upperRow = area.Y + 10 + cellSize * 6 + cellSize / 2;
        int middleRow = upperRow + cellSize;
        int lowerRow = middleRow + cellSize;

        // Every segment is centered inside a preview-grid cell.
        Point[] centers =
        [
            new(thirdColumn, lowerRow),
            new(secondColumn, lowerRow),
            new(firstColumn, lowerRow),
            new(firstColumn, middleRow),
            new(firstColumn, upperRow),
            new(secondColumn, upperRow),
            new(thirdColumn, upperRow),
            new(thirdColumn + cellSize, upperRow),
            new(thirdColumn + cellSize * 2, upperRow),
            new(thirdColumn + cellSize * 3, upperRow)
        ];

        using (var grid = new Pen(Color.FromArgb(35, 190, 210, 130), 1))
        {
            for (int x = area.Left + 18; x < area.Right; x += 26)
                g.DrawLine(grid, x, area.Top + 10, x, area.Bottom - 10);
            for (int y = area.Top + 10; y < area.Bottom; y += 26)
                g.DrawLine(grid, area.Left + 10, y, area.Right - 10, y);
        }

        if (skin.IsLegendary)
            DrawRetroLegendaryAura(g, centers[0]);

        for (int i = centers.Length - 1; i >= 0; i--)
            DrawRetroSnakeBlock(g, centers[i].X, centers[i].Y, size,
                skin, i, i == 0);

    }

    private void DrawRetroLegendaryAura(Graphics g, Point center)
    {
        int phase = (int)(Environment.TickCount64 / 180) % 8;
        Color[] colors =
        [
            Color.FromArgb(255, 220, 93),
            Color.FromArgb(210, 151, 255),
            Color.White
        ];

        for (int i = 0; i < 8; i++)
        {
            double angle = (i + phase) * Math.PI / 4;
            int radius = 34 + (i % 3) * 5;
            int x = center.X + (int)Math.Round(Math.Cos(angle) * radius);
            int y = center.Y + (int)Math.Round(Math.Sin(angle) * radius);

            using var spark = new SolidBrush(colors[i % colors.Length]);
            g.FillRectangle(spark, x, y, 5, 5);
            if (i % 2 == 0)
                g.FillRectangle(spark, x - 3, y + 2, 11, 2);
        }
    }

    private void DrawRetroSnakeBlock(Graphics g, int centerX, int centerY,
        int size, SnakeSkin skin, int index, bool head)
    {
        int x = centerX - size / 2;
        int y = centerY - size / 2;
        int pad = Math.Max(2, size / 7);

        bool glow = skin.Pattern is SkinPattern.Neon or SkinPattern.Flames or
            SkinPattern.Frost or SkinPattern.Galaxy or SkinPattern.Circuit or
            SkinPattern.Honeycomb or SkinPattern.Legendary;

        if (glow)
        {
            using var glowBrush = new SolidBrush(Color.FromArgb(42, skin.PatternColor));
            g.FillRectangle(glowBrush, x - pad, y - pad, size + pad * 2, size + pad * 2);
        }

        using var outline = new SolidBrush(skin.Outline);
        using var body = new SolidBrush(head ? skin.Head : skin.Body);
        using var pattern = new SolidBrush(skin.PatternColor);
        using var accent = new SolidBrush(skin.Highlight);

        g.FillRectangle(outline, x, y, size, size);
        g.FillRectangle(body, x + pad, y + pad, size - pad * 2, size - pad * 2);
        g.FillRectangle(accent, x + pad, y + pad, size - pad * 2, Math.Max(2, pad / 2));

        int mark = Math.Max(2, size / 6);
        switch (skin.Pattern)
        {
            case SkinPattern.Stripes:
            case SkinPattern.Tiger:
            case SkinPattern.Zebra:
                g.FillRectangle(pattern, x + size / 3, y + pad, mark, size - pad * 2);
                g.FillRectangle(pattern, x + size * 2 / 3, y + pad, mark, size - pad * 2);
                break;
            case SkinPattern.Dots:
            case SkinPattern.Coral:
                g.FillRectangle(pattern, x + pad * 2, y + pad * 2, mark, mark);
                g.FillRectangle(accent, x + size / 2, y + size / 2, mark, mark);
                if (skin.Pattern == SkinPattern.Coral)
                    g.FillRectangle(pattern, x + size - pad - mark, y + pad * 2, mark, size / 2);
                break;
            case SkinPattern.Waves:
            case SkinPattern.Candy:
                g.FillRectangle(pattern, x + pad, y + size / 2, size / 3, mark);
                g.FillRectangle(accent, x + size / 2, y + size / 3, size / 3, mark);
                break;
            case SkinPattern.Lightning:
                g.FillRectangle(pattern, x + size / 2, y + pad, mark, mark * 2);
                g.FillRectangle(pattern, x + size / 3, y + size / 2, mark, mark);
                g.FillRectangle(pattern, x + size / 2, y + size / 2, mark, mark * 2);
                break;
            case SkinPattern.Scales:
            case SkinPattern.Honeycomb:
            case SkinPattern.Prism:
                g.FillRectangle(pattern, x + size / 3, y + size / 3, mark, mark);
                g.FillRectangle(accent, x + size / 2, y + size / 2, mark, mark);
                g.FillRectangle(pattern, x + size * 2 / 3 - mark, y + size * 2 / 3 - mark, mark, mark);
                break;
            case SkinPattern.Flames:
                g.FillRectangle(pattern, x + size / 2, y, mark, mark * 2);
                g.FillRectangle(pattern, x + size / 3, y + pad, mark, mark * 2);
                g.FillRectangle(accent, x + size / 2, y + size / 2, mark, mark);
                break;
            case SkinPattern.Frost:
            case SkinPattern.Stars:
            case SkinPattern.Galaxy:
                g.FillRectangle(pattern, x + size / 2 - mark / 2, y + pad, mark, size - pad * 2);
                g.FillRectangle(pattern, x + pad, y + size / 2 - mark / 2, size - pad * 2, mark);
                break;
            case SkinPattern.Circuit:
                g.FillRectangle(pattern, x + pad, y + size / 3, size / 2, mark);
                g.FillRectangle(pattern, x + size / 2, y + size / 3, mark, size / 2);
                g.FillRectangle(accent, x + size / 2, y + size / 2, mark, mark);
                break;
            case SkinPattern.Camo:
                g.FillRectangle(pattern, x + pad, y + size / 3, size / 3, mark);
                g.FillRectangle(accent, x + size / 2, y + size / 2, size / 3, mark);
                break;
            case SkinPattern.Neon:
                g.FillRectangle(pattern, x + pad, y + size / 3, size - pad * 2, mark);
                g.FillRectangle(accent, x + pad, y + size * 2 / 3, size - pad * 2, mark);
                break;
            case SkinPattern.Royal:
                g.FillRectangle(pattern, x + pad, y + size / 2, mark, size / 3);
                g.FillRectangle(pattern, x + size / 2 - mark / 2, y + pad, mark, size / 2);
                g.FillRectangle(pattern, x + size - pad - mark, y + size / 2, mark, size / 3);
                break;
            case SkinPattern.Legendary:
                g.FillRectangle(pattern, x + size / 2 - mark / 2, y + pad, mark, size - pad * 2);
                g.FillRectangle(accent, x + pad, y + size / 2 - mark / 2, size - pad * 2, mark);
                break;
        }

        if (head)
        {
            using var eye = new SolidBrush(Color.FromArgb(10, 35, 4));
            int eyeX = x + size - pad * 2;
            int eyeTop = y + pad * 2;
            int eyeBottom = y + size - pad * 2 - mark;
            g.FillRectangle(_eyeWhiteBrush, eyeX, eyeTop, mark, mark);
            g.FillRectangle(_eyeWhiteBrush, eyeX, eyeBottom, mark, mark);
            g.FillRectangle(eye, eyeX + 1, eyeTop + 1, Math.Max(1, mark / 2), Math.Max(1, mark / 2));
            g.FillRectangle(eye, eyeX + 1, eyeBottom + 1, Math.Max(1, mark / 2), Math.Max(1, mark / 2));
        }
    }

    private void DrawPixelPanel(Graphics g, Rectangle rect, bool goldBorder)
    {
        using var fill = new SolidBrush(RetroDark);
        g.FillRectangle(fill, rect);
        DrawPixelOutline(g, rect, goldBorder ? RetroGold : RetroWhite, 4);
    }

    private void DrawPixelOutline(Graphics g, Rectangle rect, Color color, int thickness)
    {
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, rect.X, rect.Y, rect.Width, thickness);
        g.FillRectangle(brush, rect.X, rect.Bottom - thickness, rect.Width, thickness);
        g.FillRectangle(brush, rect.X, rect.Y, thickness, rect.Height);
        g.FillRectangle(brush, rect.Right - thickness, rect.Y, thickness, rect.Height);
    }

    private static string SkinPatternName(SkinPattern pattern) => pattern switch
    {
        SkinPattern.Plain => "Classic finish",
        SkinPattern.Stripes => "Ruby cross-stripes",
        SkinPattern.Dots => "Toxic spots",
        SkinPattern.Waves => "Ocean wave lines",
        SkinPattern.Lightning => "Electric lightning",
        SkinPattern.Scales => "Mint scales",
        SkinPattern.Flames => "Blazing flame marks",
        SkinPattern.Frost => "Arctic snowflakes",
        SkinPattern.Circuit => "Cyber circuit traces",
        SkinPattern.Galaxy => "Galaxy stars and orbit",
        SkinPattern.Camo => "Jungle camouflage",
        SkinPattern.Candy => "Candy twist",
        SkinPattern.Coral => "Rose coral branches",
        SkinPattern.Prism => "Diamond facets",
        SkinPattern.Stars => "Solar starbursts",
        SkinPattern.Honeycomb => "Lava honeycomb",
        SkinPattern.Tiger => "Tiger bands",
        SkinPattern.Zebra => "Zebra bands",
        SkinPattern.Neon => "Neon energy lines",
        SkinPattern.Royal => "Royal crown marks",
        _ => "Legendary phoenix aura"
    };

    private void DrawSettingsPage(Graphics g)
    {
        DrawPixelText(g, "SETTINGS", DesignWidth / 2, 76, 6, RetroWhite);

        Rectangle soundPanel = new(70, 178, 425, 128);
        Rectangle scorePanel = new(70, 326, 425, 145);
        Rectangle controlsPanel = new(70, 491, 425, 158);
        Rectangle themePanel = new(520, 145, 610, 510);

        DrawPixelPanel(g, soundPanel, true);
        DrawPixelPanel(g, scorePanel, true);
        DrawPixelPanel(g, controlsPanel, true);
        DrawPixelPanel(g, themePanel, true);

        DrawPixelText(g, "SOUND", 190, 214, 3, RetroWhite);
        DrawPixelText(g, "GAME EFFECTS", 190, 258, 2, RetroGold);
        DrawRetroButton(g, _settingsSoundButton,
            _soundEnabled ? "SOUND ON" : "SOUND OFF", 2,
            _settingsSoundButton.Contains(_mousePoint));

        DrawPixelText(g, "PERSONAL BEST", 190, 355, 3, RetroWhite);
        DrawPixelText(g, FormatScore(_bestScore), 190, 407, 4, RetroGold);
        DrawRetroButton(g, _settingsResetButton, "RESET SCORE", 2,
            _settingsResetButton.Contains(_mousePoint));

        DrawPixelText(g, "KEYBOARD", 190, 525, 3, RetroWhite);
        DrawPixelText(g, "MOVE: WASD / ARROWS", 270, 570, 2, RetroWhite);
        DrawPixelText(g, "PAUSE: SPACE", 260, 612, 2, RetroGold);

        DrawPixelText(g, "SEASONAL THEMES", 825, 174, 3, RetroWhite);
        DrawPixelText(g, "FOUR SEASONS - DAY AND NIGHT",
            825, 207, 2, RetroGold);

        for (int i = 0; i < Themes.Length; i++)
            DrawThemeCard(g, i);
    }

    private static Rectangle ThemeTileRect(int index)
    {
        int column = index % 4;
        int row = index / 4;
        return new Rectangle(540 + column * 144, 238 + row * 190, 136, 180);
    }

    private void DrawThemeCard(Graphics g, int index)
    {
        ArcadeTheme theme = Themes[index];
        Rectangle tile = ThemeTileRect(index);
        bool selected = index == _selectedTheme;
        bool hover = tile.Contains(_mousePoint);

        using var fill = new SolidBrush(theme.Dark);
        g.FillRectangle(fill, tile);
        DrawPixelOutline(g, tile,
            selected ? theme.Highlight : hover ? theme.Text : theme.Accent,
            selected ? 4 : 2);

        Rectangle art = new(tile.X + 8, tile.Y + 8, tile.Width - 16, 78);
        using (var artFill = new SolidBrush(theme.Background))
            g.FillRectangle(artFill, art);
        DrawThemeThumbnail(g, art, theme);

        DrawPixelText(g, theme.Season, tile.X + tile.Width / 2,
            tile.Y + 101, 2, theme.Text);
        DrawPixelText(g, theme.TimeOfDay, tile.X + tile.Width / 2,
            tile.Y + 128, 2, theme.Accent);

        if (selected)
            DrawPixelText(g, "SELECTED", tile.X + tile.Width / 2,
                tile.Y + 154, 1, theme.Highlight);
    }

    private void DrawThemeThumbnail(Graphics g, Rectangle area, ArcadeTheme theme)
    {
        using var ground = new SolidBrush(theme.Dark);
        g.FillRectangle(ground, area.X, area.Bottom - 18, area.Width, 18);

        int sunX = area.Right - 27;
        int sunY = area.Y + 23;
        using var skyAccent = new SolidBrush(theme.Accent);

        if (theme.IsNight)
        {
            g.FillEllipse(skyAccent, sunX - 8, sunY - 8, 18, 18);
            using var cutout = new SolidBrush(theme.Background);
            g.FillEllipse(cutout, sunX - 1, sunY - 12, 18, 18);

            using var stars = new SolidBrush(theme.Highlight);
            Point[] starPoints =
            [
                new(area.X + 15, area.Y + 16),
                new(area.X + 48, area.Y + 30),
                new(area.X + 79, area.Y + 13)
            ];
            foreach (Point star in starPoints)
                g.FillRectangle(stars, star.X, star.Y, 3, 3);
        }
        else
        {
            g.FillEllipse(skyAccent, sunX - 9, sunY - 9, 18, 18);
            using var rays = new Pen(theme.Accent, 2);
            g.DrawLine(rays, sunX, sunY - 15, sunX, sunY - 12);
            g.DrawLine(rays, sunX, sunY + 12, sunX, sunY + 15);
            g.DrawLine(rays, sunX - 15, sunY, sunX - 12, sunY);
            g.DrawLine(rays, sunX + 12, sunY, sunX + 15, sunY);
        }

        Point[] hills =
        [
            new(area.X, area.Bottom - 18),
            new(area.X + 31, area.Y + 48),
            new(area.X + 67, area.Bottom - 18),
            new(area.X + 91, area.Y + 53),
            new(area.Right, area.Bottom - 18)
        ];
        using var landscape = new SolidBrush(theme.Grid);
        g.FillPolygon(landscape, hills);

        using var detail = new SolidBrush(theme.Effect);
        switch (theme.SeasonType)
        {
            case ThemeSeason.Spring:
                DrawPixelFlower(g, area.X + 25, area.Bottom - 27, detail, theme.Accent);
                DrawPixelFlower(g, area.X + 83, area.Bottom - 34, detail, theme.Accent);
                break;
            case ThemeSeason.Summer:
                g.FillRectangle(detail, area.X + 11, area.Bottom - 25, 25, 3);
                g.FillRectangle(detail, area.X + 42, area.Bottom - 30, 24, 3);
                g.FillRectangle(detail, area.X + 69, area.Bottom - 24, 25, 3);
                break;
            case ThemeSeason.Autumn:
                DrawPixelLeaf(g, area.X + 25, area.Bottom - 31, detail, 8);
                DrawPixelLeaf(g, area.X + 82, area.Bottom - 27, detail, 7);
                break;
            case ThemeSeason.Winter:
                DrawPixelSnowflake(g, area.X + 27, area.Bottom - 30, detail, 5);
                DrawPixelSnowflake(g, area.X + 83, area.Bottom - 31, detail, 5);
                break;
        }
    }

    private static void DrawPixelFlower(Graphics g, int x, int y,
        Brush brush, Color centerColor)
    {
        g.FillRectangle(brush, x - 3, y - 1, 7, 3);
        g.FillRectangle(brush, x - 1, y - 3, 3, 7);
        using var center = new SolidBrush(centerColor);
        g.FillRectangle(center, x, y, 2, 2);
    }

    private static void DrawPixelLeaf(Graphics g, int x, int y, Brush brush, int size)
    {
        g.FillRectangle(brush, x - size / 2, y - 2, size, 5);
        g.FillRectangle(brush, x - 2, y - size / 2, 5, size);
        using var stem = new Pen(Color.FromArgb(170, 45, 31, 15), 1);
        g.DrawLine(stem, x - size / 2, y + 3, x + size / 2, y - 3);
    }

    private static void DrawPixelSnowflake(Graphics g, int x, int y, Brush brush, int size)
    {
        using var pen = new Pen(brush, 2);
        g.DrawLine(pen, x - size, y, x + size, y);
        g.DrawLine(pen, x, y - size, x, y + size);
        g.DrawLine(pen, x - size + 1, y - size + 1, x + size - 1, y + size - 1);
        g.DrawLine(pen, x - size + 1, y + size - 1, x + size - 1, y - size + 1);
    }

    private void DrawThemeAtmosphere(Graphics g)
    {
        ArcadeTheme theme = ActiveTheme;
        long tick = Environment.TickCount64;
        int drift = (int)(tick / 120) % 84;

        if (theme.IsNight)
        {
            using var stars = new SolidBrush(theme.Highlight);
            for (int i = 0; i < 12; i++)
            {
                int x = 24 + (i * 97) % (DesignWidth - 48);
                int y = 82 + (i * 53) % 610;
                int twinkle = (i + (int)(tick / 300)) % 3 == 0 ? 3 : 2;
                g.FillRectangle(stars, x, y, twinkle, twinkle);
            }
        }
        else
        {
            using var sun = new SolidBrush(Color.FromArgb(95, theme.Accent));
            g.FillEllipse(sun, 1056, 78, 54, 54);
            using var rays = new Pen(Color.FromArgb(115, theme.Accent), 2);
            for (int i = 0; i < 8; i++)
            {
                double angle = i * Math.PI / 4;
                float x1 = 1083 + (float)Math.Cos(angle) * 30;
                float y1 = 105 + (float)Math.Sin(angle) * 30;
                float x2 = 1083 + (float)Math.Cos(angle) * 38;
                float y2 = 105 + (float)Math.Sin(angle) * 38;
                g.DrawLine(rays, x1, y1, x2, y2);
            }
        }

        for (int i = 0; i < 8; i++)
        {
            int x = i % 2 == 0 ? 18 + (i * 7) % 25 : DesignWidth - 42 - (i * 5) % 20;
            int y = 105 + (i * 91 + drift) % 590;
            using var effect = new SolidBrush(Color.FromArgb(175, theme.Effect));

            switch (theme.SeasonType)
            {
                case ThemeSeason.Spring:
                    DrawPixelFlower(g, x, y, effect, theme.Accent);
                    break;
                case ThemeSeason.Summer:
                    g.FillRectangle(effect, x, y, 3, 3);
                    g.FillRectangle(effect, x - 4, y + 5, 11, 2);
                    break;
                case ThemeSeason.Autumn:
                    DrawPixelLeaf(g, x, y, effect, 8);
                    break;
                case ThemeSeason.Winter:
                    DrawPixelSnowflake(g, x, y, effect, 5);
                    break;
            }
        }
    }

    private void DrawRetroNavigation(Graphics g)
    {
        DrawRetroNavButton(g, _homeNavButton, "HOME", _view == AppView.Home);
        DrawRetroNavButton(g, _playNavButton, "PLAY", _view == AppView.Play);
        DrawRetroNavButton(g, _skinsNavButton, "SKINS", _view == AppView.Skins);
        DrawRetroNavButton(g, _settingsNavButton, "SETTINGS", _view == AppView.Settings);
    }

    private void DrawRetroNavButton(Graphics g, Rectangle rect, string label, bool active)
    {
        bool hover = rect.Contains(_mousePoint);
        using var fill = new SolidBrush(active
            ? Color.FromArgb(255, ActiveTheme.Grid)
            : hover ? Color.FromArgb(255, ActiveTheme.Background) : RetroDark);
        g.FillRectangle(fill, rect);

        Color edge = active ? ActiveTheme.Highlight : RetroGold;
        DrawPixelOutline(g, rect, edge, 2);

        int textSize = 2;
        int textTop = rect.Y + (rect.Height - 7 * textSize) / 2;
        Color textColor = active ? ActiveTheme.Highlight : RetroWhite;
        DrawPixelText(g, label, rect.X + rect.Width / 2, textTop,
            textSize, textColor);
    }

    private void DrawRetroPlayChrome(Graphics g)
    {
        DrawPixelPanel(g, _boardCard, true);
        DrawPixelPanel(g, _rightPanel, true);

        // Give each dashboard group its own panel and consistent spacing.
        DrawPixelPanel(g, new Rectangle(814, 110, 302, 130), true);
        DrawPixelPanel(g, new Rectangle(814, 246, 302, 120), true);
        DrawPixelPanel(g, new Rectangle(814, 372, 302, 122), true);
        DrawPixelPanel(g, new Rectangle(814, 500, 302, 188), true);

        using var boardFill = new SolidBrush(RetroDark);
        g.FillRectangle(boardFill, _field);
        using var grid = new Pen(Color.FromArgb(75, ActiveTheme.Grid), 1);
        for (int x = _field.Left; x <= _field.Right; x += CellSize)
            g.DrawLine(grid, x, _field.Top, x, _field.Bottom);
        for (int y = _field.Top; y <= _field.Bottom; y += CellSize)
            g.DrawLine(grid, _field.Left, y, _field.Right, y);
        DrawPixelOutline(g, _field, RetroGold, 3);

        DrawPixelText(g, "SCORE", 881, 125, 2, RetroWhite);
        DrawPixelText(g, "BEST", 1046, 125, 2, RetroGold);
        DrawPixelText(g, "MODE", 960, 265, 2, RetroWhite);
        DrawPixelText(g, "DIFFICULTY", 960, 385, 2, RetroWhite);
        DrawPixelText(g, "ACTIONS", 960, 516, 2, RetroGold);
    }

    private void DrawRetroPlayDashboard(Graphics g)
    {
        DrawPixelText(g, FormatScore(_score), 881, 160, 4, RetroWhite);
        DrawPixelText(g, FormatScore(_bestScore), 1046, 160, 4, RetroGold);

        DrawRetroButton(g, _classicButton, "CLASSIC", 1,
            _mode == GameMode.Classic, _mode == GameMode.Classic);
        DrawRetroButton(g, _wrapButton, "WRAP", 2,
            _mode == GameMode.Wrap, _mode == GameMode.Wrap);
        DrawRetroButton(g, _rushButton, "RUSH", 2,
            _mode == GameMode.Rush, _mode == GameMode.Rush);

        DrawRetroButton(g, _easyButton, "EASY", 2,
            _difficulty == Difficulty.Easy, _difficulty == Difficulty.Easy);
        DrawRetroButton(g, _normalButton, "NORMAL", 1,
            _difficulty == Difficulty.Normal, _difficulty == Difficulty.Normal);
        DrawRetroButton(g, _hardButton, "HARD", 2,
            _difficulty == Difficulty.Hard, _difficulty == Difficulty.Hard);

        string action = _state switch
        {
            GameState.Playing => "PAUSE",
            GameState.Paused => "RESUME",
            _ => "PLAY"
        };
        DrawRetroButton(g, _mainButton, action, 2, true, true);
        DrawRetroButton(g, _soundButton, _soundEnabled ? "SOUND" : "MUTE", 1,
            _soundButton.Contains(_mousePoint));
        DrawRetroButton(g, _restartButton, "RESET", 1,
            _restartButton.Contains(_mousePoint));
        DrawRetroButton(g, _skinButton, "SKINS", 2,
            _skinButton.Contains(_mousePoint));
        DrawRetroButton(g, _resetBestButton, "RESET BEST", 1,
            _resetBestButton.Contains(_mousePoint));

    }

    private static string ModeName(GameMode mode) => mode switch
    {
        GameMode.Classic => "Classic",
        GameMode.Wrap => "Wrap",
        _ => "Rush"
    };

    private static string DifficultyName(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => "Easy",
        Difficulty.Hard => "Hard",
        _ => "Normal"
    };

    private void DrawDashboard(Graphics g)
    {
        DrawScorePanel(g);

        if (_mode == GameMode.Rush)
            DrawLabel(g, $"{Math.Ceiling(_rushTimeLeft.TotalSeconds):00}s", _labelFont,
                _goldBrush, _modeTimerRect, _rightFormat);

        DrawChoice(g, _classicButton, "CLASSIC", _mode == GameMode.Classic);
        DrawChoice(g, _wrapButton, "WRAP", _mode == GameMode.Wrap);
        DrawChoice(g, _rushButton, "RUSH", _mode == GameMode.Rush);

        DrawChoice(g, _easyButton, "EASY", _difficulty == Difficulty.Easy);
        DrawChoice(g, _normalButton, "NORMAL", _difficulty == Difficulty.Normal);
        DrawChoice(g, _hardButton, "HARD", _difficulty == Difficulty.Hard);

        string mainText = _state switch
        {
            GameState.Playing => "PAUSE",
            GameState.Paused => "RESUME",
            _ => "PLAY NOW"
        };

        DrawActionButton(g, _mainButton, mainText, true);
        DrawActionButton(g, _soundButton, _soundEnabled ? "♪" : "×", false);
        DrawActionButton(g, _restartButton, "↻", false);
        DrawSecondaryButton(g, _skinButton, $"SKINS  ·  {Skins[_selectedSkin].Name}");
        DrawSecondaryButton(g, _resetBestButton, "RESET BEST SCORE", true);

        DrawLabel(g, _state switch
        {
            GameState.Playing => "IN PLAY",
            GameState.Paused => "PAUSED",
            GameState.GameOver => "FINISHED",
            _ => "READY"
        }, _labelFont,
            _state == GameState.Playing ? _greenBrush : _mutedBrush,
            _boardStatus, _centerFormat);

        using var statusPen = new Pen(Border, 1);
        DrawRoundOutline(g, _boardStatus, 10, statusPen);
    }

    private void DrawBoardDynamic(Graphics g)
    {
        GraphicsState state = g.Save();
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SetClip(_field);

        if (_state == GameState.Ready)
            DrawWelcomeScreen(g);
        else if (_state is GameState.Paused or GameState.GameOver)
            DrawPauseOrGameOver(g);
        else
        {
            DrawFood(g);
            DrawSnake(g);
            DrawGoldBurst(g);
        }

        g.Restore(state);
    }

    private void DrawWelcomeScreen(Graphics g)
    {
        g.FillRectangle(_overlayBrush, _field);
        g.FillPath(_cardBrush, _overlayPath);

        using (var accent = new LinearGradientBrush(
                   new Rectangle(185, 239, 445, 4),
                   Color.FromArgb(40, Green), Green, 0f))
            g.FillRectangle(accent, 185, 239, 445, 3);

        DrawLabel(g, "WELCOME TO THE ARCADE", _labelFont, _greenBrush,
            new Rectangle(190, 264, 434, 22), _centerFormat);
        DrawLabel(g, "SNAKE", _welcomeFont, _whiteBrush,
            new Rectangle(190, 294, 434, 58), _centerFormat);
        DrawLabel(g, "A simple classic. A fresh challenge every run.",
            _welcomeSmallFont, _mutedBrush,
            new Rectangle(180, 353, 454, 28), _centerFormat);

        DrawLabel(g, "YOUR BEST SCORE", _labelFont, _mutedBrush,
            new Rectangle(230, 392, 170, 24), _rightFormat);
        DrawLabel(g, FormatScore(_bestScore), _numberFont, _greenBrush,
            new Rectangle(412, 383, 150, 42), _leftFormat);

        DrawActionButton(g, _overlayButton, "START GAME", true);
        DrawLabel(g, "WASD  /  ARROW KEYS     •     SPACE TO PAUSE",
            _labelFont, _mutedBrush,
            new Rectangle(180, 523, 454, 20), _centerFormat);
    }

    private void DrawPauseOrGameOver(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.None;

        using var shade = new SolidBrush(Color.FromArgb(225, ActiveTheme.Dark));
        g.FillRectangle(shade, _field);

        Rectangle panel = new(138, 220, 538, 334);
        DrawPixelPanel(g, panel, true);

        using (var accent = new SolidBrush(RetroGold))
            g.FillRectangle(accent, panel.X + 24, panel.Y + 23,
                panel.Width - 48, 3);

        bool paused = _state == GameState.Paused;
        string title = paused ? "PAUSED" : _won ? "PERFECT RUN" : "GAME OVER";
        DrawPixelText(g, title, panel.X + panel.Width / 2, 263, 4,
            paused ? RetroWhite : ActiveTheme.Highlight);

        if (paused)
        {
            DrawPixelText(g, "CURRENT SCORE",
                panel.X + panel.Width / 2, 350, 2, RetroWhite);
            DrawPixelText(g, FormatScore(_score),
                panel.X + panel.Width / 2, 378, 5, ActiveTheme.Highlight);
            DrawPixelText(g, "PERSONAL BEST  " + FormatScore(_bestScore),
                panel.X + panel.Width / 2, 427, 2, RetroGold);
        }
        else
        {
            DrawPixelText(g, "FINAL SCORE",
                panel.X + panel.Width / 2, 326, 2, RetroWhite);
            DrawPixelText(g, FormatScore(_score),
                panel.X + panel.Width / 2, 351, 5,
                _newRecord ? ActiveTheme.Highlight : RetroWhite);

            string result = _scoreSaveFailed
                ? "SCORE SAVE FAILED"
                : _newRecord ? "NEW PERSONAL BEST" : "BEST  " + FormatScore(_bestScore);
            DrawPixelText(g, result, panel.X + panel.Width / 2, 411, 2,
                _scoreSaveFailed || _newRecord ? RetroGold : ActiveTheme.Highlight);
        }

        DrawRetroButton(g, _overlayButton,
            paused ? "RESUME" : "PLAY AGAIN", 2,
            _overlayButton.Contains(_mousePoint), true);

        DrawPixelText(g, paused ? "PRESS SPACE TO RESUME" : "PRESS SPACE TO RESTART",
            panel.X + panel.Width / 2, 529, 1, RetroWhite);
    }

    private Bitmap CreateAppleSprite(bool golden)
    {
        const int designSize = 96;
        const int size = 30;
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppPArgb);

        using Graphics g = Graphics.FromImage(bitmap);
        g.ScaleTransform(size / (float)designSize, size / (float)designSize);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);

        using (var glow = new SolidBrush(golden
                   ? Color.FromArgb(55, 255, 190, 66)
                   : Color.FromArgb(48, 255, 75, 75)))
            g.FillEllipse(glow, 5, 7, 86, 84);

        const float width = 59f;
        const float height = 60f;
        const float left = 18.5f;
        const float right = left + width;
        const float top = 22f;
        const float bottom = top + height;
        const float cx = 48f;

        using var applePath = new GraphicsPath();
        applePath.StartFigure();
        applePath.AddBezier(cx, top + height * 0.27f,
            cx - width * 0.17f, top - height * 0.03f,
            left + width * 0.03f, top + height * 0.04f,
            left + width * 0.04f, top + height * 0.43f);
        applePath.AddBezier(left + width * 0.04f, top + height * 0.43f,
            left - width * 0.02f, bottom - height * 0.10f,
            cx - width * 0.24f, bottom + height * 0.02f,
            cx, bottom - height * 0.06f);
        applePath.AddBezier(cx, bottom - height * 0.06f,
            cx + width * 0.23f, bottom + height * 0.02f,
            right + width * 0.02f, bottom - height * 0.10f,
            right - width * 0.04f, top + height * 0.43f);
        applePath.AddBezier(right - width * 0.04f, top + height * 0.43f,
            right - width * 0.02f, top + height * 0.04f,
            cx + width * 0.17f, top - height * 0.03f,
            cx, top + height * 0.27f);
        applePath.CloseFigure();

        Color topColor = golden ? Color.FromArgb(255, 231, 126) : Color.FromArgb(255, 117, 111);
        Color bottomColor = golden ? Color.FromArgb(214, 132, 31) : Color.FromArgb(174, 31, 48);

        using (var appleBrush = new LinearGradientBrush(
                   new RectangleF(left, top, width, height), topColor, bottomColor, 48f))
            g.FillPath(appleBrush, applePath);

        using (var outline = new Pen(golden ? Color.FromArgb(190, 156, 91, 26) :
                   Color.FromArgb(205, 119, 31, 43), 2.2f))
            g.DrawPath(outline, applePath);

        using (var stem = new Pen(Color.FromArgb(132, 83, 43), 4f)
               { StartCap = LineCap.Round, EndCap = LineCap.Round })
            g.DrawBezier(stem, cx, top + height * 0.28f,
                cx + 0.5f, top + height * 0.05f,
                cx + width * 0.16f, top - height * 0.02f,
                cx + width * 0.20f, top + height * 0.09f);

        using var leafPath = new GraphicsPath();
        leafPath.AddBezier(cx + width * 0.08f, top + height * 0.17f,
            cx + width * 0.25f, top - height * 0.08f,
            cx + width * 0.53f, top - height * 0.03f,
            cx + width * 0.48f, top + height * 0.20f);
        leafPath.AddBezier(cx + width * 0.48f, top + height * 0.20f,
            cx + width * 0.30f, top + height * 0.27f,
            cx + width * 0.18f, top + height * 0.23f,
            cx + width * 0.08f, top + height * 0.17f);

        using (var leafBrush = new LinearGradientBrush(
                   new RectangleF(cx + 2, top - 2, width * 0.5f, height * 0.35f),
                   Color.FromArgb(181, 231, 115), Color.FromArgb(43, 128, 70), 45f))
            g.FillPath(leafBrush, leafPath);

        using (var leafPen = new Pen(Color.FromArgb(170, 42, 111, 57), 1.3f))
            g.DrawPath(leafPen, leafPath);

        using (var shine = new SolidBrush(Color.FromArgb(95, 255, 232, 220)))
            g.FillEllipse(shine, left + width * 0.20f, top + height * 0.34f,
                width * 0.17f, height * 0.27f);

        return bitmap;
    }

    private void DrawFood(Graphics g)
    {
        DrawApple(g, _food, false);

        if (_goldenFood is Point golden)
        {
            bool visible = true;
            if (_goldenExpiresAt is DateTime expiresAt &&
                (expiresAt - DateTime.UtcNow).TotalSeconds <= 3)
                visible = (Environment.TickCount64 / 115) % 2 == 0;

            if (visible)
                DrawApple(g, golden, true);
        }
    }

    private void DrawApple(Graphics g, Point cell, bool golden)
    {
        Bitmap? sprite = golden ? _goldAppleSprite : _appleSprite;
        if (sprite is null)
            return;

        int x = _field.Left + cell.X * CellSize + (CellSize - sprite.Width) / 2;
        int y = _field.Top + cell.Y * CellSize + (CellSize - sprite.Height) / 2;
        g.DrawImageUnscaled(sprite, x, y);
    }

    private void DrawSnake(Graphics g)
    {
        if (_snake.Count == 0)
            return;

        float progress = _state == GameState.Playing ? CurrentProgress() : 1f;

        for (int i = 0; i < _snake.Count; i++)
        {
            Point from = i < _previousSnakeCount ? _previousSnake[i] : _snake[i];
            Point to = _snake[i];

            // A segment crossing a wrap edge must jump to its destination cell.
            // Interpolating the large coordinate delta would send it through the
            // middle of the board for one animation step.
            bool crossedWrapEdge =
                Math.Abs(to.X - from.X) > 1 ||
                Math.Abs(to.Y - from.Y) > 1;

            float cellX = crossedWrapEdge
                ? to.X
                : from.X + (to.X - from.X) * progress;
            float cellY = crossedWrapEdge
                ? to.Y
                : from.Y + (to.Y - from.Y) * progress;

            _snakeCenters[i] = new PointF(
                _field.Left + cellX * CellSize + CellSize / 2f,
                _field.Top + cellY * CellSize + CellSize / 2f);
        }

        DrawSnakeVisual(g, _snakeCenters, _snake.Count, Skins[_selectedSkin], _direction);
    }

    private void DrawSnakePreview(Graphics g, Rectangle area, SnakeSkin skin)
    {
        FillRound(g, area, 15, Color.FromArgb(245, 9, 16, 21));
        StrokeRound(g, area, 15, Color.FromArgb(58, 79, 82));

        GraphicsState state = g.Save();
        g.SetClip(area);

        using (var glow = new SolidBrush(Color.FromArgb(18, skin.PatternColor)))
            g.FillEllipse(glow, area.X + 35, area.Y + 20, area.Width - 70, area.Height - 48);

        using (var grid = new Pen(Color.FromArgb(18, 140, 180, 160), 1))
        {
            for (int x = area.Left + 20; x < area.Right; x += 24)
                g.DrawLine(grid, x, area.Top + 1, x, area.Bottom - 1);
            for (int y = area.Top + 20; y < area.Bottom; y += 24)
                g.DrawLine(grid, area.Left + 1, y, area.Right - 1, y);
        }

        float scale = Math.Clamp(area.Width / 280f, 0.78f, 1.15f);
        float headX = area.Right - 42f * scale;
        float headY = area.Top + area.Height * 0.59f;
        PointF[] previewCenters =
        [
            new(headX, headY),
            new(headX - 19 * scale, headY),
            new(headX - 38 * scale, headY),
            new(headX - 57 * scale, headY),
            new(headX - 76 * scale, headY),
            new(headX - 95 * scale, headY),
            new(headX - 114 * scale, headY),
            new(headX - 132 * scale, headY - 9 * scale),
            new(headX - 137 * scale, headY - 27 * scale),
            new(headX - 128 * scale, headY - 44 * scale),
            new(headX - 108 * scale, headY - 51 * scale),
            new(headX - 88 * scale, headY - 51 * scale)
        ];

        DrawSnakeVisual(g, previewCenters, previewCenters.Length, skin, Direction.Right);
        g.Restore(state);

        DrawLabel(g, skin.IsLegendary ? "PHOENIX PREVIEW" : "LIVE SKIN PREVIEW",
            _smallFont, skin.IsLegendary ? _goldBrush : _mutedBrush,
            new Rectangle(area.X + 14, area.Bottom - 27, area.Width - 28, 16), _leftFormat);
    }

    private void DrawSnakeVisual(Graphics g, PointF[] centers, int count,
        SnakeSkin skin, Direction direction)
    {
        if (count == 0)
            return;

        _snakePath.Reset();
        _snakeHighlightPath.Reset();

        int sectionStart = 0;
        for (int i = 1; i < count; i++)
        {
            float dx = centers[i].X - centers[i - 1].X;
            float dy = centers[i].Y - centers[i - 1].Y;

            if (Math.Abs(dx) > CellSize * 1.5f || Math.Abs(dy) > CellSize * 1.5f)
            {
                AddSnakeSection(g, centers, sectionStart, i - 1);
                sectionStart = i;
            }
        }

        AddSnakeSection(g, centers, sectionStart, count - 1);

        if (_snakePath.PointCount > 0)
        {
            if (skin.Pattern is SkinPattern.Neon or SkinPattern.Flames or
                SkinPattern.Frost or SkinPattern.Galaxy or SkinPattern.Circuit or
                SkinPattern.Honeycomb or SkinPattern.Legendary)
                g.DrawPath(_skinGlowPen, _snakePath);

            g.DrawPath(_snakeOutlinePen, _snakePath);
            g.DrawPath(_snakeBodyPen, _snakePath);
            if (_snakeHighlightPath.PointCount > 0)
                g.DrawPath(_snakeHighlightPen, _snakeHighlightPath);
        }

        if (skin.Pattern != SkinPattern.Plain)
            DrawSkinPattern(g, centers, count, skin);

        if (skin.IsLegendary)
            DrawLegendaryAura(g, centers[0], centers, count);

        DrawSnakeHead(g, centers[0], direction);
    }

    private void AddSnakeSection(Graphics g, PointF[] centers, int start, int end)
    {
        if (end < start)
            return;

        if (end == start)
        {
            PointF center = centers[start];
            const float diameter = CellSize - 6;
            g.FillEllipse(_snakeBrush, center.X - diameter / 2f,
                center.Y - diameter / 2f, diameter, diameter);
            return;
        }

        _snakePath.StartFigure();
        _snakeHighlightPath.StartFigure();

        for (int i = start + 1; i <= end; i++)
        {
            PointF previous = centers[i - 1];
            PointF current = centers[i];
            _snakePath.AddLine(previous, current);
            _snakeHighlightPath.AddLine(
                new PointF(previous.X - 1.2f, previous.Y - 3.2f),
                new PointF(current.X - 1.2f, current.Y - 3.2f));
        }
    }

    private void DrawSnakeHead(Graphics g, PointF center, Direction direction)
    {
        const float size = CellSize - 3;
        RectangleF head = new(center.X - size / 2f, center.Y - size / 2f, size, size);

        g.FillEllipse(_headShadowBrush, head.X - 3, head.Y - 3,
            head.Width + 6, head.Height + 6);
        g.FillEllipse(_headBrush, head);
        g.DrawEllipse(_headOutlinePen, head);
        g.FillEllipse(_headHighlightBrush, head.X + 3.5f, head.Y + 2.5f, 5.5f, 3.2f);

        PointF eyeOne;
        PointF eyeTwo;
        switch (direction)
        {
            case Direction.Up:
                eyeOne = new PointF(center.X - 4.2f, center.Y - 4.0f);
                eyeTwo = new PointF(center.X + 4.2f, center.Y - 4.0f);
                break;
            case Direction.Down:
                eyeOne = new PointF(center.X - 4.2f, center.Y + 4.0f);
                eyeTwo = new PointF(center.X + 4.2f, center.Y + 4.0f);
                break;
            case Direction.Left:
                eyeOne = new PointF(center.X - 4.0f, center.Y - 4.2f);
                eyeTwo = new PointF(center.X - 4.0f, center.Y + 4.2f);
                break;
            default:
                eyeOne = new PointF(center.X + 4.0f, center.Y - 4.2f);
                eyeTwo = new PointF(center.X + 4.0f, center.Y + 4.2f);
                break;
        }

        g.FillEllipse(_eyeWhiteBrush, eyeOne.X - 2.0f, eyeOne.Y - 2.0f, 4f, 4f);
        g.FillEllipse(_eyeWhiteBrush, eyeTwo.X - 2.0f, eyeTwo.Y - 2.0f, 4f, 4f);
        g.FillEllipse(_eyeBrush, eyeOne.X - 0.8f, eyeOne.Y - 0.8f, 2.2f, 2.2f);
        g.FillEllipse(_eyeBrush, eyeTwo.X - 0.8f, eyeTwo.Y - 0.8f, 2.2f, 2.2f);
    }

    private void SpawnGoldBurst(Point cell, DateTime now)
    {
        _goldBurstCenter = new PointF(
            _field.Left + cell.X * CellSize + CellSize / 2f,
            _field.Top + cell.Y * CellSize + CellSize / 2f);
        _goldBurstStarted = now;
        _goldBurstActive = true;
        _goldParticleCount = _goldParticles.Length;

        for (int i = 0; i < _goldParticleCount; i++)
        {
            double angle = _random.NextDouble() * Math.PI * 2;
            float speed = 55f + (float)_random.NextDouble() * 150f;

            _goldParticles[i] = new GoldParticle
            {
                X = _goldBurstCenter.X,
                Y = _goldBurstCenter.Y,
                VelocityX = (float)Math.Cos(angle) * speed,
                VelocityY = (float)Math.Sin(angle) * speed,
                Age = 0,
                Lifetime = 0.48f + (float)_random.NextDouble() * 0.46f,
                Size = 2.2f + (float)_random.NextDouble() * 3.4f
            };
        }
    }

    private void UpdateGoldBurst(float elapsedSeconds, DateTime now)
    {
        if (!_goldBurstActive)
            return;

        int write = 0;
        for (int i = 0; i < _goldParticleCount; i++)
        {
            GoldParticle particle = _goldParticles[i];
            particle.Age += elapsedSeconds;

            if (particle.Age >= particle.Lifetime)
                continue;

            particle.VelocityX *= 0.985f;
            particle.VelocityY = particle.VelocityY * 0.985f + 55f * elapsedSeconds;
            particle.X += particle.VelocityX * elapsedSeconds;
            particle.Y += particle.VelocityY * elapsedSeconds;
            _goldParticles[write++] = particle;
        }

        _goldParticleCount = write;
        if (_goldParticleCount == 0 && (now - _goldBurstStarted).TotalSeconds > 0.85)
            _goldBurstActive = false;
    }

    private void DrawGoldBurst(Graphics g)
    {
        if (!_goldBurstActive)
            return;

        float elapsed = (float)(DateTime.UtcNow - _goldBurstStarted).TotalSeconds;
        float ringProgress = Math.Clamp(elapsed / 0.62f, 0f, 1f);
        float radius = 9f + ringProgress * 72f;

        if (ringProgress < 1f)
            g.DrawEllipse(_goldBurstPens[elapsed < 0.22f ? 0 : elapsed < 0.42f ? 1 : 2],
                _goldBurstCenter.X - radius, _goldBurstCenter.Y - radius,
                radius * 2, radius * 2);

        for (int i = 0; i < _goldParticleCount; i++)
        {
            GoldParticle particle = _goldParticles[i];
            float remaining = 1f - particle.Age / particle.Lifetime;
            int brushIndex = remaining > 0.72f ? 0 : remaining > 0.48f ? 1 :
                remaining > 0.24f ? 2 : 3;
            float size = particle.Size * (0.45f + remaining * 0.7f);
            g.FillEllipse(_goldParticleBrushes[brushIndex],
                particle.X - size / 2f, particle.Y - size / 2f, size, size);
        }

        if (elapsed < 0.7f)
        {
            int yOffset = (int)(elapsed * 30f);
            DrawLabel(g, "+30", _effectFont, _goldBrush,
                new Rectangle((int)_goldBurstCenter.X - 30,
                    (int)_goldBurstCenter.Y - 42 - yOffset, 60, 24), _centerFormat);
        }
    }

    private static Rectangle SkinTileRect(int index)
    {
        int column = index % 3;
        int row = index / 3;
        return new Rectangle(42 + column * 236, 145 + row * 66, 224, 58);
    }

    private void DrawSecondaryButton(Graphics g, Rectangle rect, string label, bool warning = false)
    {
        bool hover = rect.Contains(_mousePoint);
        FillRound(g, rect, 8, hover
            ? Color.FromArgb(39, 52, 56)
            : Panel);
        StrokeRound(g, rect, 8, warning
            ? Color.FromArgb(99, 67, 63)
            : Border);
        DrawLabel(g, label, _buttonFont,
            warning ? (hover ? _whiteBrush : _mutedBrush) : (hover ? _greenBrush : _whiteBrush),
            rect, _centerFormat);
    }

    private void ApplySkin(int index)
    {
        _selectedSkin = Math.Clamp(index, 0, Skins.Length - 1);
        SnakeSkin skin = Skins[_selectedSkin];

        _snakeBrush.Dispose();
        _headBrush.Dispose();
        _headShadowBrush.Dispose();

        _snakeBrush = new SolidBrush(skin.Body);
        _headBrush = new SolidBrush(skin.Head);
        _headShadowBrush = new SolidBrush(Color.FromArgb(48, skin.Head));

        _snakeOutlinePen.Color = skin.Outline;
        _snakeBodyPen.Color = skin.Body;
        _snakeHighlightPen.Color = skin.Highlight;
        _headOutlinePen.Color = skin.Outline;
        _skinPatternPen.Color = skin.PatternColor;
        _skinAccentPen.Color = skin.Highlight;
        int glowAlpha = skin.Pattern is SkinPattern.Neon or SkinPattern.Flames or
            SkinPattern.Frost or SkinPattern.Galaxy or SkinPattern.Circuit or
            SkinPattern.Honeycomb or SkinPattern.Legendary ? 58 : 0;
        _skinGlowPen.Color = Color.FromArgb(glowAlpha, skin.PatternColor);

        InvalidateDesign(_field);
        InvalidateDesign(_rightPanel);
    }

    private void SaveSelectedSkin()
    {
        try
        {
            string path = SkinPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, _selectedSkin.ToString(CultureInfo.InvariantCulture));
        }
        catch
        {
            MessageBox.Show(this, "The skin could not be saved. It will remain selected for this session.",
                "Skin Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static int LoadSelectedSkin()
    {
        try
        {
            return File.Exists(SkinPath()) &&
                   int.TryParse(File.ReadAllText(SkinPath()).Trim(),
                       NumberStyles.Integer, CultureInfo.InvariantCulture, out int index)
                ? Math.Clamp(index, 0, Skins.Length - 1)
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static string SkinPath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SnakeGame",
            "selected-skin.txt");

    private static string ThemePath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SnakeGame",
            "selected-theme.txt");

    private static int LoadSelectedTheme()
    {
        try
        {
            return File.Exists(ThemePath()) &&
                   int.TryParse(File.ReadAllText(ThemePath()).Trim(),
                       NumberStyles.Integer, CultureInfo.InvariantCulture, out int index)
                ? Math.Clamp(index, 0, Themes.Length - 1)
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    private void SelectTheme(int index)
    {
        index = Math.Clamp(index, 0, Themes.Length - 1);
        if (_selectedTheme == index)
            return;

        _selectedTheme = index;
        try
        {
            string path = ThemePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, index.ToString(CultureInfo.InvariantCulture));
        }
        catch
        {
            // Keep the selected theme active for this session.
        }

        Invalidate();
    }

    private static string FormatScore(int value) =>
        Math.Max(0, value).ToString("D5", CultureInfo.InvariantCulture);

    private void ResetBestScore()
    {
        DialogResult answer = MessageBox.Show(this,
            "Reset your personal best score to zero? This cannot be undone.",
            "Reset Best Score", MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

        if (answer != DialogResult.Yes)
            return;

        try
        {
            string path = BestScorePath();
            if (File.Exists(path))
                File.Delete(path);
            if (File.Exists(path + ".bak"))
                File.Delete(path + ".bak");

            _bestScore = 0;
            _newRecord = false;
            _scoreSaveFailed = false;
            InvalidateDesign(_scoreArea);
            InvalidateDesign(_field);

            MessageBox.Show(this, "Your personal best has been reset.",
                "Best Score", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not reset the saved score.\n{ex.Message}",
                "Reset Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DrawSkinPattern(Graphics g, PointF[] centers, int count, SnakeSkin skin)
    {
        _skinPatternPen.Color = skin.PatternColor;
        _skinAccentPen.Color = skin.Highlight;

        float phase = Environment.TickCount64 / 180f;
        int stride = skin.Pattern is SkinPattern.Dots or SkinPattern.Stars or
            SkinPattern.Galaxy or SkinPattern.Coral ? 2 : 3;

        for (int i = 1; i < count - 1; i += stride)
        {
            PointF p = centers[i];
            PointF before = centers[i - 1];
            PointF after = centers[i + 1];

            // Do not build a skin pattern across the visual gap at a wrap seam.
            if (Math.Abs(p.X - before.X) > CellSize * 1.5f ||
                Math.Abs(p.Y - before.Y) > CellSize * 1.5f ||
                Math.Abs(after.X - p.X) > CellSize * 1.5f ||
                Math.Abs(after.Y - p.Y) > CellSize * 1.5f)
                continue;

            float dx = after.X - before.X;
            float dy = after.Y - before.Y;
            float length = Math.Max(1f, MathF.Sqrt(dx * dx + dy * dy));
            float tx = dx / length;
            float ty = dy / length;
            float nx = -ty;
            float ny = tx;

            PointF At(float along, float across) =>
                new(p.X + tx * along + nx * across,
                    p.Y + ty * along + ny * across);

            switch (skin.Pattern)
            {
                case SkinPattern.Stripes:
                    g.DrawLine(_skinPatternPen, At(0, -7), At(0, 7));
                    break;
                case SkinPattern.Dots:
                    g.DrawEllipse(_skinPatternPen, p.X - 3.2f, p.Y - 3.2f, 6.4f, 6.4f);
                    g.DrawEllipse(_skinAccentPen, At(-5, 4).X - 1.8f,
                        At(-5, 4).Y - 1.8f, 3.6f, 3.6f);
                    break;
                case SkinPattern.Waves:
                    g.DrawBezier(_skinPatternPen, At(-7, 0), At(-3, -6),
                        At(3, 6), At(7, 0));
                    break;
                case SkinPattern.Lightning:
                    g.DrawLine(_skinPatternPen, At(-6, -4), At(0, 1));
                    g.DrawLine(_skinPatternPen, At(0, 1), At(-2, 7));
                    g.DrawLine(_skinPatternPen, At(-2, 7), At(7, -3));
                    break;
                case SkinPattern.Scales:
                    g.DrawArc(_skinPatternPen, p.X - 6, p.Y - 5, 12, 10, 195, 150);
                    g.DrawArc(_skinAccentPen, p.X - 2, p.Y - 4, 11, 9, 195, 150);
                    break;
                case SkinPattern.Flames:
                    g.DrawLine(_skinPatternPen, At(-5, 6), At(-2, -2));
                    g.DrawLine(_skinPatternPen, At(-2, -2), At(1, -7));
                    g.DrawLine(_skinPatternPen, At(1, -7), At(3, 0));
                    g.DrawLine(_skinPatternPen, At(3, 0), At(6, 5));
                    break;
                case SkinPattern.Frost:
                    g.DrawLine(_skinPatternPen, At(-6, 0), At(6, 0));
                    g.DrawLine(_skinPatternPen, At(0, -6), At(0, 6));
                    g.DrawLine(_skinAccentPen, At(-4, -4), At(4, 4));
                    g.DrawLine(_skinAccentPen, At(-4, 4), At(4, -4));
                    break;
                case SkinPattern.Circuit:
                    g.DrawLine(_skinPatternPen, At(-7, -4), At(1, -4));
                    g.DrawLine(_skinPatternPen, At(1, -4), At(1, 4));
                    g.DrawLine(_skinPatternPen, At(1, 4), At(7, 4));
                    g.DrawEllipse(_skinAccentPen, At(5, 4).X - 2, At(5, 4).Y - 2, 4, 4);
                    break;
                case SkinPattern.Galaxy:
                    g.DrawLine(_skinPatternPen, At(-5, 0), At(5, 0));
                    g.DrawLine(_skinPatternPen, At(0, -5), At(0, 5));
                    g.DrawEllipse(_skinAccentPen, p.X - 5, p.Y - 3, 10, 6);
                    break;
                case SkinPattern.Camo:
                    g.DrawEllipse(_skinPatternPen, p.X - 5, p.Y - 3, 8, 6);
                    g.DrawEllipse(_skinAccentPen, p.X + 1, p.Y - 4, 7, 5);
                    break;
                case SkinPattern.Candy:
                    g.DrawLine(_skinPatternPen, At(-6, -5), At(5, 5));
                    g.DrawLine(_skinAccentPen, At(-6, 5), At(5, -5));
                    break;
                case SkinPattern.Coral:
                    g.DrawEllipse(_skinPatternPen, p.X - 2.5f, p.Y - 2.5f, 5, 5);
                    g.DrawLine(_skinAccentPen, At(-2, -2), At(-7, -7));
                    g.DrawLine(_skinAccentPen, At(2, 2), At(7, 7));
                    break;
                case SkinPattern.Prism:
                    g.DrawPolygon(_skinPatternPen, new[]
                    {
                        At(-6, 0), At(0, -6), At(6, 0), At(0, 6)
                    });
                    g.DrawLine(_skinAccentPen, At(-6, 0), At(0, 6));
                    break;
                case SkinPattern.Stars:
                    g.DrawLine(_skinPatternPen, At(-6, 0), At(6, 0));
                    g.DrawLine(_skinPatternPen, At(0, -6), At(0, 6));
                    g.DrawLine(_skinAccentPen, At(-4, -4), At(4, 4));
                    g.DrawLine(_skinAccentPen, At(-4, 4), At(4, -4));
                    break;
                case SkinPattern.Honeycomb:
                    g.DrawPolygon(_skinPatternPen, new[]
                    {
                        At(-5, -3), At(0, -6), At(5, -3),
                        At(5, 3), At(0, 6), At(-5, 3)
                    });
                    break;
                case SkinPattern.Tiger:
                    g.DrawLine(_skinPatternPen, At(-2, -7), At(0, 1));
                    g.DrawLine(_skinPatternPen, At(4, -6), At(6, 3));
                    break;
                case SkinPattern.Zebra:
                    g.DrawLine(_skinPatternPen, At(-5, -7), At(-3, 7));
                    g.DrawLine(_skinAccentPen, At(0, -7), At(2, 7));
                    break;
                case SkinPattern.Neon:
                    g.DrawLine(_skinPatternPen, At(-7, -4), At(7, -4));
                    g.DrawLine(_skinAccentPen, At(-7, 4), At(7, 4));
                    g.FillEllipse(_skinAccentPen.Brush, At(0, 0).X - 2,
                        At(0, 0).Y - 2, 4, 4);
                    break;
                case SkinPattern.Royal:
                    g.DrawPolygon(_skinPatternPen, new[]
                    {
                        At(-7, 4), At(-6, -3), At(-2, 1),
                        At(0, -6), At(3, 1), At(6, -3), At(7, 4)
                    });
                    g.DrawLine(_skinAccentPen, At(-7, 5), At(7, 5));
                    break;
                case SkinPattern.Legendary:
                    g.DrawLine(_skinPatternPen, At(-6, 0), At(6, 0));
                    g.DrawLine(_skinPatternPen, At(0, -6), At(0, 6));
                    g.DrawLine(_skinAccentPen, At(-4, -4), At(4, 4));
                    g.DrawLine(_skinAccentPen, At(-4, 4), At(4, -4));
                    break;
            }

            if ((skin.Pattern is SkinPattern.Neon or SkinPattern.Galaxy or SkinPattern.Legendary) &&
                i % (stride * 2) == 1)
            {
                float sparkle = 2.5f + (MathF.Sin(phase + i) + 1f) * 1.1f;
                g.DrawLine(_skinAccentPen, p.X - sparkle, p.Y,
                    p.X + sparkle, p.Y);
                g.DrawLine(_skinAccentPen, p.X, p.Y - sparkle,
                    p.X, p.Y + sparkle);
            }
        }
    }

    private void DrawLegendaryAura(Graphics g, PointF head, PointF[] centers, int count)
    {
        float time = Environment.TickCount64 / 260f;
        float pulse = 1f + MathF.Sin(time) * 0.12f;

        for (int ring = 0; ring < 3; ring++)
        {
            float radius = (18 + ring * 7) * pulse;
            g.DrawEllipse(_legendaryPens[ring], head.X - radius, head.Y - radius,
                radius * 2, radius * 2);
        }

        for (int i = 0; i < 8; i++)
        {
            float angle = time + i * MathF.PI / 4;
            float radius = 31 + MathF.Sin(time * 1.8f + i) * 5;
            float x = head.X + MathF.Cos(angle) * radius;
            float y = head.Y + MathF.Sin(angle) * radius;
            float arm = i % 2 == 0 ? 5f : 3f;

            g.DrawLine(_legendaryPens[i % 3], x - arm, y, x + arm, y);
            g.DrawLine(_legendaryPens[i % 3], x, y - arm, x, y + arm);
        }

        for (int i = 3; i < count; i += 3)
        {
            PointF p = centers[i];
            float twinkle = 2.5f + (MathF.Sin(time * 2 + i) + 1f) * 1.5f;
            g.DrawLine(_legendaryPens[2], p.X - twinkle, p.Y, p.X + twinkle, p.Y);
            g.DrawLine(_legendaryPens[2], p.X, p.Y - twinkle, p.X, p.Y + twinkle);
        }
    }

    private void DrawWindowButton(Graphics g, Rectangle rect, string label, bool hover)
    {
        FillRound(g, rect, 8, hover ? PanelLight : Panel);
        DrawLabel(g, label, _iconFont, _whiteBrush, rect, _centerFormat);
    }

    private void DrawChoice(Graphics g, Rectangle rect, string label, bool selected)
    {
        bool hover = rect.Contains(_mousePoint);
        Color fill = selected ? Color.FromArgb(42, 75, 49) :
            hover ? Color.FromArgb(34, 47, 51) : Panel;
        Color foreground = selected ? Green : hover ? White : Muted;

        FillRound(g, rect, 8, fill);
        StrokeRound(g, rect, 8, selected ? Color.FromArgb(88, 148, 78) : Border);
        DrawLabel(g, label, _buttonFont, selected ? _greenBrush : foreground == White ? _whiteBrush : _mutedBrush,
            rect, _centerFormat);
    }

    private void DrawActionButton(Graphics g, Rectangle rect, string label, bool primary)
    {
        bool hover = rect.Contains(_mousePoint);
        Color fill = primary
            ? hover ? Color.FromArgb(190, 255, 134) : Green
            : hover ? Color.FromArgb(37, 53, 56) : Panel;
        FillRound(g, rect, 10, fill);
        if (!primary)
            StrokeRound(g, rect, 10, Border);

        DrawLabel(g, label, primary ? _buttonFont : _iconFont,
            primary ? _darkBrush : _whiteBrush, rect, _centerFormat);
    }

    private void DrawStaticPanel(Graphics g, Rectangle rect, Color? fill = null)
    {
        FillRound(g, rect, 16, fill ?? Panel);
        StrokeRound(g, rect, 16, Border);
    }

    private void DrawStaticButton(Graphics g, Rectangle rect, bool primary = false)
    {
        FillRound(g, rect, 9, primary ? Green : Panel);
        StrokeRound(g, rect, 9, primary ? Color.FromArgb(180, Green) : Border);
    }

    private static void StaticText(Graphics g, string value, Rectangle rect, float size,
        FontStyle style, Color color, StringAlignment alignment = StringAlignment.Near)
    {
        using var font = new Font("Segoe UI", size, style);
        using var brush = new SolidBrush(color);
        using var format = new StringFormat
        {
            Alignment = alignment,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap
        };
        g.DrawString(value, font, brush, rect, format);
    }

    private static void DrawLabel(Graphics g, string value, Font font, Brush brush,
        Rectangle rect, StringFormat format) =>
        g.DrawString(value, font, brush, rect, format);

    private static RectangleF CellBounds(Point point, float diameter)
    {
        float offset = (CellSize - diameter) / 2f;
        return new RectangleF(
            62 + point.X * CellSize + offset,
            150 + point.Y * CellSize + offset,
            diameter, diameter);
    }

    private static void FillRound(Graphics g, Rectangle rect, int radius, Color color)
    {
        using var brush = new SolidBrush(color);
        FillRound(g, rect, radius, brush);
    }

    private static void FillRound(Graphics g, Rectangle rect, int radius, Brush brush)
    {
        using var path = RoundPath(rect, radius);
        g.FillPath(brush, path);
    }

    private static void StrokeRound(Graphics g, Rectangle rect, int radius, Color color)
    {
        using var pen = new Pen(color, 1);
        using var path = RoundPath(rect, radius);
        g.DrawPath(pen, path);
    }

    private static void DrawRoundOutline(Graphics g, Rectangle rect, int radius, Pen pen)
    {
        using var path = RoundPath(rect, radius);
        g.DrawPath(pen, path);
    }

    private static GraphicsPath RoundPath(Rectangle rect, int radius)
    {
        radius = Math.Max(1, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
        int diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void ResetSnake()
    {
        _snake.Clear();
        int x = Columns / 2;
        int y = Rows / 2;

        _snake.Add(new Point(x, y));
        _snake.Add(new Point(x - 1, y));
        _snake.Add(new Point(x - 2, y));
        _snake.Add(new Point(x - 3, y));

        _previousSnakeCount = _snake.Count;
        for (int i = 0; i < _snake.Count; i++)
            _previousSnake[i] = _snake[i];
        _direction = Direction.Right;
        _nextDirection = Direction.Right;
        _foodEaten = 0;
        _goldenFood = null;
        _goldenExpiresAt = null;
        PlaceFood();
    }

    private void StartGame()
    {
        _view = AppView.Play;
        _score = 0;
        _won = false;
        _newRecord = false;
        _rushTimeLeft = TimeSpan.FromSeconds(60);
        _lastDisplayedSecond = 60;
        ResetSnake();

        _state = GameState.Playing;
        _lastStepAt = DateTime.UtcNow;
        _lastFrameAt = _lastStepAt;
        _frozenProgress = 0;
        _animationTimer.Start();
        Focus();
        Invalidate();
    }

    private void SetState(GameState state)
    {
        DateTime now = DateTime.UtcNow;

        if (state == GameState.Paused && _state == GameState.Playing)
            _frozenProgress = CurrentProgress();

        bool resuming = state == GameState.Playing && _state == GameState.Paused;
        _state = state;

        if (state == GameState.Playing)
        {
            _lastFrameAt = now;
            _lastStepAt = resuming
                ? now - TimeSpan.FromMilliseconds(CurrentStepMilliseconds() * _frozenProgress)
                : now;
            _animationTimer.Start();
            Focus();
        }
        else
        {
            _animationTimer.Stop();
        }

        Invalidate();
    }

    private void TogglePause()
    {
        if (_state == GameState.Playing)
            SetState(GameState.Paused);
        else if (_state == GameState.Paused)
            SetState(GameState.Playing);
    }

    private int CurrentStepMilliseconds()
    {
        int start = _difficulty switch
        {
            Difficulty.Easy => 150,
            Difficulty.Hard => 82,
            _ => 112
        };

        return start;
    }

    private float CurrentProgress()
    {
        double elapsed = (DateTime.UtcNow - _lastStepAt).TotalMilliseconds;
        return Math.Clamp((float)(elapsed / CurrentStepMilliseconds()), 0f, 1f);
    }

    private void UpdateGameFrame()
    {
        if (_state != GameState.Playing)
            return;

        DateTime now = DateTime.UtcNow;
        TimeSpan frameTime = now - _lastFrameAt;
        _lastFrameAt = now;

        if (_mode == GameMode.Rush)
        {
            _rushTimeLeft -= frameTime;
            if (_rushTimeLeft <= TimeSpan.Zero)
            {
                _rushTimeLeft = TimeSpan.Zero;
                EndGame();
                return;
            }

            int second = (int)Math.Ceiling(_rushTimeLeft.TotalSeconds);
            if (second != _lastDisplayedSecond)
            {
                _lastDisplayedSecond = second;
                InvalidateDesign(_modeTimerRect);
            }
        }

        UpdateGoldBurst((float)frameTime.TotalSeconds, now);

        if (_goldenFood.HasValue && _goldenExpiresAt.HasValue && now >= _goldenExpiresAt.Value)
        {
            _goldenFood = null;
            _goldenExpiresAt = null;
        }

        if (now - _lastStepAt >= TimeSpan.FromMilliseconds(CurrentStepMilliseconds()))
        {
            if (!MoveSnake(now))
                return;

            _lastStepAt = now;
        }

        // Only the playfield is repainted during animation; the dashboard is cached.
        InvalidateDesign(_field);
    }

    private bool MoveSnake(DateTime now)
    {
        _previousSnakeCount = _snake.Count;
        for (int i = 0; i < _snake.Count; i++)
            _previousSnake[i] = _snake[i];
        _direction = _nextDirection;

        Point head = _snake[0];
        Point next = _direction switch
        {
            Direction.Up => new Point(head.X, head.Y - 1),
            Direction.Down => new Point(head.X, head.Y + 1),
            Direction.Left => new Point(head.X - 1, head.Y),
            _ => new Point(head.X + 1, head.Y)
        };

        if (_mode == GameMode.Wrap)
        {
            next.X = (next.X + Columns) % Columns;
            next.Y = (next.Y + Rows) % Rows;
        }
        else if (next.X < 0 || next.X >= Columns || next.Y < 0 || next.Y >= Rows)
        {
            EndGame();
            return false;
        }

        bool eatingRegular = next == _food;
        bool eatingGolden = _goldenFood.HasValue && next == _goldenFood.Value;
        bool eating = eatingRegular || eatingGolden;
        int bodyCount = eating ? _snake.Count : _snake.Count - 1;

        for (int i = 0; i < bodyCount; i++)
        {
            if (_snake[i] == next)
            {
                EndGame();
                return false;
            }
        }

        _snake.Insert(0, next);

        if (eatingRegular || eatingGolden)
        {
            _score += eatingGolden ? 30 : 10;

            if (_score > _bestScore)
            {
                _bestScore = _score;
                _newRecord = true;
                SaveBestScore();
            }

            PlaySound(eatingGolden ? 2 : 0);

            if (eatingGolden)
            {
                SpawnGoldBurst(_goldenFood!.Value, now);
                _goldenFood = null;
                _goldenExpiresAt = null;
            }
            else
            {
                _foodEaten++;
                PlaceFood();

                if (_foodEaten % 5 == 0)
                    PlaceGoldenFood(now);
            }

            if (_snake.Count >= BoardCells)
            {
                _won = true;
                EndGame();
                return false;
            }

            InvalidateDesign(_scoreArea);
        }
        else
        {
            _snake.RemoveAt(_snake.Count - 1);
        }

        return true;
    }

    private void PlaceFood()
    {
        _food = FindFreeCell();
    }

    private void PlaceGoldenFood(DateTime now)
    {
        _goldenFood = FindFreeCell();
        _goldenExpiresAt = now.AddSeconds(7);
    }

    private Point FindFreeCell()
    {
        for (int i = 0; i < 1000; i++)
        {
            Point candidate = new(_random.Next(Columns), _random.Next(Rows));
            if (!_snake.Contains(candidate) && candidate != _food && candidate != _goldenFood)
                return candidate;
        }

        for (int y = 0; y < Rows; y++)
        for (int x = 0; x < Columns; x++)
        {
            Point candidate = new(x, y);
            if (!_snake.Contains(candidate) && candidate != _food && candidate != _goldenFood)
                return candidate;
        }

        return Point.Empty;
    }

    private void EndGame()
    {
        _animationTimer.Stop();
        _state = GameState.GameOver;
        if (_score > _bestScore)
        {
            _bestScore = _score;
            _newRecord = true;
        }

        if (_newRecord || _scoreSaveFailed)
            SaveBestScore();

        PlaySound(_won ? 2 : 1);
        Invalidate();
    }

    private void ChangeDirection(Direction direction)
    {
        if (_state == GameState.Ready)
            return;

        if (_state == GameState.GameOver)
            StartGame();

        if (_state != GameState.Playing)
            return;

        bool opposite =
            (_nextDirection == Direction.Up && direction == Direction.Down) ||
            (_nextDirection == Direction.Down && direction == Direction.Up) ||
            (_nextDirection == Direction.Left && direction == Direction.Right) ||
            (_nextDirection == Direction.Right && direction == Direction.Left);

        if (!opposite)
            _nextDirection = direction;
    }

    private void CycleDifficulty(int direction)
    {
        if (_state is GameState.Playing or GameState.Paused)
            return;

        int next = ((int)_difficulty + direction + 3) % 3;
        _difficulty = (Difficulty)next;
        Invalidate();
    }


    private void MoveSkinSelection(int amount)
    {
        int next = Math.Clamp(_selectedSkin + amount, 0, Skins.Length - 1);
        ApplySkin(next);
        SaveSelectedSkin();
        Invalidate();
    }

    private void SetDifficulty(Difficulty difficulty)
    {
        if (_state is GameState.Playing or GameState.Paused)
            return;

        _difficulty = difficulty;
        InvalidateDesign(_rightPanel);
    }

    private void SetMode(GameMode mode)
    {
        if (_state is GameState.Playing or GameState.Paused)
            return;

        _mode = mode;
        _rushTimeLeft = TimeSpan.FromSeconds(60);
        _lastDisplayedSecond = 60;
        InvalidateDesign(_rightPanel);
    }

    private void PlaySound(int type)
    {
        if (!_soundEnabled)
            return;

        try
        {
            if (type == 0)
                SystemSounds.Asterisk.Play();
            else if (type == 1)
                SystemSounds.Hand.Play();
            else
                SystemSounds.Exclamation.Play();
        }
        catch
        {
            // Sound is optional; gameplay never depends on it.
        }
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (_view == AppView.Home)
        {
            if (e.Alt && e.KeyCode == Keys.Enter)
            {
                ToggleFullscreen();
                e.Handled = true;
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    StartGame();
                    e.Handled = true;
                    break;
                case Keys.S:
                    NavigateTo(AppView.Skins);
                    e.Handled = true;
                    break;
                case Keys.T:
                    NavigateTo(AppView.Settings);
                    e.Handled = true;
                    break;
                case Keys.Left:
                case Keys.A:
                    CycleDifficulty(-1);
                    e.Handled = true;
                    break;
                case Keys.Right:
                case Keys.D:
                    CycleDifficulty(1);
                    e.Handled = true;
                    break;
                case Keys.F11:
                    ToggleFullscreen();
                    e.Handled = true;
                    break;
                case Keys.Escape:
                    Close();
                    e.Handled = true;
                    break;
            }
            return;
        }

        if (_view == AppView.Skins)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.Escape:
                    NavigateTo(AppView.Home);
                    e.Handled = true;
                    break;
                case Keys.Left:
                    MoveSkinSelection(-1);
                    e.Handled = true;
                    break;
                case Keys.Right:
                    MoveSkinSelection(1);
                    e.Handled = true;
                    break;
                case Keys.Up:
                    MoveSkinSelection(-3);
                    e.Handled = true;
                    break;
                case Keys.Down:
                    MoveSkinSelection(3);
                    e.Handled = true;
                    break;
                case Keys.F11:
                    ToggleFullscreen();
                    e.Handled = true;
                    break;
            }
            return;
        }

        if (_view != AppView.Play)
        {
            if (e.KeyCode == Keys.F11 || (e.KeyCode == Keys.Enter && e.Alt))
                ToggleFullscreen();
            else if (e.KeyCode == Keys.Escape)
                NavigateTo(AppView.Home);
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Up:
            case Keys.W:
                ChangeDirection(Direction.Up);
                e.Handled = true;
                break;
            case Keys.Down:
            case Keys.S:
                ChangeDirection(Direction.Down);
                e.Handled = true;
                break;
            case Keys.Left:
            case Keys.A:
                ChangeDirection(Direction.Left);
                e.Handled = true;
                break;
            case Keys.Right:
            case Keys.D:
                ChangeDirection(Direction.Right);
                e.Handled = true;
                break;
            case Keys.Space:
                if (_state is GameState.Ready or GameState.GameOver)
                    StartGame();
                else
                    TogglePause();
                e.Handled = true;
                break;
            case Keys.F11:
                ToggleFullscreen();
                e.Handled = true;
                break;
            case Keys.Enter when e.Alt:
                ToggleFullscreen();
                e.Handled = true;
                break;
            case Keys.Escape:
                if (_isFullscreen)
                    ToggleFullscreen();
                else
                    TogglePause();
                e.Handled = true;
                break;
        }
    }

    private void HandleMouseMove(object? sender, MouseEventArgs e)
    {
        Point point = ClientToDesign(e.Location);
        _mousePoint = point;

        if (_dragging)
        {
            Location = new Point(Cursor.Position.X - _dragOffset.X,
                Cursor.Position.Y - _dragOffset.Y);
            return;
        }

        UpdateHover(point);
        Cursor = _hoverId == 0 ? Cursors.Default : Cursors.Hand;
    }

    private void UpdateHover(Point point)
    {
        int nextHover = GetHoverId(point);
        if (nextHover == _hoverId)
            return;

        Rectangle oldRect = HoverRectangle(_hoverId);
        Rectangle newRect = HoverRectangle(nextHover);
        _hoverId = nextHover;
        _mousePoint = point;

        if (!oldRect.IsEmpty)
            InvalidateDesign(oldRect);
        if (!newRect.IsEmpty)
            InvalidateDesign(newRect);
    }

    private int GetHoverId(Point point)
    {
        if (_closeButton.Contains(point)) return 2;
        if (_minimizeButton.Contains(point)) return 1;
        if (_fullscreenButton.Contains(point)) return 13;

        if (_view is not (AppView.Home or AppView.Skins))
        {
            if (_homeNavButton.Contains(point)) return 17;
            if (_playNavButton.Contains(point)) return 18;
            if (_skinsNavButton.Contains(point)) return 19;
            if (_settingsNavButton.Contains(point)) return 20;
        }

        if (_view == AppView.Home)
        {
            if (_retroStartButton.Contains(point)) return 21;
            if (_retroSkinsButton.Contains(point)) return 22;
            if (_retroSettingsButton.Contains(point)) return 23;
            if (_retroDifficultyLeft.Contains(point)) return 26;
            if (_retroDifficultyRight.Contains(point)) return 27;
            if (_retroDifficultyText.Contains(point)) return 28;
            return 0;
        }

        if (_view == AppView.Skins)
        {
            if (_skinPickerClose.Contains(point)) return 16;
            for (int i = 0; i < Skins.Length; i++)
                if (SkinTileRect(i).Contains(point)) return 100 + i;
            return 0;
        }

        if (_view == AppView.Settings)
        {
            for (int i = 0; i < Themes.Length; i++)
                if (ThemeTileRect(i).Contains(point)) return 200 + i;
            if (_settingsSoundButton.Contains(point)) return 24;
            if (_settingsResetButton.Contains(point)) return 25;
            return 0;
        }

        if (_skinButton.Contains(point)) return 14;
        if (_resetBestButton.Contains(point)) return 15;
        if (_mainButton.Contains(point)) return 3;
        if (_soundButton.Contains(point)) return 4;
        if (_restartButton.Contains(point)) return 5;
        if (_overlayButton.Contains(point) &&
            (_state is GameState.Ready or GameState.Paused or GameState.GameOver)) return 6;
        if (_classicButton.Contains(point)) return 7;
        if (_wrapButton.Contains(point)) return 8;
        if (_rushButton.Contains(point)) return 9;
        if (_easyButton.Contains(point)) return 10;
        if (_normalButton.Contains(point)) return 11;
        if (_hardButton.Contains(point)) return 12;
        return 0;
    }

    private Rectangle HoverRectangle(int id)
    {
        if (id >= 100 && id < 100 + Skins.Length)
            return SkinTileRect(id - 100);
        if (id >= 200 && id < 200 + Themes.Length)
            return ThemeTileRect(id - 200);

        return id switch
        {
            1 => _minimizeButton,
            2 => _closeButton,
            3 => _mainButton,
            4 => _soundButton,
            5 => _restartButton,
            6 => _overlayButton,
            7 => _classicButton,
            8 => _wrapButton,
            9 => _rushButton,
            10 => _easyButton,
            11 => _normalButton,
            12 => _hardButton,
            13 => _fullscreenButton,
            14 => _skinButton,
            15 => _resetBestButton,
            16 => _skinPickerClose,
            17 => _homeNavButton,
            18 => _playNavButton,
            19 => _skinsNavButton,
            20 => _settingsNavButton,
            21 => _retroStartButton,
            22 => _retroSkinsButton,
            23 => _retroSettingsButton,
            24 => _settingsSoundButton,
            25 => _settingsResetButton,
            26 => _retroDifficultyLeft,
            27 => _retroDifficultyRight,
            28 => _retroDifficultyText,
            _ => Rectangle.Empty
        };
    }

    private void HandleMouseDown(object? sender, MouseEventArgs e)
    {
        Focus();

        Point point = ClientToDesign(e.Location);
        if (e.Button == MouseButtons.Left && point.Y < 70 && !_isFullscreen &&
            !_closeButton.Contains(point) && !_minimizeButton.Contains(point) &&
            !_fullscreenButton.Contains(point) &&
            (_view is AppView.Home or AppView.Skins ||
             (!_homeNavButton.Contains(point) && !_playNavButton.Contains(point) &&
              !_skinsNavButton.Contains(point) && !_settingsNavButton.Contains(point))))
        {
            _dragging = true;
            _dragOffset = e.Location;
        }
    }

    private void HandleMouseClick(object? sender, MouseEventArgs e)
    {
        Point point = ClientToDesign(e.Location);

        if (_closeButton.Contains(point))
        {
            Close();
            return;
        }
        if (_minimizeButton.Contains(point))
        {
            WindowState = FormWindowState.Minimized;
            return;
        }
        if (_fullscreenButton.Contains(point))
        {
            ToggleFullscreen();
            return;
        }

        if (_view is not (AppView.Home or AppView.Skins))
        {
            if (_homeNavButton.Contains(point)) { NavigateTo(AppView.Home); return; }
            if (_playNavButton.Contains(point)) { NavigateTo(AppView.Play); return; }
            if (_skinsNavButton.Contains(point)) { NavigateTo(AppView.Skins); return; }
            if (_settingsNavButton.Contains(point)) { NavigateTo(AppView.Settings); return; }
        }

        if (_view == AppView.Home)
        {
            if (_retroStartButton.Contains(point))
                StartGame();
            else if (_retroSkinsButton.Contains(point))
                NavigateTo(AppView.Skins);
            else if (_retroSettingsButton.Contains(point))
                NavigateTo(AppView.Settings);
            else if (_retroDifficultyLeft.Contains(point))
                CycleDifficulty(-1);
            else if (_retroDifficultyRight.Contains(point))
                CycleDifficulty(1);
            else if (_retroDifficultyText.Contains(point))
                CycleDifficulty(1);
            return;
        }

        if (_view == AppView.Skins)
        {
            if (_skinPickerClose.Contains(point))
            {
                NavigateTo(AppView.Home);
                return;
            }

            for (int i = 0; i < Skins.Length; i++)
            {
                if (!SkinTileRect(i).Contains(point))
                    continue;

                ApplySkin(i);
                SaveSelectedSkin();
                Invalidate();
                return;
            }
            return;
        }

        if (_view == AppView.Settings)
        {
            for (int i = 0; i < Themes.Length; i++)
            {
                if (!ThemeTileRect(i).Contains(point))
                    continue;

                SelectTheme(i);
                return;
            }

            if (_settingsSoundButton.Contains(point))
            {
                _soundEnabled = !_soundEnabled;
                Invalidate();
            }
            else if (_settingsResetButton.Contains(point))
                ResetBestScore();
            return;
        }

        if (_skinButton.Contains(point))
        {
            NavigateTo(AppView.Skins);
            return;
        }
        if (_resetBestButton.Contains(point))
        {
            ResetBestScore();
            return;
        }

        if (_classicButton.Contains(point)) SetMode(GameMode.Classic);
        else if (_wrapButton.Contains(point)) SetMode(GameMode.Wrap);
        else if (_rushButton.Contains(point)) SetMode(GameMode.Rush);

        if (_easyButton.Contains(point)) SetDifficulty(Difficulty.Easy);
        else if (_normalButton.Contains(point)) SetDifficulty(Difficulty.Normal);
        else if (_hardButton.Contains(point)) SetDifficulty(Difficulty.Hard);

        if (_soundButton.Contains(point))
        {
            _soundEnabled = !_soundEnabled;
            InvalidateDesign(_soundButton);
        }

        if (_restartButton.Contains(point))
        {
            StartGame();
            return;
        }

        if (_mainButton.Contains(point) || _overlayButton.Contains(point))
        {
            if (_state == GameState.Playing)
                TogglePause();
            else if (_state == GameState.Paused)
                SetState(GameState.Playing);
            else
                StartGame();
        }
    }

    private int LoadBestScore()
    {
        if (TryReadScore(BestScorePath(), out int score))
            return score;

        // Recover the last known good score if a previous save was interrupted.
        return TryReadScore(BestScorePath() + ".bak", out score) ? score : 0;
    }

    private static bool TryReadScore(string path, out int score)
    {
        score = 0;

        try
        {
            return File.Exists(path) &&
                   int.TryParse(File.ReadAllText(path).Trim(),
                       NumberStyles.Integer, CultureInfo.InvariantCulture, out score) &&
                   score >= 0;
        }
        catch
        {
            return false;
        }
    }

    private void SaveBestScore()
    {
        string path = BestScorePath();
        string directory = Path.GetDirectoryName(path)!;
        string temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        string backupPath = path + ".bak";

        try
        {
            Directory.CreateDirectory(directory);

            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew,
                       FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(_bestScore.ToString(CultureInfo.InvariantCulture));
                writer.Flush();
                stream.Flush(true);
            }

            if (File.Exists(path))
            {
                if (File.Exists(backupPath))
                    File.Delete(backupPath);

                File.Replace(temporaryPath, path, backupPath, true);
            }
            else
            {
                File.Move(temporaryPath, path);
            }

            _scoreSaveFailed = false;
        }
        catch
        {
            _scoreSaveFailed = true;

            try
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
            catch
            {
                // A leftover temporary file is safe; the previous score is untouched.
            }
        }
    }

    private static string BestScorePath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SnakeGame",
            "best-score.txt");

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _animationTimer.Stop();
        _animationTimer.Dispose();
        _ambientTimer.Stop();
        _ambientTimer.Dispose();
        _chrome?.Dispose();
        _overlayPath.Dispose();

        _smallFont.Dispose();
        _labelFont.Dispose();
        _scoreFont.Dispose();
        _numberFont.Dispose();
        _heroFont.Dispose();
        _welcomeFont.Dispose();
        _welcomeSmallFont.Dispose();
        _effectFont.Dispose();
        _buttonFont.Dispose();
        _iconFont.Dispose();

        _whiteBrush.Dispose();
        _darkBrush.Dispose();
        _mutedBrush.Dispose();
        _greenBrush.Dispose();
        _goldBrush.Dispose();
        _snakeBrush.Dispose();
        _headBrush.Dispose();
        _headShadowBrush.Dispose();
        _eyeWhiteBrush.Dispose();
        _eyeBrush.Dispose();
        _foodBrush.Dispose();
        _foodGlowBrush.Dispose();
        _goldGlowBrush.Dispose();
        _overlayBrush.Dispose();
        _cardBrush.Dispose();
        _goldPen.Dispose();
        _skinPatternPen.Dispose();
        _skinAccentPen.Dispose();
        _skinGlowPen.Dispose();
        foreach (Pen pen in _legendaryPens)
            pen.Dispose();
        foreach (Pen pen in _goldBurstPens)
            pen.Dispose();
        foreach (SolidBrush brush in _goldParticleBrushes)
            brush.Dispose();
        _appleSprite?.Dispose();
        _goldAppleSprite?.Dispose();
        _snakePath.Dispose();
        _snakeHighlightPath.Dispose();
        _viewMatrix.Dispose();
        _headHighlightBrush.Dispose();
        _snakeOutlinePen.Dispose();
        _snakeBodyPen.Dispose();
        _snakeHighlightPen.Dispose();
        _headOutlinePen.Dispose();

        if (_scoreSaveFailed)
            SaveBestScore();

        _leftFormat.Dispose();
        _centerFormat.Dispose();
        _rightFormat.Dispose();

        base.OnFormClosed(e);
    }
}
