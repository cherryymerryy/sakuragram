using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using TdLib;
using static Microsoft.Toolkit.Uwp.Notifications.ToastActivationType;

namespace CherryMerryGramDesktop.Services;

public class NotificationService
{
    private static TdClient _client = App._client;
    public TdApi.User _user;
    
    public virtual async void SendNotification(TdApi.Message message)
    {
        var chat = await _client.ExecuteAsync(new TdApi.GetChat {ChatId = message.ChatId});
        if (chat.NotificationSettings.MuteFor > 1) return;
        var userId = message.SenderId switch
        {
            TdApi.MessageSender.MessageSenderUser u => u.UserId,
            TdApi.MessageSender.MessageSenderChat c => c.ChatId,
            _ => 0
        };
        var user = _client.ExecuteAsync(new TdApi.GetUser {UserId = userId}).Result;
        
        new ToastContentBuilder()
            .AddArgument("action", "viewConversation")
            .AddArgument("ChatId", message.ChatId)
            .AddText($"{user.FirstName} {user.LastName}")
            .AddText($"{message.Content}")
            .Show();
    }
}