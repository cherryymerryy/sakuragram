using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CherryMerryGramDesktop.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;
using WinRT;

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
            
            //_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
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
            var chat = _client.GetChatAsync(chatId).Result;
            ChatTitle.Text = chat.Title;
            
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
        
        public async Task GetMessages(long chatId)
        {
            var offset = 0;
            const int limit = 100;

            while (true)
            {
                var messages = await _client.ExecuteAsync(new TdApi.GetChatHistory
                {
                    ChatId = chatId,
                    Offset = offset,
                    Limit = limit
                });

                await GenerateMessage(messages);

                if (messages.Messages_.Length < limit)
                {
                    break;
                }

                offset += limit;
            }

            async Task GenerateMessage(TdApi.Messages messages)
            {
                foreach (var message in messages.Messages_)
                {
                    var chatMessage = new ChatMessage();
                    chatMessage._forwardService = _forwardService;
                    chatMessage._replyService = _replyService;
                    chatMessage.UpdateMessage(message);
                    MessagesList.Children.Add(chatMessage);
                    _messagesList.Add(message);
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
