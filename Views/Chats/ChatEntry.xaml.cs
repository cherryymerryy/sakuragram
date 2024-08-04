using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGram.Views.Chats
{
    public sealed partial class ChatEntry : Page
    {
        public Grid ChatPage;
        private static Chat _chatWidget;

        private static readonly TdClient _client = MainWindow._client;
        
        private long _chatId;
        
        public ChatEntry()
        {
            InitializeComponent();
        }

        public void UpdateChat(TdApi.Chat chat)
        {
            _chatId = chat.Id;
            TextBlockChatName.Text = chat.Title;

            if (chat.UnreadCount > 0)
            {
                UnreadMessagesCount.Visibility = Visibility.Visible;
                UnreadMessagesCount.Text = chat.UnreadCount.ToString();

            }
            else
            {
                UnreadMessagesCount.Visibility = Visibility.Collapsed;
            }
            
            SendTime.Text = chat.LastMessage.Date != 0 ? DateTime.FromFileTime(chat.LastMessage.Date).ToString("HH:mm") : string.Empty;
            GetChatPhoto(chat);
            GetLastMessage(chat);
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
            _chatWidget.ChatId = _chatId;
            _chatWidget.UpdateChat(_chatId);
            _chatWidget.GetMessages(_chatId);
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
                if (chat.Type is TdApi.ChatType.ChatTypeSupergroup or TdApi.ChatType.ChatTypePrivate &&
                    !chat.Permissions.CanSendBasicMessages)
                {
                    TextBlockChatLastMessage.Text = chat.LastMessage.Content switch
                    {
                        TdApi.MessageContent.MessageText messageText => $"{messageText.Text.Text}",
                        TdApi.MessageContent.MessageAudio messageAudio =>
                            $"Audio message ({messageAudio.Audio.Duration})",
                        TdApi.MessageContent.MessageVoiceNote messageVoiceNote =>
                            $"Voice message ({messageVoiceNote.VoiceNote.Duration})",
                        TdApi.MessageContent.MessageVideo messageVideo =>
                            $"Video message ({messageVideo.Video.Duration} sec)",
                        TdApi.MessageContent.MessagePhoto messagePhoto =>
                            $"Photo message ({messagePhoto.Photo.Minithumbnail.Width}x{messagePhoto.Photo.Minithumbnail.Height})",
                        TdApi.MessageContent.MessageSticker messageSticker =>
                            $"{messageSticker.Sticker.Emoji} Sticker message",
                        TdApi.MessageContent.MessagePoll messagePoll => $"{messagePoll.Poll.Question} Poll message",
                        TdApi.MessageContent.MessagePinMessage messagePinMessage =>
                            $"{GetMessageText(chat.Id, messagePinMessage.MessageId)}",
                        _ => TextBlockChatLastMessage.Text
                    };
                }
                else if (chat.Type is not TdApi.ChatType.ChatTypePrivate &&
                         chat.Permissions.CanSendBasicMessages)
                {
                    var user = GetUser(chat.LastMessage).Result;

                    TextBlockChatLastMessage.Text = chat.LastMessage.Content switch
                    {
                        TdApi.MessageContent.MessageText messageText => $"{user.FirstName}: {messageText.Text.Text}",
                        TdApi.MessageContent.MessageAudio messageAudio =>
                            $"{user.FirstName}: Audio message ({messageAudio.Audio.Duration})",
                        TdApi.MessageContent.MessageVoiceNote messageVoiceNote =>
                            $"{user.FirstName}: Voice message ({messageVoiceNote.VoiceNote.Duration})",
                        TdApi.MessageContent.MessageVideo messageVideo =>
                            $"{user.FirstName}: Video message ({messageVideo.Video.Duration} sec)",
                        TdApi.MessageContent.MessagePhoto messagePhoto =>
                            $"Photo message ({messagePhoto.Photo.Minithumbnail.Width}x{messagePhoto.Photo.Minithumbnail.Height})",
                        TdApi.MessageContent.MessageSticker messageSticker =>
                            $"{user.FirstName}: {messageSticker.Sticker.Emoji} Sticker message",
                        TdApi.MessageContent.MessagePoll messagePoll => $"{messagePoll.Poll.Question} Poll message",
                        TdApi.MessageContent.MessagePinMessage messagePinMessage =>
                            $"{user.FirstName} pinned {GetMessageText(chat.Id, messagePinMessage.MessageId)}",
                        _ => TextBlockChatLastMessage.Text
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
