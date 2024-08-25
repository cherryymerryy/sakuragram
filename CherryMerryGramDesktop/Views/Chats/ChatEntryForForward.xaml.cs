using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats;

public partial class ChatEntryForForward : Button
{
    private static TdClient _client = App._client;
    private TdApi.Chat _chat;
    public long _fromChatId;
    public long[] _messageIds;
    
    public async void UpdateEntry(TdApi.Chat chat)
    {
        _chat = chat;
        ChatName.Text = _chat.Title;
        
        // var chatPhoto = await _client.ExecuteAsync(new TdApi.DownloadFile
        // {
        //     FileId = _chat.Photo.Small.Id,
        //     Priority = 1
        // });
        //
        // if (chatPhoto != null)
        // {
        //     ProfilePicture.ImageSource = new BitmapImage(new Uri(chatPhoto.Local.Path));
        // }
    }

    private void ChatEntryForForward_OnClick(object sender, RoutedEventArgs e)
    {
        _client.ForwardMessagesAsync(chatId: _chat.Id, fromChatId: _fromChatId, messageIds: _messageIds);
    }
}