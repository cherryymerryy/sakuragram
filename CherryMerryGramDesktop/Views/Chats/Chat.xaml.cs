using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using CherryMerryGramDesktop.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats
{
    public sealed partial class Chat : Page
    {
        private static TdClient _client = App._client;
        public long ChatId;
        private List<TdApi.Message> _messagesList = [];
        
        private ForwardService _forwardService;
        private ReplyService _replyService;
        
        public Chat()
        {
            InitializeComponent();

            _forwardService = new ForwardService();
            _replyService = new ReplyService();
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }

        private async Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateNewMessage updateNewMessage:
                {
                    MessagesList.DispatcherQueue.TryEnqueue(() => _ = GetMessagesAsync(updateNewMessage.Message.ChatId));
                    break;
                }
                case TdApi.Update.UpdateChatTitle updateChatTitle:
                {
                    ChatTitle.DispatcherQueue.TryEnqueue(() => UpdateChatTitle(updateChatTitle.Title));
                    break;
                }
            }
        }

        public Task UpdateChat(long chatId)
        {
            var chat = _client.GetChatAsync(chatId).Result;
            ChatTitle.Text = chat.Title;
            ChatMembers.Text = chat.Type.ToString();
            
            /*_client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = (int)chat.Background.Background.Id, 
                Priority = 1
            });
            
            ThemeBackground.ImageSource = new BitmapImage();

            switch (chat.Type)
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

        public Task UpdateChatTitle(string newTitle)
        {
            ChatTitle.Text = newTitle;
            return Task.CompletedTask;
        }
        
        public async Task GetMessagesAsync(long chatId)
        {
            var messages = await _client.ExecuteAsync(new TdApi.GetChatHistory
            {
                ChatId = chatId,
                Limit = 100
            });

            foreach (var message in messages.Messages_.Reverse())
            {
                var chatMessage = new ChatMessage
                {
                    _forwardService = _forwardService,
                    _replyService = _replyService
                };
                chatMessage.UpdateMessage(message);
                MessagesList.Children.Add(chatMessage);
                _messagesList.Add(message);
            }
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
            if (_replyService.GetReplyMessageId() == 0)
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
            else
            {
                _replyService.ReplyOnMessage(ChatId, _replyService.GetReplyMessageId(), UserMessageInput.Text);
            }
            
            UserMessageInput.ClearValue(TextBox.TextProperty);
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
