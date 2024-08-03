using System;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TdLib;
using Microsoft.UI.Xaml;

namespace CherryMerryGram.Views.Chats
{
    public sealed partial class Chat : Page
    {
        private static TdClient _client = MainWindow._client;
        private static IEnumerable<Task<TdApi.Chat>> _chat;
        private static long _chatId;
        
        public Chat()
        {
            this.InitializeComponent();
        }

        public void UpdateChat(long chatId)
        {
            _chatId = chatId;
            var chat = _client.GetChatAsync(chatId);
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
            
            foreach (var message in messages.Result.Messages_.Reverse())
            {
                var chatMessage = new ChatMessage();
                chatMessage.UpdateMessage(message);
                MessagesList.Children.Add(chatMessage);
            }
        }

        private static IEnumerable<Task<TdApi.Chat>> GetChat(long chatId)
        {
            var chat = _client.ExecuteAsync(new TdApi.GetChat
            {
                ChatId = chatId
            });

            yield return chat;
        }

        private static long GetId(TdApi.MessageSender sender)
        {
            var userId = sender switch {
                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                _ => 0
            };

            return userId;
        }
        
        private async void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            await _client.ExecuteAsync(new TdApi.SendMessage
            {
                ChatId = _chatId,
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

        private void Call_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var chat = _client.GetChatAsync(_chatId);
                var messageSenderId = GetId(chat.Result.MessageSenderId);

                _client.ExecuteAsync(new TdApi.CreateCall { IsVideo = false, Protocol = new TdApi.CallProtocol(), UserId = messageSenderId });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
