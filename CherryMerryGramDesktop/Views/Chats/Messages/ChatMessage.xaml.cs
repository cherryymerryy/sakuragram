using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using CherryMerryGramDesktop.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats
{
    public sealed partial class ChatMessage : Page
    {
        private static TdClient _client = App._client;
        private bool _isContextMenuOpen = false;
        
        private long _chatId;
        public long _messageId;

        public ReplyService _replyService;
        public MessageService _messageService;

        private bool _bIsSelected = false;
        
        public ChatMessage()
        {
            InitializeComponent();
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateMessageEdited:  
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
            
            return Task.CompletedTask;
        }

        public async void UpdateMessage(TdApi.Message message)
        {
            try
            {
                _chatId = message.ChatId;
                _messageId = message.Id;

                var content = message.Content;
                var user = GetUser(message).Result;
                var chatMember = GetChatMember(message.ChatId, message.SenderId).Result;
                var chat = GetChat(message.ChatId).Result;
                var chatType = chat.Type;
                var userProfilePicture = user.ProfilePhoto;
                
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
                
                if (chat.Type is not TdApi.ChatType.ChatTypePrivate && chat.Permissions.CanSendBasicMessages)
                {
                    try
                    {
                        var userProfilePictureFile = await _client.ExecuteAsync(new TdApi.DownloadFile
                        {
                            FileId = userProfilePicture.Small.Id,
                            Priority = 1
                        });

                        ProfilePicture.ImageSource = new BitmapImage(new Uri(userProfilePictureFile.Local.Path));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    ProfilePicture.ImageSource = null;
                    BorderProfilePicture.Visibility = Visibility.Collapsed;
                }

                DisplayName.Text = chat.Type switch
                {
                    TdApi.ChatType.ChatTypePrivate => DisplayName.Text = $"{user.FirstName} {user.LastName}",
                    TdApi.ChatType.ChatTypeSecret => DisplayName.Text = $"{user.FirstName} {user.LastName}",
                    TdApi.ChatType.ChatTypeBasicGroup => DisplayName.Text = chat.Title,
                    _ => DisplayName.Text
                };

                if (chatType is TdApi.ChatType.ChatTypeSupergroup && chat.Permissions.CanSendBasicMessages)
                {
                    DisplayName.Text = $"{user.FirstName} {user.LastName}";
                }
                else if (chatType is TdApi.ChatType.ChatTypeSupergroup && !chat.Permissions.CanSendBasicMessages)
                {
                    DisplayName.Text = chat.Title;
                }
                
                Status.Visibility = chat.Type switch
                {
                    TdApi.ChatType.ChatTypePrivate => Status.Visibility = Visibility.Collapsed,
                    TdApi.ChatType.ChatTypeSecret => Status.Visibility = Visibility.Collapsed,
                    TdApi.ChatType.ChatTypeSupergroup => Status.Visibility = Visibility.Visible,
                    TdApi.ChatType.ChatTypeBasicGroup => Status.Visibility = Visibility.Collapsed,
                    _ => Status.Visibility
                };

                if (chatType is TdApi.ChatType.ChatTypeSupergroup && chat.Permissions.CanSendBasicMessages)
                {
                    Status.Text = chatMember.Status switch
                    {
                        TdApi.ChatMemberStatus.ChatMemberStatusCreator => Status.Text += " (creator)",
                        TdApi.ChatMemberStatus.ChatMemberStatusAdministrator => Status.Text += " (admin)",
                        TdApi.ChatMemberStatus.ChatMemberStatusMember => Status.Text += " (member)",
                        TdApi.ChatMemberStatus.ChatMemberStatusLeft => Status.Text += " (left)",
                        TdApi.ChatMemberStatus.ChatMemberStatusBanned => Status.Text += " (banned)",
                        _ => Status.Text
                    };
                }

                MessageContent.Text = message.Content switch
                {
                    TdApi.MessageContent.MessageText messageText => MessageContent.Text = messageText.Text.Text,
                    TdApi.MessageContent.MessagePhoto messagePhoto => MessageContent.Text = messagePhoto.Caption.Text,
                    TdApi.MessageContent.MessageVideo messageVideo => MessageContent.Text = messageVideo.Caption.Text,
                    TdApi.MessageContent.MessageUnsupported => MessageContent.Text = "Your version of CherryMerryGram does not support this type of message, make sure that you are using the latest version of the client.",
                    _ => MessageContent.Text
                };

                switch (message.Content)
                {
                    case TdApi.MessageContent.MessageText:
                    {
                        MessageContent.Visibility = Visibility.Visible;
                        MessageSticker.Visibility = Visibility.Collapsed;
                        break;
                    }
                    case TdApi.MessageContent.MessageSticker messageSticker:
                    {
                        MessageContent.Visibility = Visibility.Collapsed;
                        MessageSticker.Visibility = Visibility.Visible;
                        var stickerFile = await _client.ExecuteAsync(new TdApi.DownloadFile
                        {
                            FileId = messageSticker.Sticker.Sticker_.Id,
                            Priority = 1
                        });
                        MessageSticker.Source = new BitmapImage(new Uri(stickerFile.Local.Path));
                        break;
                    }
                }
                
                if (chat.Permissions.CanPinMessages)
                {
                    ContextMenuPin.IsEnabled = false;
                }
            }
            catch { }
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