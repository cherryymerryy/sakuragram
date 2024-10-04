using System;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
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
            string folderIconName = userFolder.Icon.Name.ToLower();
            ImageSource folderIcon = new BitmapImage(new Uri($"ms-appx:///Assets/icons/folders/folder_{folderIconName}.png"));

            SettingsCard card = new();
            card.Header = userFolder.Title;
            card.Description = _client.GetChatFolderAsync(userFolder.Id).Result.IncludedChatIds.Length + " chats";
            // card.HeaderIcon = new ImageIcon {Source = folderIcon};
            
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