using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGramDesktop.Views.Chats.Messages;

public partial class ChatDocumentMessage : Page
{
    private static TdClient _client = App._client;
    private TdApi.MessageContent.MessageDocument _messageDocument;
    
    public ChatDocumentMessage()
    {
        InitializeComponent();
    }

    public void UpdateMessage(TdApi.Message message)
    {
        switch (message.Content)
        {
            case TdApi.MessageContent.MessageDocument messageDocument:
            {
                _messageDocument = messageDocument;
                DocumentName.Text = messageDocument.Document.FileName;
                DocumentSize.Text = messageDocument.Document.Document_.Size.ToString();

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
                
                break;
            }
        }
    }

    private void ButtonDownloadDocument_OnClick(object sender, RoutedEventArgs e)
    {
        _client.ExecuteAsync(new TdApi.DownloadFile
        {
            FileId = _messageDocument.Document.Document_.Id,
            Priority = 1
        });
    }
}