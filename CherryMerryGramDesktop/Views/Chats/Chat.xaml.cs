using System;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TdLib;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CherryMerryGram.Views.Chats
{
    public sealed partial class Chat : Page
    {
        private static TdClient _client = MainWindow._client;
        public long ChatId;
        private List<TdApi.Message> _messagesList = [];
        
        public Chat()
        {
            this.InitializeComponent();
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }

        private async Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateNewMessage:
                {
                    await GetMessages(ChatId); 
                    break;
                }
                case TdApi.Update.UpdateChatTitle:
                {
                    await UpdateChat(ChatId); 
                    break;
                }
            }
        }

        public Task UpdateChat(long chatId)
        {
            var chat = _client.GetChatAsync(chatId);
            ChatTitle.Text = chat.Result.Title;
            
            //_client.ExecuteAsync(new TdApi.DownloadFile { FileId = chat.Result.Background.Background.Id, Priority = 1 });
            
            //ThemeBackground.ImageSource = new BitmapImage();

            /*switch (chat.Result.Type)
            {
                case TdApi.ChatType.ChatTypeBasicGroup:
                {
                    var basicGroup = _client.ExecuteAsync(new TdApi.GetBasicGroupFullInfo {BasicGroupId = ChatId});
                    if (basicGroup == null) return Task.CompletedTask;
                    ChatMembers.Text = $"{basicGroup.Result.Members.Length} members";
                    break;
                }
                case TdApi.ChatType.ChatTypeSupergroup:
                {
                    var superGroup = _client.ExecuteAsync(new TdApi.GetSupergroupFullInfo {SupergroupId = ChatId});
                    if (superGroup == null) return Task.CompletedTask;
                    ChatMembers.Text = $"{superGroup.Result.MemberCount} members";
                    break;
                }
                default:
                {
                    ChatMembers.Visibility = Visibility.Collapsed;
                    break;
                }
            }*/
            
            return Task.CompletedTask;
        }
        
        public Task GetMessages(long chatId)
        {
            var messages = _client.ExecuteAsync(new TdApi.GetChatHistory
            {
                ChatId = chatId,
                Limit = 100
            });

            GenerateMessage();
            
            return Task.CompletedTask;

            void GenerateMessage()
            {
                foreach (var message in messages.Result.Messages_.Reverse())
                {
                    var chatMessage = new ChatMessage();
                    chatMessage.UpdateMessage(message);
                    MessagesList.Children.Add(chatMessage);
                    _messagesList.Add(message);

                    if (_messagesList.Count == messages.Result.Messages_.Length ||
                        _messagesList.Contains(message)) continue;
                    GenerateMessage();
                    return;
                }
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
                ChatId = ChatId,
                InputMessageContent = new TdApi.InputMessageContent.InputMessageText
                {
                    Text = new TdApi.FormattedText
                    {
                        Text = UserMessageInput.Text
                    }
                }
            });
            
            UserMessageInput.ClearValue(null);
        }

        private void Call_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var chat = _client.GetChatAsync(ChatId);
                var messageSenderId = GetId(chat.Result.MessageSenderId);

                _client.ExecuteAsync(new TdApi.CreateCall { IsVideo = false, Protocol = new TdApi.CallProtocol(), UserId = messageSenderId });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void Back_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
