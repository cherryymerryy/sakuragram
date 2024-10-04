using System.Threading.Tasks;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace sakuragram.Views.Settings;

public partial class Folders : Page
{
    private readonly TdClient _client = App._client;
    private TdApi.ChatFolderInfo[] _chatFolders = App._folders;
    
    public Folders()
    {
        InitializeComponent();
        
        foreach (var userFolder in _chatFolders)
        {
            SettingsCard card = new()
            {
                Header = userFolder.Title,
                Description = _client.GetChatFolderAsync(userFolder.Id).Result.IncludedChatIds.Length + " chats"
            };
            
            PanelUserFolders.Children.Add(card);
        }
        
        var recommendedFolders = _client.ExecuteAsync(new TdApi.GetRecommendedChatFolders()).Result;

        foreach (var folder in recommendedFolders.ChatFolders)
        {
            SettingsCard card = new()
            {
                Header = folder.Folder.Title,
                Description = folder.Description
            };

            Button button = new()
            {
                Content = "Add"
            };

            card.Content = button;
            PanelUserRecommendedFolders.Children.Add(card);
        }
    }
}