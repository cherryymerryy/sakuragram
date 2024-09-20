using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using CherryMerryGramDesktop.Services;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats
{
    public sealed partial class ChatTextMessage : Page
    {
        private static TdClient _client = App._client;
        private bool _isContextMenuOpen = false;
        
        private long _chatId;
        public long _messageId;
        private int _profilePhotoFileId;

        public ReplyService _replyService;
        public MessageService _messageService;

        private bool _bIsSelected = false;
        
        public ChatTextMessage()
        {
            InitializeComponent();
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateMessageEdited:
                {
                    var message = _client.ExecuteAsync(new TdApi.GetMessage
                    {
                        ChatId = _chatId,
                        MessageId = _messageId
                    });

                    MessageContent.Text = message.Result.Content switch
                    {
                        TdApi.MessageContent.MessageText messageText => MessageContent.Text = messageText.Text.Text,
                        _ => MessageContent.Text
                    };
                    break;
                }
                case TdApi.Update.UpdateFile updateFile:
                {
                    if (updateFile.File.Id != _profilePhotoFileId) return Task.CompletedTask;
                    ProfilePicture.DispatcherQueue.TryEnqueue(() =>
                    {
                        if (updateFile.File.Local.Path != "")
                        {
                            ProfilePicture.ProfilePicture = new BitmapImage(new Uri(updateFile.File.Local.Path));
                        }
                        else
                        {
                            ProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, 
                                () => ProfilePicture.DisplayName = GetUser(
                                    _client.GetMessageAsync(chatId: _chatId, messageId: _messageId
                                        ).Result
                                    ).Result.FirstName);
                        }
                    });
                    break;
                }
            }
            
            return Task.CompletedTask;
        }

        public void UpdateMessage(TdApi.Message message)
        {
            _chatId = message.ChatId;
            _messageId = message.Id;
            
            if (message.ReplyTo != null)
            {
                var replyMessage = _client.ExecuteAsync(new TdApi.GetRepliedMessage
                {
                    ChatId = message.ChatId,
                    MessageId = _messageId
                }).Result;
                
                var replyUserId = replyMessage.SenderId switch {
                    TdApi.MessageSender.MessageSenderUser u => u.UserId,
                    TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                    _ => 0
                };
                
                var replyUser = _client.ExecuteAsync(new TdApi.GetUser{
                    UserId = replyUserId
                }).Result;
                
                ReplyFirstName.Text = $"{replyUser.FirstName} {replyUser.LastName}";
                ReplyInputContent.Text  = replyMessage.Content switch
                {
                    TdApi.MessageContent.MessageText messageText => ReplyInputContent.Text = messageText.Text.Text,
                    _ => ReplyInputContent.Text
                };
                
                Reply.Visibility = Visibility.Visible;
            }
            
            var sender = message.SenderId switch
            {
                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                _ => 0
            };

            if (sender > 0) // if senderId > 0 then it's a user
            {
                var user = _client.GetUserAsync(userId: sender).Result;
                DisplayName.Text = user.FirstName + " " + user.LastName;
                GetChatPhoto(user);
            }
            else // if senderId < 0 then it's a chat
            {
                var chat = _client.GetChatAsync(chatId: sender).Result;
                DisplayName.Text = chat.Title;
                ProfilePicture.Visibility = Visibility.Collapsed;
            }

            if (message.ForwardInfo != null)
            {
                if (message.ForwardInfo.Source != null)
                {
                    TextBlockForwardInfo.Text = $"Forwarded from {message.ForwardInfo.Source.SenderName}";
                    TextBlockForwardInfo.Visibility = Visibility.Visible;
                }
                else if (message.ForwardInfo.Origin != null)
                {
                    switch (message.ForwardInfo.Origin)
                    {
                        case TdApi.MessageOrigin.MessageOriginChannel channel:
                        {
                            string forwardInfo = string.Empty;
                            var chat = _client.GetChatAsync(chatId: channel.ChatId).Result;

                            forwardInfo = chat.Title;

                            if (channel.AuthorSignature != string.Empty)
                            {
                                forwardInfo = forwardInfo + $" ({channel.AuthorSignature})";
                            }
                            
                            TextBlockForwardInfo.Text = $"Forwarded from {forwardInfo}";
                            TextBlockForwardInfo.Visibility = Visibility.Visible;
                            break;
                        }
                        case TdApi.MessageOrigin.MessageOriginChat chat:
                        {
                            TextBlockForwardInfo.Text = $"Forwarded from {chat.AuthorSignature}";
                            TextBlockForwardInfo.Visibility = Visibility.Visible;
                            break;
                        }
                        case TdApi.MessageOrigin.MessageOriginUser user:
                        {
                            var originUser = _client.GetUserAsync(userId: user.SenderUserId).Result;
                            TextBlockForwardInfo.Text = $"Forwarded from {originUser.FirstName} {originUser.LastName}";
                            TextBlockForwardInfo.Visibility = Visibility.Visible;
                            break;
                        }
                        case TdApi.MessageOrigin.MessageOriginHiddenUser hiddenUser:
                        {
                            TextBlockForwardInfo.Text = $"Forwarded from {hiddenUser.SenderName}";
                            TextBlockForwardInfo.Visibility = Visibility.Visible;
                            break;
                        }
                    }
                }
            }
            else
            {
                TextBlockForwardInfo.Text = string.Empty;
                TextBlockForwardInfo.Visibility = Visibility.Collapsed;
            }
            
            switch (message.Content)
            {
                case TdApi.MessageContent.MessageText messageText:
                    MessageContent.Text = messageText.Text.Text;
                    break;
                case TdApi.MessageContent.MessageUnsupported:
                    MessageContent.Text = "Your version of CherryMerryGram does not support this type of message, make sure that you are using the latest version of the client.";
                    break;
                default:
                    MessageContent.Text = "Unsupported message type";
                    break;
            }
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }
        
        private void GetChatPhoto(TdApi.User user)
        {
            if (user.ProfilePhoto == null)
            {
                ProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, 
                    () => ProfilePicture.DisplayName = user.FirstName + " " + user.LastName);
                return;
            }
            if (user.ProfilePhoto.Big.Local.Path != "")
            {
                try
                {
                    ProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                        () => ProfilePicture.ProfilePicture = new BitmapImage(new Uri(user.ProfilePhoto.Big.Local.Path)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                _profilePhotoFileId = user.ProfilePhoto.Big.Id;
                
                var file = _client.ExecuteAsync(new TdApi.DownloadFile
                {
                    FileId = _profilePhotoFileId,
                    Priority = 1
                }).Result;
            }
        }
        
        private static Task<TdApi.User> GetUser(TdApi.Message message)
        {
            long userId = message.SenderId switch {
                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                _ => 0
            };
            
            var user = _client.ExecuteAsync(new TdApi.GetUser
            {
                UserId = userId
            });

            return user;
        }

        private static Task<TdApi.ChatMember> GetChatMember(long chatId, TdApi.MessageSender user)
        {
            var chatMember = _client.ExecuteAsync(new TdApi.GetChatMember
            {
                ChatId = chatId,
                MemberId = user
            });
            
            return chatMember;
        }

        private static Task<TdApi.Chat> GetChat(long chatId)
        {
            var chat = _client.ExecuteAsync(new TdApi.GetChat
            {
                ChatId = chatId
            });
            
            return chat;
        }

        private void ShowMenu(bool isTransient)
        {
            _isContextMenuOpen = isTransient;
            FlyoutShowOptions myOption = new FlyoutShowOptions();
            myOption.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;
            CommandBarFlyout1.ShowAt(Message, myOption);
        }
        
        private void UIElement_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowMenu(!_isContextMenuOpen);
        }

        private void Reply_OnClick(object sender, RoutedEventArgs e)
        {
            _replyService.SelectMessageForReply(_messageId);
        }

        private async void Forward_OnClick(object sender, RoutedEventArgs e)
        {
            if (_messageService.GetSelectedMessages().Length == 0)
            {
                _messageService.SelectMessage(_messageId);
            }
            await ForwardMessageList.ShowAsync();
        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            _client.ExecuteAsync(new TdApi.EditMessageText
            {

            });
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            DeleteMessagesConfirmation.ShowAsync();
        }

        private void Pin_OnClick(object sender, RoutedEventArgs e)
        {
            PinMessageConfirmation.ShowAsync();
        }

        private void MessageLink_OnClick(object sender, RoutedEventArgs e)
        {
            var messageLink = _client.GetMessageLinkAsync(_chatId, _messageId);
            var dataPackage = new DataPackage();
            
            dataPackage.SetText(messageLink.Result.Link);
            Clipboard.SetContent(dataPackage);
            
            ShowMenu(false);
        }

        private void Select_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_bIsSelected)
            {
                _messageService.SelectMessage(messageId: _messageId);
                MessageBackground.Background = new SolidColorBrush(Colors.Gray);
                _bIsSelected = true;
            }
            else
            {
                _messageService.DeselectMessage(messageId: _messageId);
                MessageBackground.Background = new SolidColorBrush(Colors.Black);
                _bIsSelected = false;
            }
        }

        private void Report_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void ChatMessage_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _replyService.SelectMessageForReply(_messageId);
        }

        private async void ForwardMessageList_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            var chats = _client.ExecuteAsync(new TdApi.GetChats 
                { ChatList = new TdApi.ChatList.ChatListMain(), Limit = 100 }).Result;
            
            foreach (var chatId in chats.ChatIds)
            {
                var chat = await _client.ExecuteAsync(new TdApi.GetChat
                {
                    ChatId = chatId
                });
                
                if (!chat.Permissions.CanSendBasicMessages) continue;
                
                var chatEntry = new ChatEntryForForward
                {
                    _fromChatId = _chatId,
                    _messageIds = _messageService.GetSelectedMessages()
                };
                ChatList.Children.Add(chatEntry);
                chatEntry.UpdateEntry(chat);
            }
        }

        private void PinMessageConfirmation_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _client.PinChatMessageAsync(_chatId, _messageId, NotifyAllMembers.IsChecked.Value);
        }

        private void DeleteMessagesConfirmation_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_messageService._isMessageSelected)
            {
                _client.ExecuteAsync(new TdApi.DeleteMessages
                {
                    ChatId = _chatId,
                    MessageIds = _messageService.GetSelectedMessages(),
                    Revoke = Revoke.IsChecked.Value
                });
            }
            else
            {
                _client.ExecuteAsync(new TdApi.DeleteMessages
                {
                    ChatId = _chatId,
                    MessageIds = new long[] { _messageId },
                    Revoke = Revoke.IsChecked.Value
                });
            }
        }

        private void ChatMessage_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (_messageService._isMessageSelected) Select_OnClick(null, null);
        }
    }
}
