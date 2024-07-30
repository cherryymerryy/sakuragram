using Microsoft.UI.Xaml.Controls;

namespace CherryMerryGram.Views.Chats
{
    public sealed partial class ChatEntry : Page
    {
        public ChatEntry()
        {
            this.InitializeComponent();
        }

        public void UpdateChat(int chatId, string chatName)
        {
            textBlock_ChatId.Text = chatId.ToString();
            textBlock_ChatName.Text = chatName;
        }
    }
}
