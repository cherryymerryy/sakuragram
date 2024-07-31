using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TdLib;
using WinRT;
using CherryMerryGram;

namespace CherryMerryGram.Views.Chats
{
    public sealed partial class ChatEntry : Page
    {
        public StackPanel ChatPage;
        private static Chat _chatWidget;

        private long _chatId;
        private string _chatTitle;
        private static TdApi.Client _client = MainWindow._client;
        
        public ChatEntry()
        {
            this.InitializeComponent();
        }

        public void UpdateChat(TdApi.Chat chat)
        {
            _chatId = chat.Id;
            _chatTitle = chat.Title;

            textBlock_Chat_NameAndId.Text = $"{chat.Title} ({chat.Id})";

            if (chat.LastMessage.Content is not TdApi.MessageContent.MessageText messageText) return;
            var text = messageText.Text;
            textBlock_Chat_LastMessage.Text = text.Text;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (ChatPage != null && _chatWidget != null)
            {
                ChatPage.Children.Remove(_chatWidget);
                _chatWidget = null;
            }
            
            _chatWidget = new Chat();
            ChatPage.Children.Add(_chatWidget);
            _chatWidget.UpdateChat(_chatId, _chatTitle);
        }

        private static IEnumerable<string> GetLastMessage(TdApi.Chat chat)
        {
            switch (chat.LastMessage.DataType)
            {
                case "messageText":
                    yield return chat.LastMessage.Content.ToString();
                    break;
                case "messagePhoto":
                    yield return chat.LastMessage.Content.DataType[4].ToString();
                    break;
                case "messageSticker":
                    yield return chat.LastMessage.Content.DataType[5].ToString();
                    break;
            }
        }
    }
}
