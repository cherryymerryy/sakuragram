using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats.Messages;

public partial class ChatVideoMessage : Page
{
    private static TdClient _client = App._client;
    private TdApi.MessageContent _messageMediaContent;
    private TdApi.Message _message;
    private TdApi.ProfilePhoto _profilePhoto;
    private int _profilePhotoFileId;
    private int _mediaFileId;
    private bool _isPlaying = false;
    private TimeSpan _pausedVideoPosition;

    public ChatVideoMessage()
    {
        InitializeComponent();
    }

    private async Task ProcessUpdate(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateFile updateFile:
            {
                if (updateFile.File.Id == _mediaFileId)
                {
                    switch (_messageMediaContent)
                    {
                        case TdApi.MessageContent.MessageVideo messageVideo:
                        {
                            if (messageVideo.Video.Video_.Local.Path != "")
                            {
                                MediaPlayerElement.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                                        MediaPlayerElement.Source =
                                            MediaSource.CreateFromUri(new Uri(messageVideo.Video.Video_.Local.Path));
                                    });
                            }
                            else if (updateFile.File.Local.Path != "")
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
            case TdApi.Update.UpdateMessageEdited updateMessageEdited:
            {
                if (updateMessageEdited.MessageId == _message.Id)
                {
                    DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => { 
                        var message = _client.ExecuteAsync(new TdApi.GetMessage{ChatId = _message.ChatId, MessageId = _message.Id}).Result;
                        EditMessage(message); 
                    });
                }
                break;
            }
        }
    }
    
    private void EditMessage(TdApi.Message editedMessage)
    {
        switch (editedMessage.Content)
        {
            case TdApi.MessageContent.MessageVideo messageVideo:
            {
                if (MediaPlayerElement.Source.ToString() != messageVideo.Video.Video_.Local.Path)
                {
                    _mediaFileId = messageVideo.Video.Video_.Id;
                
                    var file = _client.ExecuteAsync(new TdApi.DownloadFile
                    {
                        FileId = _mediaFileId,
                        Priority = 1
                    }).Result;
                }
                
                if (MessageContent.Text != messageVideo.Caption.Text)
                {
                    MessageContent.Text = messageVideo.Caption.Text;
                }
                break;
            }
        }
    }
    
    public void UpdateMessage(TdApi.Message message)
    {
        _messageMediaContent = message.Content;
        _message = message;
        
        MediaPlayerElement.AutoPlay = true;
        MediaPlayerElement.MediaPlayer.IsMuted = true;
        MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
        MediaPlayerElement.MediaPlayer.MediaEnded += MediaPlayerElement_OnMediaEnded;
        
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

        switch (message.Content)
        {
            case TdApi.MessageContent.MessageVideo messageVideo:
            {
                if (messageVideo.Video.Video_.Local.Path != "")
                {
                    MediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(messageVideo.Video.Video_.Local.Path));
                }
                else
                {
                    _mediaFileId = messageVideo.Video.Video_.Id;
                
                    var file = _client.ExecuteAsync(new TdApi.DownloadFile
                    {
                        FileId = _mediaFileId,
                        Priority = 1
                    }).Result;
                }

                if (messageVideo.Caption.Text != "")
                {
                    MessageContent.Text = messageVideo.Caption.Text;
                    MessageContent.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageContent.Text = "";
                    MessageContent.Visibility = Visibility.Collapsed;
                }
                
                break;
            }
        }
        
        _client.UpdateReceived += async (_, update) => { await ProcessUpdate(update); };
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

    private void MediaPlayerElement_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (MediaPlayerElement.Source == null) return;
        if (!_isPlaying)
        {
            MediaPlayerElement.MediaPlayer.Position = _pausedVideoPosition != TimeSpan.Zero ? _pausedVideoPosition : TimeSpan.Zero;
            MediaPlayerElement.MediaPlayer.IsMuted = false;
            MediaPlayerElement.MediaPlayer.PlaybackRate = 1;
            MediaPlayerElement.MediaPlayer.Volume = 0.5;
            MediaPlayerElement.MediaPlayer.Play();
            _isPlaying = true;
        }
        else
        {
            _pausedVideoPosition = MediaPlayerElement.MediaPlayer.Position;
            MediaPlayerElement.MediaPlayer.Pause();
            _isPlaying = false;
        }
    }

    private void MediaPlayerElement_OnMediaEnded(MediaPlayer sender, object args)
    {
        if (_isPlaying)
        {
            _isPlaying = false;
            MediaPlayerElement.MediaPlayer.IsMuted = true;
        }
    }
}