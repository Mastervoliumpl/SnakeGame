using System.Windows;

namespace SnakeGame
{
    public partial class EndGameWindow : Window
    {
        public EndGameWindow(int finalScore)
        {
            InitializeComponent();

            // Display the final score
            tbFinalScore.Text = $"Your final score: {finalScore}";
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            // Close the EndGame window
            this.DialogResult = true;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Allow dragging the window
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }
    }
}
