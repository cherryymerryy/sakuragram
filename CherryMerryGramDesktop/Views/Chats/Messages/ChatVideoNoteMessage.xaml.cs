using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Microsoft.UI.Dispatching;
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
                break;
            }
        }
    }

    public void UpdateMessage(TdApi.Message message)
    {
        _messageContent = message.Content;
        var sender = message.SenderId switch
        {
            TdApi.MessageSender.MessageSenderUser u => u.UserId,
            TdApi.MessageSender.MessageSenderChat c => c.ChatId,
            _ => 0
        };
        var user = _client.GetUserAsync(userId: sender).Result;
        DisplayName.Text = user.FirstName + " " + user.LastName;
        MediaPlayerElement.AutoPlay = true;
        MediaPlayerElement.MediaPlayer.IsMuted = true;
        MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
        GetChatPhoto(user);
        
        try
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddSeconds(message.Date).ToLocalTime();
            string sendTime = dateTime.ToShortTimeString();

            SendTime.Text = sendTime;
        } 
        catch 
        {
            // ignored
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
        }
    }
}