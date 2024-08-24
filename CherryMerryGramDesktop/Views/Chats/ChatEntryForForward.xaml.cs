using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats;

public partial class ChatEntryForForward : Button
{
    private static TdClient _client = App._client;
    
    public async void UpdateEntry(TdApi.Chat chat)
    {
        ChatName.Text = chat.Title;
        
        var chatPhoto = await _client.ExecuteAsync(new TdApi.DownloadFile
        {
            FileId = chat.Photo.Small.Id,
            Priority = 1
        });
        
        if (chatPhoto != null)
        {
            ProfilePicture.ImageSource = new BitmapImage(new Uri(chatPhoto.Local.Path));
        }
    }
}