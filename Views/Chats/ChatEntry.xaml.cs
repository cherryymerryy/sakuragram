using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;
using WinRT;

namespace CherryMerryGram.Views.Chats
{
    public sealed partial class ChatEntry : Page
    {
        public ListView ChatPage;
        private static Chat _chat;

        private long ChatId;
        private string ChatTitle;
        
        public ChatEntry()
        {
            this.InitializeComponent();
        }

        public void UpdateChat(long chatId, string chatName, TdApi.Message chatLastMessage )
        {
            ChatId = chatId;
            ChatTitle = chatName;
            textBlock_Chat_NameAndId.Text = $"{chatName} ({chatId})";
            textBlock_Chat_LastMessage.Text = chatLastMessage.Content.ToString();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (ChatPage != null && _chat != null)
            {
                ChatPage.Items.Remove(_chat);
                _chat = null;
            }
            
            _chat = new Chat();
            ChatPage.Items.Add(_chat);
            _chat.UpdateChat(ChatId, ChatTitle);
        }
    }
}
