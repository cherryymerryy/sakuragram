using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats
{
    public sealed partial class ChatEntry : Button
    {
        public Grid ChatPage;
        private static Chat _chatWidget;
        public ChatsView _ChatsView;

        private static readonly TdClient _client = App._client;
        
        public TdApi.Chat Chat;
        public long ChatId;
        private int _profilePhotoFileId;
        
        public ChatEntry()
        {
            InitializeComponent();
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateChatTitle updateChatTitle:
                {
                    if (updateChatTitle.ChatId == ChatId)
                    {
                        TextBlockChatName.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal,
                            () => TextBlockChatName.Text = updateChatTitle.Title);
                    }
                    break;
                }
                case TdApi.Update.UpdateChatPhoto updateChatPhoto:
                {
                    if (updateChatPhoto.ChatId == ChatId)
                    {
                        ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(() => GetChatPhoto(Chat));
                    }
                    break;
                }
                case TdApi.Update.UpdateFile updateFile:
                {
                    if (updateFile.File.Id != _profilePhotoFileId) return Task.CompletedTask;
                    ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(() =>
                    {
                        if (updateFile.File.Local.Path != "")
                        {
                            ChatEntryProfilePicture.ProfilePicture = new BitmapImage(new Uri(updateFile.File.Local.Path));
                        }
                        else
                        {
                            ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, 
                                () => ChatEntryProfilePicture.DisplayName = Chat.Title);
                        }
                    });
                    break;
                }
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

            if (Chat.NotificationSettings.MuteFor <= 0)
            {
                UnreadMessagesCount.Background = new SolidColorBrush(Colors.Gray);
            }
            
            switch (Chat.Type)
            {
                case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                {
                    var supergroup = _client.GetSupergroupAsync(
                            supergroupId: typeSupergroup.SupergroupId)
                        .Result;
                    if (supergroup.IsForum)
                    {
                        ChatEntryProfilePicture.CornerRadius = new CornerRadius(0);
                    }
                    else
                    {
                        break;
                    }

                    break;
                }
            }
        }
        
        private void GetChatPhoto(TdApi.Chat chat)
        {
            if (chat.Photo == null)
            {
                ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, 
                    () => ChatEntryProfilePicture.DisplayName = chat.Title);
                return;
            }
            if (chat.Photo.Big.Local.Path != "")
            {
                try
                {
                    ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                        () => ChatEntryProfilePicture.ProfilePicture = new BitmapImage(new Uri(chat.Photo.Big.Local.Path)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                _profilePhotoFileId = chat.Photo.Big.Id;
                
                var file = _client.ExecuteAsync(new TdApi.DownloadFile
                {
                    FileId = _profilePhotoFileId,
                    Priority = 1
                }).Result;
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
                _chatWidget.CloseChat();
                ChatPage.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, 
                    () => ChatPage.Children.Remove(_chatWidget));
                _chatWidget = null;
            }
            
            ChatEntry_OnRightTapped(null, null);
            _chatWidget = new Chat
            {
                _ChatsView = _ChatsView,
                _chatId = ChatId
            };
            _ChatsView._currentChat = _chatWidget;
            
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
        
        private void ChatEntry_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ContextMenu.ShowAt(ButtonChatEntry);
        }

        private void ContextMenuMarkAs_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void ContextMenuNotifications_OnClick(object sender, RoutedEventArgs e)
        {
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
