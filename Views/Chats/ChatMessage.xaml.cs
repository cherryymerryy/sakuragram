using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGram.Views.Chats
{
    public sealed partial class ChatMessage : Page
    {
        public ChatMessage()
        {
            this.InitializeComponent();
        }
        
        public void UpdateMessage(TdApi.Message message)
        {
            Username.Text = message.AuthorSignature;
        }
    }
}
