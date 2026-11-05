using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Media;
using System.Text;

namespace SnakeGame;

public sealed class GameForm : Form
{
    private enum GameState { Ready, Playing, Paused, GameOver }
    private enum Direction { Up, Down, Left, Right }
    private enum Difficulty { Easy, Normal, Hard }
    private enum GameMode { Classic, Wrap, Rush }
    private enum SkinPattern
    {
        Plain, Stripes, Dots, Waves, Lightning, Scales, Flames, Frost,
        Circuit, Galaxy, Camo, Candy, Coral, Prism, Stars, Honeycomb,
        Tiger, Zebra, Neon, Royal, Legendary
    }

    private sealed record SnakeSkin(
        string Name, Color Body, Color Head, Color Outline,
        Color Highlight, Color PatternColor, SkinPattern Pattern, bool IsLegendary = false);

    private static readonly SnakeSkin[] Skins =
    [
        new("Classic Lime", Color.FromArgb(104,193,91), Color.FromArgb(183,247,122), Color.FromArgb(42,104,57), Color.FromArgb(75,223,255,187), Color.FromArgb(165,239,104), SkinPattern.Plain),
        new("Ruby Fang", Color.FromArgb(191,48,66), Color.FromArgb(255,112,120), Color.FromArgb(103,24,39), Color.FromArgb(255,190,190), Color.FromArgb(255,205,210), SkinPattern.Stripes),
        new("Ocean Wave", Color.FromArgb(38,139,191), Color.FromArgb(103,211,245), Color.FromArgb(16,70,111), Color.FromArgb(180,233,255), Color.FromArgb(145,232,255), SkinPattern.Waves),
        new("Arctic Frost", Color.FromArgb(142,207,224), Color.FromArgb(218,249,255), Color.FromArgb(61,120,151), Color.FromArgb(255,255,255), Color.FromArgb(236,255,255), SkinPattern.Frost),
        new("Solar Gold", Color.FromArgb(218,157,41), Color.FromArgb(255,220,105), Color.FromArgb(117,74,18), Color.FromArgb(255,244,177), Color.FromArgb(255,231,142), SkinPattern.Stars),
        new("Cyber Neon", Color.FromArgb(32,205,178), Color.FromArgb(115,255,224), Color.FromArgb(7,91,90), Color.FromArgb(190,255,245), Color.FromArgb(77,255,226), SkinPattern.Neon),
        new("Toxic Slime", Color.FromArgb(115,190,43), Color.FromArgb(193,255,75), Color.FromArgb(45,91,22), Color.FromArgb(228,255,146), Color.FromArgb(211,255,74), SkinPattern.Dots),
        new("Purple Haze", Color.FromArgb(131,86,190), Color.FromArgb(193,139,255), Color.FromArgb(61,38,111), Color.FromArgb(230,204,255), Color.FromArgb(213,161,255), SkinPattern.Stars),
        new("Sunset Blaze", Color.FromArgb(224,91,52), Color.FromArgb(255,166,81), Color.FromArgb(117,43,38), Color.FromArgb(255,211,148), Color.FromArgb(255,194,104), SkinPattern.Flames),
        new("Tiger Rush", Color.FromArgb(221,133,37), Color.FromArgb(255,190,76), Color.FromArgb(82,48,26), Color.FromArgb(255,229,160), Color.FromArgb(39,28,26), SkinPattern.Tiger),
        new("Zebra Stripe", Color.FromArgb(218,224,229), Color.FromArgb(255,255,255), Color.FromArgb(55,62,72), Color.FromArgb(255,255,255), Color.FromArgb(54,63,76), SkinPattern.Zebra),
        new("Mint Chip", Color.FromArgb(77,177,145), Color.FromArgb(154,238,195), Color.FromArgb(26,93,75), Color.FromArgb(218,255,228), Color.FromArgb(222,255,233), SkinPattern.Dots),
        new("Candy Pop", Color.FromArgb(231,99,164), Color.FromArgb(255,174,213), Color.FromArgb(124,42,94), Color.FromArgb(255,223,241), Color.FromArgb(255,244,255), SkinPattern.Candy),
        new("Lava Core", Color.FromArgb(184,58,35), Color.FromArgb(255,118,49), Color.FromArgb(75,25,24), Color.FromArgb(255,194,104), Color.FromArgb(255,224,93), SkinPattern.Flames),
        new("Galaxy Dust", Color.FromArgb(72,70,153), Color.FromArgb(154,137,255), Color.FromArgb(34,28,93), Color.FromArgb(226,217,255), Color.FromArgb(255,221,135), SkinPattern.Galaxy),
        new("Volt Strike", Color.FromArgb(62,120,217), Color.FromArgb(144,210,255), Color.FromArgb(26,52,122), Color.FromArgb(221,244,255), Color.FromArgb(255,237,99), SkinPattern.Lightning),
        new("Jungle Camo", Color.FromArgb(91,119,57), Color.FromArgb(166,187,91), Color.FromArgb(39,65,36), Color.FromArgb(202,222,143), Color.FromArgb(44,69,40), SkinPattern.Camo),
        new("Rose Quartz", Color.FromArgb(203,110,142), Color.FromArgb(255,188,206), Color.FromArgb(104,48,78), Color.FromArgb(255,229,238), Color.FromArgb(255,226,239), SkinPattern.Coral),
        new("Diamond Skin", Color.FromArgb(100,183,205), Color.FromArgb(208,251,255), Color.FromArgb(42,103,133), Color.FromArgb(255,255,255), Color.FromArgb(225,255,255), SkinPattern.Honeycomb),
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

    private readonly System.Windows.Forms.Timer _animationTimer = new() { Interval = 16 };
    private readonly Random _random = new();
    private readonly List<Point> _snake = [];
    private readonly Point[] _previousSnake = new Point[BoardCells];
    private int _previousSnakeCount;
    private Point _food;
    private Point? _goldenFood;

    private GameState _state = GameState.Ready;
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
    private bool _wrappedThisStep;
    private bool _scoreSaveFailed;
    private bool _isFullscreen;
    private bool _showSkinPicker;
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
    private readonly Rectangle _skinPickerClose = new(686, 176, 34, 30);
    private readonly Rectangle _boardStatus = new(660, 119, 94, 24);
    private readonly Rectangle _overlayButton = new(287, 462, 240, 52);
    private readonly Rectangle _lobbyStartButton = new(245, 500, 280, 58);
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
    private readonly Rectangle _fullscreenButton = new(1030, 19, 34, 30);
    private readonly Rectangle _minimizeButton = new(1070, 19, 34, 30);
    private readonly Rectangle _closeButton = new(1110, 19, 34, 30);

    private Point _mousePoint = new(-1, -1);
    private Point _dragOffset;
    private bool _dragging;
    private int _hoverId;

    private readonly Font _smallFont = new("Segoe UI", 8, FontStyle.Regular);
    private readonly Font _labelFont = new("Segoe UI", 8, FontStyle.Bold);
    private readonly Font _scoreFont = new("Segoe UI", 34, FontStyle.Bold);
    private readonly Font _numberFont = new("Segoe UI", 22, FontStyle.Bold);
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
        _selectedSkin = LoadSelectedSkin();
        ApplySkin(_selectedSkin);
        ResetSnake();
        _appleSprite = CreateAppleSprite(false);
        _goldAppleSprite = CreateAppleSprite(true);
        BuildChrome();

        _animationTimer.Tick += (_, _) => UpdateGameFrame();
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
        g.DrawImageUnscaled(_chrome, 0, 0);
        g.Transform = _viewMatrix;

        if (_state == GameState.Ready)
        {
            DrawLobby(g);
            if (_showSkinPicker)
                DrawSkinPicker(g);
            DrawWindowButton(g, _fullscreenButton, _isFullscreen ? "❐" : "□", _hoverId == 13);
            DrawWindowButton(g, _minimizeButton, "–", _hoverId == 1);
            DrawWindowButton(g, _closeButton, "×", _hoverId == 2);
            return;
        }

        Rectangle clip = ClientToDesign(e.ClipRectangle);
        if (clip.IntersectsWith(_field))
            DrawBoardDynamic(g);

        if (clip.IntersectsWith(_rightPanel))
            DrawDashboard(g);

        if (clip.IntersectsWith(_fullscreenButton))
            DrawWindowButton(g, _fullscreenButton, _isFullscreen ? "❐" : "□", _hoverId == 13);
        if (clip.IntersectsWith(_minimizeButton))
            DrawWindowButton(g, _minimizeButton, "–", _hoverId == 1);
        if (clip.IntersectsWith(_closeButton))
            DrawWindowButton(g, _closeButton, "×", _hoverId == 2);
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

        StaticText(g, "SCORE", new Rectangle(844, 139, 100, 16), 7, FontStyle.Bold, Muted);
        StaticText(g, "BEST", new Rectangle(1020, 139, 70, 16), 7, FontStyle.Bold, Muted);
        using (var divider = new Pen(Border, 1))
            g.DrawLine(divider, 1004, 144, 1004, 232);

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

    private void DrawLobby(Graphics g)
    {
        using (var background = new LinearGradientBrush(
                   new Rectangle(0, 0, DesignWidth, DesignHeight),
                   Color.FromArgb(14, 25, 28), Background, 90f))
            g.FillRectangle(background, 0, 0, DesignWidth, DesignHeight);

        using (var glow = new SolidBrush(Color.FromArgb(22, 123, 210, 105)))
            g.FillEllipse(glow, 20, 360, 620, 520);

        using (var line = new Pen(Border, 1))
        {
            g.DrawLine(line, 0, 70, DesignWidth, 70);
            g.DrawLine(line, 32, 724, 1128, 724);
        }

        FillRound(g, new Rectangle(34, 18, 34, 34), 11, Green);
        StaticText(g, "S", new Rectangle(34, 17, 34, 35), 18, FontStyle.Bold,
            Background, StringAlignment.Center);
        StaticText(g, "SNAKE", new Rectangle(80, 15, 105, 23), 14, FontStyle.Bold, White);
        StaticText(g, "ARCADE", new Rectangle(81, 38, 100, 15), 7, FontStyle.Bold, Muted);
        FillRound(g, new Rectangle(864, 24, 7, 7), 4, Green);
        StaticText(g, "OFFLINE  ARCADE", new Rectangle(879, 17, 180, 20),
            8, FontStyle.Bold, Muted);

        DrawStaticPanel(g, new Rectangle(32, 100, 750, 600));
        DrawStaticPanel(g, _rightPanel);
        DrawLabel(g, "WELCOME BACK", _labelFont, _greenBrush,
            new Rectangle(76, 150, 300, 24), _leftFormat);
        DrawLabel(g, "THE SNAKE", _welcomeFont, _whiteBrush,
            new Rectangle(72, 194, 600, 68), _leftFormat);
        DrawLabel(g, "A classic arcade challenge, rebuilt for your next high score.",
            _welcomeSmallFont, _mutedBrush, new Rectangle(78, 270, 610, 30), _leftFormat);

        DrawStaticPanel(g, new Rectangle(78, 340, 190, 104), PanelLight);
        DrawStaticPanel(g, new Rectangle(282, 340, 190, 104), PanelLight);
        DrawStaticPanel(g, new Rectangle(486, 340, 238, 104), PanelLight);
        DrawLabel(g, "PERSONAL BEST", _labelFont, _mutedBrush,
            new Rectangle(96, 355, 160, 20), _leftFormat);
        DrawLabel(g, _bestScore.ToString("0000"), _scoreFont, _greenBrush,
            new Rectangle(96, 377, 155, 55), _leftFormat);
        DrawLabel(g, "20 CUSTOM SKINS", _labelFont, _mutedBrush,
            new Rectangle(300, 355, 160, 20), _leftFormat);
        DrawLabel(g, Skins[_selectedSkin].Name, _buttonFont, _whiteBrush,
            new Rectangle(300, 385, 160, 28), _leftFormat);
        DrawLabel(g, "3 GAME MODES", _labelFont, _mutedBrush,
            new Rectangle(504, 355, 190, 20), _leftFormat);
        DrawLabel(g, "CLASSIC  ·  WRAP  ·  RUSH", _buttonFont, _whiteBrush,
            new Rectangle(504, 385, 205, 28), _leftFormat);

        DrawActionButton(g, _lobbyStartButton, "START GAME", true);
        DrawLabel(g, "WASD / ARROWS  ·  SPACE TO PAUSE",
            _labelFont, _mutedBrush, new Rectangle(78, 582, 420, 22), _leftFormat);

        DrawLabel(g, "GAME SETUP", _labelFont, _mutedBrush,
            new Rectangle(842, 124, 180, 20), _leftFormat);
        DrawLabel(g, "GAME MODE", _labelFont, _mutedBrush,
            new Rectangle(842, 282, 180, 18), _leftFormat);
        DrawChoice(g, _classicButton, "CLASSIC", _mode == GameMode.Classic);
        DrawChoice(g, _wrapButton, "WRAP", _mode == GameMode.Wrap);
        DrawChoice(g, _rushButton, "RUSH", _mode == GameMode.Rush);

        DrawLabel(g, "DIFFICULTY", _labelFont, _mutedBrush,
            new Rectangle(842, 402, 180, 18), _leftFormat);
        DrawChoice(g, _easyButton, "EASY", _difficulty == Difficulty.Easy);
        DrawChoice(g, _normalButton, "NORMAL", _difficulty == Difficulty.Normal);
        DrawChoice(g, _hardButton, "HARD", _difficulty == Difficulty.Hard);

        DrawLabel(g, "PERSONALIZE", _labelFont, _mutedBrush,
            new Rectangle(842, 528, 180, 18), _leftFormat);
        DrawActionButton(g, _soundButton, _soundEnabled ? "♪" : "×", false);
        DrawLabel(g, "SOUND", _smallFont, _mutedBrush,
            new Rectangle(842, 552, 150, 42), _leftFormat);
        DrawSecondaryButton(g, _skinButton, $"SKINS  ·  {Skins[_selectedSkin].Name}");
        DrawSecondaryButton(g, _resetBestButton, "RESET BEST SCORE", true);
        DrawLabel(g, "READY WHEN YOU ARE", _labelFont, _greenBrush,
            new Rectangle(842, 680, 260, 18), _leftFormat);
    }

    private void DrawDashboard(Graphics g)
    {
        DrawLabel(g, _score.ToString("0000"), _scoreFont, _whiteBrush,
            new Rectangle(842, 161, 154, 60), _leftFormat);

        DrawLabel(g, _bestScore.ToString("0000"), _numberFont, _greenBrush,
            new Rectangle(1020, 172, 76, 42), _leftFormat);

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

        if (_showSkinPicker)
            DrawSkinPicker(g);
        else if (_state == GameState.Ready)
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
        DrawLabel(g, _bestScore.ToString("0000"), _numberFont, _greenBrush,
            new Rectangle(412, 383, 120, 42), _leftFormat);

        DrawActionButton(g, _overlayButton, "START GAME", true);
        DrawLabel(g, "WASD  /  ARROW KEYS     •     SPACE TO PAUSE",
            _labelFont, _mutedBrush,
            new Rectangle(180, 523, 454, 20), _centerFormat);
    }

    private void DrawPauseOrGameOver(Graphics g)
    {
        g.FillRectangle(_overlayBrush, _field);
        g.FillPath(_cardBrush, _overlayPath);

        string title = _state == GameState.Paused
            ? "PAUSED"
            : _won ? "PERFECT RUN" : "GAME OVER";

        string subtitle = _state == GameState.Paused
            ? "Your run is waiting right here."
            : _scoreSaveFailed ? "SCORE SAVE FAILED" :
              _newRecord ? "NEW PERSONAL BEST" : $"FINAL SCORE  {_score:0000}";

        DrawLabel(g, title, _heroFont, _whiteBrush,
            new Rectangle(220, 304, 374, 54), _centerFormat);
        DrawLabel(g, subtitle, _smallFont,
            _state == GameState.GameOver && _newRecord ? _goldBrush : _mutedBrush,
            new Rectangle(220, 366, 374, 25), _centerFormat);

        DrawActionButton(g, _overlayButton,
            _state == GameState.Paused ? "RESUME" : "PLAY AGAIN", true);
        DrawLabel(g, "PRESS SPACE", _labelFont, _mutedBrush,
            new Rectangle(280, 523, 254, 20), _centerFormat);
    }

    private Bitmap CreateAppleSprite(bool golden)
    {
        const int size = 96;
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppPArgb);

        using Graphics g = Graphics.FromImage(bitmap);
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
        float pulse = 1f + (float)Math.Sin(Environment.TickCount64 / 180.0) * 0.045f;
        DrawApple(g, _food, pulse, false);

        if (_goldenFood is Point golden)
        {
            bool visible = true;
            if (_goldenExpiresAt is DateTime expiresAt &&
                (expiresAt - DateTime.UtcNow).TotalSeconds <= 3)
                visible = (Environment.TickCount64 / 115) % 2 == 0;

            if (visible)
                DrawApple(g, golden, pulse, true);
        }
    }

    private void DrawApple(Graphics g, Point cell, float pulse, bool golden)
    {
        Bitmap? sprite = golden ? _goldAppleSprite : _appleSprite;
        if (sprite is null)
            return;

        float size = 29f * pulse;
        float x = _field.Left + cell.X * CellSize + CellSize / 2f - size / 2f;
        float y = _field.Top + cell.Y * CellSize + CellSize / 2f - size / 2f;
        g.DrawImage(sprite, x, y, size, size);
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

            float cellX = _wrappedThisStep && i == 0
                ? to.X
                : from.X + (to.X - from.X) * progress;
            float cellY = _wrappedThisStep && i == 0
                ? to.Y
                : from.Y + (to.Y - from.Y) * progress;

            _snakeCenters[i] = new PointF(
                _field.Left + cellX * CellSize + CellSize / 2f,
                _field.Top + cellY * CellSize + CellSize / 2f);
        }

        _snakePath.Reset();
        _snakeHighlightPath.Reset();

        int sectionStart = 0;
        for (int i = 1; i < _snake.Count; i++)
        {
            float dx = _snakeCenters[i].X - _snakeCenters[i - 1].X;
            float dy = _snakeCenters[i].Y - _snakeCenters[i - 1].Y;

            if (Math.Abs(dx) > CellSize * 1.5f || Math.Abs(dy) > CellSize * 1.5f)
            {
                AddSnakeSection(g, sectionStart, i - 1);
                sectionStart = i;
            }
        }

        AddSnakeSection(g, sectionStart, _snake.Count - 1);

        if (_snakePath.PointCount > 0)
        {
            g.DrawPath(_snakeOutlinePen, _snakePath);
            g.DrawPath(_snakeBodyPen, _snakePath);
            if (_snakeHighlightPath.PointCount > 0)
                g.DrawPath(_snakeHighlightPen, _snakeHighlightPath);
        }

        if (Skins[_selectedSkin].Pattern != SkinPattern.Plain)
            DrawSkinPattern(g);

        if (Skins[_selectedSkin].IsLegendary)
            DrawLegendaryAura(g, _snakeCenters[0]);

        DrawSnakeHead(g, _snakeCenters[0]);
    }

    private void AddSnakeSection(Graphics g, int start, int end)
    {
        if (end < start)
            return;

        if (end == start)
        {
            PointF center = _snakeCenters[start];
            const float diameter = CellSize - 6;
            g.FillEllipse(_snakeBrush, center.X - diameter / 2f, center.Y - diameter / 2f,
                diameter, diameter);
            return;
        }

        _snakePath.StartFigure();
        _snakeHighlightPath.StartFigure();

        for (int i = start + 1; i <= end; i++)
        {
            PointF previous = _snakeCenters[i - 1];
            PointF current = _snakeCenters[i];
            _snakePath.AddLine(previous, current);
            _snakeHighlightPath.AddLine(
                new PointF(previous.X - 1.2f, previous.Y - 3.2f),
                new PointF(current.X - 1.2f, current.Y - 3.2f));
        }
    }

    private void DrawSnakeHead(Graphics g, PointF center)
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
        switch (_direction)
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

    private void DrawSkinPicker(Graphics g)
    {
        g.FillRectangle(_overlayBrush, _field);
        FillRound(g, _skinPickerPanel, 16, Color.FromArgb(250, 17, 26, 32));
        StrokeRound(g, _skinPickerPanel, 16, Color.FromArgb(76, 103, 106));

        DrawLabel(g, "CHOOSE YOUR SNAKE", _heroFont, _whiteBrush,
            new Rectangle(100, 174, 430, 35), _leftFormat);
        DrawLabel(g, "20 custom skins  ·  1 Legendary", _smallFont, _mutedBrush,
            new Rectangle(102, 207, 430, 18), _leftFormat);
        DrawWindowButton(g, _skinPickerClose, "×", _skinPickerClose.Contains(_mousePoint));

        for (int i = 0; i < Skins.Length; i++)
        {
            Rectangle tile = SkinTileRect(i);
            SnakeSkin skin = Skins[i];
            bool selected = i == _selectedSkin;
            bool hover = tile.Contains(_mousePoint);

            FillRound(g, tile, 9, selected
                ? Color.FromArgb(37, 68, 48)
                : hover ? Color.FromArgb(34, 48, 54) : PanelLight);
            StrokeRound(g, tile, 9, selected
                ? Green
                : skin.IsLegendary ? Gold : Border);

            using var preview = new Pen(skin.Body, 8)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            g.DrawLine(preview, tile.X + 15, tile.Y + 19, tile.X + 32, tile.Y + 19);
            using var head = new SolidBrush(skin.Head);
            g.FillEllipse(head, tile.X + 27, tile.Y + 11, 16, 16);

            DrawLabel(g, skin.Name, _buttonFont,
                skin.IsLegendary ? _goldBrush : selected ? _greenBrush : _whiteBrush,
                new Rectangle(tile.X + 48, tile.Y + 5, tile.Width - 54, 25), _leftFormat);

            if (skin.IsLegendary)
                DrawLabel(g, "LEGENDARY", _smallFont, _goldBrush,
                    new Rectangle(tile.X + 48, tile.Y + 27, tile.Width - 54, 16), _leftFormat);
            else if (selected)
                DrawLabel(g, "EQUIPPED", _smallFont, _greenBrush,
                    new Rectangle(tile.X + 48, tile.Y + 27, tile.Width - 54, 16), _leftFormat);
        }

        DrawLabel(g, "Select a skin to equip it. Your choice is saved on this PC.",
            _smallFont, _mutedBrush, new Rectangle(100, 586, 600, 20), _leftFormat);
    }

    private static Rectangle SkinTileRect(int index)
    {
        int column = index % 4;
        int row = index / 4;
        return new Rectangle(94 + column * 157, 228 + row * 59, 149, 51);
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

    private void DrawSkinPattern(Graphics g)
    {
        SnakeSkin skin = Skins[_selectedSkin];
        _skinPatternPen.Color = skin.PatternColor;
        float phase = Environment.TickCount64 / 180f;

        int stride = skin.Pattern is SkinPattern.Dots or SkinPattern.Stars or SkinPattern.Galaxy ? 2 : 3;
        for (int i = 1; i < _snake.Count - 1; i += stride)
        {
            PointF p = _snakeCenters[i];
            PointF before = _snakeCenters[i - 1];
            PointF after = _snakeCenters[i + 1];

            float dx = after.X - before.X;
            float dy = after.Y - before.Y;
            float length = Math.Max(1f, MathF.Sqrt(dx * dx + dy * dy));
            float nx = -dy / length;
            float ny = dx / length;

            switch (skin.Pattern)
            {
                case SkinPattern.Stripes:
                case SkinPattern.Tiger:
                case SkinPattern.Zebra:
                case SkinPattern.Candy:
                case SkinPattern.Neon:
                    g.DrawLine(_skinPatternPen,
                        p.X - nx * 7, p.Y - ny * 7, p.X + nx * 7, p.Y + ny * 7);
                    break;

                case SkinPattern.Dots:
                case SkinPattern.Camo:
                case SkinPattern.Coral:
                    g.DrawEllipse(_skinPatternPen, p.X - 3.5f, p.Y - 3.5f, 7, 7);
                    break;

                case SkinPattern.Waves:
                    g.DrawArc(_skinPatternPen, p.X - 8, p.Y - 5, 16, 10,
                        (float)(phase * 20 % 180), 145);
                    break;

                case SkinPattern.Lightning:
                case SkinPattern.Circuit:
                    g.DrawLine(_skinPatternPen, p.X - 5, p.Y - 4, p.X + 1, p.Y);
                    g.DrawLine(_skinPatternPen, p.X + 1, p.Y, p.X - 2, p.Y + 5);
                    break;

                case SkinPattern.Scales:
                case SkinPattern.Honeycomb:
                    g.DrawEllipse(_skinPatternPen, p.X - 5, p.Y - 4, 10, 8);
                    break;

                case SkinPattern.Flames:
                    g.DrawLine(_skinPatternPen, p.X - 4, p.Y + 4, p.X, p.Y - 5);
                    g.DrawLine(_skinPatternPen, p.X, p.Y - 5, p.X + 4, p.Y + 2);
                    break;

                case SkinPattern.Frost:
                case SkinPattern.Stars:
                case SkinPattern.Prism:
                case SkinPattern.Galaxy:
                case SkinPattern.Royal:
                    g.DrawLine(_skinPatternPen, p.X - 4, p.Y, p.X + 4, p.Y);
                    g.DrawLine(_skinPatternPen, p.X, p.Y - 4, p.X, p.Y + 4);
                    break;
            }
        }

    }

    private void DrawLegendaryAura(Graphics g, PointF head)
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

        for (int i = 3; i < _snake.Count; i += 7)
        {
            PointF p = _snakeCenters[i];
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
        _wrappedThisStep = false;
        _goldenFood = null;
        _goldenExpiresAt = null;
        PlaceFood();
    }

    private void StartGame()
    {
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

        bool wrapped = false;
        if (_mode == GameMode.Wrap)
        {
            wrapped = next.X < 0 || next.X >= Columns || next.Y < 0 || next.Y >= Rows;
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

        _wrappedThisStep = wrapped;
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
        if (_state == GameState.Ready || _state == GameState.GameOver)
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
        if (_showSkinPicker)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _showSkinPicker = false;
                InvalidateDesign(_field);
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
        if (_showSkinPicker)
        {
            if (_skinPickerClose.Contains(point)) return 16;
            for (int i = 0; i < Skins.Length; i++)
                if (SkinTileRect(i).Contains(point)) return 100 + i;
            return 0;
        }

        if (_closeButton.Contains(point)) return 2;
        if (_state == GameState.Ready && _lobbyStartButton.Contains(point)) return 6;
        if (_minimizeButton.Contains(point)) return 1;
        if (_fullscreenButton.Contains(point)) return 13;
        if (_skinButton.Contains(point)) return 14;
        if (_resetBestButton.Contains(point)) return 15;
        if (_mainButton.Contains(point)) return 3;
        if (_soundButton.Contains(point)) return 4;
        if (_restartButton.Contains(point)) return 5;
        if (_overlayButton.Contains(point) && _state != GameState.Playing) return 6;
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

        return id switch
        {
            1 => _minimizeButton,
            2 => _closeButton,
            3 => _mainButton,
            4 => _soundButton,
            5 => _restartButton,
            6 => _state == GameState.Ready ? _lobbyStartButton : _overlayButton,
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
            _ => Rectangle.Empty
        };
    }

    private void HandleMouseDown(object? sender, MouseEventArgs e)
    {
        Focus();

        Point point = ClientToDesign(e.Location);
        if (e.Button == MouseButtons.Left && point.Y < 70 && !_isFullscreen &&
            !_closeButton.Contains(point) && !_minimizeButton.Contains(point) &&
            !_fullscreenButton.Contains(point))
        {
            _dragging = true;
            _dragOffset = e.Location;
        }
    }

    private void HandleMouseClick(object? sender, MouseEventArgs e)
    {
        Point point = ClientToDesign(e.Location);

        if (_showSkinPicker)
        {
            if (_skinPickerClose.Contains(point))
            {
                _showSkinPicker = false;
                InvalidateDesign(_field);
                return;
            }

            for (int i = 0; i < Skins.Length; i++)
            {
                if (!SkinTileRect(i).Contains(point))
                    continue;

                ApplySkin(i);
                SaveSelectedSkin();
                _showSkinPicker = false;
                InvalidateDesign(_field);
                return;
            }

            return;
        }

        if (_closeButton.Contains(point))
        {
            Close();
            return;
        }

        if (_fullscreenButton.Contains(point))
        {
            ToggleFullscreen();
            return;
        }

        if (_skinButton.Contains(point))
        {
            if (_state == GameState.Playing)
                SetState(GameState.Paused);

            _showSkinPicker = true;
            InvalidateDesign(_field);
            return;
        }

        if (_resetBestButton.Contains(point))
        {
            ResetBestScore();
            return;
        }

        if (_minimizeButton.Contains(point))
        {
            WindowState = FormWindowState.Minimized;
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

        if ((_state == GameState.Ready && _lobbyStartButton.Contains(point)) ||
            _mainButton.Contains(point) || _overlayButton.Contains(point))
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
