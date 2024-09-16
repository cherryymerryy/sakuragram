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
        
        var blockedUsers = _client.ExecuteAsync(new TdApi.GetBlockedMessageSenders
        {
            BlockList = new TdApi.BlockList.BlockListMain(), Limit = 100
        }).Result.TotalCount;
        var connectedWebsites = _client.ExecuteAsync(new TdApi.GetConnectedWebsites()).Result.Websites.Length;
        var activeSessions = _client.ExecuteAsync(new TdApi.GetActiveSessions()).Result.Sessions_.Length;
        
        CardBlockedUsers.Description = $"There are currently {blockedUsers} blocked users";
        CardConnectedWebsites.Description = $"There are currently {connectedWebsites} connected websites";
        CardActiveSessions.Description = $"There are currently {activeSessions} active sessions";
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void ClearPaymentInfo_OnClick(object sender, RoutedEventArgs e)
    {
    }
}