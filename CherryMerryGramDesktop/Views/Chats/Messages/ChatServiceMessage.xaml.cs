using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats.Messages
{
    public partial class ChatServiceMessage : Page
    {
        public ChatServiceMessage()
        {
            InitializeComponent();
        }

        public void UpdateMessage(TdApi.Message message)
        {
            MessageText.Text = message.Content switch
            {
                TdApi.MessageContent.MessageChatBoost messageChatBoostText => $"Boosted this chat {messageChatBoostText.BoostCount}",
                TdApi.MessageContent.MessageGameScore messageGameScoreText => $"Scored {messageGameScoreText.Score} in game",
                TdApi.MessageContent.MessageGiftedPremium messageGiftedPremiumText => 
                    $"received a {messageGiftedPremiumText.MonthCount}-month premium for {messageGiftedPremiumText.Amount} {messageGiftedPremiumText.Currency}",
                TdApi.MessageContent.MessagePinMessage messagePinMessageText => MessageText.Text = $"pinned this message ({messagePinMessageText.MessageId})",
                //TdApi.MessageContent.MessageChatAddMembers messageChatAddMembersText => MessageText.Text = messageChatAddMembersText.
                _ => MessageText.Text
            };
        }
    }
}
