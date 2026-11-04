using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Media;

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
    private int _combo;
    private DateTime _lastFoodAt;
    private bool _soundEnabled = true;
    private bool _won;
    private bool _newRecord;

    private Bitmap? _chrome;
    private readonly GraphicsPath _overlayPath = RoundPath(new Rectangle(193, 267, 428, 245), 20);

    private readonly Rectangle _boardCard = new(32, 100, 750, 600);
    private readonly Rectangle _field = new(62, 150, Columns * CellSize, Rows * CellSize);
    private readonly Rectangle _rightPanel = new(802, 100, 326, 600);
    private readonly Rectangle _scoreArea = new(824, 124, 280, 132);
    private readonly Rectangle _statsArea = new(824, 268, 280, 78);
    private readonly Rectangle _modeArea = new(824, 358, 280, 105);
    private readonly Rectangle _difficultyArea = new(824, 475, 280, 105);
    private readonly Rectangle _actionArea = new(824, 592, 280, 88);
    private readonly Rectangle _boardStatus = new(660, 119, 94, 24);
    private readonly Rectangle _overlayButton = new(292, 431, 230, 46);
    private readonly Rectangle _mainButton = new(838, 622, 174, 42);
    private readonly Rectangle _soundButton = new(1020, 622, 38, 42);
    private readonly Rectangle _restartButton = new(1066, 622, 38, 42);
    private readonly Rectangle _classicButton = new(838, 414, 82, 32);
    private readonly Rectangle _wrapButton = new(926, 414, 82, 32);
    private readonly Rectangle _rushButton = new(1014, 414, 82, 32);
    private readonly Rectangle _easyButton = new(838, 531, 82, 32);
    private readonly Rectangle _normalButton = new(926, 531, 82, 32);
    private readonly Rectangle _hardButton = new(1014, 531, 82, 32);
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
    private readonly Font _buttonFont = new("Segoe UI", 8, FontStyle.Bold);
    private readonly Font _iconFont = new("Segoe UI Symbol", 17, FontStyle.Regular);

    private readonly SolidBrush _whiteBrush = new(White);
    private readonly SolidBrush _darkBrush = new(Color.FromArgb(12, 23, 17));
    private readonly SolidBrush _mutedBrush = new(Muted);
    private readonly SolidBrush _greenBrush = new(Green);
    private readonly SolidBrush _goldBrush = new(Gold);
    private readonly SolidBrush _snakeBrush = new(Color.FromArgb(104, 193, 91));
    private readonly SolidBrush _headBrush = new(Color.FromArgb(183, 247, 122));
    private readonly SolidBrush _eyeBrush = new(Color.FromArgb(10, 25, 20));
    private readonly SolidBrush _foodBrush = new(Color.FromArgb(255, 103, 105));
    private readonly SolidBrush _foodGlowBrush = new(Color.FromArgb(38, 255, 103, 105));
    private readonly SolidBrush _goldGlowBrush = new(Color.FromArgb(42, 255, 194, 83));
    private readonly SolidBrush _overlayBrush = new(Color.FromArgb(218, 5, 10, 13));
    private readonly SolidBrush _cardBrush = new(Color.FromArgb(248, 19, 29, 35));
    private readonly Pen _goldPen = new(Color.FromArgb(235, Gold), 2);
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
        g.DrawImageUnscaled(_chrome, 0, 0);

        Rectangle clip = e.ClipRectangle;
        if (clip.IntersectsWith(_field))
            DrawBoardDynamic(g);

        if (clip.IntersectsWith(_rightPanel))
            DrawDashboard(g);

        if (clip.IntersectsWith(_minimizeButton))
            DrawWindowButton(g, _minimizeButton, "–", _hoverId == 1);
        if (clip.IntersectsWith(_closeButton))
            DrawWindowButton(g, _closeButton, "×", _hoverId == 2);
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
        StaticDrawLabel(g, "S", new Rectangle(34, 17, 34, 35), 18, FontStyle.Bold,
            Background, StringAlignment.Center);

        StaticDrawLabel(g, "SNAKE", new Rectangle(80, 15, 105, 23), 14, FontStyle.Bold, White);
        StaticDrawLabel(g, "ARCADE", new Rectangle(81, 38, 100, 15), 7, FontStyle.Bold, Muted);

        FillRound(g, new Rectangle(864, 24, 7, 7), 4, Green);
        StaticDrawLabel(g, "OFFLINE", new Rectangle(879, 17, 80, 20), 8, FontStyle.Bold, Muted);
        StaticDrawLabel(g, "NO LOGIN  ·  NO ADS", new Rectangle(879, 37, 145, 15), 7, FontStyle.Regular,
            Color.FromArgb(90, 111, 115));

        DrawStaticPanel(g, _boardCard);
        DrawStaticPanel(g, _rightPanel);
        DrawStaticPanel(g, _scoreArea, PanelLight);
        DrawStaticPanel(g, new Rectangle(824, 268, 134, 78), PanelLight);
        DrawStaticPanel(g, new Rectangle(970, 268, 134, 78), PanelLight);
        DrawStaticPanel(g, _modeArea, PanelLight);
        DrawStaticPanel(g, _difficultyArea, PanelLight);
        DrawStaticPanel(g, _actionArea, PanelLight);

        StaticDrawLabel(g, "PLAYFIELD", new Rectangle(62, 116, 180, 22), 8, FontStyle.Bold, Muted);
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

        StaticDrawLabel(g, "SCORE", new Rectangle(844, 139, 100, 16), 7, FontStyle.Bold, Muted);
        StaticDrawLabel(g, "BEST", new Rectangle(1020, 139, 70, 16), 7, FontStyle.Bold, Muted);
        using (var divider = new Pen(Border, 1))
            g.DrawLine(divider, 1004, 144, 1004, 232);

        StaticDrawLabel(g, "LEVEL", new Rectangle(842, 279, 100, 18), 7, FontStyle.Bold, Muted);
        StaticDrawLabel(g, "COMBO", new Rectangle(988, 279, 100, 18), 7, FontStyle.Bold, Muted);

        StaticDrawLabel(g, "GAME MODE", new Rectangle(842, 371, 180, 17), 8, FontStyle.Bold, Muted);
        StaticDrawLabel(g, "Pick your style", new Rectangle(842, 390, 180, 16), 7, FontStyle.Regular,
            Color.FromArgb(105, 126, 128));

        StaticDrawLabel(g, "DIFFICULTY", new Rectangle(842, 488, 180, 17), 8, FontStyle.Bold, Muted);
        StaticDrawLabel(g, "Set the pace", new Rectangle(842, 507, 180, 16), 7, FontStyle.Regular,
            Color.FromArgb(105, 126, 128));

        DrawStaticButton(g, _classicButton);
        DrawStaticButton(g, _wrapButton);
        DrawStaticButton(g, _rushButton);
        DrawStaticButton(g, _easyButton);
        DrawStaticButton(g, _normalButton);
        DrawStaticButton(g, _hardButton);
        DrawStaticButton(g, _mainButton, true);
        DrawStaticButton(g, _soundButton);
        DrawStaticButton(g, _restartButton);

        StaticDrawLabel(g, "SPACE  PAUSE", new Rectangle(62, 650, 130, 20), 7, FontStyle.Bold,
            Color.FromArgb(95, 118, 118));
        StaticDrawLabel(g, "WASD  /  ARROWS", new Rectangle(201, 650, 160, 20), 7, FontStyle.Bold,
            Color.FromArgb(95, 118, 118));
        StaticDrawLabel(g, "GOLDEN FRUIT  +30", new Rectangle(540, 650, 210, 20), 7, FontStyle.Bold,
            Color.FromArgb(133, 115, 78));
    }

    private void DrawDashboard(Graphics g)
    {
        DrawLabel(g, _score.ToString("0000"), _scoreFont, _whiteBrush,
            new Rectangle(842, 161, 154, 60), _leftFormat);

        DrawLabel(g, Math.Max(_bestScore, _score).ToString("0000"), _numberFont, _greenBrush,
            new Rectangle(1020, 172, 76, 42), _leftFormat);

        int level = 1 + _score / 50;
        DrawLabel(g, level.ToString("00"), _numberFont, _whiteBrush,
            new Rectangle(842, 297, 100, 39), _leftFormat);

        string secondStat = _mode == GameMode.Rush
            ? $"{Math.Ceiling(_rushTimeLeft.TotalSeconds):00}s"
            : $"x{Math.Max(1, _combo)}";
        DrawLabel(g, secondStat, _numberFont, _mode == GameMode.Rush ? _goldBrush : _whiteBrush,
            new Rectangle(988, 297, 100, 39), _leftFormat);

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
        g.SetClip(_field);

        if (_state is GameState.Ready or GameState.GameOver or GameState.Paused)
        {
            g.FillRectangle(_overlayBrush, _field);
            g.FillPath(_cardBrush, _overlayPath);

            string title = _state switch
            {
                GameState.Paused => "PAUSED",
                GameState.GameOver => _won ? "PERFECT RUN" : "GAME OVER",
                _ => "SNAKE"
            };

            string subtitle = _state switch
            {
                GameState.Paused => "Your run is saved right here.",
                GameState.GameOver => _newRecord ? "NEW PERSONAL BEST" : $"FINAL SCORE  {_score:0000}",
                _ => "A classic arcade game. One more round?"
            };

            DrawLabel(g, title, _heroFont, _whiteBrush,
                new Rectangle(220, 304, 374, 54), _centerFormat);

            DrawLabel(g, subtitle, _smallFont,
                _state == GameState.GameOver && _newRecord ? _goldBrush : _mutedBrush,
                new Rectangle(220, 366, 374, 25), _centerFormat);

            DrawActionButton(g, _overlayButton,
                _state == GameState.Paused ? "RESUME" :
                _state == GameState.GameOver ? "PLAY AGAIN" : "START GAME", true);

            DrawLabel(g, "PRESS SPACE", _labelFont, _mutedBrush,
                new Rectangle(280, 490, 254, 20), _centerFormat);
        }
        else
        {
            DrawFood(g);
            DrawSnake(g);
        }

        g.Restore(state);
    }

    private void DrawFood(Graphics g)
    {
        DateTime now = DateTime.UtcNow;
        float pulse = 1f + (float)Math.Sin(now.Millisecond / 1000f * Math.PI * 2) * 0.08f;

        RectangleF regular = CellBounds(_food, 13 * pulse);
        g.FillEllipse(_foodGlowBrush, regular.X - 7, regular.Y - 7, regular.Width + 14, regular.Height + 14);
        g.FillEllipse(_foodBrush, regular);

        if (_goldenFood is not Point golden)
            return;

        RectangleF special = CellBounds(golden, 15 * pulse);
        g.FillEllipse(_goldGlowBrush, special.X - 8, special.Y - 8,
            special.Width + 16, special.Height + 16);
        g.FillEllipse(_goldBrush, special);

        float cx = special.X + special.Width / 2;
        float cy = special.Y + special.Height / 2;
        g.DrawLine(_goldPen, cx - 5, cy, cx + 5, cy);
        g.DrawLine(_goldPen, cx, cy - 5, cx, cy + 5);
    }

    private void DrawSnake(Graphics g)
    {
        float progress = _state == GameState.Playing ? CurrentProgress() : 1f;

        for (int i = _snake.Count - 1; i >= 0; i--)
        {
            Point from = i < _previousSnake.Length ? _previousSnake[i] : _snake[i];
            Point to = _snake[i];

            float x = _field.Left + (from.X + (to.X - from.X) * progress) * CellSize;
            float y = _field.Top + (from.Y + (to.Y - from.Y) * progress) * CellSize;
            float inset = i == 0 ? 2.5f : 3.5f;
            RectangleF part = new(x + inset, y + inset,
                CellSize - inset * 2, CellSize - inset * 2);

            g.FillEllipse(i == 0 ? _headBrush : _snakeBrush, part);
        }

        if (_snake.Count == 0)
            return;

        Point oldHead = _previousSnake.Length > 0 ? _previousSnake[0] : _snake[0];
        Point newHead = _snake[0];
        float headX = _field.Left + (oldHead.X + (newHead.X - oldHead.X) * progress) * CellSize;
        float headY = _field.Top + (oldHead.Y + (newHead.Y - oldHead.Y) * progress) * CellSize;

        float eye1X;
        float eye1Y;
        float eye2X;
        float eye2Y;

        switch (_direction)
        {
            case Direction.Up:
                eye1X = headX + 7; eye1Y = headY + 6;
                eye2X = headX + 16; eye2Y = headY + 6;
                break;
            case Direction.Down:
                eye1X = headX + 7; eye1Y = headY + 16;
                eye2X = headX + 16; eye2Y = headY + 16;
                break;
            case Direction.Left:
                eye1X = headX + 6; eye1Y = headY + 7;
                eye2X = headX + 6; eye2Y = headY + 16;
                break;
            default:
                eye1X = headX + 16; eye1Y = headY + 7;
                eye2X = headX + 16; eye2Y = headY + 16;
                break;
        }

        g.FillEllipse(_eyeBrush, eye1X, eye1Y, 3, 3);
        g.FillEllipse(_eyeBrush, eye2X, eye2Y, 3, 3);
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
        _combo = 0;
        _lastFoodAt = default;
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

        return Math.Max(48, start - (_score / 50 * 4));
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
                Invalidate(_statsArea);
            }
        }

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
        Invalidate(_field);
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
            if (_lastFoodAt != default && (now - _lastFoodAt).TotalSeconds <= 2.8)
                _combo = Math.Min(5, _combo + 1);
            else
                _combo = 1;

            _lastFoodAt = now;

            _score += eatingGolden ? 30 : 10 + (_combo - 1) * 2;
            _newRecord = _score > _bestScore;

            PlaySound(eatingGolden ? 2 : 0);

            if (eatingGolden)
            {
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

            Invalidate(_statsArea);
            Invalidate(_scoreArea);
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
        _newRecord = _score > _bestScore;

        if (_newRecord)
        {
            _bestScore = _score;
            SaveBestScore();
        }

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
        Invalidate(_rightPanel);
    }

    private void SetMode(GameMode mode)
    {
        if (_state is GameState.Playing or GameState.Paused)
            return;

        _mode = mode;
        _rushTimeLeft = TimeSpan.FromSeconds(60);
        _lastDisplayedSecond = 60;
        Invalidate(_rightPanel);
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
            case Keys.Escape:
                TogglePause();
                e.Handled = true;
                break;
        }
    }

    private void HandleMouseMove(object? sender, MouseEventArgs e)
    {
        _mousePoint = e.Location;

        if (_dragging)
        {
            Location = new Point(Cursor.Position.X - _dragOffset.X,
                Cursor.Position.Y - _dragOffset.Y);
            return;
        }

        UpdateHover(e.Location);
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
            Invalidate(oldRect);
        if (!newRect.IsEmpty)
            Invalidate(newRect);
    }

    private int GetHoverId(Point point)
    {
        if (_closeButton.Contains(point)) return 2;
        if (_minimizeButton.Contains(point)) return 1;
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
        _ => Rectangle.Empty
    };

    private void HandleMouseDown(object? sender, MouseEventArgs e)
    {
        Focus();

        if (e.Button == MouseButtons.Left && e.Y < 70 &&
            !_closeButton.Contains(e.Location) && !_minimizeButton.Contains(e.Location))
        {
            _dragging = true;
            _dragOffset = e.Location;
        }
    }

    private void HandleMouseClick(object? sender, MouseEventArgs e)
    {
        Point point = e.Location;

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

        if (_classicButton.Contains(point)) SetMode(GameMode.Classic);
        else if (_wrapButton.Contains(point)) SetMode(GameMode.Wrap);
        else if (_rushButton.Contains(point)) SetMode(GameMode.Rush);

        if (_easyButton.Contains(point)) SetDifficulty(Difficulty.Easy);
        else if (_normalButton.Contains(point)) SetDifficulty(Difficulty.Normal);
        else if (_hardButton.Contains(point)) SetDifficulty(Difficulty.Hard);

        if (_soundButton.Contains(point))
        {
            _soundEnabled = !_soundEnabled;
            Invalidate(_soundButton);
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
        try
        {
            string path = BestScorePath();
            return File.Exists(path) && int.TryParse(File.ReadAllText(path), out int value)
                ? Math.Max(0, value)
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    private void SaveBestScore()
    {
        try
        {
            string path = BestScorePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, _bestScore.ToString());
        }
        catch
        {
            // A failed save must not interrupt the game.
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
        _buttonFont.Dispose();
        _iconFont.Dispose();

        _whiteBrush.Dispose();
        _darkBrush.Dispose();
        _mutedBrush.Dispose();
        _greenBrush.Dispose();
        _goldBrush.Dispose();
        _snakeBrush.Dispose();
        _headBrush.Dispose();
        _eyeBrush.Dispose();
        _foodBrush.Dispose();
        _foodGlowBrush.Dispose();
        _goldGlowBrush.Dispose();
        _overlayBrush.Dispose();
        _cardBrush.Dispose();
        _goldPen.Dispose();

        _leftFormat.Dispose();
        _centerFormat.Dispose();
        _rightFormat.Dispose();

        base.OnFormClosed(e);
    }
}
