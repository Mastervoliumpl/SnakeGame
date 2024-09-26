using System.Configuration;
using System.Data;
using System.Windows;

namespace SnakeGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Create the startup window
            SnakeWindow window = new SnakeWindow();

            // Show the window
            window.Show();
        }
    }

}
