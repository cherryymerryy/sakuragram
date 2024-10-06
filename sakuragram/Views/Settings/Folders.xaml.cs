using System;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI;
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
            var folderIcon = new ImageIcon {
                Source = new BitmapImage(new Uri($"ms-appx:///Assets/icons/folders/folder_{folderIconName}@3.png")), 
                Width = 72,
                Height = 72,
                MinWidth = 72,
                MinHeight = 72,
                MaxWidth = 72,
                MaxHeight = 72,
                Foreground = new SolidColorBrush(Colors.White)
            };
            TdApi.ChatFolder chatFolderInfo = _client.GetChatFolderAsync(userFolder.Id).Result;

            SettingsCard card = new();
            card.Header = userFolder.Title;
            card.Description = chatFolderInfo.IncludedChatIds.Length + " chats";
            card.HeaderIcon = folderIcon;
            
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