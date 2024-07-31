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

        public async void UpdateChat(long chatId, string Title)
        {
            ChatId = chatId;
            var chat = _client.GetChatAsync(chatId: chatId);
            ChatTitle.Text = chat.Result.Title;

            var messages = GetMessages();

            await foreach (var message in messages)
            {
                var messageEntry = new ChatMessage();
                //messageEntry.UpdateMessage(message: message);
                MessagesList.Items.Add(messageEntry);
            }
        }

        private static async IAsyncEnumerable<TdApi.Messages> GetMessages()
        {
            var messages = await _client.ExecuteAsync(new TdApi.GetChatHistory
            {
                ChatId = ChatId,
                FromMessageId = 0,
                Offset = 0,
                Limit = 100
            });

            foreach (var message in messages.Messages_)
            {
                yield return messages;
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
        }
    }
}
