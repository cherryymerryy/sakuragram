using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats.Messages;

public partial class ChatStickerMessage : Page
{
    private static TdClient _client = App._client;
    private TdApi.MessageContent _messageMediaContent;
    private int _mediaFileId;
    private int _profilePhotoFileId;
    
    public ChatStickerMessage()
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
                switch (_messageMediaContent)
                {
                    case TdApi.MessageContent.MessageSticker messageSticker:
                    {
                        if (updateFile.File.Id == _mediaFileId)
                        {
                            if (messageSticker.Sticker.Sticker_.Local.Path != "")
                            {
                                switch (messageSticker.Sticker.Format)
                                {
                                    case TdApi.StickerFormat.StickerFormatWebp:
                                    {
                                        ImageMedia.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                                                ImageMedia.Source = new BitmapImage(
                                                    new Uri(messageSticker.Sticker.Sticker_.Local.Path));
                                                SetStickerTransform(messageSticker.Sticker);
                                        });
                                        break;
                                    }
                                    case TdApi.StickerFormat.StickerFormatWebm or TdApi.StickerFormat.StickerFormatTgs:
                                    {
                                        MediaPlayerElement.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                                                MediaPlayerElement.Source = MediaSource.CreateFromUri(
                                                    new Uri(messageSticker.Sticker.Sticker_.Local.Path));
                                                MediaPlayerElement.AutoPlay = true;
                                                MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
                                                SetStickerTransform(messageSticker.Sticker);
                                        });
                                        break;
                                    }
                                }
                            }
                            else if (updateFile.File.Local.Path != "")
                            {
                                switch (messageSticker.Sticker.Format)
                                {
                                    case TdApi.StickerFormat.StickerFormatWebp:
                                    {
                                        ImageMedia.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                                                ImageMedia.Source = new BitmapImage(
                                                    new Uri(updateFile.File.Local.Path));
                                                SetStickerTransform(messageSticker.Sticker);
                                        });
                                        break;
                                    }
                                    case TdApi.StickerFormat.StickerFormatWebm or TdApi.StickerFormat.StickerFormatTgs:
                                    {
                                        MediaPlayerElement.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                                            MediaPlayerElement.Source = MediaSource.CreateFromUri(
                                                new Uri(updateFile.File.Local.Path));
                                                MediaPlayerElement.AutoPlay = true;
                                                MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
                                                SetStickerTransform(messageSticker.Sticker);
                                        });
                                        break;
                                    }
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
                    case TdApi.MessageContent.MessageAnimation messageAnimation:
                    {
                        if (updateFile.File.Id == _mediaFileId)
                        {
                            if (updateFile.File.Local.Path != "")
                            {
                                MediaPlayerElement.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                                    MediaPlayerElement.Source = MediaSource.CreateFromUri(
                                        new Uri(updateFile.File.Local.Path));
                                    MediaPlayerElement.AutoPlay = true;
                                    MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
                                    SetGIFTransform(messageAnimation.Animation);
                                });
                            }
                        }
                        break;
                    }
                }
                break;
            }
        }
    }

    public void UpdateMessage(TdApi.Message message)
    {
        _messageMediaContent = message.Content;
        
        var sender = message.SenderId switch
        {
            TdApi.MessageSender.MessageSenderUser u => u.UserId,
            TdApi.MessageSender.MessageSenderChat c => c.ChatId,
            _ => 0
        };

        if (sender > 0) // if senderId > 0 then it's a user
        {
            var user = _client.GetUserAsync(userId: sender).Result;
            GetChatPhoto(user);
        }
        else // if senderId < 0 then it's a chat
        {
            var chat = _client.GetChatAsync(chatId: sender).Result;
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
            case TdApi.MessageContent.MessageSticker messageSticker:
            {
                if (messageSticker.Sticker.Sticker_.Local.Path != "")
                {
                    try
                    {
                        switch (messageSticker.Sticker.Format)
                        {
                            case TdApi.StickerFormat.StickerFormatWebp:
                            {
                                ImageMedia.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                                        ImageMedia.Source = new BitmapImage(
                                            new Uri(messageSticker.Sticker.Sticker_.Local.Path));
                                        SetStickerTransform(messageSticker.Sticker);
                                    });
                                break;
                            }
                            case TdApi.StickerFormat.StickerFormatWebm or TdApi.StickerFormat.StickerFormatTgs:
                            {
                                MediaPlayerElement.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => {
                                    MediaPlayerElement.Source = MediaSource.CreateFromUri(
                                        new Uri(messageSticker.Sticker.Sticker_.Local.Path));
                                    MediaPlayerElement.AutoPlay = true;
                                    MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
                                    SetStickerTransform(messageSticker.Sticker);
                                });
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
                else
                {
                    _mediaFileId = messageSticker.Sticker.Sticker_.Id;
                
                    var file = _client.ExecuteAsync(new TdApi.DownloadFile
                    {
                        FileId = _mediaFileId,
                        Priority = 1
                    }).Result;
                }
                break;
            }
            case TdApi.MessageContent.MessageAnimation messageAnimation:
            {
                if (messageAnimation.Animation.Animation_.Local.Path != "")
                {
                    MediaPlayerElement.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
                    {
                        MediaPlayerElement.Source = MediaSource.CreateFromUri(
                            new Uri(messageAnimation.Animation.Animation_.Local.Path));
                        MediaPlayerElement.AutoPlay = true;
                        MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
                        SetGIFTransform(messageAnimation.Animation);
                    });
                }
                else
                {
                    _mediaFileId = messageAnimation.Animation.Animation_.Id;
                
                    var file = _client.ExecuteAsync(new TdApi.DownloadFile
                    {
                        FileId = _mediaFileId,
                        Priority = 1
                    }).Result;
                }
                break;
            }
        }
    }

    private void SetStickerTransform(TdApi.Sticker sticker)
    {
        switch (sticker.Format)
        {
            case TdApi.StickerFormat.StickerFormatWebp:
            {
                ImageMedia.Height = sticker.Height / 3;
                ImageMedia.Width = sticker.Width / 3;
                break;
            }
            case TdApi.StickerFormat.StickerFormatWebm or TdApi.StickerFormat.StickerFormatTgs:
            {
                MediaPlayerElement.Height = sticker.Height / 3;
                MediaPlayerElement.Width = sticker.Width / 3;
                break;
            }
        }
    }
    
    private void SetGIFTransform(TdApi.Animation animation)
    {
        MediaPlayerElement.Height = animation.Height / 1;
        MediaPlayerElement.Width = animation.Width / 1;
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
}