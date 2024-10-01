using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Windows.AppNotifications.Builder;
using TdLib;

namespace sakuragram.Services;

public class NotificationService
{
    private static TdClient _client = App._client;

    public NotificationService()
    {
        _client.UpdateReceived += async (_, update) => { await ProcessUpdate(update); };
    }

    private async Task ProcessUpdate(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateNewMessage updateNewMessage:
            {
                SendNotification(updateNewMessage.Message);
                break;
            }
        }
    }

    private async void SendNotification(TdApi.Message message)
    {
        if (message is null || _client is null) return;
        try
        {
            TdApi.Chat chat = await _client.ExecuteAsync(new TdApi.GetChat {ChatId = message.ChatId});
            
            switch (chat.NotificationSettings.MuteFor)
            {
                case 419749343:
                    return;
                case 0:
                {
                    long sender = message.SenderId switch
                    {
                        TdApi.MessageSender.MessageSenderUser u => u.UserId,
                        TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                        _ => 0
                    };

                    if (sender > 0)
                    {
                        TdApi.User user = _client.GetUserAsync(sender).Result;
                        var builder = new AppNotificationBuilder()
                            .AddArgument("conversationId", chat.Id.ToString())
                            .AddText(user.FirstName + " " + user.LastName + " sent a message")
                            .AddText(message.Content.ToString())
                            .AddButton(new AppNotificationButton("Reply")
                                .AddArgument("action", "reply"))
                            .AddButton(new AppNotificationButton("Mark as read")
                                .AddArgument("action", "markasread"));
                    }
                    else
                    {
                        var builder = new AppNotificationBuilder()
                            .AddArgument("conversationId", chat.Id.ToString())
                            .AddText(chat.Title + " sent a message")
                            .AddText(message.Content.ToString())
                            .AddButton(new AppNotificationButton("Reply")
                                .AddArgument("action", "reply"))
                            .AddButton(new AppNotificationButton("Mark as read")
                                .AddArgument("action", "markasread"));
                    }

                    break;
                }
            }
        }
        catch (TdException e)
        {
            Debug.WriteLine(e.Message);
            throw;
        }
    }
}