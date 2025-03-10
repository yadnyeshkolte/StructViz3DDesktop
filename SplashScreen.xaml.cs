using System;
using System.Windows;
using System.Windows.Threading;

namespace StructViz3D
{
    public partial class SplashScreen : Window
    {
        private DispatcherTimer timer;

        public SplashScreen()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            // Create the main window only at this point
            var mainWindow = new MainWindow();

            // Make it the application's main window
            Application.Current.MainWindow = mainWindow;

            // Show the main window
            mainWindow.Show();

            // Close the splash screen
            this.Close();
        }
    }
}