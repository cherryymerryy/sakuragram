using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using CherryMerryGramDesktop.Services;
using CherryMerryGramDesktop.Views.Calls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using TdLib;
using DispatcherQueuePriority = Microsoft.UI.Dispatching.DispatcherQueuePriority;

namespace CherryMerryGramDesktop.Views.Chats
{
    public sealed partial class Chat : Page
    {
        private static TdClient _client = App._client;
        private static TdApi.Chat _chat;
        public ChatsView _ChatsView;
        private List<TdApi.Message> _messagesList = [];
        
        public long _chatId;
        private int _backgroundId;
        private int _memberCount;
        private int _onlineMemberCount;
        private int _offset;
        private int _pollOptionsCount = 1;
        
        private ReplyService _replyService;
        private MessageService _messageService;

        public Chat()
        {
            InitializeComponent();

            _replyService = new ReplyService();
            _messageService = new MessageService();
            
            #if DEBUG
            {
                CreateVideoCall.IsEnabled = true;
            }
            #else
            {
                CreateVideoCall.IsEnabled = false;
            }
            #endif
            
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
                        MessagesList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
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
                    ChatTitle.DispatcherQueue.TryEnqueue(() => ChatTitle.Text = updateChatTitle.Title);
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
                        _onlineMemberCount = updateChatOnlineMemberCount.OnlineMemberCount;
                        ChatMembers.DispatcherQueue.TryEnqueue(UpdateChatMembersText);   
                    }
                    break;
                }
                case TdApi.Update.UpdateBasicGroup updateBasicGroup:
                {
                    if (updateBasicGroup.BasicGroup.Id == _chatId)
                    {
                        _memberCount = updateBasicGroup.BasicGroup.MemberCount;
                        ChatMembers.DispatcherQueue.TryEnqueue(UpdateChatMembersText);
                    }
                    break;
                }
                case TdApi.Update.UpdateSupergroup updateSupergroup:
                {
                    if (updateSupergroup.Supergroup.Id == _chatId)
                    {
                        _memberCount = updateSupergroup.Supergroup.MemberCount;
                        ChatMembers.DispatcherQueue.TryEnqueue(UpdateChatMembersText);
                    }
                    break;
                }
                case TdApi.Update.UpdateDeleteMessages updateDeleteMessages:
                {
                    if (updateDeleteMessages.ChatId == _chatId)
                    {
                        var messages = MessagesList.Children;
                        var messagesToRemove = messages.OfType<ChatMessage>();
                        
                        foreach (var messageId in updateDeleteMessages.MessageIds)
                        {
                            MessagesList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
                            {
                                foreach (var messageToRemove in messagesToRemove)
                                {
                                    if (messageToRemove._messageId == messageId)
                                    {
                                        MessagesList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low,
                                            () => MessagesList.Children.Remove(messageToRemove));
                                    }
                                }
                            });
                        }
                    }
                    break;
                } 
            }
        }

        public void UpdateChat(long chatId)
        {
            var chat = _client.GetChatAsync(chatId).Result;
            _chat = chat;
            _chatId = chatId;
            ChatTitle.Text = chat.Title;

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
                    var supergroup = _client.GetSupergroupAsync(
                        supergroupId: typeSupergroup.SupergroupId)
                        .Result;

                    if (supergroup.IsChannel)
                    {
                        UserActionsPanel.Visibility = supergroup.Status switch
                        {
                            TdApi.ChatMemberStatus.ChatMemberStatusCreator => Visibility.Visible,
                            TdApi.ChatMemberStatus.ChatMemberStatusAdministrator => Visibility.Visible,
                            TdApi.ChatMemberStatus.ChatMemberStatusMember => Visibility.Collapsed,
                            TdApi.ChatMemberStatus.ChatMemberStatusBanned => Visibility.Collapsed,
                            TdApi.ChatMemberStatus.ChatMemberStatusRestricted => Visibility.Collapsed,
                            TdApi.ChatMemberStatus.ChatMemberStatusLeft => Visibility.Collapsed,
                            _ => Visibility.Collapsed
                        };
                    }
                    
                    ChatMembers.Text = supergroupInfo.MemberCount + " members";
                    break;
            }
            
            _client.ExecuteAsync(new TdApi.OpenChat {ChatId = chatId});
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }

        private void UpdateChatMembersText()
        {
            ChatMembers.Text = _onlineMemberCount > 0 ? $"{_memberCount} members, {_onlineMemberCount} online" : 
                $"{_memberCount} members";
        }

        private async Task GetMessagesAsync(long chatId)
        {
            var offset = 0;
            chatId = _chatId;
            bool moreMessages = true;

            while (moreMessages)
            {
                var messages = await _client.ExecuteAsync(new TdApi.GetChatHistory
                {
                    ChatId = chatId,
                    Limit = 100,
                    Offset = offset,
                    FromMessageId = _chat.LastMessage.Id,
                    OnlyLocal = false
                });

                if (messages.Messages_.Length == 0)
                {
                    moreMessages = false;
                    break;
                }

                offset -= messages.Messages_.Length;

                var chatMessages = messages.Messages_.Reverse().Select(message =>
                {
                    var chatMessage = new ChatMessage
                    {
                        _replyService = _replyService,
                        _messageService = _messageService
                    };
                    chatMessage.UpdateMessage(message);
                    return chatMessage;
                }).ToList();

                MessagesList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
                {
                    foreach (var chatMessage in chatMessages)
                    {
                        MessagesList.Children.Add(chatMessage);
                    }

                    _messagesList.AddRange(messages.Messages_);
                });
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

        private void MoreActions_OnClick(object sender, RoutedEventArgs e)
        {
            ContextMenu.ShowAt(MoreActions);
        }

        private void Back_OnClick(object sender, RoutedEventArgs e)
        {
            CloseChat();
        }
        
        public void CloseChat()
        {
            if (MessagesList.Children.Count > 0) MessagesList.Children.Clear();
            _client.ExecuteAsync(new TdApi.CloseChat {ChatId = _chatId});
            _ChatsView.CloseChat();
        }

        private async void MessagesList_OnLoaded(object sender, RoutedEventArgs e)
        {
            await GetMessagesAsync(_chatId);
        }

        private void UserMessageInput_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && UserMessageInput.Text != "")
            {
                SendMessage_OnClick(sender, null);
            }
        }
        
        private void ContextMenuNotifications_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ContextMenuViewGroupInfo_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ContextMenuToBeginning_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ContextMenuManageGroup_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ContextMenuBoostGroup_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ContextMenuCreatePoll_OnClick(object sender, RoutedEventArgs e)
        {
            CreatePoll.ShowAsync();
        }

        private void ContextMenuReport_OnClick(object sender, RoutedEventArgs e)
        {
            _client.ExecuteAsync(new TdApi.ReportChat
            {
                ChatId = _chatId, 
                Reason = new TdApi.ReportReason.ReportReasonCopyright()
            });
        }

        private void ContextMenuClearHistory_OnClick(object sender, RoutedEventArgs e)
        {
            _client.ExecuteAsync(new TdApi.DeleteChatHistory
            {
                ChatId = _chatId, RemoveFromChatList = false, Revoke = true
            });
        }

        private void ContextMenuLeaveGroup_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CreatePoll_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void CreatePoll_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_pollOptionsCount >= 10) return;
            _pollOptionsCount += 1;
            var NewPollOption = new TextBox();
            NewPollOption.PlaceholderText = "Add an option";
            PollOptions.Children.Add(NewPollOption);
        }

        private void SearchMessages_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CreateVideoCall_OnClick(object sender, RoutedEventArgs e)
        {
            var videoCall = new VoiceCall();
            videoCall.Activate();
        }
    }
}
