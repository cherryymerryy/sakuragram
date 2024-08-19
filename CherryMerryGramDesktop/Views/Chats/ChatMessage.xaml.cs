using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using CherryMerryGramDesktop.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats
{
    public sealed partial class ChatMessage : Page
    {
        private static TdClient _client = App._client;
        private bool _isContextMenuOpen = false;
        
        private long _chatId;
        private long _messageId;

        public ForwardService _forwardService;
        
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
            _chatId = message.ChatId;
            _messageId = message.Id;
            
            var user = GetUser(message).Result;
            var chatMember = GetChatMember(message.ChatId, message.SenderId).Result;
            var chat = GetChat(message.ChatId).Result;
            var chatType = chat.Type;
            var userProfilePicture = user.ProfilePhoto;

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

            //DisplayName.Foreground = user.AccentColorId;

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
                _ => MessageContent.Text
            };
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

        private void UIElement_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void Reply_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void Forward_OnClick(object sender, RoutedEventArgs e)
        {
            _forwardService.ForwardMessages(-4214922794, _chatId);
        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            _client.ExecuteAsync(new TdApi.EditMessageText
            {

            });
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            _client.ExecuteAsync(new TdApi.DeleteMessages
            {
                ChatId = _chatId,
                MessageIds = new[] { _messageId }
            });
        }

        private void Pin_OnClick(object sender, RoutedEventArgs e)
        {
            _client.PinChatMessageAsync(_chatId, _messageId, true);
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
            _forwardService.SelectMessageToForward(_messageId);
        }

        private void Report_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
