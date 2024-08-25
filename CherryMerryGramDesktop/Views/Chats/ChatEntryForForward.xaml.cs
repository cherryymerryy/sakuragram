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
    
    public ChatEntryForForward()
    {
        InitializeComponent();
    }
    
    public async void UpdateEntry(TdApi.Chat chat)
    {
        if (ChatName == null) return;
        
        _chat = chat;
        ChatName.Text = _chat.Title;
    }

    private void ChatEntryForForward_OnClick(object sender, RoutedEventArgs e)
    {
        _client.ForwardMessagesAsync(chatId: _chat.Id, fromChatId: _fromChatId, messageIds: _messageIds);
    }
}