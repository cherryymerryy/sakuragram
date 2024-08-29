using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats
{
    public sealed partial class ChatEntry : Button
    {
        public Grid ChatPage;
        private static Chat _chatWidget;

        private static readonly TdClient _client = App._client;
        private bool _isContextMenuOpen = false;
        
        public TdApi.Chat Chat;
        public long ChatId;
        
        public ChatEntry()
        {
            InitializeComponent();
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                // case TdApi.Update.UpdateChatLastMessage:
                // {
                //     TextBlockChatLastMessage.DispatcherQueue.TryEnqueue(() => GetLastMessage(_client.GetChatAsync(ChatId).Result));
                //     break;
                // }
                // case TdApi.Update.UpdateChatReadInbox updateChatReadInbox:
                // {
                //     UnreadMessagesCount.DispatcherQueue.TryEnqueue(() =>
                //     {
                //         if (Chat.UnreadCount > 0)
                //         {
                //             UnreadMessagesCount.Visibility = Visibility.Visible;
                //             UnreadMessagesCount.Value = updateChatReadInbox.UnreadCount;
                //         }
                //         else
                //         {
                //             UnreadMessagesCount.Visibility = Visibility.Collapsed;
                //         } 
                //     });
                //     break;
                // }
                case TdApi.Update.UpdateChatTitle updateChatTitle:
                {
                    TextBlockChatName.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, 
                        () => TextBlockChatName.Text = updateChatTitle.Title);
                    break;
                }
                case TdApi.Update.UpdateChatPhoto:
                {
                    ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(() => GetChatPhoto(Chat));
                    break;
                }
                // case TdApi.Update.UpdateChatAddedToList:
                // {
                //     TextBlockChatName.Text = Chat.Title;
                //
                //     GetChatPhoto(Chat);
                //     GetLastMessage(_client.GetChatAsync(ChatId).Result);
                //
                //     if (Chat.UnreadCount > 0)
                //     {
                //         if (UnreadMessagesCount.Visibility == Visibility.Collapsed)
                //             UnreadMessagesCount.Visibility = Visibility.Visible;
                //         UnreadMessagesCount.Value = Chat.UnreadCount;
                //     }
                //     else
                //     {
                //         UnreadMessagesCount.Visibility = Visibility.Collapsed;
                //     }
                //
                //     break;
                // }
            }

            return Task.CompletedTask;
        }

        public void UpdateChatInfo()
        {
            if (Chat == null) return;
            
            TextBlockChatName.Text = Chat.Title;
            
            GetChatPhoto(Chat);
            GetLastMessage(_client.GetChatAsync(ChatId).Result); 
                    
            if (Chat.UnreadCount > 0)
            {
                if (UnreadMessagesCount.Visibility == Visibility.Collapsed) UnreadMessagesCount.Visibility = Visibility.Visible;
                UnreadMessagesCount.Value = Chat.UnreadCount;
            }
            else
            {
                UnreadMessagesCount.Visibility = Visibility.Collapsed;
            }
        }
        
        private async void GetChatPhoto(TdApi.Chat chat)
        {
            try
            {
                var chatPhoto = await _client.ExecuteAsync(new TdApi.DownloadFile
                {
                    FileId = chat.Photo.Small.Id,
                    Priority = 1
                });
                
                ChatEntryProfilePicture.ImageSource = new BitmapImage(new Uri(chatPhoto.Local.Path));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static string GetMessageText(long chatId, long messageId)
        {
            if (_client.ExecuteAsync(new TdApi.GetMessage
                {
                    ChatId = chatId,
                    MessageId = messageId
                }).Result.Content is not TdApi.MessageContent.MessageText message) return null;
            var messageText = message.Text.Text;
            return messageText;
        }
        
        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (ChatPage != null && _chatWidget != null && _chatWidget._chatId != ChatId)
            {
                ChatPage.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, 
                    () => ChatPage.Children.Remove(_chatWidget));
                _chatWidget = null;
                ChatPage.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, 
                    () => ChatPage.Children.Remove(_chatWidget));
                _chatWidget = null;
            }
            
            _chatWidget = new Chat
            {
                _chatId = ChatId
            };
            
            _chatWidget.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => _chatWidget.UpdateChat(Chat.Id));
            ChatPage?.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => ChatPage.Children.Add(_chatWidget));
        }

        private static Task<TdApi.User> GetUser(TdApi.Message message)
        {
            var userId = message.SenderId switch {
                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                _ => 0
            };
            
            var user = _client.ExecuteAsync(new TdApi.GetUser
            {
                UserId = userId
            });

            return user;
        }
        
        private void GetLastMessage(TdApi.Chat chat)
        {
            try
            {
                if (chat.Type is TdApi.ChatType.ChatTypePrivate) return;
                if ((chat.Type as TdApi.ChatType.ChatTypeSupergroup)!.IsChannel) return;
                
                var user = GetUser(chat.LastMessage).Result;
                var userFirstName = ""; //chat.Permissions.CanSendBasicMessages ? $"{user.FirstName}: " : "";
                
                TextBlockChatLastMessage.Text = chat.LastMessage.Content switch
                {
                    TdApi.MessageContent.MessageText messageText => $"{userFirstName}: {messageText.Text.Text}",
                    TdApi.MessageContent.MessageAudio messageAudio =>
                        $"{userFirstName}Audio message ({messageAudio.Audio.Duration})",
                    TdApi.MessageContent.MessageVoiceNote messageVoiceNote =>
                        $"{userFirstName}Voice message ({messageVoiceNote.VoiceNote.Duration})",
                    TdApi.MessageContent.MessageVideo messageVideo =>
                        $"{userFirstName}Video message ({messageVideo.Video.Duration} sec)",
                    TdApi.MessageContent.MessagePhoto messagePhoto =>
                        $"{userFirstName}Photo message ({messagePhoto.Photo.Minithumbnail.Width}x" +
                        $"{messagePhoto.Photo.Minithumbnail.Height}), {messagePhoto.Caption.Text}",
                    TdApi.MessageContent.MessageSticker messageSticker =>
                        $"{userFirstName}{messageSticker.Sticker.Emoji} Sticker message",
                    TdApi.MessageContent.MessagePoll messagePoll => $"{userFirstName}{messagePoll.Poll.Question} Poll message",
                    TdApi.MessageContent.MessagePinMessage messagePinMessage =>
                        $"{userFirstName}pinned {GetMessageText(chat.Id, messagePinMessage.MessageId)}",
                    _ => TextBlockChatLastMessage.Text
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        private void ShowMenu(bool isTransient)
        {
            _isContextMenuOpen = isTransient;
            FlyoutShowOptions myOption = new FlyoutShowOptions();
            myOption.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;
            CommandBarFlyout1.ShowAt(ChatEntryInfo, myOption);
        }
        
        private void ChatEntry_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowMenu(!_isContextMenuOpen);
        }

        private void ContextMenuMarkAs_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void ContextMenuNotifications_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ContextMenuPin_OnClick(object sender, RoutedEventArgs e)
        {
            _client.ExecuteAsync(new TdApi.SetPinnedChats { });
        }

        private void ContextMenuArchive_OnClick(object sender, RoutedEventArgs e)
        {
            var chat = _client.ExecuteAsync(new TdApi.GetChat { ChatId = ChatId }).Result;

            if (chat.ChatLists is TdApi.ChatList.ChatListMain)
            {
                _client.ExecuteAsync(new TdApi.AddChatToList
                {
                    ChatId = ChatId, 
                    ChatList = new TdApi.ChatList.ChatListArchive()
                });
            }
            else if (chat.ChatLists is TdApi.ChatList.ChatListArchive)
            {
                _client.ExecuteAsync(new TdApi.AddChatToList
                {
                    ChatId = ChatId, 
                    ChatList = new TdApi.ChatList.ChatListMain()
                });
            }
        }

        private void ContextMenuLeave_OnClick(object sender, RoutedEventArgs e)
        {
            _client.ExecuteAsync(new TdApi.LeaveChat { ChatId = ChatId });
        }
    }
}
