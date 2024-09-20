using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats.Messages;

public partial class ChatDocumentMessage : Page
{
    private static TdClient _client = App._client;
    private TdApi.MessageContent.MessageDocument _messageDocument;
    private int _profilePhotoFileId;
    
    public ChatDocumentMessage()
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
                if (updateFile.File.Id == _messageDocument.Document.Document_.Id)
                {
                    if (updateFile.File.Local.Path != string.Empty)
                    {
                        Icon.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => Icon.Glyph = "\uE7C3");
                    }
                    else if (_messageDocument.Document.Document_.Local.Path != string.Empty)
                    {
                        Icon.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => Icon.Glyph = "\uE7C3");
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
            case TdApi.MessageContent.MessageDocument messageDocument:
            {
                _messageDocument = messageDocument;
                DocumentName.Text = messageDocument.Document.FileName;
                var megabytes = (messageDocument.Document.Document_.Size / 1024.0) / 1024.0;
                var roundedMegabytes = Math.Round(megabytes, 1);
                DocumentSize.Text = roundedMegabytes + "MB";

                if (messageDocument.Caption.Text != string.Empty)
                {
                    MessageCaptionText.Text = messageDocument.Caption.Text;
                    MessageCaptionText.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageCaptionText.Text = string.Empty;
                    MessageCaptionText.Visibility = Visibility.Collapsed;
                }

                if (messageDocument.Document.Document_.Local.Path != string.Empty)
                {
                    Icon.Glyph = "\uE7C3";
                }
                else
                {
                    Icon.Glyph = "\uE896";
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

    private void ButtonDownloadDocument_OnClick(object sender, RoutedEventArgs e)
    {
        if (_messageDocument.Document.Document_.Local.Path != string.Empty)
        {
            Process.Start("explorer.exe", _messageDocument.Document.Document_.Local.Path);
        }
        else
        {
            _client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = _messageDocument.Document.Document_.Id,
                Priority = 1
            });
            Icon.Glyph = "\uE769";
        }
    }
}