using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using TdLib;

namespace CherryMerryGramDesktop.Services;

public class NotificationService
{
    private static TdClient _client = App._client;
    public TdApi.User _user;
    
    public void SendNotification(TdApi.Message message)
    {
        //TODO: add check for notification is enabled
        
        new ToastContentBuilder()
            .AddArgument("action", "viewConversation")
            .AddArgument("ChatId", message.ChatId)
            .AddText("User sent you a message")
            .AddText($"{message.Content}")
            .Show();
    }
}