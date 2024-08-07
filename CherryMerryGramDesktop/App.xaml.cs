using Microsoft.UI.Xaml;

namespace CherryMerryGramDesktop
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _mWindow = new MainWindow();
            _mWindow.Activate();
        }

        private Window _mWindow;
    }
}
