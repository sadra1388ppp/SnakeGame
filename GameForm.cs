using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace SnakeGame;

public sealed class GameForm : Form
{
    private enum GameState { Ready, Playing, Paused, GameOver }
    private enum Direction { Up, Down, Left, Right }
    private enum Difficulty { Easy, Normal, Hard }

    private const int CellSize = 23;
    private const int Columns = 30;
    private const int Rows = 21;

    private static readonly Color Background = Color.FromArgb(10, 14, 19);
    private static readonly Color Panel = Color.FromArgb(17, 24, 32);
    private static readonly Color PanelLight = Color.FromArgb(22, 31, 40);
    private static readonly Color Border = Color.FromArgb(40, 53, 64);
    private static readonly Color Muted = Color.FromArgb(130, 148, 160);
    private static readonly Color White = Color.FromArgb(235, 243, 246);
    private static readonly Color Green = Color.FromArgb(157, 230, 93);
    private static readonly Color GreenDark = Color.FromArgb(45, 112, 67);
    private static readonly Color Cyan = Color.FromArgb(74, 208, 190);

    private readonly System.Windows.Forms.Timer _timer = new();
    private readonly Random _random = new();
    private readonly List<Point> _snake = [];
    private Point _food;
    private Direction _direction = Direction.Right;
    private Direction _nextDirection = Direction.Right;
    private GameState _state = GameState.Ready;
    private Difficulty _difficulty = Difficulty.Normal;
    private Point _mousePosition = new(-1, -1);
    private Point _dragOffset;
    private bool _dragging;
    private bool _won;
    private int _score;
    private int _bestScore;
    private int _foodEaten;

    private readonly Rectangle _boardRect = new(40, 132, 770, 570);
    private readonly Rectangle _fieldRect = new(75, 166, Columns * CellSize, Rows * CellSize);
    private readonly Rectangle _rightRect = new(838, 132, 302, 570);
    private Rectangle _overlayButton;
    private Rectangle _mainActionButton;
    private Rectangle _restartButton;
    private Rectangle _easyButton;
    private Rectangle _normalButton;
    private Rectangle _hardButton;
    private Rectangle _minimizeButton = new(1080, 18, 34, 30);
    private Rectangle _closeButton = new(1120, 18, 34, 30);

    public GameForm()
    {
        Text = "SnakeGame";
        ClientSize = new Size(1180, 780);
        MinimumSize = Size;
        MaximumSize = Size;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Background;
        KeyPreview = true;
        DoubleBuffered = true;

        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);

        _bestScore = LoadBestScore();
        _timer.Interval = CurrentSpeed();
        _timer.Tick += (_, _) => MoveSnake();

        MouseMove += HandleMouseMove;
        MouseDown += HandleMouseDown;
        MouseUp += (_, _) => _dragging = false;
        MouseClick += HandleMouseClick;
        KeyDown += HandleKeyDown;

        ResetSnake();
    }

    private void ResetSnake()
    {
        _snake.Clear();
        int centerX = Columns / 2;
        int centerY = Rows / 2;

        _snake.Add(new Point(centerX, centerY));
        _snake.Add(new Point(centerX - 1, centerY));
        _snake.Add(new Point(centerX - 2, centerY));
        _snake.Add(new Point(centerX - 3, centerY));

        _direction = Direction.Right;
        _nextDirection = Direction.Right;
        _foodEaten = 0;
        PlaceFood();
    }

    private void StartGame()
    {
        _score = 0;
        _won = false;
        ResetSnake();
        _state = GameState.Playing;
        _timer.Interval = CurrentSpeed();
        _timer.Start();
        Focus();
        Invalidate();
    }

    private void SetState(GameState state)
    {
        _state = state;

        if (state == GameState.Playing)
        {
            _timer.Interval = CurrentSpeed();
            _timer.Start();
            Focus();
        }
        else
        {
            _timer.Stop();
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

    private int CurrentSpeed()
    {
        int start = _difficulty switch
        {
            Difficulty.Easy => 175,
            Difficulty.Hard => 95,
            _ => 135
        };

        return Math.Max(58, start - (_score / 5 * 6));
    }

    private void MoveSnake()
    {
        _direction = _nextDirection;

        Point head = _snake[0];
        Point next = _direction switch
        {
            Direction.Up => new Point(head.X, head.Y - 1),
            Direction.Down => new Point(head.X, head.Y + 1),
            Direction.Left => new Point(head.X - 1, head.Y),
            _ => new Point(head.X + 1, head.Y)
        };

        bool eating = next == _food;

        if (next.X < 0 || next.X >= Columns || next.Y < 0 || next.Y >= Rows)
        {
            EndGame();
            return;
        }

        int bodyPartsToCheck = eating ? _snake.Count : _snake.Count - 1;
        for (int i = 0; i < bodyPartsToCheck; i++)
        {
            if (_snake[i] == next)
            {
                EndGame();
                return;
            }
        }

        _snake.Insert(0, next);

        if (eating)
        {
            _score += 10;
            _foodEaten++;

            if (_score > _bestScore)
            {
                _bestScore = _score;
                SaveBestScore();
            }

            if (_snake.Count == Columns * Rows)
            {
                _won = true;
                EndGame();
                return;
            }

            PlaceFood();
            _timer.Interval = CurrentSpeed();
        }
        else
        {
            _snake.RemoveAt(_snake.Count - 1);
        }

        Invalidate();
    }

    private void EndGame()
    {
        _won = _snake.Count == Columns * Rows;
        SetState(GameState.GameOver);
    }

    private void PlaceFood()
    {
        do
        {
            _food = new Point(_random.Next(Columns), _random.Next(Rows));
        }
        while (_snake.Contains(_food));
    }

    private void ChangeDirection(Direction direction)
    {
        if (_state == GameState.Ready)
            StartGame();

        if (_state != GameState.Playing)
            return;

        bool opposite =
            (_direction == Direction.Up && direction == Direction.Down) ||
            (_direction == Direction.Down && direction == Direction.Up) ||
            (_direction == Direction.Left && direction == Direction.Right) ||
            (_direction == Direction.Right && direction == Direction.Left);

        if (!opposite)
            _nextDirection = direction;
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
            // The game can still be played when local score saving is unavailable.
        }
    }

    private static string BestScorePath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SnakeGame",
            "best-score.txt");

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
                if (_state == GameState.Ready || _state == GameState.GameOver)
                    StartGame();
                else
                    TogglePause();
                e.Handled = true;
                break;

            case Keys.Escape:
                if (_state == GameState.Playing)
                    SetState(GameState.Paused);
                else if (_state == GameState.Paused)
                    SetState(GameState.Playing);
                e.Handled = true;
                break;
        }
    }

    private void HandleMouseMove(object? sender, MouseEventArgs e)
    {
        _mousePosition = e.Location;

        if (_dragging)
            Location = new Point(Cursor.Position.X - _dragOffset.X, Cursor.Position.Y - _dragOffset.Y);

        Cursor = _closeButton.Contains(e.Location) ||
                 _minimizeButton.Contains(e.Location) ||
                 _mainActionButton.Contains(e.Location) ||
                 _restartButton.Contains(e.Location) ||
                 _easyButton.Contains(e.Location) ||
                 _normalButton.Contains(e.Location) ||
                 _hardButton.Contains(e.Location) ||
                 _overlayButton.Contains(e.Location)
            ? Cursors.Hand
            : Cursors.Default;

        Invalidate();
    }

    private void HandleMouseDown(object? sender, MouseEventArgs e)
    {
        Focus();

        if (e.Button == MouseButtons.Left && e.Y < 65 &&
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

        if (_easyButton.Contains(point))
            SetDifficulty(Difficulty.Easy);
        else if (_normalButton.Contains(point))
            SetDifficulty(Difficulty.Normal);
        else if (_hardButton.Contains(point))
            SetDifficulty(Difficulty.Hard);

        if (_mainActionButton.Contains(point) || _overlayButton.Contains(point))
        {
            if (_state == GameState.Playing)
                TogglePause();
            else if (_state == GameState.Paused)
                SetState(GameState.Playing);
            else
                StartGame();
        }

        if (_restartButton.Contains(point))
            StartGame();
    }

    private void SetDifficulty(Difficulty difficulty)
    {
        _difficulty = difficulty;

        if (_state == GameState.Playing)
            _timer.Interval = CurrentSpeed();

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        using (var backgroundBrush = new LinearGradientBrush(
                   ClientRectangle, Color.FromArgb(13, 19, 25), Background, 90f))
            g.FillRectangle(backgroundBrush, ClientRectangle);

        DrawBackgroundDetails(g);
        DrawHeader(g);
        DrawBoard(g);
        DrawRightPanel(g);
        DrawFooter(g);
    }

    private void DrawBackgroundDetails(Graphics g)
    {
        using var glow = new SolidBrush(Color.FromArgb(13, Green));
        g.FillEllipse(glow, -180, 430, 420, 420);

        using var linePen = new Pen(Color.FromArgb(15, 145, 230, 130), 1);
        for (int x = 0; x < ClientSize.Width; x += 38)
            g.DrawLine(linePen, x, 78, x, 112);
    }

    private void DrawHeader(Graphics g)
    {
        using var linePen = new Pen(Border, 1);
        g.DrawLine(linePen, 0, 68, Width, 68);

        FillRounded(g, new Rectangle(40, 18, 34, 34), 10, Green);
        DrawText(g, "S", new Font("Segoe UI", 17, FontStyle.Bold), Background,
            new Rectangle(40, 17, 34, 35), StringAlignment.Center, StringAlignment.Center);

        DrawText(g, "SNAKE", new Font("Segoe UI", 15, FontStyle.Bold), White,
            new Rectangle(86, 13, 100, 23));
        DrawText(g, "ARCADE EDITION", new Font("Segoe UI", 8, FontStyle.Bold),
            Muted, new Rectangle(87, 37, 130, 17));

        FillRounded(g, new Rectangle(252, 23, 9, 9), 5,
            _state == GameState.Playing ? Green : Muted);
        DrawText(g, _state switch
        {
            GameState.Playing => "SESSION LIVE",
            GameState.Paused => "PAUSED",
            GameState.GameOver => "SESSION ENDED",
            _ => "READY WHEN YOU ARE"
        }, new Font("Segoe UI", 8.5f, FontStyle.Bold), Muted,
            new Rectangle(270, 17, 180, 22));

        DrawText(g, "OFFLINE GAME  /  NO ACCOUNT NEEDED",
            new Font("Segoe UI", 8, FontStyle.Bold), Color.FromArgb(91, 111, 122),
            new Rectangle(720, 20, 330, 24), StringAlignment.Far, StringAlignment.Center);

        FillRounded(g, _minimizeButton, 8,
            _minimizeButton.Contains(_mousePosition) ? PanelLight : Panel);
        DrawText(g, "–", new Font("Segoe UI", 15), Muted, _minimizeButton,
            StringAlignment.Center, StringAlignment.Center);

        FillRounded(g, _closeButton, 8,
            _closeButton.Contains(_mousePosition) ? Color.FromArgb(133, 55, 62) : Panel);
        DrawText(g, "×", new Font("Segoe UI", 15), White, _closeButton,
            StringAlignment.Center, StringAlignment.Center);
    }

    private void DrawBoard(Graphics g)
    {
        DrawPanel(g, _boardRect, Panel);
        DrawText(g, "THE PLAYING FIELD", new Font("Segoe UI", 8, FontStyle.Bold),
            Muted, new Rectangle(62, 142, 180, 18));

        FillRounded(g, new Rectangle(680, 140, 105, 23), 11, PanelLight);
        DrawText(g, _state switch
        {
            GameState.Playing => "●  PLAYING",
            GameState.Paused => "Ⅱ  PAUSED",
            GameState.GameOver => "■  FINISHED",
            _ => "○  STANDBY"
        }, new Font("Segoe UI", 7.5f, FontStyle.Bold),
            _state == GameState.Playing ? Green : Muted,
            new Rectangle(680, 140, 105, 23), StringAlignment.Center, StringAlignment.Center);

        FillRounded(g, _fieldRect, 13, Color.FromArgb(10, 17, 22));

        using (var borderPen = new Pen(Color.FromArgb(47, 72, 73), 1))
            DrawRounded(g, _fieldRect, 13, borderPen);

        using var gridPen = new Pen(Color.FromArgb(22, 101, 139, 118), 1);
        for (int x = 1; x < Columns; x++)
        {
            int lineX = _fieldRect.Left + x * CellSize;
            g.DrawLine(gridPen, lineX, _fieldRect.Top + 1, lineX, _fieldRect.Bottom - 1);
        }

        for (int y = 1; y < Rows; y++)
        {
            int lineY = _fieldRect.Top + y * CellSize;
            g.DrawLine(gridPen, _fieldRect.Left + 1, lineY, _fieldRect.Right - 1, lineY);
        }

        DrawFood(g);
        DrawSnake(g);

        if (_state is GameState.Ready or GameState.Paused or GameState.GameOver)
            DrawOverlay(g);
        else
            _overlayButton = Rectangle.Empty;
    }

    private void DrawSnake(Graphics g)
    {
        for (int i = _snake.Count - 1; i >= 0; i--)
        {
            Point segment = _snake[i];
            Rectangle cell = CellRectangle(segment);

            cell.Inflate(-3, -3);

            if (i == 0)
            {
                using var shadow = new SolidBrush(Color.FromArgb(45, Green));
                g.FillEllipse(shadow, cell.X - 4, cell.Y - 4, cell.Width + 8, cell.Height + 8);

                using var headBrush = new LinearGradientBrush(
                    cell, Color.FromArgb(206, 255, 142), GreenDark, 45f);
                FillRounded(g, cell, 7, headBrush);

                using var headPen = new Pen(Color.FromArgb(210, 210, 255, 174), 1);
                DrawRounded(g, cell, 7, headPen);
            }
            else
            {
                int alpha = Math.Max(110, 230 - i * 3);
                Color segmentColor = Color.FromArgb(alpha,
                    Math.Max(45, 118 - i), Math.Max(100, 195 - i * 2), 83);

                FillRounded(g, cell, 6, segmentColor);
                using var shine = new Pen(Color.FromArgb(48, 220, 255, 190), 1);
                DrawRounded(g, cell, 6, shine);
            }
        }

        DrawSnakeEyes(g);
    }

    private void DrawSnakeEyes(Graphics g)
    {
        Point head = _snake[0];
        Rectangle cell = CellRectangle(head);
        float cx = cell.Left + CellSize / 2f;
        float cy = cell.Top + CellSize / 2f;

        PointF first;
        PointF second;

        switch (_direction)
        {
            case Direction.Up:
                first = new PointF(cx - 5, cy - 5);
                second = new PointF(cx + 5, cy - 5);
                break;
            case Direction.Down:
                first = new PointF(cx - 5, cy + 5);
                second = new PointF(cx + 5, cy + 5);
                break;
            case Direction.Left:
                first = new PointF(cx - 5, cy - 5);
                second = new PointF(cx - 5, cy + 5);
                break;
            default:
                first = new PointF(cx + 5, cy - 5);
                second = new PointF(cx + 5, cy + 5);
                break;
        }

        using var eyeBrush = new SolidBrush(Color.FromArgb(12, 27, 24));
        g.FillEllipse(eyeBrush, first.X - 1.7f, first.Y - 1.7f, 3.4f, 3.4f);
        g.FillEllipse(eyeBrush, second.X - 1.7f, second.Y - 1.7f, 3.4f, 3.4f);
    }

    private void DrawFood(Graphics g)
    {
        Rectangle cell = CellRectangle(_food);
        float cx = cell.Left + CellSize / 2f;
        float cy = cell.Top + CellSize / 2f;

        using var glow = new SolidBrush(Color.FromArgb(35, 255, 104, 104));
        g.FillEllipse(glow, cx - 14, cy - 14, 28, 28);

        using var foodBrush = new LinearGradientBrush(
            new RectangleF(cx - 7, cy - 7, 14, 14),
            Color.FromArgb(255, 151, 126), Color.FromArgb(229, 68, 78), 45f);
        g.FillEllipse(foodBrush, cx - 7, cy - 7, 14, 14);

        using var stemPen = new Pen(Color.FromArgb(166, 220, 145), 2);
        g.DrawLine(stemPen, cx + 1, cy - 7, cx + 4, cy - 10);
    }

    private void DrawOverlay(Graphics g)
    {
        using var shade = new SolidBrush(Color.FromArgb(205, 7, 12, 16));
        FillRounded(g, _fieldRect, 13, shade);

        Rectangle card = new(250, 300, 390, 250);
        DrawPanel(g, card, Color.FromArgb(235, 18, 27, 34));

        string eyebrow = _state switch
        {
            GameState.Paused => "TAKE YOUR TIME",
            GameState.GameOver => _won ? "INCREDIBLE RUN" : "NICE TRY",
            _ => "A CLASSIC, REIMAGINED"
        };

        string title = _state switch
        {
            GameState.Paused => "GAME PAUSED",
            GameState.GameOver => _won ? "BOARD CLEARED!" : "GAME OVER",
            _ => "READY TO PLAY?"
        };

        string description = _state switch
        {
            GameState.Paused => "Your run is waiting right where you left it.",
            GameState.GameOver => $"You collected {_score / 10} food items. Best score: {_bestScore}.",
            _ => "Collect the glowing food. Don't run into yourself."
        };

        DrawText(g, eyebrow, new Font("Segoe UI", 8, FontStyle.Bold), Green,
            new Rectangle(card.X + 20, card.Y + 28, card.Width - 40, 18),
            StringAlignment.Center, StringAlignment.Center);

        DrawText(g, title, new Font("Segoe UI", 22, FontStyle.Bold), White,
            new Rectangle(card.X + 20, card.Y + 54, card.Width - 40, 42),
            StringAlignment.Center, StringAlignment.Center);

        DrawText(g, description, new Font("Segoe UI", 9), Muted,
            new Rectangle(card.X + 25, card.Y + 103, card.Width - 50, 30),
            StringAlignment.Center, StringAlignment.Center);

        _overlayButton = new Rectangle(card.X + 75, card.Y + 158, card.Width - 150, 48);
        DrawButton(g, _overlayButton,
            _state == GameState.Paused ? "RESUME GAME" :
            _state == GameState.GameOver ? "PLAY AGAIN" : "START PLAYING",
            Green, Color.FromArgb(12, 22, 17));

        DrawText(g, "PRESS SPACE TO CONTINUE", new Font("Segoe UI", 7.5f, FontStyle.Bold),
            Color.FromArgb(108, 129, 138),
            new Rectangle(card.X + 20, card.Y + 215, card.Width - 40, 18),
            StringAlignment.Center, StringAlignment.Center);
    }

    private void DrawRightPanel(Graphics g)
    {
        DrawPanel(g, _rightRect, Panel);

        Rectangle scoreCard = new(856, 145, 266, 126);
        DrawPanel(g, scoreCard, PanelLight);

        DrawText(g, "YOUR SCORE", new Font("Segoe UI", 8, FontStyle.Bold), Muted,
            new Rectangle(875, 162, 150, 18));

        DrawText(g, _score.ToString("0000"), new Font("Segoe UI", 31, FontStyle.Bold),
            White, new Rectangle(872, 182, 190, 52));

        FillRounded(g, new Rectangle(1060, 188, 42, 42), 13,
            Color.FromArgb(28, 63, 47));
        DrawText(g, "✦", new Font("Segoe UI Symbol", 19), Green,
            new Rectangle(1060, 188, 42, 42), StringAlignment.Center, StringAlignment.Center);

        DrawText(g, "10 POINTS PER FOOD", new Font("Segoe UI", 7.5f, FontStyle.Bold),
            Color.FromArgb(112, 139, 129), new Rectangle(875, 239, 190, 17));

        Rectangle bestCard = new(856, 284, 126, 96);
        Rectangle speedCard = new(992, 284, 130, 96);
        DrawPanel(g, bestCard, PanelLight);
        DrawPanel(g, speedCard, PanelLight);

        DrawText(g, "PERSONAL BEST", new Font("Segoe UI", 7, FontStyle.Bold), Muted,
            new Rectangle(868, 298, 110, 17));
        DrawText(g, _bestScore.ToString("0000"), new Font("Segoe UI", 20, FontStyle.Bold),
            White, new Rectangle(867, 321, 110, 35));

        DrawText(g, "GAME SPEED", new Font("Segoe UI", 7, FontStyle.Bold), Muted,
            new Rectangle(1004, 298, 110, 17));
        DrawText(g, $"{1000f / CurrentSpeed():0.0}", new Font("Segoe UI", 20, FontStyle.Bold),
            White, new Rectangle(1003, 321, 110, 35));
        DrawText(g, "TILES / SEC", new Font("Segoe UI", 6.5f, FontStyle.Bold),
            Color.FromArgb(112, 139, 129), new Rectangle(1005, 355, 110, 15));

        Rectangle difficultyCard = new(856, 394, 266, 132);
        DrawPanel(g, difficultyCard, PanelLight);
        DrawText(g, "DIFFICULTY", new Font("Segoe UI", 8, FontStyle.Bold), Muted,
            new Rectangle(873, 408, 180, 18));
        DrawText(g, "Choose your pace", new Font("Segoe UI", 8), Color.FromArgb(103, 122, 133),
            new Rectangle(873, 429, 180, 18));

        _easyButton = new Rectangle(873, 463, 74, 42);
        _normalButton = new Rectangle(953, 463, 81, 42);
        _hardButton = new Rectangle(1040, 463, 92, 42);

        DrawDifficultyButton(g, _easyButton, "EASY", Difficulty.Easy);
        DrawDifficultyButton(g, _normalButton, "NORMAL", Difficulty.Normal);
        DrawDifficultyButton(g, _hardButton, "HARD", Difficulty.Hard);

        Rectangle actionCard = new(856, 540, 266, 146);
        DrawPanel(g, actionCard, PanelLight);
        DrawText(g, "READY FOR ANOTHER RUN?", new Font("Segoe UI", 8, FontStyle.Bold),
            Muted, new Rectangle(873, 553, 225, 18));

        _mainActionButton = new Rectangle(873, 580, 232, 46);
        string actionText = _state switch
        {
            GameState.Playing => "PAUSE GAME",
            GameState.Paused => "RESUME GAME",
            _ => "START GAME"
        };
        DrawButton(g, _mainActionButton, actionText, Green, Color.FromArgb(12, 22, 17));

        _restartButton = new Rectangle(873, 636, 232, 34);
        DrawButton(g, _restartButton, "↻   RESTART RUN", Panel,
            _restartButton.Contains(_mousePosition) ? White : Muted, Border);
    }

    private void DrawDifficultyButton(Graphics g, Rectangle rect, string text, Difficulty difficulty)
    {
        bool selected = _difficulty == difficulty;
        Color fill = selected ? Color.FromArgb(39, 68, 49) : Panel;
        Color foreground = selected ? Green : Muted;
        Color outline = selected ? Color.FromArgb(80, 143, 90) : Border;

        FillRounded(g, rect, 9, fill);
        using var pen = new Pen(outline, 1);
        DrawRounded(g, rect, 9, pen);
        DrawText(g, text, new Font("Segoe UI", 7.5f, FontStyle.Bold),
            foreground, rect, StringAlignment.Center, StringAlignment.Center);
    }

    private void DrawFooter(Graphics g)
    {
        using var line = new Pen(Border, 1);
        g.DrawLine(line, 40, 728, 1140, 728);

        DrawText(g, "CONTROLS", new Font("Segoe UI", 7.5f, FontStyle.Bold),
            Muted, new Rectangle(42, 741, 75, 18));

        DrawKey(g, "W", 120, 739);
        DrawKey(g, "A", 148, 739);
        DrawKey(g, "S", 176, 739);
        DrawKey(g, "D", 204, 739);
        DrawText(g, "OR  ARROW KEYS", new Font("Segoe UI", 7.5f), Muted,
            new Rectangle(242, 739, 120, 19));

        DrawKey(g, "SPACE", 375, 739, 52);
        DrawText(g, "PAUSE / RESUME", new Font("Segoe UI", 7.5f), Muted,
            new Rectangle(433, 739, 120, 19));

        DrawText(g, "MADE FOR YOUR OFFLINE ARCADE", new Font("Segoe UI", 7.5f, FontStyle.Bold),
            Color.FromArgb(83, 105, 116), new Rectangle(830, 739, 310, 19),
            StringAlignment.Far, StringAlignment.Center);
    }

    private static void DrawKey(Graphics g, string text, int x, int y, int width = 24)
    {
        Rectangle rect = new(x, y, width, 22);
        FillRounded(g, rect, 5, PanelLight);
        using var pen = new Pen(Border, 1);
        DrawRounded(g, rect, 5, pen);
        DrawText(g, text, new Font("Segoe UI", 7, FontStyle.Bold), White,
            rect, StringAlignment.Center, StringAlignment.Center);
    }

    private static Rectangle CellRectangle(Point point) =>
        new(_fieldRectLeft() + point.X * CellSize,
            _fieldRectTop() + point.Y * CellSize, CellSize, CellSize);

    private static int _fieldRectLeft() => 75;
    private static int _fieldRectTop() => 166;

    private static void DrawButton(Graphics g, Rectangle rect, string text,
        Color fill, Color foreground, Color? outline = null)
    {
        FillRounded(g, rect, 10, fill);

        if (outline.HasValue)
        {
            using var pen = new Pen(outline.Value, 1);
            DrawRounded(g, rect, 10, pen);
        }

        using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        DrawText(g, text, font, foreground, rect,
            StringAlignment.Center, StringAlignment.Center);
    }

    private static void DrawPanel(Graphics g, Rectangle rect, Color fill)
    {
        FillRounded(g, rect, 15, fill);
        using var pen = new Pen(Border, 1);
        DrawRounded(g, rect, 15, pen);
    }

    private static void FillRounded(Graphics g, Rectangle rect, int radius, Color color)
    {
        using var brush = new SolidBrush(color);
        FillRounded(g, rect, radius, brush);
    }

    private static void FillRounded(Graphics g, Rectangle rect, int radius, Brush brush)
    {
        using var path = RoundedPath(rect, radius);
        g.FillPath(brush, path);
    }

    private static void DrawRounded(Graphics g, Rectangle rect, int radius, Pen pen)
    {
        using var path = RoundedPath(rect, radius);
        g.DrawPath(pen, path);
    }

    private static GraphicsPath RoundedPath(Rectangle rect, int radius)
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

    private static void DrawText(Graphics g, string text, Font font, Color color,
        Rectangle rect, StringAlignment horizontal = StringAlignment.Near,
        StringAlignment vertical = StringAlignment.Center)
    {
        using var brush = new SolidBrush(color);
        using var format = new StringFormat
        {
            Alignment = horizontal,
            LineAlignment = vertical,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };

        try
        {
            g.DrawString(text, font, brush, rect, format);
        }
        finally
        {
            font.Dispose();
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
        _timer.Dispose();
        base.OnFormClosed(e);
    }
}
