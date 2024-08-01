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
        public StackPanel ChatPage;
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
            textBlock_Chat_NameAndId.Text = $"{chat.Title} ({chat.Id})";
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
                
                ChatEntry_ProfilePicture.ImageSource = new BitmapImage(new Uri(chatPhoto.Local.Path));
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
        
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (ChatPage != null && _chatWidget != null)
            {
                ChatPage.Children.Remove(_chatWidget);
                _chatWidget = null;
            }
            
            _chatWidget = new Chat();
            ChatPage?.Children.Add(_chatWidget);
            _chatWidget.UpdateChat(_chatId);
        }

        private void GetLastMessage(TdApi.Chat chat)
        {
            textBlock_Chat_LastMessage.Text = chat.LastMessage.Content switch
            {
                TdApi.MessageContent.MessageText messageText => messageText.Text.Text,
                TdApi.MessageContent.MessageAudio messageAudio => $"Audio message ({messageAudio.Audio.Duration})",
                TdApi.MessageContent.MessageVoiceNote messageVoiceNote => $"Voice message ({messageVoiceNote.VoiceNote.Duration})",
                TdApi.MessageContent.MessageVideo messageVideo => $"Video message ({messageVideo.Video.Duration} sec)",
                TdApi.MessageContent.MessagePhoto messagePhoto => 
                    $"Photo message ({messagePhoto.Photo.Minithumbnail.Width}x{messagePhoto.Photo.Minithumbnail.Height})",
                TdApi.MessageContent.MessageSticker messageSticker => $"{messageSticker.Sticker.Emoji} Sticker message",
                TdApi.MessageContent.MessagePoll messagePoll => $"{messagePoll.Poll.Question} Poll message",
                TdApi.MessageContent.MessagePinMessage messagePinMessage => 
                    $"(FirstName) pinned {GetMessageText(chat.Id, messagePinMessage.MessageId)}",
                _ => textBlock_Chat_LastMessage.Text
            };
        }
    }
}
