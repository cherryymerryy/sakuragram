using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Windows.System;
using CherryMerryGramDesktop.Services;
using CherryMerryGramDesktop.Views.Calls;
using CherryMerryGramDesktop.Views.Chats.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        private List<TdApi.FormattedText> _pollOptionsList = [];
        
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
                            GenerateMessageByType(updateNewMessage.Message);
                            
                            // For debug
                            // var message = new ChatDebugMessage();
                            // MessagesList.Children.Add(message);
                            // message.UpdateMessage(updateNewMessage.Message);
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
                    // if (updateDeleteMessages.ChatId == _chatId)
                    // {
                    //     var messages = MessagesList.Children;
                    //     var messagesToRemove = messages.OfType<ChatMessage>();
                    //     
                    //     foreach (var messageId in updateDeleteMessages.MessageIds)
                    //     {
                    //         MessagesList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
                    //         {
                    //             foreach (var messageToRemove in messagesToRemove)
                    //             {
                    //                 if (messageToRemove._messageId == messageId)
                    //                 {
                    //                     MessagesList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low,
                    //                         () => MessagesList.Children.Remove(messageToRemove));
                    //                 }
                    //             }
                    //         });
                    //     }
                    // }
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
                    try
                    {
                        var supergroup = _client.GetSupergroupAsync(
                                supergroupId: typeSupergroup.SupergroupId)
                            .Result;
                        var supergroupInfo = _client.GetSupergroupFullInfoAsync(
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
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    break;
            }

            _client.ExecuteAsync(new TdApi.OpenChat { ChatId = chatId });
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }

        private void UpdateChatMembersText()
        {
            ChatMembers.Text = _onlineMemberCount > 0 ? $"{_memberCount} members, {_onlineMemberCount} online" : 
                $"{_memberCount} members";
        }

        private void GetMessagesAsync(long chatId)
        {
            var messages = _client.ExecuteAsync(new TdApi.GetChatHistory
            {
                ChatId = chatId,
                Offset = -99,
                Limit = 100,
                OnlyLocal = false
            }).Result;

            foreach (var message in messages.Messages_)
            {
                GenerateMessageByType(message);
            }
        }

        private void GenerateMessageByType(TdApi.Message message)
        {
            switch (message.Content)
            {
                case TdApi.MessageContent.MessageText:
                {
                    var textMessage = new ChatTextMessage();
                    textMessage._messageService = _messageService;
                    MessagesList.Children.Add(textMessage);
                    textMessage.UpdateMessage(message);
                    break;
                }
                case TdApi.MessageContent.MessageChatChangeTitle or TdApi.MessageContent.MessagePinMessage 
                    or TdApi.MessageContent.MessageGiftedPremium
                    or TdApi.MessageContent.MessageGameScore or TdApi.MessageContent.MessageChatBoost:
                {
                    var changeTitleMessage = new ChatServiceMessage();
                    MessagesList.Children.Add(changeTitleMessage);
                    changeTitleMessage.UpdateMessage(message);
                    break;
                }
                case TdApi.MessageContent.MessageSticker or TdApi.MessageContent.MessageAnimation:
                {
                    var stickerMessage = new ChatStickerMessage();
                    MessagesList.Children.Add(stickerMessage);
                    stickerMessage.UpdateMessage(message);
                    break;
                }
                case TdApi.MessageContent.MessageVideo:
                {
                    var videoMessage = new ChatVideoMessage();
                    MessagesList.Children.Add(videoMessage);
                    videoMessage.UpdateMessage(message);
                    break;
                }
                case TdApi.MessageContent.MessagePhoto:
                {
                    var photoMessage = new ChatPhotoMessage();
                    MessagesList.Children.Add(photoMessage);
                    photoMessage.UpdateMessage(message);
                    break;
                }
                case TdApi.MessageContent.MessageDocument:
                {
                    var documentMessage = new ChatDocumentMessage();
                    MessagesList.Children.Add(documentMessage);
                    documentMessage.UpdateMessage(message);
                    break;
                }
                case TdApi.MessageContent.MessageVideoNote:
                {
                    var videoNoteMessage = new ChatVideoNoteMessage();
                    MessagesList.Children.Add(videoNoteMessage);
                    videoNoteMessage.UpdateMessage(message);
                    break;
                }
                case TdApi.MessageContent.MessageVoiceNote:
                {
                    var voiceNoteMessage = new ChatVoiceNoteMessage();
                    MessagesList.Children.Add(voiceNoteMessage);
                    voiceNoteMessage.UpdateMessage(message);
                    break;
                }
            }
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

        private void MessagesList_OnLoaded(object sender, RoutedEventArgs e)
        {
            GetMessagesAsync(_chatId);
        }
        
        private void ContextMenuNotifications_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ContextMenuViewGroupInfo_OnClick(object sender, RoutedEventArgs e)
        {
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
            if (PollQuestion.Text == string.Empty) return;
            
            try
            {
                _pollOptionsList.Add(new TdApi.FormattedText {Text = DefaultPollOption.Text});
                _pollOptionsList.Add(new TdApi.FormattedText {Text = DefaultSecondaryPollOption.Text});
                
                foreach (var pollOption in AdditionalPollOptions.Children.OfType<TextBox>())
                {
                    _pollOptionsList.Add(new TdApi.FormattedText {Text = pollOption.Text});
                }
                
                _client.ExecuteAsync(new TdApi.SendMessage
                {
                    ChatId = _chatId,
                    InputMessageContent = new TdApi.InputMessageContent.InputMessagePoll
                    {
                        Type = QuizMode.IsChecked.Value ? new TdApi.PollType.PollTypeQuiz
                        {
                            CorrectOptionId = Convert.ToInt32(TextBoxCorrectAnswer.Text) - 1
                        } : new TdApi.PollType.PollTypeRegular
                        {
                            AllowMultipleAnswers = MultipleAnswers.IsChecked.Value
                        },
                        Question = new TdApi.FormattedText { Text = PollQuestion.Text },
                        Options = _pollOptionsList.ToArray(),
                        IsClosed = false,
                        IsAnonymous = AnonymousVoting.IsChecked.Value,
                    }
                });
                
                _pollOptionsList.Clear();
                AdditionalPollOptions.Children.Clear();
                DefaultPollOption.Text = string.Empty;
                DefaultSecondaryPollOption.Text = string.Empty;
            }
            catch (TdException e)
            {
                CreatePoll.Hide();
                TextBlockException.Text = e.Message;
                ExceptionDialog.ShowAsync();
                throw;
            }
        }

        private void CreatePoll_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            if (_pollOptionsCount >= 10) return;

            DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => {
                var newPollOption = new TextBox
                {
                    PlaceholderText = "Add an option",
                    Margin = new Thickness(0, 0, 0, 4)
                };
                
                _pollOptionsCount += 1;
                AdditionalPollOptions.Children.Add(newPollOption);
            });
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

        private void MessagesScrollViewer_OnViewChanging_(object sender, ScrollViewerViewChangingEventArgs e)
        {
        }

        private void Chat_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Enter when UserMessageInput.Text != "":
                    SendMessage_OnClick(sender, null);
                    break;
                case VirtualKey.Escape:
                    CloseChat();
                    break;
            }
        }

        private void ButtonAttachMedia_OnClick(object sender, RoutedEventArgs e)
        {
            ContextMenuMedia.ShowAt(ButtonAttachMedia);
        }

        private void ContextMenuPhotoOrVideo_OnClick(object sender, RoutedEventArgs e)
        {
            // SendMediaMessage.ShowAsync();
        }

        private void ContextMenuFile_OnClick(object sender, RoutedEventArgs e)
        {
            // SendFileMessage.ShowAsync();
        }

        private void ContextMenuPoll_OnClick(object sender, RoutedEventArgs e)
        {
            CreatePoll.ShowAsync();
        }

        private void QuizMode_OnChecked(object sender, RoutedEventArgs e)
        {
            TextBoxCorrectAnswer.IsEnabled = QuizMode.IsChecked.Value;
            MultipleAnswers.IsEnabled = !QuizMode.IsChecked.Value;
        }
    }
}