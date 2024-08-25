using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CherryMerryGramDesktop.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats
{
    public sealed partial class Chat : Page
    {
        private static TdClient _client = App._client;
        private static TdApi.Chat _chat;
        private List<TdApi.Message> _messagesList = [];
        
        public long _chatId;
        private int _backgroundId;
        
        private ReplyService _replyService;
        private MessageService _messageService;
        
        public Chat()
        {
            InitializeComponent();

            _replyService = new ReplyService();
            _messageService = new MessageService();
            
            //var pinnedMessage = _client.ExecuteAsync(new TdApi.GetChatPinnedMessage {ChatId = ChatId});
        }

        private async Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateNewMessage updateNewMessage:
                {
                    if (updateNewMessage.Message.ChatId == _chatId)
                    {
                        MessagesList.DispatcherQueue.TryEnqueue(() =>
                        {
                            var chatMessage = new ChatMessage
                            {
                                _replyService = _replyService,
                                _messageService = _messageService
                            };
                            chatMessage.UpdateMessage(updateNewMessage.Message);
                            MessagesList.Children.Add(chatMessage);
                            _messagesList.Add(updateNewMessage.Message);
                        });
                    }
                    break;
                }
                case TdApi.Update.UpdateChatTitle updateChatTitle:
                {
                    ChatTitle.DispatcherQueue.TryEnqueue(() => UpdateChatTitle(updateChatTitle.Title));
                    break;
                }
                case TdApi.Update.UpdateUserStatus updateUserStatus:
                {
                    if (_chat.Type is TdApi.ChatType.ChatTypePrivate)
                    {
                        ChatMembers.DispatcherQueue.TryEnqueue(() =>
                        {
                            ChatMembers.Text = updateUserStatus.Status switch
                            {
                                TdApi.UserStatus.UserStatusOnline => "Online",
                                TdApi.UserStatus.UserStatusOffline => "Offline",
                                TdApi.UserStatus.UserStatusRecently => "Recently",
                                TdApi.UserStatus.UserStatusLastWeek => "Last week",
                                TdApi.UserStatus.UserStatusLastMonth => "Last month",
                                TdApi.UserStatus.UserStatusEmpty => "A long time",
                                _ => "Unknown"
                            };
                        });
                    }
                    break;
            }
                case TdApi.Update.UpdateChatOnlineMemberCount updateChatOnlineMemberCount:
                {
                    if (_chat.Type is TdApi.ChatType.ChatTypeSupergroup or TdApi.ChatType.ChatTypeBasicGroup &&
                        _chat.Permissions.CanSendBasicMessages)
                    {
                        ChatMembers.DispatcherQueue.TryEnqueue(() =>
                        {
                            ChatMembers.Text =
                                $"{ChatMembers.Text}, {updateChatOnlineMemberCount.OnlineMemberCount} online";
                        });   
                    }
                    break;
                }
            }
        }

        public async void UpdateChat(long chatId)
        {
            var chat = _client.GetChatAsync(chatId).Result;

            switch (chat.Type)
            {
                case TdApi.ChatType.ChatTypePrivate typePrivate:
                    var user = _client.GetUserAsync(userId: typePrivate.UserId).Result;
                    ChatMembers.Text = user.Status switch
                    {
                        TdApi.UserStatus.UserStatusOnline => "Online",
                        TdApi.UserStatus.UserStatusOffline => "Offline",
                        TdApi.UserStatus.UserStatusRecently => "Recently",
                        TdApi.UserStatus.UserStatusLastWeek => "Last week",
                        TdApi.UserStatus.UserStatusLastMonth => "Last month",
                        TdApi.UserStatus.UserStatusEmpty => "A long time",
                        _ => "Unknown"
                    };
                    break;
                case TdApi.ChatType.ChatTypeBasicGroup typeBasicGroup:
                    var basicGroupInfo = _client.GetBasicGroupFullInfoAsync(
                        basicGroupId: typeBasicGroup.BasicGroupId
                        ).Result;
                    ChatMembers.Text = basicGroupInfo.Members.Length + " members";
                    break;
                case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                    var supergroupInfo = _client.GetSupergroupFullInfoAsync(
                        supergroupId: typeSupergroup.SupergroupId)
                        .Result;
                    ChatMembers.Text = supergroupInfo.MemberCount + " members";
                    break;
            }
            
            _chatId = chatId;
            ChatTitle.Text = chat.Title;
            
            _chat = _client.ExecuteAsync(new TdApi.GetChat {ChatId = _chatId}).Result;
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
            await GetMessagesAsync(chatId);
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
                    _replyService = _replyService,
                    _messageService = _messageService
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
                    ChatId = _chatId,
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
                _replyService.ReplyOnMessage(_chatId, _replyService.GetReplyMessageId(), UserMessageInput.Text);
            }
            
            UserMessageInput.ClearValue(TextBox.TextProperty);
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

        private void Back_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
