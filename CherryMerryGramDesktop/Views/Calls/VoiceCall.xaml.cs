using Windows.Foundation;
using Windows.Graphics;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;

namespace CherryMerryGramDesktop.Views.Calls
{
    public partial class VoiceCall : Window
    {
        public VoiceCall()
        {
            InitializeComponent();
            
            ExtendsContentIntoTitleBar = true;
            AppWindow.Resize(new SizeInt32(400, 800));
            TrySetDesktopAcrylicBackdrop();
        }
        
        private bool TrySetDesktopAcrylicBackdrop()
        {
            if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
            {
                Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop DesktopAcrylicBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
                SystemBackdrop = DesktopAcrylicBackdrop;

                return true;
            }

            return false;
        }
    }
}