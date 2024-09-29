using Windows.Graphics;
using Microsoft.UI.Xaml;
using TdLib;

namespace sakuragram.Views.Calls
{
    public partial class VoiceCall : Window
    {
        private static TdClient _client = App._client;
        private bool _isIncomingCall;
        private bool _isOutgoingCall;
        private bool _isMuted = false;
        
        public VoiceCall()
        {
            InitializeComponent();
            
            ExtendsContentIntoTitleBar = true;
            AppWindow.Resize(new SizeInt32(600, 300));
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

        private void ButtonMute_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonShareScreen_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonEnableVideo_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonEndCall_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}