using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats.Messages;

public partial class ChatVoiceNoteMessage : Page
{
    private static TdClient _client = App._client;
    private TdApi.MessageContent.MessageVoiceNote _messageVoiceNote;
    private MediaPlayerElement _mediaPlayerElement;
    private TimeSpan _position;
    
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