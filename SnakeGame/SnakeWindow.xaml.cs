using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnakeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SnakeWindow : Window
    {
        private string _gameDifficulty = "Medium";

        private int _snakeStartSpeed = 400;
        private int _speedIncreasePerFood = 10;
        private int _actionsTaken = 0;
        private bool _isPaused = false;
        private int _snakeLength;
        private int _currentScore = 0;
        private int _snakeSpeedThreshold = 50;
        private const int _snakeSquareSize = 20;
        private const int _snakeStartLength = 3;

        private SolidColorBrush _snakeBodyBrush = Brushes.Green;
        private SolidColorBrush _snakeHeadBrush = Brushes.YellowGreen;
        private List<SnakePart> _snakeParts = new List<SnakePart>();

        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection _snakeDirection = SnakeDirection.Right;

        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
        
        private Random _rnd = new Random();
        private UIElement _snakeFood = null;
        private SolidColorBrush _foodBrush = Brushes.Red;

        public SnakeWindow()
        {
            InitializeComponent();
            gameTickTimer.Tick += GameTickTimer_Tick;
            this.KeyDown += SnakeWindow_KeyDown;
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
            _actionsTaken = 0;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
            ShowSettingsWindow();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SnakeWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if Left Ctrl and Q are pressed
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.Q)
            {
                EndGame();
            }
        }

        private void UpdateGameStatus()
        {
            // Update the score
            this.tbStatusScore.Text = _currentScore.ToString();

            // Display speed in tiles per second
            double tilesPerSecond = 1000 / gameTickTimer.Interval.TotalMilliseconds;
            this.tbStatusSpeed.Text = Math.Round(tilesPerSecond, 2).ToString(); // Rounded to 2 decimals
        }

        private void EndGame()
        {
            // Stop the timer/game
            gameTickTimer.IsEnabled = false;

            // Open the EndGameWindow with final score
            EndGameWindow endGameWindow = new EndGameWindow(_currentScore);
            bool? result = endGameWindow.ShowDialog();

            if (result == true)
            {
                // Show settings window
                ShowSettingsWindow();
            }
        }

        private void TogglePause()
        {
            if (_isPaused)
            {
                gameTickTimer.Start();
            }
            else
            {
                gameTickTimer.Stop();
            }
            _isPaused = !_isPaused;
        }

        private void ShowSettingsWindow()
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            bool? result = settingsWindow.ShowDialog(); // Show as a modal dialog

            if (result == true)
            {
                _gameDifficulty = settingsWindow.SelectedDifficulty;
                SetGameDifficulty();
                StartNewGame();
            }
            else
            {
                this.Close(); // Close SnakeWindow if the settings window gets closed
            }
        }

        private void SetGameDifficulty()
        {
            switch (_gameDifficulty)
            {
                case "Easy":
                    _snakeStartSpeed = 500;
                    _snakeSpeedThreshold = 200;
                    _speedIncreasePerFood = 20;
                    break;
                case "Medium":
                    _snakeStartSpeed = 400;
                    _snakeSpeedThreshold = 120;
                    _speedIncreasePerFood = 40;
                    break;
                case "Hard":
                    _snakeStartSpeed = 300;
                    _snakeSpeedThreshold = 80;
                    _speedIncreasePerFood = 60;
                    break;
            }
        }

        private void StartNewGame()
        {
            // Remove potential dead snake parts and leftover food...
            foreach (SnakePart snakeBodyPart in _snakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
            }
            _snakeParts.Clear();
            if (_snakeFood != null)
                GameArea.Children.Remove(_snakeFood);

            // Reset stuff
            _currentScore = 0;
            _snakeLength = _snakeStartLength;
            _snakeDirection = SnakeDirection.Right;
            _snakeParts.Add(new SnakePart() { Position = new Point(_snakeSquareSize * 5, _snakeSquareSize * 5) });
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(_snakeStartSpeed);

            // Draw the snake again and a new food.
            DrawSnake();
            DrawSnakeFood();

            // Update status
            UpdateGameStatus();

            // Go!        
            gameTickTimer.IsEnabled = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = _snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    
                    if (_snakeDirection != SnakeDirection.Down && _actionsTaken < 1)
                        _snakeDirection = SnakeDirection.Up;
                    _actionsTaken++;
                    break;
                case Key.Down:
                    if (_snakeDirection != SnakeDirection.Up && _actionsTaken < 1)
                        _snakeDirection = SnakeDirection.Down;
                    _actionsTaken++;
                    break;
                case Key.Left:
                    if (_snakeDirection != SnakeDirection.Right && _actionsTaken < 1)
                        _snakeDirection = SnakeDirection.Left;
                    _actionsTaken++;
                    break;
                case Key.Right:
                    if (_snakeDirection != SnakeDirection.Left && _actionsTaken < 1)
                        _snakeDirection = SnakeDirection.Right;
                    _actionsTaken++;
                    break;
                case Key.P:
                    TogglePause();
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
            }
        }

        private void DrawGameArea()
        {
            SolidColorBrush gridBackgroundColor = (SolidColorBrush)FindResource("GridBackgroundColor");
            SolidColorBrush gridLineColor = (SolidColorBrush)FindResource("GridLineColor");

            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;

            while (!doneDrawingBackground)
            {
                // Draw background grid square
                Rectangle rect = new Rectangle
                {
                    Width = _snakeSquareSize,
                    Height = _snakeSquareSize,
                    Fill = gridBackgroundColor
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                // Draw grid lines
                Rectangle gridLine = new Rectangle
                {
                    Width = _snakeSquareSize,
                    Height = _snakeSquareSize,
                    Stroke = gridLineColor,
                    StrokeThickness = 1
                };
                GameArea.Children.Add(gridLine);
                Canvas.SetTop(gridLine, nextY);
                Canvas.SetLeft(gridLine, nextX);

                // Move to the next square
                nextX += _snakeSquareSize;
                if (nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += _snakeSquareSize;
                }

                if (nextY >= GameArea.ActualHeight)
                    doneDrawingBackground = true;
            }
        }

        private void DrawSnake()
        {
            foreach (SnakePart snakePart in _snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width = _snakeSquareSize,
                        Height = _snakeSquareSize,
                        Fill = (snakePart.IsHead ? _snakeHeadBrush : _snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }

        private void MoveSnake()
        {
            while (_snakeParts.Count >= _snakeLength)
            {
                GameArea.Children.Remove(_snakeParts[0].UiElement);
                _snakeParts.RemoveAt(0);
            }

            foreach (SnakePart snakePart in _snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = _snakeBodyBrush;
                snakePart.IsHead = false;
            }

            SnakePart snakeHead = _snakeParts[_snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (_snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= _snakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += _snakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= _snakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += _snakeSquareSize;
                    break;
            }

            _snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });

            DrawSnake();
            DoCollisionCheck();          
        }

        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / _snakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / _snakeSquareSize);
            int foodX = _rnd.Next(0, maxX) * _snakeSquareSize;
            int foodY = _rnd.Next(0, maxY) * _snakeSquareSize;

            foreach (SnakePart snakePart in _snakeParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    return GetNextFoodPosition();
            }

            return new Point(foodX, foodY);
        }

        private void DrawSnakeFood()
        {
            Point foodPosition = GetNextFoodPosition();
            _snakeFood = new Ellipse()
            {
                Width = _snakeSquareSize,
                Height = _snakeSquareSize,
                Fill = _foodBrush
            };
            GameArea.Children.Add(_snakeFood);
            Canvas.SetTop(_snakeFood, foodPosition.Y);
            Canvas.SetLeft(_snakeFood, foodPosition.X);
        }

        private void EatSnakeFood()
        {
            _snakeLength++;
            _currentScore++;

            // Decrease the interval by a fixed amount down to the threshold
            int newInterval = (int)gameTickTimer.Interval.TotalMilliseconds - _speedIncreasePerFood;
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(_snakeSpeedThreshold, newInterval));

            // Remove the food + gen new food
            GameArea.Children.Remove(_snakeFood);
            DrawSnakeFood();

            UpdateGameStatus();
        }

        private bool IsCollisionWithWall(SnakePart snakeHead)
        {
            return snakeHead.Position.Y < 0 ||
                   snakeHead.Position.Y >= GameArea.ActualHeight ||
                   snakeHead.Position.X < 0 ||
                   snakeHead.Position.X >= GameArea.ActualWidth;
        }

        private bool IsCollisionWithSelf(SnakePart snakeHead)
        {
            return _snakeParts.Take(_snakeParts.Count - 1)
                             .Any(part => part.Position.X == snakeHead.Position.X &&
                                          part.Position.Y == snakeHead.Position.Y);
        }

        private bool IsCollisionWithFood(SnakePart snakeHead)
        {
            return snakeHead.Position.X == Canvas.GetLeft(_snakeFood) &&
                   snakeHead.Position.Y == Canvas.GetTop(_snakeFood);
        }

        private void DoCollisionCheck()
        {
            SnakePart snakeHead = _snakeParts.Last();

            if (IsCollisionWithWall(snakeHead))
            {
                EndGame();
                return;
            }

            if (IsCollisionWithSelf(snakeHead))
            {
                EndGame();
                return;
            }

            if (IsCollisionWithFood(snakeHead))
            {
                EatSnakeFood();
                return;
            }
        }
    }
}