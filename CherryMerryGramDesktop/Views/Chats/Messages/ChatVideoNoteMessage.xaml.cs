using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats.Messages;

public partial class ChatVideoNoteMessage : Page
{
    private static TdClient _client = App._client;
    private TdApi.MessageContent _messageContent;
    private int _videoNoteId;
    private int _profilePhotoFileId;
    
    public ChatVideoNoteMessage()
    {
        InitializeComponent();
        
        _client.UpdateReceived += async (_, update) => { await ProcessUpdate(update); };
    }

    private async Task ProcessUpdate(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateFile updateFile:
            {
                if (updateFile.File.Id == _videoNoteId)
                {
                    switch (_messageContent)
                    {
                        case TdApi.MessageContent.MessageVideoNote messageVideoNote:
                        {
                            if (messageVideoNote.VideoNote.Video.Local.Path != string.Empty)
                            {
                                MediaPlayerElement.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                                    MediaPlayerElement.Source =
                                        MediaSource.CreateFromUri(new Uri(messageVideoNote.VideoNote.Video.Local.Path));
                                });
                            }
                            else if (updateFile.File.Local.Path != string.Empty)
                            {
                                MediaPlayerElement.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                                    MediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(updateFile.File.Local.Path)); 
                                });
                            }
                            break;
                        }
                    }
                }
                if (updateFile.File.Id == _profilePhotoFileId)
                {
                    ProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                        () => ProfilePicture.ProfilePicture = new BitmapImage(new Uri(updateFile.File.Local.Path)));
                }
                break;
            }
        }
    }

    public void UpdateMessage(TdApi.Message message)
    {
        _messageContent = message.Content;
        MediaPlayerElement.AutoPlay = true;
        MediaPlayerElement.MediaPlayer.IsMuted = true;
        MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
        
        var sender = message.SenderId switch
        {
            TdApi.MessageSender.MessageSenderUser u => u.UserId,
            TdApi.MessageSender.MessageSenderChat c => c.ChatId,
            _ => 0
        };

        if (sender > 0) // if senderId > 0 then it's a user
        {
            var user = _client.GetUserAsync(userId: sender).Result;
            DisplayName.Text = user.FirstName + " " + user.LastName;
            GetChatPhoto(user);
        }
        else // if senderId < 0 then it's a chat
        {
            var chat = _client.GetChatAsync(chatId: sender).Result;
            DisplayName.Text = chat.Title;
            ProfilePicture.Visibility = Visibility.Collapsed;
        }
        
        if (message.ForwardInfo != null)
        {
            if (message.ForwardInfo.Source != null)
            {
                TextBlockForwardInfo.Text = $"Forwarded from {message.ForwardInfo.Source.SenderName}";
                TextBlockForwardInfo.Visibility = Visibility.Visible;
            }
            else if (message.ForwardInfo.Origin != null)
            {
                switch (message.ForwardInfo.Origin)
                {
                    case TdApi.MessageOrigin.MessageOriginChannel channel:
                    {
                        string forwardInfo = string.Empty;
                        var chat = _client.GetChatAsync(chatId: channel.ChatId).Result;

                        forwardInfo = chat.Title;

                        if (channel.AuthorSignature != string.Empty)
                        {
                            forwardInfo = forwardInfo + $" ({channel.AuthorSignature})";
                        }
                        
                        TextBlockForwardInfo.Text = $"Forwarded from {forwardInfo}";
                        TextBlockForwardInfo.Visibility = Visibility.Visible;
                        break;
                    }
                    case TdApi.MessageOrigin.MessageOriginChat chat:
                    {
                        TextBlockForwardInfo.Text = $"Forwarded from {chat.AuthorSignature}";
                        TextBlockForwardInfo.Visibility = Visibility.Visible;
                        break;
                    }
                    case TdApi.MessageOrigin.MessageOriginUser user:
                    {
                        var originUser = _client.GetUserAsync(userId: user.SenderUserId).Result;
                        TextBlockForwardInfo.Text = $"Forwarded from {originUser.FirstName} {originUser.LastName}";
                        TextBlockForwardInfo.Visibility = Visibility.Visible;
                        break;
                    }
                    case TdApi.MessageOrigin.MessageOriginHiddenUser hiddenUser:
                    {
                        TextBlockForwardInfo.Text = $"Forwarded from {hiddenUser.SenderName}";
                        TextBlockForwardInfo.Visibility = Visibility.Visible;
                        break;
                    }
                }
            }
        }
        else
        {
            TextBlockForwardInfo.Text = string.Empty;
            TextBlockForwardInfo.Visibility = Visibility.Collapsed;
        }
        
        try
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddSeconds(message.Date).ToLocalTime();
            string sendTime = dateTime.ToShortTimeString();

            TextBlockSendTime.Text = sendTime;
        } 
        catch 
        {
            // ignored
        }

        TextBlockEdited.Visibility = message.EditDate != 0 ? Visibility.Visible : Visibility.Collapsed;

        if (message.CanGetViewers && message.IsChannelPost)
        {
            TextBlockViews.Text = message.InteractionInfo.ViewCount + " views";
            TextBlockViews.Visibility = Visibility.Visible;
        }
        else
        {
            TextBlockViews.Text = string.Empty;
            TextBlockViews.Visibility = Visibility.Collapsed;
        }

        if (message.InteractionInfo?.ReplyInfo != null)
        {
            if (message.InteractionInfo.ReplyInfo.ReplyCount > 0)
            {
                TextBlockReplies.Text = message.InteractionInfo.ReplyInfo.ReplyCount + " replies";
                TextBlockReplies.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockReplies.Text = string.Empty;
                TextBlockReplies.Visibility = Visibility.Collapsed;
            }
        }
        
        switch (_messageContent)
        {
            case TdApi.MessageContent.MessageVideoNote messageVideoNote:
            {
                if (messageVideoNote.VideoNote.Video.Local.Path != string.Empty)
                {
                    MediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(messageVideoNote.VideoNote.Video.Local.Path));
                }
                else
                {
                    _videoNoteId = messageVideoNote.VideoNote.Video.Id;
                
                    var file = _client.ExecuteAsync(new TdApi.DownloadFile
                    {
                        FileId = _videoNoteId,
                        Priority = 1
                    }).Result;
                }
                break;
            }
        }
    }
    
    private void GetChatPhoto(TdApi.User user)
    {
        if (user.ProfilePhoto == null)
        {
            ProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, 
                () => ProfilePicture.DisplayName = user.FirstName + " " + user.LastName);
            return;
        }
        if (user.ProfilePhoto.Big.Local.Path != string.Empty)
        {
            try
            {
                ProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                    () => ProfilePicture.ProfilePicture = new BitmapImage(new Uri(user.ProfilePhoto.Big.Local.Path)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        else
        {
            _profilePhotoFileId = user.ProfilePhoto.Big.Id;
                
            var file = _client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = _profilePhotoFileId,
                Priority = 1
            }).Result;
            
            ProgressRing.Visibility = Visibility.Visible;
        }
    }
}