using System.Configuration;
using System.Data;
using System.Windows;

namespace StructViz3D;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Do NOT create MainWindow at all at this point
        this.MainWindow = null;  // Explicitly set to null

        // Only create and show the splash screen
        var splashScreen = new SplashScreen();
        splashScreen.Show();
    }
}
