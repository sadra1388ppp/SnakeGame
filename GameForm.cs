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
    private Point[] _previousSnake = [];
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
    private readonly SolidBrush _snakeBrush = new(Color.FromArgb(104, 193, 91));
    private readonly SolidBrush _headBrush = new(Color.FromArgb(183, 247, 122));
    private readonly SolidBrush _headShadowBrush = new(Color.FromArgb(45, 138, 233, 104));
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
        MinimumSize = Size;
        MaximumSize = Size;
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
        ResetSnake();
        UpdateViewTransform();
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
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Transform = _viewMatrix;
        g.DrawImageUnscaled(_chrome, 0, 0);

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
        _viewScale = _isFullscreen
            ? Math.Min(ClientSize.Width / (float)DesignWidth, ClientSize.Height / (float)DesignHeight)
            : 1f;
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
            MinimumSize = new Size(DesignWidth, DesignHeight);
            MaximumSize = new Size(DesignWidth, DesignHeight);
        }

        UpdateViewTransform();
        Invalidate();
    }

    private void BuildChrome()
    {
        _chrome?.Dispose();
        _chrome = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);

        using Graphics g = Graphics.FromImage(_chrome);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using (var background = new LinearGradientBrush(
                   new Rectangle(0, 0, Width, Height),
                   Color.FromArgb(13, 21, 25), Background, 90f))
            g.FillRectangle(background, ClientRectangle);

        using (var ambientGlow = new SolidBrush(Color.FromArgb(15, 132, 205, 105)))
            g.FillEllipse(ambientGlow, -210, 430, 500, 500);

        using (var topLine = new Pen(Border, 1))
        {
            g.DrawLine(topLine, 0, 70, Width, 70);
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

        StaticText(g, "SPACE  PAUSE", new Rectangle(62, 650, 130, 20), 7, FontStyle.Bold,
            Color.FromArgb(95, 118, 118));
        StaticText(g, "WASD  /  ARROWS", new Rectangle(201, 650, 160, 20), 7, FontStyle.Bold,
            Color.FromArgb(95, 118, 118));
        StaticText(g, "GOLDEN FRUIT  +30", new Rectangle(540, 650, 210, 20), 7, FontStyle.Bold,
            Color.FromArgb(133, 115, 78));
    }

    private void DrawDashboard(Graphics g)
    {
        DrawLabel(g, _score.ToString("0000"), _scoreFont, _whiteBrush,
            new Rectangle(842, 161, 154, 60), _leftFormat);

        DrawLabel(g, Math.Max(_bestScore, _score).ToString("0000"), _numberFont, _greenBrush,
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
            Point from = i < _previousSnake.Length ? _previousSnake[i] : _snake[i];
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

        _previousSnake = _snake.ToArray();
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
        _previousSnake = _snake.ToArray();
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

    private Rectangle HoverRectangle(int id) => id switch
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
        _ => Rectangle.Empty
    };

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
