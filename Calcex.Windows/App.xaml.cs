using System;
using System.Windows;
using Bluegrams.Application;

namespace Calcex.Windows
{
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            PortableSettingsProvider.ApplyProvider(Calcex.Windows.Properties.Settings.Default);
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
