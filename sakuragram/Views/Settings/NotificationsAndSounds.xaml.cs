using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace sakuragram.Views.Settings;

public partial class NotificationsAndSounds : Page
{
    private TdClient _client = App._client;
    
    public NotificationsAndSounds()
    {
        InitializeComponent();
    }
}