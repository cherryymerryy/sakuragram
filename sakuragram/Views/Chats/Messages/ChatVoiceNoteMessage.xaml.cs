using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace sakuragram.Views.Chats.Messages;

public partial class ChatVoiceNoteMessage : Page
{
    private static TdClient _client = App._client;
    private TdApi.MessageContent.MessageVoiceNote _messageVoiceNote;
    private MediaPlayerElement _mediaPlayerElement;
    private TimeSpan _position;
    private int _profilePhotoFileId;
    private TdApi.ProfilePhoto _profilePhoto;

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
                if (updateFile.File.Id == _profilePhotoFileId)
                {
                    if (updateFile.File.Local.Path != string.Empty)
                    {
                        ProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                            () => ProfilePicture.ProfilePicture = new BitmapImage(new Uri(updateFile.File.Local.Path)));
                    }
                    else if (_profilePhoto.Small.Local.Path != string.Empty)
                    {
                        ProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                            () => ProfilePicture.ProfilePicture = new BitmapImage(new Uri(_profilePhoto.Small.Local.Path)));
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
        
        if (message.ReplyTo != null)
        {
            var replyMessage = _client.ExecuteAsync(new TdApi.GetRepliedMessage
            {
                ChatId = message.ChatId,
                MessageId = message.Id
            }).Result;
            
            var replyUserId = replyMessage.SenderId switch {
                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                _ => 0
            };

            if (replyUserId > 0) // if senderId > 0 then it's a user
            {
                var replyUser = _client.GetUserAsync(replyUserId).Result;
                ReplyFirstName.Text = $"{replyUser.FirstName} {replyUser.LastName}";
            }
            else // if senderId < 0 then it's a chat
            {
                var replyChat = _client.GetChatAsync(replyUserId).Result;
                ReplyFirstName.Text = replyChat.Title;
            }
            
            ReplyInputContent.Text  = replyMessage.Content switch
            {
                TdApi.MessageContent.MessageText messageText => messageText.Text.Text,
                TdApi.MessageContent.MessageAnimation messageAnimation => messageAnimation.Caption.Text,
                TdApi.MessageContent.MessageAudio messageAudio => messageAudio.Caption.Text,
                TdApi.MessageContent.MessageDocument messageDocument => messageDocument.Caption.Text,
                TdApi.MessageContent.MessagePhoto messagePhoto => messagePhoto.Caption.Text,
                TdApi.MessageContent.MessagePoll messagePoll => messagePoll.Poll.Question.Text,
                TdApi.MessageContent.MessageVideo messageVideo => messageVideo.Caption.Text,
                TdApi.MessageContent.MessagePinMessage => "pinned message",
                TdApi.MessageContent.MessageVoiceNote messageVoiceNote => messageVoiceNote.Caption.Text,
                _ => "Unsupported message type"
            };
            
            Reply.Visibility = Visibility.Visible;
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
        
        _profilePhoto = user.ProfilePhoto;
        _profilePhotoFileId = user.ProfilePhoto.Small.Id;
        
        if (user.ProfilePhoto.Small.Local.Path != "")
        {
            try
            {
                ProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                    () => ProfilePicture.ProfilePicture = new BitmapImage(new Uri(user.ProfilePhoto.Small.Local.Path)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        else
        {
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