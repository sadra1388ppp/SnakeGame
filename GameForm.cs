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
            Color.FromArgb(174, 91, 39), Color.FromArgb(91, 42, 19),
            Color.FromArgb(255, 207, 83), Color.FromArgb(255, 246, 218),
            Color.FromArgb(255, 170, 91), Color.FromArgb(194, 111, 53),
            Color.FromArgb(255, 220, 116)),
        new("SUMMER", "NIGHT", ThemeSeason.Summer, true,
            Color.FromArgb(38, 38, 67), Color.FromArgb(14, 19, 39),
            Color.FromArgb(255, 178, 92), Color.FromArgb(255, 239, 218),
            Color.FromArgb(255, 143, 113), Color.FromArgb(73, 61, 94),
            Color.FromArgb(255, 210, 132)),
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
    private static readonly Rectangle ThemeGridViewport = new(40, 150, 1080, 540);
    private int _selectedTheme;
    private int _themeMenuLevel;
    private int _themeScrollOffset;
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
    private readonly Rectangle _skinPickerClose = new(980, 95, 128, 36);
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
    private readonly Rectangle _themeCategoryTile = new(790, 420, 260, 60);
    private readonly Rectangle _seasonCategoryTile = new(70, 210, 500, 330);
    private readonly Rectangle _seasonBackButton = new(52, 82, 150, 42);
    private readonly Rectangle _settingsSoundButton = new(790, 166, 260, 52);
    private readonly Rectangle _settingsResetButton = new(790, 298, 260, 52);
    private readonly Rectangle _minimizeButton = new(986, 18, 40, 36);
    private readonly Rectangle _fullscreenButton = new(1032, 18, 40, 36);
    private readonly Rectangle _closeButton = new(1078, 18, 40, 36);

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
    private readonly Font _displayFont = new("Segoe UI", 48, FontStyle.Bold);
    private readonly Font _pageTitleFont = new("Segoe UI", 25, FontStyle.Bold);
    private readonly Font _sectionFont = new("Segoe UI", 10.5f, FontStyle.Bold);
    private readonly Font _bodyFont = new("Segoe UI", 9, FontStyle.Regular);
    private readonly Font _microFont = new("Segoe UI", 7.5f, FontStyle.Bold);

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
        BackColor = RetroBackground;
        _selectedSkin = LoadSelectedSkin();
        ApplySkin(_selectedSkin);
        ResetSnake();
        _appleSprite = CreateAppleSprite(false);
        _goldAppleSprite = CreateAppleSprite(true);
        BuildChrome();

        _animationTimer.Tick += (_, _) => UpdateGameFrame();
        _ambientTimer.Tick += (_, _) =>
        {
            if (_view == AppView.Play && _state == GameState.Playing)
                InvalidateDesign(_field);
            else
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

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (_view == AppView.Settings && _themeMenuLevel == 2)
        {
            int rows = (Themes.Length + 3) / 4;
            int maxScroll = Math.Max(0,
                158 + (rows - 1) * 264 + 232 - ThemeGridViewport.Bottom);
            int nextOffset = Math.Clamp(
                _themeScrollOffset - Math.Sign(e.Delta) * 60, 0, maxScroll);

            if (nextOffset != _themeScrollOffset)
            {
                _themeScrollOffset = nextOffset;
                UpdateHover(ClientToDesign(e.Location));
                Invalidate();
            }
        }

        base.OnMouseWheel(e);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (_chrome is null)
            base.OnPaintBackground(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_chrome is null)
            return;

        Graphics g = e.Graphics;
        g.Clear(RetroBackground);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;

        g.Transform = _viewMatrix;
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.None;
        g.Clear(RetroBackground);
        // Full seasonal scenery belongs to the menus. During gameplay, keep only
        // the clipped seasonal effects inside the board so trees and decorations
        // never cover the snake, food, or game controls.
        if (_view != AppView.Play)
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
                break;
            case AppView.Play:
                DrawRetroPlayChrome(g);
                DrawBoardDynamic(g);
                DrawRetroPlayDashboard(g);
                break;
        }

        DrawUiHeader(g);
        DrawUiWindowControls(g);
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

        if (view == AppView.Settings && _view != AppView.Settings)
            _themeMenuLevel = 0;

        _view = view;
        Invalidate();
    }

    private void DrawPageBackground(Graphics g)
    {
        using var background = new SolidBrush(Color.FromArgb(10, 17, 21));
        g.FillRectangle(background, 0, 0, DesignWidth, DesignHeight);

        using var grid = new Pen(Color.FromArgb(19, ActiveTheme.Grid), 1);
        for (int x = 16; x < DesignWidth; x += 32)
            g.DrawLine(grid, x, 72, x, 724);
        for (int y = 88; y < 724; y += 32)
            g.DrawLine(grid, 16, y, DesignWidth - 16, y);

        using var edge = new Pen(Color.FromArgb(100, ActiveTheme.Accent), 2);
        g.DrawLine(edge, 0, 70, DesignWidth, 70);
        g.DrawLine(edge, 32, 724, 1128, 724);
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
        using var fill = new SolidBrush(active
            ? Color.FromArgb(230, ActiveTheme.Grid)
            : hover ? Color.FromArgb(170, ActiveTheme.Dark) : Color.Transparent);
        using var edge = new Pen(active || hover ? ActiveTheme.Highlight : ActiveTheme.Grid, active ? 2 : 1);
        g.FillRectangle(fill, rect);
        g.DrawRectangle(edge, rect);
        if (active)
        {
            using var marker = new SolidBrush(ActiveTheme.Accent);
            g.FillRectangle(marker, rect.X + 5, rect.Y + 5, 4, rect.Height - 10);
        }
        using var brush = new SolidBrush(active || hover ? ActiveTheme.Highlight : ActiveTheme.Text);
        DrawLabel(g, label, _buttonFont, brush, rect, _centerFormat);
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



    private void DrawUiPanel(Graphics g, Rectangle rect, bool highlighted = false,
        int radius = 18)
    {
        using var fill = new SolidBrush(Color.FromArgb(235, ActiveTheme.Dark));
        using var edge = new Pen(highlighted ? ActiveTheme.Highlight : ActiveTheme.Grid, highlighted ? 2 : 1);
        using var inner = new Pen(Color.FromArgb(55, ActiveTheme.Text), 1);
        g.FillRectangle(fill, rect);
        g.DrawRectangle(edge, rect);
        if (rect.Width > 18 && rect.Height > 18)
            g.DrawRectangle(inner, rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 10);
        using var corner = new SolidBrush(ActiveTheme.Accent);
        g.FillRectangle(corner, rect.X - 2, rect.Y + 12, 4, 18);
        g.FillRectangle(corner, rect.Right - 2, rect.Bottom - 30, 4, 18);
    }

    private void DrawUiButton(Graphics g, Rectangle rect, string label,
        bool primary = false, bool selected = false, bool warning = false)
    {
        bool hover = rect.Contains(_mousePoint);
        Color fill = primary ? ActiveTheme.Accent :
            selected ? ActiveTheme.Grid :
            hover ? ActiveTheme.Background : ActiveTheme.Dark;
        using var brush = new SolidBrush(fill);
        using var edge = new Pen(primary || selected || hover ? ActiveTheme.Highlight : ActiveTheme.Grid, primary ? 3 : 2);
        g.FillRectangle(brush, rect);
        g.DrawRectangle(edge, rect);
        using var textBrush = new SolidBrush(primary ? ActiveTheme.Dark :
            warning && !hover ? Color.FromArgb(215, 185, 145, 140) : ActiveTheme.Text);
        DrawLabel(g, label, _buttonFont, textBrush, rect, _centerFormat);
        g.FillRectangle(new SolidBrush(primary ? ActiveTheme.Dark : ActiveTheme.Accent),
            rect.X + 5, rect.Y + 5, 4, rect.Height - 10);
    }

    private void DrawUiNavButton(Graphics g, Rectangle rect, string label, bool active)
    {
        bool hover = rect.Contains(_mousePoint);
        if (active || hover)
        {
            using var fill = new SolidBrush(Color.FromArgb(active ? 220 : 100, ActiveTheme.Grid));
            using var edge = new Pen(active ? ActiveTheme.Highlight : ActiveTheme.Grid, 2);
            g.FillRectangle(fill, rect);
            g.DrawRectangle(edge, rect);
        }
        using var brush = new SolidBrush(active ? ActiveTheme.Highlight : ActiveTheme.Text);
        DrawLabel(g, label, _buttonFont, brush, rect, _centerFormat);
    }

    private void DrawUiHeader(Graphics g)
    {
        DrawUiPanel(g, new Rectangle(24, 12, 1112, 56), false);

        using var logo = new SolidBrush(ActiveTheme.Accent);
        g.FillRectangle(logo, 40, 23, 34, 34);
        DrawPixelText(g, "S", 57, 29, 3, ActiveTheme.Dark);

        using var title = new SolidBrush(ActiveTheme.Text);
        DrawLabel(g, "SNAKE", _sectionFont, title, new Rectangle(84, 20, 95, 19), _leftFormat);
        DrawLabel(g, "ARCADE EDITION", _microFont, title, new Rectangle(84, 38, 110, 17), _leftFormat);

        DrawUiNavButton(g, _homeNavButton, "HOME", _view == AppView.Home);
        DrawUiNavButton(g, _playNavButton, "PLAY", _view == AppView.Play);
        DrawUiNavButton(g, _skinsNavButton, "SKINS", _view == AppView.Skins);
        DrawUiNavButton(g, _settingsNavButton, "SETTINGS", _view == AppView.Settings);

        using var status = new SolidBrush(ActiveTheme.Highlight);
        g.FillRectangle(status, 832, 36, 7, 7);
        DrawLabel(g, "READY TO PLAY", _microFont, title, new Rectangle(846, 25, 115, 25), _leftFormat);
    }

    private void DrawUiWindowControls(Graphics g)
    {
        DrawWindowButton(g, _minimizeButton, 1, _hoverId == 1);
        DrawWindowButton(g, _fullscreenButton, 2, _hoverId == 13);
        DrawWindowButton(g, _closeButton, 3, _hoverId == 2);
    }

    private void DrawUiPageTitle(Graphics g, string title, string subtitle)
    {
        using var titleBrush = new SolidBrush(ActiveTheme.Text);
        using var subBrush = new SolidBrush(Color.FromArgb(190, ActiveTheme.Text));
        DrawLabel(g, title, _pageTitleFont, titleBrush,
            new Rectangle(62, 91, 760, 42), _leftFormat);
        DrawLabel(g, subtitle, _bodyFont, subBrush,
            new Rectangle(64, 132, 800, 24), _leftFormat);
        using var line = new Pen(Color.FromArgb(105, ActiveTheme.Accent), 2);
        g.DrawLine(line, 64, 160, 124, 160);
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
        DrawUiPageTitle(g, "SNAKE SKINS", "Choose a look for your next run.");
        DrawUiButton(g, _skinPickerClose, "←  BACK");

        DrawUiPanel(g, new Rectangle(32, 166, 714, 484), false, 18);
        DrawSkinPicker(g);

        DrawUiPanel(g, new Rectangle(780, 166, 348, 309), true, 20);
        using (var previewGlow = new SolidBrush(Color.FromArgb(28, ActiveTheme.Highlight)))
            g.FillEllipse(previewGlow, 835, 188, 240, 205);
        DrawSnakePreview(g, new Rectangle(804, 176, 300, 270), Skins[_selectedSkin]);

        DrawUiPanel(g, new Rectangle(780, 488, 348, 162), false, 18);
        SnakeSkin skin = Skins[_selectedSkin];
        using var titleBrush = new SolidBrush(skin.IsLegendary
            ? ActiveTheme.Highlight : ActiveTheme.Text);
        using var subBrush = new SolidBrush(Color.FromArgb(185, ActiveTheme.Text));
        DrawLabel(g, SkinShortName(_selectedSkin), _sectionFont, titleBrush,
            new Rectangle(802, 508, 300, 24), _centerFormat);
        DrawLabel(g, SkinPatternName(skin.Pattern).ToUpperInvariant(),
            _microFont, subBrush, new Rectangle(802, 535, 300, 20), _centerFormat);
        DrawLabel(g, skin.IsLegendary ? "LEGENDARY SPECIAL EFFECTS" : "CUSTOM PIXEL DESIGN",
            _microFont, titleBrush, new Rectangle(802, 560, 300, 20), _centerFormat);
        DrawLabel(g, "CLICK ANY CARD TO EQUIP",
            _microFont, subBrush, new Rectangle(802, 600, 300, 20), _centerFormat);
    }

    
    private void DrawSkinPicker(Graphics g)
    {
        for (int i = 0; i < Skins.Length; i++)
        {
            Rectangle tile = SkinTileRect(i);
            SnakeSkin skin = Skins[i];
            bool selected = i == _selectedSkin;
            bool hover = tile.Contains(_mousePoint);

            DrawUiPanel(g, tile, selected || hover, 10);
            if (selected)
                FillRound(g, new Rectangle(tile.X + 8, tile.Y + 8, 4, tile.Height - 16),
                    2, ActiveTheme.Highlight);

            DrawSkinSwatch(g, tile, skin);
            using var nameBrush = new SolidBrush(skin.IsLegendary
                ? ActiveTheme.Highlight
                : selected ? ActiveTheme.Text : Color.FromArgb(210, ActiveTheme.Text));
            using var infoBrush = new SolidBrush(Color.FromArgb(145, ActiveTheme.Text));
            DrawLabel(g, SkinShortName(i), _buttonFont, nameBrush,
                new Rectangle(tile.X + 82, tile.Y + 8, tile.Width - 92, 23), _leftFormat);
            DrawLabel(g, skin.IsLegendary ? "RARE EDITION" :
                SkinPatternName(skin.Pattern).ToUpperInvariant(),
                _microFont, infoBrush,
                new Rectangle(tile.X + 82, tile.Y + 30, tile.Width - 92, 17), _leftFormat);
        }

        using var hintBrush = new SolidBrush(Color.FromArgb(165, ActiveTheme.Text));
        DrawLabel(g, "21 DESIGNS  ·  SELECT A CARD TO EQUIP",
            _microFont, hintBrush, new Rectangle(54, 628, 670, 20), _centerFormat);
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
        if (_themeMenuLevel > 0)
        {
            DrawThemeBrowserPage(g);
            return;
        }

        DrawPixelText(g, "SETTINGS", DesignWidth / 2, 78, 6, RetroWhite);

        Rectangle soundPanel = new(70, 130, 1020, 120);
        Rectangle scorePanel = new(70, 260, 1020, 120);
        Rectangle themePanel = new(70, 390, 1020, 120);
        Rectangle controlsPanel = new(70, 520, 1020, 165);

        DrawPixelPanel(g, soundPanel, true);
        DrawPixelPanel(g, scorePanel, true);
        DrawPixelPanel(g, controlsPanel, true);
        DrawPixelPanel(g, themePanel, true);

        DrawPixelText(g, "SOUND", 245, 164, 3, RetroWhite);
        DrawPixelText(g, "GAME EFFECTS", 245, 210, 2, RetroGold);
        DrawRetroButton(g, _settingsSoundButton,
            _soundEnabled ? "SOUND ON" : "SOUND OFF", 2,
            _settingsSoundButton.Contains(_mousePoint));

        DrawPixelText(g, "PERSONAL BEST", 270, 291, 3, RetroWhite);
        DrawPixelText(g, FormatScore(_bestScore), 550, 310, 4, RetroGold);
        DrawRetroButton(g, _settingsResetButton, "RESET SCORE", 2,
            _settingsResetButton.Contains(_mousePoint));

        DrawPixelText(g, "KEYBOARD CONTROLS", 280, 552, 3, RetroWhite);
        DrawPixelText(g, "MOVE : WASD / ARROWS", 650, 558, 2, RetroGold);
        DrawPixelText(g, "PAUSE : SPACE", 650, 602, 2, RetroWhite);

        DrawThemeSectionCard(g);
    }

    private void DrawThemeBrowserPage(Graphics g)
    {
        string title = _themeMenuLevel == 1 ? "THEMES" : "SEASON THEMES";
        DrawPixelText(g, title, DesignWidth / 2, 91, 5, RetroWhite);
        DrawRetroButton(g, _seasonBackButton, "BACK", 2,
            _seasonBackButton.Contains(_mousePoint));

        if (_themeMenuLevel == 1)
        {
            DrawPixelText(g, "CHOOSE A THEME CATEGORY",
                DesignWidth / 2, 145, 2, RetroGold);
            DrawSeasonCategoryCard(g);
            return;
        }

        DrawPixelText(g, "CHOOSE YOUR WORLD",
            DesignWidth / 2, 132, 2, RetroGold);

        GraphicsState state = g.Save();
        g.SetClip(ThemeGridViewport);
        for (int i = 0; i < Themes.Length; i++)
            DrawThemeCard(g, i);
        g.Restore(state);

        int rows = (Themes.Length + 3) / 4;
        int maxScroll = Math.Max(0, 158 + (rows - 1) * 264 + 232 - ThemeGridViewport.Bottom);
        if (maxScroll > 0)
        {
            DrawPixelText(g, "SCROLL TO VIEW MORE",
                DesignWidth / 2, 704, 1, RetroGold);
        }
    }

        private Rectangle ThemeTileRect(int index)
    {
        int column = index % 4;
        int row = index / 4;
        return new Rectangle(60 + column * 260,
            158 + row * 264 - _themeScrollOffset, 240, 232);
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

        Rectangle art = new(tile.X + 10, tile.Y + 10, tile.Width - 20, 132);
        using (var artFill = new SolidBrush(theme.Background))
            g.FillRectangle(artFill, art);
        DrawThemeThumbnail(g, art, theme);
        DrawPixelOutline(g, art, theme.Grid, 1);

        DrawPixelText(g, theme.Season, tile.X + tile.Width / 2,
            tile.Y + 160, 3, theme.Text);
        DrawPixelText(g, theme.TimeOfDay, tile.X + tile.Width / 2,
            tile.Y + 190, 2, theme.Accent);

        if (selected)
            DrawPixelText(g, "SELECTED", tile.X + tile.Width / 2,
                tile.Y + 214, 1, theme.Highlight);
    }

private void DrawThemeSectionCard(Graphics g)
    {
        DrawPixelText(g, "THEMES", 245, 450, 3, RetroWhite);
        DrawRetroButton(g, _themeCategoryTile, "OPEN THEMES", 2,
            _themeCategoryTile.Contains(_mousePoint), true);
    }

    private void DrawSeasonCategoryCard(Graphics g)
    {
        Rectangle tile = _seasonCategoryTile;

        using var fill = new SolidBrush(RetroDark);
        g.FillRectangle(fill, tile);
        DrawPixelOutline(g, tile, RetroGold, 4);

        ArcadeTheme[] previews = [Themes[0], Themes[2], Themes[4], Themes[6]];
        string[] names = ["SPRING", "SUMMER", "AUTUMN", "WINTER"];
        int gap = 8;
        int artWidth = (tile.Width - 40 - gap * 3) / 4;

        for (int i = 0; i < previews.Length; i++)
        {
            int x = tile.X + 20 + i * (artWidth + gap);
            Rectangle art = new(x, tile.Y + 20, artWidth, 150);

            using var artFill = new SolidBrush(previews[i].Background);
            g.FillRectangle(artFill, art);
            DrawThemeThumbnail(g, art, previews[i]);
            DrawPixelOutline(g, art, previews[i].Accent, 2);

            DrawPixelText(g, names[i], x + artWidth / 2,
                tile.Y + 190, 1, previews[i].Text);
        }

        DrawPixelText(g, "SEASON THEMES",
            tile.X + tile.Width / 2, tile.Y + 242, 2, RetroGold);
        DrawPixelText(g, "OPEN TO VIEW ALL SEASONAL THEMES",
            tile.X + tile.Width / 2, tile.Y + 285, 1, RetroWhite);
    }

    private void DrawThemeNavigationCard(
        Graphics g, Rectangle tile, string label, bool showSeasonPreviews)
    {
        bool hover = tile.Contains(_mousePoint);

        using (var shadow = new SolidBrush(Color.FromArgb(45, 0, 0, 0)))
            FillRound(g, new Rectangle(tile.X + 2, tile.Y + 4, tile.Width, tile.Height),
                14, shadow);
        using (var fill = new LinearGradientBrush(tile,
                   ActiveTheme.Dark, ActiveTheme.Background, 90f))
            FillRound(g, tile, 14, fill);
        StrokeRound(g, tile, 14,
            hover ? ActiveTheme.Highlight : Color.FromArgb(125, ActiveTheme.Grid));

        using var title = new SolidBrush(ActiveTheme.Text);
        using var arrow = new SolidBrush(ActiveTheme.Highlight);

        if (showSeasonPreviews)
        {
            ArcadeTheme[] previews = [Themes[0], Themes[2], Themes[4], Themes[6]];
            int gap = 8;
            int width = (tile.Width - 24 - gap * 3) / 4;
            for (int i = 0; i < previews.Length; i++)
            {
                Rectangle art = new(tile.X + 12 + i * (width + gap),
                    tile.Y + 12, width, 126);
                using var artFill = new SolidBrush(previews[i].Background);
                FillRound(g, art, 8, artFill);
                GraphicsState state = g.Save();
                using (GraphicsPath clip = RoundPath(art, 8))
                    g.SetClip(clip);
                DrawThemeThumbnail(g, art, previews[i]);
                g.Restore(state);
            }

            DrawLabel(g, label, _pageTitleFont, title,
                new Rectangle(tile.X + 18, tile.Y + 151, tile.Width - 90, 48), _leftFormat);
        }
        else
        {
            // A neutral category symbol keeps the Themes entry free of seasonal artwork.
            using var swatchOne = new SolidBrush(ActiveTheme.Accent);
            using var swatchTwo = new SolidBrush(ActiveTheme.Highlight);
            using var swatchThree = new SolidBrush(ActiveTheme.Grid);
            Rectangle icon = new(tile.X + 42, tile.Y + 76, 68, 68);
            StrokeRound(g, icon, 18, Color.FromArgb(135, ActiveTheme.Highlight));
            g.FillEllipse(swatchOne, icon.X + 13, icon.Y + 13, 18, 18);
            g.FillEllipse(swatchTwo, icon.X + 37, icon.Y + 13, 18, 18);
            g.FillEllipse(swatchThree, icon.X + 13, icon.Y + 37, 18, 18);
            g.FillEllipse(title, icon.X + 37, icon.Y + 37, 18, 18);

            DrawLabel(g, label, _pageTitleFont, title,
                new Rectangle(tile.X + 138, tile.Y + 78, tile.Width - 230, 64), _leftFormat);
        }

        DrawLabel(g, "›", _displayFont, arrow,
            new Rectangle(tile.Right - 76, tile.Y + 145, 54, 58), _centerFormat);
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
                DrawBlossomTree(g, area.X + 25, area.Bottom - 12, 0.20f, theme);
                DrawPixelFlower(g, area.X + 78, area.Bottom - 25, detail, theme.Accent);
                DrawPixelFlower(g, area.X + 95, area.Bottom - 31, detail, theme.Accent);
                break;
            case ThemeSeason.Summer:
                DrawPalmTree(g, area.X + 24, area.Bottom - 12, 0.20f, theme);
                g.FillRectangle(detail, area.X + 48, area.Bottom - 25, 25, 3);
                g.FillRectangle(detail, area.X + 70, area.Bottom - 30, 24, 3);
                break;
            case ThemeSeason.Autumn:
                DrawBareTree(g, area.X + 26, area.Bottom - 12, 0.20f, theme);
                DrawPixelLeaf(g, area.X + 78, area.Bottom - 24, detail, 8);
                DrawPixelLeaf(g, area.X + 96, area.Bottom - 28, detail, 7);
                using (var rain = new Pen(Color.FromArgb(145, theme.Highlight), 1))
                {
                    g.DrawLine(rain, area.X + 60, area.Y + 9, area.X + 56, area.Y + 23);
                    g.DrawLine(rain, area.X + 84, area.Y + 6, area.X + 80, area.Y + 20);
                }
                break;
            case ThemeSeason.Winter:
                DrawPineTree(g, area.X + 26, area.Bottom - 12, 0.22f, theme);
                DrawPixelSnowflake(g, area.X + 83, area.Bottom - 30, detail, 5);
                DrawPixelSnowflake(g, area.X + 99, area.Bottom - 38, detail, 4);
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

        if (theme.IsNight)
        {
            using (var moon = new SolidBrush(Color.FromArgb(220, theme.Accent)))
                g.FillEllipse(moon, 1058, 80, 42, 42);
            using (var shadow = new SolidBrush(theme.Background))
                g.FillEllipse(shadow, 1075, 73, 42, 42);

            using var stars = new SolidBrush(theme.Highlight);
            for (int i = 0; i < 28; i++)
            {
                int x = 25 + (i * 83) % (DesignWidth - 50);
                int y = 82 + (i * 47) % 585;
                int twinkle = (i + (int)(tick / 280)) % 4 == 0 ? 4 : 2;
                g.FillRectangle(stars, x, y, twinkle, twinkle);
            }
        }
        else
        {
            using var sunGlow = new SolidBrush(Color.FromArgb(40, theme.Accent));
            using var sun = new SolidBrush(Color.FromArgb(220, theme.Accent));
            g.FillEllipse(sunGlow, 1034, 56, 100, 100);
            g.FillEllipse(sun, 1058, 80, 52, 52);

            using var rays = new Pen(Color.FromArgb(135, theme.Accent), 3);
            for (int i = 0; i < 12; i++)
            {
                double angle = i * Math.PI / 6;
                float x1 = 1084 + (float)Math.Cos(angle) * 31;
                float y1 = 106 + (float)Math.Sin(angle) * 31;
                float x2 = 1084 + (float)Math.Cos(angle) * 41;
                float y2 = 106 + (float)Math.Sin(angle) * 41;
                g.DrawLine(rays, x1, y1, x2, y2);
            }
        }

        switch (theme.SeasonType)
        {
            case ThemeSeason.Spring:
                DrawBlossomTree(g, 14, 724, 0.98f, theme);
                DrawBlossomTree(g, 1146, 724, 0.92f, theme);
                DrawFlowerMeadow(g, tick, theme);
                DrawFallingPetals(g, tick, theme, 22);
                break;

            case ThemeSeason.Summer:
                DrawPalmTree(g, 14, 724, 0.98f, theme);
                DrawPalmTree(g, 1146, 724, 0.92f, theme);
                DrawWarmHaze(g, tick, theme);
                DrawSummerGlints(g, tick, theme, 20);
                break;

            case ThemeSeason.Autumn:
                DrawBareTree(g, 14, 724, 1.0f, theme);
                DrawBareTree(g, 1146, 724, 0.94f, theme);
                DrawAutumnGround(g, theme);
                DrawFallingLeaves(g, tick, theme, 30);
                DrawRain(g, tick, theme, 76);
                break;

            case ThemeSeason.Winter:
                DrawPineTree(g, 14, 724, 1.0f, theme);
                DrawPineTree(g, 1146, 724, 0.94f, theme);
                DrawSnowGround(g, theme);
                DrawSnowfall(g, tick, theme, 64);
                break;
        }
    }

    private void DrawSeasonalPlayfieldEffects(Graphics g)
    {
        GraphicsState state = g.Save();
        g.SetClip(_field);

        long tick = Environment.TickCount64;
        ArcadeTheme theme = ActiveTheme;

        switch (theme.SeasonType)
        {
            case ThemeSeason.Spring:
                DrawFallingPetals(g, tick, theme, 12, _field);
                break;
            case ThemeSeason.Summer:
                DrawWarmHaze(g, tick, theme, _field);
                DrawSummerGlints(g, tick, theme, 12, _field);
                break;
            case ThemeSeason.Autumn:
                DrawFallingLeaves(g, tick, theme, 18, _field);
                DrawRain(g, tick, theme, 52, _field);
                break;
            case ThemeSeason.Winter:
                DrawSnowfall(g, tick, theme, 48, _field);
                break;
        }

        g.Restore(state);
    }


    private static void DrawTreeBranch(Graphics g, PointF start, PointF control1,
        PointF control2, PointF end, float width, Color color)
    {
        using var pen = new Pen(color, width)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        g.DrawBezier(pen, start, control1, control2, end);
    }

    private static void DrawTreeRoots(Graphics g, float x, float groundY,
        float scale, Color color)
    {
        DrawTreeBranch(g, new PointF(x, groundY - 2 * scale),
            new PointF(x - 12 * scale, groundY + 2 * scale),
            new PointF(x - 28 * scale, groundY + 4 * scale),
            new PointF(x - 43 * scale, groundY + 1 * scale), 5 * scale, color);
        DrawTreeBranch(g, new PointF(x + 1 * scale, groundY - 2 * scale),
            new PointF(x + 14 * scale, groundY + 2 * scale),
            new PointF(x + 30 * scale, groundY + 4 * scale),
            new PointF(x + 47 * scale, groundY), 5 * scale, color);
    }

    private static void DrawBlossomMass(Graphics g, float x, float y,
        float radius, ArcadeTheme theme, int seed)
    {
        using var shadow = new SolidBrush(Color.FromArgb(100, 94, 54, 87));
        using var deep = new SolidBrush(Color.FromArgb(205, 210, 119, 157));
        using var mid = new SolidBrush(Color.FromArgb(225, theme.Effect));
        using var light = new SolidBrush(Color.FromArgb(220, 255, 222, 231));
        using var center = new SolidBrush(Color.FromArgb(235, 255, 231, 164));

        g.FillEllipse(shadow, x - radius * 0.85f, y - radius * 0.45f,
            radius * 1.8f, radius * 1.15f);

        for (int i = 0; i < 9; i++)
        {
            double angle = (i * 40 + seed * 13) * Math.PI / 180;
            float ox = (float)Math.Cos(angle) * radius * (0.42f + (i % 3) * 0.08f);
            float oy = (float)Math.Sin(angle) * radius * 0.42f;
            float size = radius * (0.62f + (i % 4) * 0.08f);
            Brush brush = i % 4 == 0 ? deep : mid;
            g.FillEllipse(brush, x + ox - size * 0.55f,
                y + oy - size * 0.48f, size * 1.1f, size * 0.92f);
        }

        for (int i = 0; i < 7; i++)
        {
            float px = x + (float)Math.Sin(i * 2.7f + seed) * radius * 0.62f;
            float py = y + (float)Math.Cos(i * 1.9f + seed) * radius * 0.42f;
            float size = Math.Max(3, radius * 0.12f);
            g.FillEllipse(light, px - size, py - size, size * 2, size * 1.8f);
            g.FillEllipse(center, px - size * 0.22f, py - size * 0.22f,
                size * 0.44f, size * 0.44f);
        }
    }

    private static void DrawBlossomTree(Graphics g, int x, int groundY,
        float scale, ArcadeTheme theme)
    {
        Color bark = Color.FromArgb(230, 83, 53, 48);
        Color branch = Color.FromArgb(225, 105, 69, 58);
        float topY = groundY - 245 * scale;

        DrawTreeBranch(g, new PointF(x, groundY),
            new PointF(x - 19 * scale, groundY - 72 * scale),
            new PointF(x + 24 * scale, groundY - 166 * scale),
            new PointF(x + 4 * scale, topY), 19 * scale, Color.FromArgb(120, 40, 25, 28));
        DrawTreeBranch(g, new PointF(x, groundY),
            new PointF(x - 19 * scale, groundY - 72 * scale),
            new PointF(x + 24 * scale, groundY - 166 * scale),
            new PointF(x + 4 * scale, topY), 14 * scale, bark);
        DrawTreeBranch(g, new PointF(x - 4 * scale, groundY - 38 * scale),
            new PointF(x - 54 * scale, groundY - 88 * scale),
            new PointF(x - 67 * scale, groundY - 138 * scale),
            new PointF(x - 110 * scale, groundY - 174 * scale), 8 * scale, branch);
        DrawTreeBranch(g, new PointF(x - 34 * scale, groundY - 87 * scale),
            new PointF(x - 76 * scale, groundY - 104 * scale),
            new PointF(x - 90 * scale, groundY - 148 * scale),
            new PointF(x - 94 * scale, groundY - 190 * scale), 4 * scale, branch);
        DrawTreeBranch(g, new PointF(x - 74 * scale, groundY - 142 * scale),
            new PointF(x - 112 * scale, groundY - 150 * scale),
            new PointF(x - 125 * scale, groundY - 174 * scale),
            new PointF(x - 142 * scale, groundY - 187 * scale), 3 * scale, branch);

        DrawTreeBranch(g, new PointF(x + 5 * scale, groundY - 82 * scale),
            new PointF(x + 53 * scale, groundY - 102 * scale),
            new PointF(x + 72 * scale, groundY - 146 * scale),
            new PointF(x + 112 * scale, groundY - 180 * scale), 8 * scale, branch);
        DrawTreeBranch(g, new PointF(x + 53 * scale, groundY - 111 * scale),
            new PointF(x + 75 * scale, groundY - 129 * scale),
            new PointF(x + 91 * scale, groundY - 174 * scale),
            new PointF(x + 91 * scale, groundY - 205 * scale), 4 * scale, branch);
        DrawTreeBranch(g, new PointF(x + 82 * scale, groundY - 147 * scale),
            new PointF(x + 112 * scale, groundY - 151 * scale),
            new PointF(x + 128 * scale, groundY - 174 * scale),
            new PointF(x + 145 * scale, groundY - 180 * scale), 3 * scale, branch);
        DrawTreeBranch(g, new PointF(x + 2 * scale, groundY - 142 * scale),
            new PointF(x - 25 * scale, groundY - 174 * scale),
            new PointF(x - 23 * scale, groundY - 211 * scale),
            new PointF(x - 39 * scale, groundY - 227 * scale), 5 * scale, branch);

        DrawTreeRoots(g, x, groundY, scale, bark);

        DrawBlossomMass(g, x - 107 * scale, groundY - 188 * scale,
            40 * scale, theme, 1);
        DrawBlossomMass(g, x - 49 * scale, groundY - 231 * scale,
            49 * scale, theme, 2);
        DrawBlossomMass(g, x + 14 * scale, groundY - 249 * scale,
            53 * scale, theme, 3);
        DrawBlossomMass(g, x + 85 * scale, groundY - 217 * scale,
            47 * scale, theme, 4);
        DrawBlossomMass(g, x + 126 * scale, groundY - 181 * scale,
            35 * scale, theme, 5);
        DrawBlossomMass(g, x - 2 * scale, groundY - 173 * scale,
            43 * scale, theme, 6);
    }

    private static void DrawPalmTree(Graphics g, int x, int groundY,
        float scale, ArcadeTheme theme)
    {
        PointF crown = new(x + 13 * scale, groundY - 250 * scale);
        DrawTreeBranch(g, new PointF(x, groundY),
            new PointF(x - 21 * scale, groundY - 70 * scale),
            new PointF(x + 35 * scale, groundY - 166 * scale),
            crown, 17 * scale, Color.FromArgb(105, 61, 36, 22));
        DrawTreeBranch(g, new PointF(x, groundY),
            new PointF(x - 21 * scale, groundY - 70 * scale),
            new PointF(x + 35 * scale, groundY - 166 * scale),
            crown, 12 * scale, Color.FromArgb(235, 139, 86, 48));

        using var ringPen = new Pen(Color.FromArgb(125, 86, 50, 29), 2 * scale);
        for (int i = 0; i < 9; i++)
        {
            float t = 0.18f + i * 0.075f;
            float px = x - 18 * scale * (1 - t) + 10 * scale * t;
            float py = groundY - 245 * scale * t;
            g.DrawArc(ringPen, px - 6 * scale, py - 4 * scale,
                13 * scale, 8 * scale, 10, 155);
        }

        Color leafDark = Color.FromArgb(235, 27, 91, 55);
        Color leafMid = Color.FromArgb(245, 48, 142, 77);
        using var rib = new Pen(Color.FromArgb(220, 170, 208, 113), 2 * scale);
        using var darkLeaflet = new Pen(leafDark, 3.2f * scale)
            { StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var midLeaflet = new Pen(leafMid, 3.2f * scale)
            { StartCap = LineCap.Round, EndCap = LineCap.Round };
        for (int frond = 0; frond < 8; frond++)
        {
            double angle = (-168 + frond * 32) * Math.PI / 180;
            float length = (112 + frond % 3 * 12) * scale;
            PointF end = new(
                crown.X + (float)Math.Cos(angle) * length,
                crown.Y + (float)Math.Sin(angle) * length * 0.70f + 16 * scale);

            PointF c1 = new(crown.X + (end.X - crown.X) * 0.34f,
                crown.Y + (end.Y - crown.Y) * 0.20f - 22 * scale);
            PointF c2 = new(crown.X + (end.X - crown.X) * 0.72f,
                crown.Y + (end.Y - crown.Y) * 0.75f + 25 * scale);
            using var spine = new Pen(leafDark, 6 * scale)
                { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawBezier(spine, crown, c1, c2, end);
            g.DrawBezier(rib, crown, c1, c2, end);

            for (int j = 1; j <= 11; j++)
            {
                float t = j / 12f;
                float bx = crown.X + (end.X - crown.X) * t;
                float by = crown.Y + (end.Y - crown.Y) * t +
                    MathF.Sin(t * MathF.PI) * 12 * scale;
                float leafLen = (22 - t * 9) * scale;
                float nx = -(end.Y - crown.Y) / Math.Max(1, length) * leafLen;
                float ny = (end.X - crown.X) / Math.Max(1, length) * leafLen;
                Pen leaflet = j % 3 == 0 ? darkLeaflet : midLeaflet;
                g.DrawLine(leaflet, bx, by, bx + nx, by + ny);
                g.DrawLine(leaflet, bx, by, bx - nx * 0.78f, by - ny * 0.78f);
            }
        }

        using var coconuts = new SolidBrush(Color.FromArgb(255, 113, 70, 36));
        g.FillEllipse(coconuts, crown.X - 8 * scale, crown.Y - 1 * scale,
            13 * scale, 13 * scale);
        g.FillEllipse(coconuts, crown.X + 3 * scale, crown.Y + 2 * scale,
            12 * scale, 12 * scale);
        g.FillEllipse(coconuts, crown.X - 2 * scale, crown.Y + 8 * scale,
            10 * scale, 10 * scale);
        DrawTreeRoots(g, x, groundY, scale, Color.FromArgb(195, 112, 67, 39));
    }

    private static void DrawBareTree(Graphics g, int x, int groundY,
        float scale, ArcadeTheme theme)
    {
        Color trunk = Color.FromArgb(235, 84, 52, 35);
        Color branch = Color.FromArgb(220, 113, 71, 47);
        PointF root = new(x, groundY);
        PointF crown = new(x + 4 * scale, groundY - 255 * scale);

        DrawTreeBranch(g, root,
            new PointF(x - 15 * scale, groundY - 75 * scale),
            new PointF(x + 20 * scale, groundY - 168 * scale),
            crown, 20 * scale, Color.FromArgb(110, 45, 27, 20));
        DrawTreeBranch(g, root,
            new PointF(x - 15 * scale, groundY - 75 * scale),
            new PointF(x + 20 * scale, groundY - 168 * scale),
            crown, 14 * scale, trunk);

        // Main limbs split into thinner, upward-curving branches instead of straight spokes.
        DrawTreeBranch(g, new PointF(x - 3 * scale, groundY - 77 * scale),
            new PointF(x - 55 * scale, groundY - 112 * scale),
            new PointF(x - 80 * scale, groundY - 163 * scale),
            new PointF(x - 127 * scale, groundY - 184 * scale), 8 * scale, branch);
        DrawTreeBranch(g, new PointF(x - 71 * scale, groundY - 145 * scale),
            new PointF(x - 104 * scale, groundY - 151 * scale),
            new PointF(x - 112 * scale, groundY - 193 * scale),
            new PointF(x - 124 * scale, groundY - 217 * scale), 4 * scale, branch);
        DrawTreeBranch(g, new PointF(x - 105 * scale, groundY - 170 * scale),
            new PointF(x - 130 * scale, groundY - 176 * scale),
            new PointF(x - 141 * scale, groundY - 201 * scale),
            new PointF(x - 154 * scale, groundY - 206 * scale), 2.5f * scale, branch);
        DrawTreeBranch(g, new PointF(x - 39 * scale, groundY - 124 * scale),
            new PointF(x - 52 * scale, groundY - 155 * scale),
            new PointF(x - 49 * scale, groundY - 193 * scale),
            new PointF(x - 62 * scale, groundY - 218 * scale), 4 * scale, branch);

        DrawTreeBranch(g, new PointF(x + 4 * scale, groundY - 105 * scale),
            new PointF(x + 54 * scale, groundY - 133 * scale),
            new PointF(x + 83 * scale, groundY - 171 * scale),
            new PointF(x + 129 * scale, groundY - 192 * scale), 8 * scale, branch);
        DrawTreeBranch(g, new PointF(x + 72 * scale, groundY - 158 * scale),
            new PointF(x + 109 * scale, groundY - 167 * scale),
            new PointF(x + 118 * scale, groundY - 201 * scale),
            new PointF(x + 135 * scale, groundY - 222 * scale), 4 * scale, branch);
        DrawTreeBranch(g, new PointF(x + 39 * scale, groundY - 127 * scale),
            new PointF(x + 49 * scale, groundY - 164 * scale),
            new PointF(x + 42 * scale, groundY - 198 * scale),
            new PointF(x + 54 * scale, groundY - 228 * scale), 3.5f * scale, branch);
        DrawTreeBranch(g, new PointF(x + 4 * scale, groundY - 173 * scale),
            new PointF(x - 12 * scale, groundY - 207 * scale),
            new PointF(x - 4 * scale, groundY - 233 * scale),
            new PointF(x - 17 * scale, groundY - 249 * scale), 4 * scale, branch);

        using var barkLine = new Pen(Color.FromArgb(100, 214, 157, 112), 1.4f * scale);
        g.DrawBezier(barkLine, x - 2 * scale, groundY - 25 * scale,
            x - 7 * scale, groundY - 90 * scale, x + 25 * scale,
            groundY - 170 * scale, x + 3 * scale, groundY - 244 * scale);

        // A few leaves remain near the outer twigs.
        using var leafBrush = new SolidBrush(Color.FromArgb(210, theme.Effect));
        for (int i = 0; i < 13; i++)
        {
            float lx = x + (float)Math.Sin(i * 2.2f) * (72 + i % 4 * 16) * scale;
            float ly = groundY - (170 + (i % 4) * 17) * scale;
            float size = (5 + i % 4) * scale;
            g.FillEllipse(leafBrush, lx - size, ly - size * 0.45f,
                size * 2, size);
        }

        using var leafPile = new SolidBrush(Color.FromArgb(220, theme.Effect));
        g.FillEllipse(leafPile, x - 43 * scale, groundY - 8 * scale,
            43 * scale, 11 * scale);
        g.FillEllipse(leafPile, x + 4 * scale, groundY - 7 * scale,
            48 * scale, 10 * scale);
        DrawTreeRoots(g, x, groundY, scale, trunk);
    }

    private static void DrawPineTree(Graphics g, int x, int groundY,
        float scale, ArcadeTheme theme)
    {
        using var trunk = new SolidBrush(Color.FromArgb(235, 86, 62, 51));
        FillRound(g, new Rectangle(
            (int)(x - 7 * scale), (int)(groundY - 250 * scale),
            Math.Max(1, (int)(14 * scale)), Math.Max(1, (int)(253 * scale))),
            Math.Max(2, (int)(5 * scale)), trunk);

        using var pineShadow = new SolidBrush(Color.FromArgb(235, 10, 48, 59));
        using var pine = new SolidBrush(Color.FromArgb(245, 20, 83, 83));
        using var snow = new SolidBrush(Color.FromArgb(242, theme.Highlight));

        for (int tier = 0; tier < 5; tier++)
        {
            float y = groundY - (75 + tier * 39) * scale;
            float half = (70 - tier * 8) * scale;
            PointF[] points =
            [
                new(x + 1 * scale, y - 73 * scale),
                new(x - half * 0.28f, y - 28 * scale),
                new(x - half * 0.88f, y + 8 * scale),
                new(x - half * 0.52f, y + 6 * scale),
                new(x - half * 0.63f, y + 22 * scale),
                new(x, y + 13 * scale),
                new(x + half * 0.62f, y + 23 * scale),
                new(x + half * 0.52f, y + 6 * scale),
                new(x + half * 0.90f, y + 8 * scale),
                new(x + half * 0.25f, y - 31 * scale)
            ];
            g.FillPolygon(pineShadow, points);
            using var edge = new Pen(Color.FromArgb(130, 5, 35, 45), 2 * scale);
            g.DrawPolygon(edge, points);

            PointF[] inner =
            [
                new(x + 1 * scale, y - 67 * scale),
                new(x - half * 0.28f, y - 27 * scale),
                new(x - half * 0.78f, y + 5 * scale),
                new(x - half * 0.42f, y + 4 * scale),
                new(x, y + 13 * scale),
                new(x + half * 0.75f, y + 6 * scale),
                new(x + half * 0.24f, y - 28 * scale)
            ];
            g.FillPolygon(pine, inner);

            using var snowEdge = new Pen(Color.FromArgb(238, theme.Highlight), 5 * scale)
                { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            g.DrawLine(snowEdge, x - half * 0.52f, y + 5 * scale,
                x - half * 0.28f, y - 27 * scale);
            g.DrawLine(snowEdge, x - half * 0.28f, y - 27 * scale,
                x + 1 * scale, y - 67 * scale);
            g.DrawLine(snowEdge, x + 1 * scale, y - 67 * scale,
                x + half * 0.24f, y - 28 * scale);
            g.FillEllipse(snow, x - half * 0.65f, y + 1 * scale,
                half * 0.56f, 8 * scale);
        }

        g.FillEllipse(snow, x - 21 * scale, groundY - 8 * scale,
            42 * scale, 14 * scale);
        DrawTreeRoots(g, x, groundY, scale, Color.FromArgb(190, 86, 62, 51));
    }

        private static void DrawFlowerMeadow(Graphics g, long tick, ArcadeTheme theme)
    {
        using var stem = new Pen(Color.FromArgb(190, 80, 150, 73), 2);
        using var petals = new SolidBrush(Color.FromArgb(220, theme.Effect));
        int phase = (int)(tick / 450) % 13;

        for (int i = 0; i < 26; i++)
        {
            int x = (i * 47 + phase * 3) % DesignWidth;
            int y = 711 + (i % 3) * 9;
            int height = 8 + i % 5;
            g.DrawLine(stem, x, y + 8, x + (i % 2 == 0 ? 1 : -1), y - height);
            DrawPixelFlower(g, x, y - height, petals, theme.Accent);
        }
    }

    private static void DrawWarmHaze(Graphics g, long tick, ArcadeTheme theme,
        Rectangle? area = null)
    {
        Rectangle bounds = area ?? new Rectangle(0, 0, DesignWidth, DesignHeight);
        using var haze = new Pen(Color.FromArgb(45, theme.Highlight), 2);
        float phase = tick / 340f;

        for (int i = 0; i < 4; i++)
        {
            float y = bounds.Top + bounds.Height * (0.38f + i * 0.13f);
            float x = bounds.Left + (float)(Math.Sin(phase + i) * 22);
            g.DrawBezier(haze, x, y, x + bounds.Width * 0.25f, y - 5,
                x + bounds.Width * 0.60f, y + 5, bounds.Right, y);
        }
    }

    private static void DrawSummerGlints(Graphics g, long tick, ArcadeTheme theme,
        int count, Rectangle? area = null)
    {
        Rectangle bounds = area ?? new Rectangle(0, 0, DesignWidth, DesignHeight);
        using var glint = new SolidBrush(Color.FromArgb(145, theme.Effect));

        for (int i = 0; i < count; i++)
        {
            int x = bounds.Left + (i * 83 + (int)(tick / 22)) % Math.Max(1, bounds.Width);
            int y = bounds.Top + (i * 59 + (int)(tick / 38)) % Math.Max(1, bounds.Height);
            int size = i % 3 == 0 ? 4 : 2;
            g.FillRectangle(glint, x, y, size, size);
            if (i % 4 == 0)
                g.FillRectangle(glint, x - 3, y + 1, size + 6, 1);
        }
    }

    private static void DrawFallingPetals(Graphics g, long tick, ArcadeTheme theme,
        int count, Rectangle? area = null)
    {
        Rectangle bounds = area ?? new Rectangle(0, 0, DesignWidth, DesignHeight);
        for (int i = 0; i < count; i++)
        {
            float y = bounds.Top + (i * 67 + tick / 34) % Math.Max(1, bounds.Height);
            float drift = MathF.Sin(tick / 370f + i * 1.7f) * 22;
            float x = bounds.Left + (i * 89 + drift + tick / 65f) % Math.Max(1, bounds.Width);
            DrawFallingLeaf(g, x, y, theme.Effect, 5 + i % 4, tick / 45f + i);
        }
    }

    private static void DrawFallingLeaves(Graphics g, long tick, ArcadeTheme theme,
        int count, Rectangle? area = null)
    {
        Rectangle bounds = area ?? new Rectangle(0, 0, DesignWidth, DesignHeight);
        for (int i = 0; i < count; i++)
        {
            float y;
            float x;

            if (area is null)
            {
                // Leaves begin around the bare trees, then drift down with the wind.
                y = 505 + (i * 43 + tick / 24) % 215;
                float sway = MathF.Sin(tick / 290f + i * 1.4f) * 34;
                float treeSide = i % 2 == 0 ? bounds.Left + 35 : bounds.Right - 35;
                x = treeSide + sway + MathF.Sin(tick / 520f + i) * 18;
            }
            else
            {
                y = bounds.Top + (i * 71 + tick / 24) % Math.Max(1, bounds.Height);
                float sway = MathF.Sin(tick / 290f + i * 1.4f) * 25;
                x = bounds.Left + (i * 97 + sway + tick / 95f) % Math.Max(1, bounds.Width);
            }

            Color color = i % 3 == 0 ? theme.Accent : theme.Effect;
            DrawFallingLeaf(g, x, y, color, 6 + i % 5, tick / 60f + i);
        }
    }

    private static void DrawFallingLeaf(Graphics g, float x, float y,
        Color color, float size, float angle)
    {
        GraphicsState state = g.Save();
        g.TranslateTransform(x, y);
        g.RotateTransform(angle * 24f);

        using var leaf = new SolidBrush(color);
        using var stem = new Pen(Color.FromArgb(180, 95, 58, 32), 1);
        PointF[] points =
        [
            new(-size, 0), new(-size * 0.35f, -size * 0.72f),
            new(size * 0.52f, -size * 0.60f), new(size, 0),
            new(size * 0.45f, size * 0.62f), new(-size * 0.35f, size * 0.65f)
        ];
        g.FillPolygon(leaf, points);
        g.DrawLine(stem, -size * 0.8f, 0, size * 0.85f, 0);
        g.Restore(state);
    }

    private static void DrawRain(Graphics g, long tick, ArcadeTheme theme,
        int count, Rectangle? area = null)
    {
        Rectangle bounds = area ?? new Rectangle(0, 0, DesignWidth, DesignHeight);
        using var rain = new Pen(Color.FromArgb(95, theme.Highlight), 1.3f);
        using var rainBright = new Pen(Color.FromArgb(145, theme.Text), 1.6f);

        for (int i = 0; i < count; i++)
        {
            int x = bounds.Left + (i * 67 + (int)(tick / 18)) % Math.Max(1, bounds.Width);
            int y = bounds.Top + (i * 43 + (int)(tick / 5)) % Math.Max(1, bounds.Height);
            Pen pen = i % 5 == 0 ? rainBright : rain;
            g.DrawLine(pen, x, y, x - 5, y + 13);
        }
    }

    private static void DrawAutumnGround(Graphics g, ArcadeTheme theme)
    {
        Point[] ground =
        [
            new(0, 760), new(0, 720), new(180, 728), new(340, 716),
            new(520, 733), new(720, 719), new(920, 729), new(1160, 717),
            new(1160, 760)
        ];
        using var soil = new SolidBrush(Color.FromArgb(220, 75, 43, 25));
        g.FillPolygon(soil, ground);

        for (int i = 0; i < 44; i++)
        {
            int x = (i * 31) % DesignWidth;
            int y = 720 + i % 4 * 9;
            DrawFallingLeaf(g, x, y, i % 3 == 0 ? theme.Accent : theme.Effect,
                4 + i % 4, i * 0.7f);
        }
    }

    private static void DrawSnowGround(Graphics g, ArcadeTheme theme)
    {
        Point[] ground =
        [
            new(0, 760), new(0, 708), new(105, 718), new(220, 730),
            new(360, 710), new(510, 728), new(680, 710), new(840, 730),
            new(1000, 708), new(1160, 720), new(1160, 760)
        ];
        using var snow = new SolidBrush(Color.FromArgb(245, theme.Highlight));
        using var shadow = new SolidBrush(Color.FromArgb(220, theme.Grid));
        g.FillPolygon(shadow, ground);
        g.FillEllipse(snow, 80, 726, 210, 30);
        g.FillEllipse(snow, 360, 729, 240, 28);
        g.FillEllipse(snow, 770, 727, 230, 29);
        g.FillEllipse(snow, 1010, 727, 130, 28);
    }

    private static void DrawSnowfall(Graphics g, long tick, ArcadeTheme theme,
        int count, Rectangle? area = null)
    {
        Rectangle bounds = area ?? new Rectangle(0, 0, DesignWidth, DesignHeight);
        using var snow = new SolidBrush(Color.FromArgb(190, theme.Highlight));
        using var flake = new Pen(Color.FromArgb(155, theme.Text), 1.2f);

        for (int i = 0; i < count; i++)
        {
            int x = bounds.Left + (i * 73 + (int)(tick / 27)) % Math.Max(1, bounds.Width);
            int y = bounds.Top + (i * 53 + (int)(tick / 9)) % Math.Max(1, bounds.Height);
            int size = i % 5 == 0 ? 5 : i % 2 == 0 ? 3 : 2;
            g.FillEllipse(snow, x, y, size, size);

            if (i % 6 == 0)
            {
                g.DrawLine(flake, x - size, y + size / 2, x + size * 2, y + size / 2);
                g.DrawLine(flake, x + size / 2, y - size, x + size / 2, y + size * 2);
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


    private void DrawRetroGamePanel(Graphics g, Rectangle rect, bool highlighted = false)
    {
        using var fill = new SolidBrush(Color.FromArgb(235, RetroDark));
        using var edge = new Pen(highlighted ? RetroGold : Color.FromArgb(150, RetroGold), 2);
        using var inner = new Pen(Color.FromArgb(95, RetroGold), 1);
        g.FillRectangle(fill, rect);
        g.DrawRectangle(edge, rect);
        g.DrawRectangle(inner, rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 10);
        using var corner = new SolidBrush(RetroGold);
        g.FillRectangle(corner, rect.X - 2, rect.Y + 12, 4, 18);
        g.FillRectangle(corner, rect.Right - 2, rect.Bottom - 30, 4, 18);
    }

    private void DrawRetroPlayChrome(Graphics g)
    {
        DrawPixelPanel(g, _boardCard, true);
        DrawPixelPanel(g, _rightPanel, true);

        // Keep each dashboard section inside its own pixel-style box.
        DrawPixelPanel(g, _scoreArea);
        DrawPixelPanel(g, _modeArea);
        DrawPixelPanel(g, _difficultyArea);
        DrawPixelPanel(g, _actionArea);

        using var boardFill = new SolidBrush(RetroDark);
        g.FillRectangle(boardFill, _field);
        using var grid = new Pen(Color.FromArgb(70, 185, 143, 17), 1);
        for (int x = _field.Left; x <= _field.Right; x += CellSize)
            g.DrawLine(grid, x, _field.Top, x, _field.Bottom);
        for (int y = _field.Top; y <= _field.Bottom; y += CellSize)
            g.DrawLine(grid, _field.Left, y, _field.Right, y);
        DrawPixelOutline(g, _field, RetroGold, 3);

        DrawPixelText(g, "SCORE", 875, 125, 2, RetroWhite);
        DrawPixelText(g, "BEST", 1040, 125, 2, RetroGold);
        DrawPixelText(g, "MODE", 960, 265, 2, RetroWhite);
        DrawPixelText(g, "DIFFICULTY", 960, 385, 2, RetroWhite);
    }
    private void DrawRetroPlayButton(Graphics g, Rectangle rect, string label,
        int pixelSize, bool hover, bool selected = false)
    {
        using var fill = new SolidBrush(hover
            ? Color.FromArgb(20, 105, 5)
            : selected ? Color.FromArgb(8, 62, 0) : RetroDark);
        g.FillRectangle(fill, rect);
        DrawPixelOutline(g, rect,
            hover ? Color.FromArgb(255, 239, 177) : RetroGold,
            selected ? 4 : 3);

        int textHeight = pixelSize * 7;
        int top = rect.Y + (rect.Height - textHeight) / 2;
        DrawPixelText(g, label, rect.X + rect.Width / 2,
            top, pixelSize, RetroWhite);
    }

    private void DrawRetroPlayDashboard(Graphics g)
    {
        DrawPixelText(g, FormatScore(_score), 875, 160, 4, RetroWhite);
        DrawPixelText(g, FormatScore(_bestScore), 1040, 160, 4, RetroGold);

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
        using var shade = new SolidBrush(Color.FromArgb(160, ActiveTheme.Dark));
        FillRound(g, _field, 10, shade);

        Rectangle panel = new(138, 220, 538, 334);
        DrawUiPanel(g, panel, true, 22);

        using var eyebrow = new SolidBrush(ActiveTheme.Highlight);
        using var title = new SolidBrush(ActiveTheme.Text);
        using var muted = new SolidBrush(Color.FromArgb(180, ActiveTheme.Text));
        DrawLabel(g, "WELCOME TO THE ARCADE", _microFont, eyebrow,
            new Rectangle(175, 258, 465, 24), _centerFormat);
        DrawLabel(g, "SNAKE", _welcomeFont, title,
            new Rectangle(175, 286, 465, 60), _centerFormat);
        DrawLabel(g, "A simple classic. A fresh challenge every run.",
            _bodyFont, muted, new Rectangle(175, 350, 465, 28), _centerFormat);
        DrawLabel(g, "YOUR BEST SCORE", _microFont, muted,
            new Rectangle(220, 393, 160, 22), _rightFormat);
        DrawLabel(g, FormatScore(_bestScore), _numberFont, eyebrow,
            new Rectangle(395, 383, 160, 42), _leftFormat);
        DrawUiButton(g, _overlayButton, "START GAME", true);
        DrawLabel(g, "WASD / ARROWS TO MOVE     ·     SPACE TO PAUSE",
            _microFont, muted, new Rectangle(175, 523, 465, 22), _centerFormat);
    }

    
    private void DrawPauseOrGameOver(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.None;

        using var shade = new SolidBrush(Color.FromArgb(225, 3, 25, 0));
        g.FillRectangle(shade, _field);

        Rectangle panel = new(138, 220, 538, 334);
        DrawPixelPanel(g, panel, true);

        using (var accent = new SolidBrush(RetroGold))
            g.FillRectangle(accent, panel.X + 24, panel.Y + 23,
                panel.Width - 48, 3);

        bool paused = _state == GameState.Paused;
        string title = paused ? "PAUSED" : _won ? "PERFECT RUN" : "GAME OVER";
        DrawPixelText(g, title, panel.X + panel.Width / 2, 263, 4,
            paused ? RetroWhite : Color.FromArgb(255, 230, 139));

        if (paused)
        {
            DrawPixelText(g, "CURRENT SCORE",
                panel.X + panel.Width / 2, 350, 2, RetroWhite);
            DrawPixelText(g, FormatScore(_score),
                panel.X + panel.Width / 2, 378, 5, Color.FromArgb(194, 255, 132));
            DrawPixelText(g, "PERSONAL BEST  " + FormatScore(_bestScore),
                panel.X + panel.Width / 2, 427, 2, RetroGold);
        }
        else
        {
            DrawPixelText(g, "FINAL SCORE",
                panel.X + panel.Width / 2, 326, 2, RetroWhite);
            DrawPixelText(g, FormatScore(_score),
                panel.X + panel.Width / 2, 351, 5,
                _newRecord ? Color.FromArgb(255, 225, 104) : RetroWhite);

            string result = _scoreSaveFailed
                ? "SCORE SAVE FAILED"
                : _newRecord ? "NEW PERSONAL BEST" : "BEST  " + FormatScore(_bestScore);
            DrawPixelText(g, result, panel.X + panel.Width / 2, 411, 2,
                _scoreSaveFailed || _newRecord ? RetroGold : Color.FromArgb(190, 255, 142));
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
        return new Rectangle(42 + column * 236, 170 + row * 66, 224, 58);
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
        BackColor = RetroBackground;
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

    private void DrawWindowButton(Graphics g, Rectangle rect, int type, bool hover)
    {
        bool close = type == 3;
        Color fillColor = close
            ? hover ? Color.FromArgb(220, 190, 55, 64) : Color.FromArgb(34, 232, 105, 112)
            : hover ? Color.FromArgb(65, ActiveTheme.Highlight) : Color.FromArgb(38, ActiveTheme.Text);
        using (var fill = new SolidBrush(fillColor))
            FillRound(g, rect, 9, fill);

        Color edgeColor = close && hover
            ? Color.FromArgb(235, 255, 195, 198)
            : hover ? Color.FromArgb(180, ActiveTheme.Highlight) : Color.FromArgb(55, ActiveTheme.Text);
        StrokeRound(g, rect, 9, edgeColor);

        Color iconColor = close && hover
            ? Color.FromArgb(255, 255, 245, 245)
            : ActiveTheme.Text;
        using var iconPen = new Pen(iconColor, 2f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        int cx = rect.X + rect.Width / 2;
        int cy = rect.Y + rect.Height / 2;

        if (type == 1)
        {
            g.DrawLine(iconPen, cx - 6, cy + 4, cx + 6, cy + 4);
        }
        else if (type == 2)
        {
            if (_isFullscreen)
            {
                g.DrawRectangle(iconPen, cx - 5, cy - 2, 11, 10);
                g.DrawLine(iconPen, cx - 5, cy - 2, cx - 2, cy - 2);
                g.DrawLine(iconPen, cx - 5, cy - 2, cx - 5, cy + 1);
                g.DrawRectangle(iconPen, cx - 7, cy - 5, 11, 10);
            }
            else
            {
                g.DrawRectangle(iconPen, cx - 6, cy - 5, 12, 11);
            }
        }
        else
        {
            g.DrawLine(iconPen, cx - 5, cy - 5, cx + 5, cy + 5);
            g.DrawLine(iconPen, cx + 5, cy - 5, cx - 5, cy + 5);
        }
    }

    private void DrawChoice(Graphics g, Rectangle rect, string label, bool selected)
    {
        bool hover = rect.Contains(_mousePoint);
        using var fill = new SolidBrush(selected ? Color.FromArgb(235, ActiveTheme.Grid) :
            hover ? Color.FromArgb(220, ActiveTheme.Background) : Panel);
        using var edge = new Pen(selected || hover ? ActiveTheme.Highlight : ActiveTheme.Grid, selected ? 2 : 1);
        g.FillRectangle(fill, rect);
        g.DrawRectangle(edge, rect);
        using var text = new SolidBrush(selected ? ActiveTheme.Highlight : ActiveTheme.Text);
        DrawLabel(g, label, _buttonFont, text, rect, _centerFormat);
    }

    private void DrawActionButton(Graphics g, Rectangle rect, string label, bool primary)
    {
        bool hover = rect.Contains(_mousePoint);
        using var fill = new SolidBrush(primary ? ActiveTheme.Accent :
            hover ? ActiveTheme.Grid : ActiveTheme.Dark);
        using var edge = new Pen(primary || hover ? ActiveTheme.Highlight : ActiveTheme.Grid, 2);
        g.FillRectangle(fill, rect);
        g.DrawRectangle(edge, rect);
        using var text = new SolidBrush(primary ? ActiveTheme.Dark : ActiveTheme.Text);
        DrawLabel(g, label, primary ? _buttonFont : _iconFont, text, rect, _centerFormat);
    }

    private void DrawStaticPanel(Graphics g, Rectangle rect, Color? fill = null)
    {
        using var background = new SolidBrush(fill ?? Panel);
        using var edge = new Pen(ActiveTheme.Grid, 2);
        g.FillRectangle(background, rect);
        g.DrawRectangle(edge, rect);
        if (rect.Width > 16 && rect.Height > 16)
        {
            using var inner = new Pen(Color.FromArgb(45, ActiveTheme.Text), 1);
            g.DrawRectangle(inner, rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8);
        }
    }

    private void DrawStaticButton(Graphics g, Rectangle rect, bool primary = false)
    {
        using var fill = new SolidBrush(primary ? ActiveTheme.Accent : ActiveTheme.Dark);
        using var edge = new Pen(primary ? ActiveTheme.Highlight : ActiveTheme.Grid, 2);
        g.FillRectangle(fill, rect);
        g.DrawRectangle(edge, rect);
    }

    private static void StaticText(Graphics g, string value, Rectangle rect, float size,
        FontStyle style, Color color, StringAlignment alignment = StringAlignment.Near)
    {
        using var font = new Font("Consolas", size, style);
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
        Rectangle rect, StringFormat format)
    {
        using var pixelFont = new Font("Consolas", Math.Max(6, font.Size), font.Style);
        g.DrawString(value, pixelFont, brush, rect, format);
    }

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
            {
                if (_view == AppView.Settings && _themeMenuLevel > 0)
                {
                    _themeMenuLevel--;
                    Invalidate();
                }
                else
                {
                    NavigateTo(AppView.Home);
                }
                e.Handled = true;
            }
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

        if (_homeNavButton.Contains(point)) return 17;
        if (_playNavButton.Contains(point)) return 18;
        if (_skinsNavButton.Contains(point)) return 19;
        if (_settingsNavButton.Contains(point)) return 20;

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
            if (_themeMenuLevel == 2)
            {
                if (_seasonBackButton.Contains(point)) return 30;
                if (ThemeGridViewport.Contains(point))
                {
                    for (int i = 0; i < Themes.Length; i++)
                        if (ThemeTileRect(i).Contains(point)) return 200 + i;
                }
                return 0;
            }

            if (_themeMenuLevel == 1)
            {
                if (_seasonBackButton.Contains(point)) return 30;
                if (_seasonCategoryTile.Contains(point)) return 31;
                return 0;
            }

            if (_themeCategoryTile.Contains(point)) return 29;
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
            29 => _themeCategoryTile,
            30 => _seasonBackButton,
            31 => _seasonCategoryTile,
            _ => Rectangle.Empty
        };
    }

    private void HandleMouseDown(object? sender, MouseEventArgs e)
    {
        Focus();

        Point point = ClientToDesign(e.Location);
        if (e.Button == MouseButtons.Left && point.Y < 70 && !_isFullscreen &&
            !_closeButton.Contains(point) && !_minimizeButton.Contains(point) &&
            !_fullscreenButton.Contains(point) && !_homeNavButton.Contains(point) &&
            !_playNavButton.Contains(point) && !_skinsNavButton.Contains(point) &&
            !_settingsNavButton.Contains(point))
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

        if (_homeNavButton.Contains(point)) { NavigateTo(AppView.Home); return; }
        if (_playNavButton.Contains(point)) { NavigateTo(AppView.Play); return; }
        if (_skinsNavButton.Contains(point)) { NavigateTo(AppView.Skins); return; }
        if (_settingsNavButton.Contains(point)) { NavigateTo(AppView.Settings); return; }

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
            if (_themeMenuLevel == 2)
            {
                if (_seasonBackButton.Contains(point))
                {
                    _themeMenuLevel = 1;
                    _themeScrollOffset = 0;
                    Invalidate();
                    return;
                }

                if (ThemeGridViewport.Contains(point))
                {
                    for (int i = 0; i < Themes.Length; i++)
                    {
                        if (!ThemeTileRect(i).Contains(point))
                            continue;

                        SelectTheme(i);
                        return;
                    }
                }
                return;
            }

            if (_themeMenuLevel == 1)
            {
                if (_seasonBackButton.Contains(point))
                {
                    _themeMenuLevel = 0;
                    Invalidate();
                    return;
                }

                if (_seasonCategoryTile.Contains(point))
                {
                    _themeMenuLevel = 2;
                    _themeScrollOffset = 0;
                    Invalidate();
                    return;
                }
                return;
            }

            if (_themeCategoryTile.Contains(point))
            {
                _themeMenuLevel = 1;
                Invalidate();
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
        _displayFont.Dispose();
        _pageTitleFont.Dispose();
        _sectionFont.Dispose();
        _bodyFont.Dispose();
        _microFont.Dispose();

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
