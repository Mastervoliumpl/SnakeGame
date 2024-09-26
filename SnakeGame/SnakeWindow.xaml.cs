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
        private string gameDifficulty = "Medium";

        private int SnakeStartSpeed = 400;
        private int speedIncreasePerFood = 10;
        private int actionsTaken = 0;
        private bool isPaused = false;
        private int snakeLength;
        private int currentScore = 0;
        private int SnakeSpeedThreshold = 50;
        private const int SnakeSquareSize = 20;
        private const int SnakeStartLength = 3;

        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;
        private List<SnakePart> snakeParts = new List<SnakePart>();

        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection snakeDirection = SnakeDirection.Right;

        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
        
        private Random rnd = new Random();
        private UIElement snakeFood = null;
        private SolidColorBrush foodBrush = Brushes.Red;

        public SnakeWindow()
        {
            InitializeComponent();
            gameTickTimer.Tick += GameTickTimer_Tick;
            this.KeyDown += SnakeWindow_KeyDown;
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
            actionsTaken = 0;
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
            this.tbStatusScore.Text = currentScore.ToString();

            // Display speed in tiles per second
            double tilesPerSecond = 1000 / gameTickTimer.Interval.TotalMilliseconds;
            this.tbStatusSpeed.Text = Math.Round(tilesPerSecond, 2).ToString(); // Rounded to 2 decimals
        }

        private void EndGame()
        {
            // Stop the game
            gameTickTimer.IsEnabled = false;

            // Open the EndGameWindow and pass the final score
            EndGameWindow endGameWindow = new EndGameWindow(currentScore);
            bool? result = endGameWindow.ShowDialog();

            if (result == true)
            {
                // After pressing Continue, show the settings window
                ShowSettingsWindow();
            }
        }

        private void TogglePause()
        {
            if (isPaused)
            {
                gameTickTimer.Start();
            }
            else
            {
                gameTickTimer.Stop();
            }
            isPaused = !isPaused;
        }

        private void ShowSettingsWindow()
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            bool? result = settingsWindow.ShowDialog(); // Show as a modal dialog

            if (result == true)
            {
                gameDifficulty = settingsWindow.SelectedDifficulty;
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
            switch (gameDifficulty)
            {
                case "Easy":
                    SnakeStartSpeed = 500;
                    SnakeSpeedThreshold = 200;
                    speedIncreasePerFood = 20;
                    break;
                case "Medium":
                    SnakeStartSpeed = 400;
                    SnakeSpeedThreshold = 120;
                    speedIncreasePerFood = 40;
                    break;
                case "Hard":
                    SnakeStartSpeed = 300;
                    SnakeSpeedThreshold = 80;
                    speedIncreasePerFood = 60;
                    break;
            }
        }

        private void StartNewGame()
        {
            // Remove potential dead snake parts and leftover food...
            foreach (SnakePart snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
            }
            snakeParts.Clear();
            if (snakeFood != null)
                GameArea.Children.Remove(snakeFood);

            // Reset stuff
            currentScore = 0;
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakePart() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);

            // Draw the snake again and some new food.
            DrawSnake();
            DrawSnakeFood();

            // Update status
            UpdateGameStatus();

            // Go!        
            gameTickTimer.IsEnabled = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    
                    if (snakeDirection != SnakeDirection.Down && actionsTaken < 1)
                        snakeDirection = SnakeDirection.Up;
                    actionsTaken++;
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up && actionsTaken < 1)
                        snakeDirection = SnakeDirection.Down;
                    actionsTaken++;
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right && actionsTaken < 1)
                        snakeDirection = SnakeDirection.Left;
                    actionsTaken++;
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left && actionsTaken < 1)
                        snakeDirection = SnakeDirection.Right;
                    actionsTaken++;
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
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = gridBackgroundColor
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                // Draw grid lines
                Rectangle gridLine = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Stroke = gridLineColor,
                    StrokeThickness = 1
                };
                GameArea.Children.Add(gridLine);
                Canvas.SetTop(gridLine, nextY);
                Canvas.SetLeft(gridLine, nextX);

                // Move to the next square
                nextX += SnakeSquareSize;
                if (nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += SnakeSquareSize;
                }

                if (nextY >= GameArea.ActualHeight)
                    doneDrawingBackground = true;
            }
        }

        private void DrawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }

        private void MoveSnake()
        {
            while (snakeParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            }

            foreach (SnakePart snakePart in snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }

            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }

            snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });

            DrawSnake();
            DoCollisionCheck();          
        }

        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int foodX = rnd.Next(0, maxX) * SnakeSquareSize;
            int foodY = rnd.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakePart snakePart in snakeParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    return GetNextFoodPosition();
            }

            return new Point(foodX, foodY);
        }

        private void DrawSnakeFood()
        {
            Point foodPosition = GetNextFoodPosition();
            snakeFood = new Ellipse()
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = foodBrush
            };
            GameArea.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodPosition.Y);
            Canvas.SetLeft(snakeFood, foodPosition.X);
        }

        private void EatSnakeFood()
        {
            snakeLength++;
            currentScore++;

            // Decrease the interval by a fixed amount down to the threshold
            int newInterval = (int)gameTickTimer.Interval.TotalMilliseconds - speedIncreasePerFood;
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(SnakeSpeedThreshold, newInterval));

            // Remove the food + gen new food
            GameArea.Children.Remove(snakeFood);
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
            return snakeParts.Take(snakeParts.Count - 1)
                             .Any(part => part.Position.X == snakeHead.Position.X &&
                                          part.Position.Y == snakeHead.Position.Y);
        }

        private bool IsCollisionWithFood(SnakePart snakeHead)
        {
            return snakeHead.Position.X == Canvas.GetLeft(snakeFood) &&
                   snakeHead.Position.Y == Canvas.GetTop(snakeFood);
        }

        private void DoCollisionCheck()
        {
            SnakePart snakeHead = snakeParts.Last();

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