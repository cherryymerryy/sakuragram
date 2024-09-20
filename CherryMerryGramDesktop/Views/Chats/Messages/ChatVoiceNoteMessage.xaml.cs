using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats.Messages;

public partial class ChatVoiceNoteMessage : Page
{
    private static TdClient _client = App._client;
    private TdApi.MessageContent.MessageVoiceNote _messageVoiceNote;
    private MediaPlayerElement _mediaPlayerElement;
    private TimeSpan _position;
    private int _profilePhotoFileId;
    
    public ChatVoiceNoteMessage()
    {
        InitializeComponent();
        
        _mediaPlayerElement = new MediaPlayerElement();
        _client.UpdateReceived += async (_, update) => { await ProcessUpdate(update); };
    }

    private void MediaPlayerElement_MediaEnded(MediaPlayer sender, object args)
    {
        Icon.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => Icon.Glyph = "\uE768");
        _position = TimeSpan.Zero;
    }

    private async Task ProcessUpdate(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateFile updateFile:
            {
                if (updateFile.File.Id == _messageVoiceNote.VoiceNote.Voice.Id)
                {
                    if (updateFile.File.Local.Path != string.Empty)
                    {
                        _mediaPlayerElement.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                            PlayVoiceNote(updateFile.File.Local.Path);
                        });
                    }
                    else if (_messageVoiceNote.VoiceNote.Voice.Local.Path != string.Empty)
                    {
                        _mediaPlayerElement.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                            PlayVoiceNote(_messageVoiceNote.VoiceNote.Voice.Local.Path);
                        });
                    }
                }
                break;
            }
        }
    }

    public void UpdateMessage(TdApi.Message message)
    {
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
        
        switch (message.Content)
        {
            case TdApi.MessageContent.MessageVoiceNote messageVoiceNote:
            {
                _messageVoiceNote = messageVoiceNote;
                VoiceNoteDuration.Text = messageVoiceNote.VoiceNote.Duration.ToString();

                if (messageVoiceNote.Caption.Text != string.Empty)
                {
                    MessageCaptionText.Text = messageVoiceNote.Caption.Text;
                    MessageCaptionText.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageCaptionText.Text = string.Empty;
                    MessageCaptionText.Visibility = Visibility.Collapsed;
                }

                if (messageVoiceNote.VoiceNote.Voice.Local.Path != string.Empty)
                {
                    Icon.Glyph = "\uE768";
                    _mediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(messageVoiceNote.VoiceNote.Voice.Local.Path));
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
        if (user.ProfilePhoto.Big.Local.Path != "")
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
    
    private void ButtonDownloadVoiceNote_OnClick(object sender, RoutedEventArgs e)
    {
        if (_messageVoiceNote == null) return;
        if (_messageVoiceNote.VoiceNote.Voice.Local.Path != string.Empty)
        {
            switch (_mediaPlayerElement.MediaPlayer.CurrentState)
            {
                case MediaPlayerState.Paused:
                    PlayVoiceNote(_messageVoiceNote.VoiceNote.Voice.Local.Path);
                    break;
                case MediaPlayerState.Playing:
                    PauseVoiceNote();
                    break;
            }
        }
        else
        {
            _client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = _messageVoiceNote.VoiceNote.Voice.Id,
                Priority = 1
            });
        }
    }

    private void PlayVoiceNote(string path)
    {
        _mediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(path));
        if (_position != TimeSpan.Zero)
        {
            _mediaPlayerElement.MediaPlayer.Position = _position;
        }
        _mediaPlayerElement.MediaPlayer.Play();
        _mediaPlayerElement.MediaPlayer.MediaEnded += MediaPlayerElement_MediaEnded;
        Icon.Glyph = "\uE769";
    }
    
    private void PauseVoiceNote()
    {
        _mediaPlayerElement.MediaPlayer.Pause();
        _mediaPlayerElement.MediaPlayer.MediaEnded -= MediaPlayerElement_MediaEnded;
        _position = _mediaPlayerElement.MediaPlayer.Position;
        Icon.Glyph = "\uE768";
    }
}