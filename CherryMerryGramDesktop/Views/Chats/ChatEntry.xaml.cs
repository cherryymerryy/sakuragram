using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats
{
    public sealed partial class ChatEntry : Page
    {
        public Grid ChatPage;
        private static Chat _chatWidget;

        private static readonly TdClient _client = App._client;
        public TdApi.Chat Chat;
        public long ChatId;
        
        public ChatEntry()
        {
            InitializeComponent();
            
            //_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            if (Chat == null) return Task.CompletedTask;
            
            switch (update)
            {
                case TdApi.Update.UpdateChatLastMessage:
                {
                    GetLastMessage(_client.GetChatAsync(ChatId).Result);
                    break;
                }
                case TdApi.Update.UpdateChatReadInbox:
                {
                    if (Chat.UnreadCount > 0)
                    {
                        UnreadMessagesCount.Visibility = Visibility.Visible;
                        UnreadMessagesCount.Text = Chat.UnreadCount.ToString();

                    }
                    else
                    {
                        UnreadMessagesCount.Visibility = Visibility.Collapsed;
                    }

                    break;
                }
                case TdApi.Update.UpdateChatTitle:
                {
                    TextBlockChatName.Text = Chat.Title;
                    break;
                }
                case TdApi.Update.UpdateChatPhoto:
                {
                    GetChatPhoto(Chat);
                    break;
                }
                case TdApi.Update.UpdateChatAddedToList:
                {
                    TextBlockChatName.Text = Chat.Title;

                    GetChatPhoto(Chat);
                    GetLastMessage(_client.GetChatAsync(ChatId).Result);

                    if (Chat.UnreadCount > 0)
                    {
                        if (UnreadMessagesCount.Visibility == Visibility.Collapsed)
                            UnreadMessagesCount.Visibility = Visibility.Visible;
                        UnreadMessagesCount.Text = Chat.UnreadCount.ToString();
                    }
                    else
                    {
                        UnreadMessagesCount.Visibility = Visibility.Collapsed;
                    }

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
                UnreadMessagesCount.Text = Chat.UnreadCount.ToString();
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
            if (ChatPage != null && _chatWidget != null)
            {
                ChatPage.Children.Remove(_chatWidget);
                _chatWidget = null;
            }
            
            _chatWidget = new Chat();
            _chatWidget.ChatId = ChatId;
            _chatWidget.UpdateChat(ChatId);
            _chatWidget.GetMessages(ChatId);
            ChatPage?.Children.Add(_chatWidget);
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
                        $"{userFirstName}Photo message ({messagePhoto.Photo.Minithumbnail.Width}x{messagePhoto.Photo.Minithumbnail.Height})",
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
    }
}