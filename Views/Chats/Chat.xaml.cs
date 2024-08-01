using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using TdLib;
using Microsoft.UI.Xaml;

namespace CherryMerryGram.Views.Chats
{
    public sealed partial class Chat : Page
    {
        private static TdClient _client = MainWindow._client;
        private static long ChatId;

        public Chat()
        {
            this.InitializeComponent();
        }

        public void UpdateChat(long chatId)
        {
            ChatId = chatId;
            var chat = _client.GetChatAsync(chatId: chatId);
            ChatTitle.Text = chat.Result.Title;
            GetMessages(chatId);
        }
        
        private void GetMessages(long chatId)
        {
            var messages = _client.ExecuteAsync(new TdApi.GetChatHistory
            {
                ChatId = chatId,
                Limit = 100
            });
            
            foreach (var message in messages.Result.Messages_)
            {
                var chatMessage = new ChatMessage();
                chatMessage.UpdateMessage(message);
                MessagesList.Items.Add(chatMessage);
            }
        }
        
        private async void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            await _client.ExecuteAsync(new TdApi.SendMessage
            {
                ChatId = ChatId,
                InputMessageContent = new TdApi.InputMessageContent.InputMessageText
                {
                    Text = new TdApi.FormattedText
                    {
                        Text = UserMessageInput.Text
                    }
                }
            });
            
            UserMessageInput.Text = "";
        }
    }
}
