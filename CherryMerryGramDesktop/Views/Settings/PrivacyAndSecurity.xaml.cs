using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGramDesktop.Views.Settings;

public partial class PrivacyAndSecurity : Page
{
    private TdClient _client = App._client;
    
    public PrivacyAndSecurity()
    {
        InitializeComponent();
        UpdateInfo();
    }

    private async void UpdateInfo()
    {
        var blockedUsers = await _client.ExecuteAsync(new TdApi.GetBlockedMessageSenders
        {
            BlockList = new TdApi.BlockList.BlockListMain(), Limit = 100
        });
        var connectedWebsites = await _client.ExecuteAsync(new TdApi.GetConnectedWebsites());
        var activeSessions = await _client.ExecuteAsync(new TdApi.GetActiveSessions());
        
        CardBlockedUsers.Description = $"There are currently {blockedUsers.TotalCount} blocked users";
        CardConnectedWebsites.Description = $"There are currently {connectedWebsites.Websites.Length} connected websites";
        CardActiveSessions.Description = $"There are currently {activeSessions.Sessions_.Length} active sessions";
    }
    
    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void ClearPaymentInfo_OnClick(object sender, RoutedEventArgs e)
    {
    }
}