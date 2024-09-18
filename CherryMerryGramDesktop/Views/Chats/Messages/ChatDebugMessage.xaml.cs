using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats.Messages;

public partial class ChatDebugMessage : Page
{
    public ChatDebugMessage()
    {
        InitializeComponent();
    }

    public void UpdateMessage(TdApi.Message message)
    {
        TextBlockMessageType.Text = message.Content.ToString();
    }
}