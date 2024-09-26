using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SnakeGame
{
    public partial class SettingsWindow : Window
    {
        public string SelectedDifficulty { get; private set; } = "Medium";

        public SettingsWindow()
        {
            InitializeComponent();
            this.Focus();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Start the game and set the difficulty
        private void BtnStartGame_Click(object sender, RoutedEventArgs e)
        {
            // Determine which RadioButton is selected
            if (rbEasy.IsChecked == true)
            {
                SelectedDifficulty = "Easy";
            }
            else if (rbMedium.IsChecked == true)
            {
                SelectedDifficulty = "Medium";
            }
            else if (rbHard.IsChecked == true)
            {
                SelectedDifficulty = "Hard";
            }

            // Close the settings window and start the game
            this.DialogResult = true;
        }

    }
}
