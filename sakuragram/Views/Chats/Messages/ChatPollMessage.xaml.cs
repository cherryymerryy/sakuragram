using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Views.Chats.Messages;

public partial class ChatPollMessage : Page
{
    private static TdClient _client = App._client;
    private static TdApi.Poll _poll;
    private static TdApi.FormattedText _explanation;

    private static long _chatId;
    private static long _messageId;
    private static int _correctOptionId;
    private static bool _isMultipleAnswers;
    private static bool _isAnonymous;
    private static bool _hasSelectedOption;
    private static List<int> _pollOptionsIds = [];
    
    private MediaService _mediaService = new MediaService();

    public ChatPollMessage()
    {
        InitializeComponent();
    }

    private Task ProcessUpdate(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdatePoll updatePoll:
            {
                if (updatePoll.Poll.Id == _poll.Id)
                {
                    DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                    {
                        StackPanelPollOptions.Children.Clear();

                        for (int i = 0; i < updatePoll.Poll.Options.Length; i++)
                        {
                            var pollOption = updatePoll.Poll.Options[i];
                            GeneratePollOption(pollOption, i);
                        }
                    });
                }
                break;
            }
            case TdApi.Update.UpdatePollAnswer updatePollAnswer:
            {
                if (updatePollAnswer.PollId == _poll.Id)
                {
                    DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                    {
                        StackPanelPollOptions.Children.Clear();
                        
                        UpdateMessage(_client.GetMessageAsync(chatId: _chatId, messageId: _messageId).Result);
                    });
                }
                break;
            }
        }

        return Task.CompletedTask;
    }
    
    public void UpdateMessage(TdApi.Message message)
    {
        _chatId = message.ChatId;
        _messageId = message.Id;
        
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
            MediaService.GetUserPhoto(user, ProfilePicture);
        }
        else // if senderId < 0 then it's a chat
        {
            var chat = _client.GetChatAsync(chatId: sender).Result;
            DisplayName.Text = chat.Title;
            MediaService.GetChatPhoto(chat, ProfilePicture);
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
            case TdApi.MessageContent.MessagePoll messagePoll:
            {
                _poll = messagePoll.Poll;
                _isAnonymous = messagePoll.Poll.IsAnonymous;
                
                PollType.Text = messagePoll.Poll.Type switch
                {
                    TdApi.PollType.PollTypeRegular => messagePoll.Poll.IsAnonymous ? "Anonymous poll" : "Poll",
                    TdApi.PollType.PollTypeQuiz => messagePoll.Poll.IsAnonymous ? "Anonymous quiz" : "Quiz",
                    _ => "Unsupported poll type"
                };
                
                if (messagePoll.Poll.Question.Text != string.Empty)
                {
                    MessageContent.Text = messagePoll.Poll.Question.Text;
                    MessageContent.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageContent.Text = string.Empty;
                    MessageContent.Visibility = Visibility.Collapsed;
                }

                if (messagePoll.Poll.TotalVoterCount > 0)
                {
                    PollTotalVoteCount.Text = ", " + messagePoll.Poll.TotalVoterCount + " votes";
                    PollTotalVoteCount.Visibility = Visibility.Visible;
                }
                else
                {
                    PollTotalVoteCount.Text = string.Empty;
                    PollTotalVoteCount.Visibility = Visibility.Collapsed;
                }
                
                switch (messagePoll.Poll.Type)
                {
                    case TdApi.PollType.PollTypeRegular regular:
                    {
                        _isMultipleAnswers = regular.AllowMultipleAnswers;
                        ButtonAnswer.Visibility = _isMultipleAnswers ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    }
                    case TdApi.PollType.PollTypeQuiz quiz:
                    {
                        _correctOptionId = quiz.CorrectOptionId;
                        _explanation = quiz.Explanation;
                        break;
                    }
                }
                
                for (int i = 0; i < messagePoll.Poll.Options.Length; i++)
                {
                    var pollOption = messagePoll.Poll.Options[i];
                    if (pollOption.IsChosen)
                    {
                        _hasSelectedOption = true;
                        _pollOptionsIds.Add(i);
                    }
                    GeneratePollOption(pollOption, i);
                }
                
                ButtonAnswer.Visibility = _hasSelectedOption ? Visibility.Collapsed : Visibility.Visible;
                break;
            }
        }
        
        _client.UpdateReceived += async (_, update) => { await ProcessUpdate(update); };
    }

    private void GeneratePollOption(TdApi.PollOption pollOption, int id)
    {
        var pollOptionText = new TextBlock();
        pollOptionText.Text = $"{pollOption.VotePercentage}% | {pollOption.Text.Text}";
        
        if (_isMultipleAnswers)
        {
            CheckBox checkBox = new();
            checkBox.IsChecked = pollOption.IsChosen;
            checkBox.Click += (sender, args) => CheckBoxPollOption_Click(sender, args, id);
            
            checkBox.Content = pollOptionText;
            StackPanelPollOptions.Children.Add(checkBox);
        }
        else
        {
            RadioButton radioButton = new();
            radioButton.IsChecked = pollOption.IsChosen;
            radioButton.Click += (sender, args) => RadioButtonPollOption_Click(sender, args, id);
            
            radioButton.Content = pollOptionText;
            StackPanelPollOptions.Children.Add(radioButton);
        }
        
        _pollOptionsIds.Add(id);
    }

    private void CheckBoxPollOption_Click(object sender, RoutedEventArgs args, int id)
    {
        if (_pollOptionsIds.Contains(id))
        {
            _pollOptionsIds.Remove(id);
            if (_pollOptionsIds.Count <= 0) ButtonAnswer.Visibility = Visibility.Visible;
        }
        else _pollOptionsIds.Add(id);
    }

    private async void RadioButtonPollOption_Click(object sender, RoutedEventArgs e, int id)
    {
        _pollOptionsIds.Add(id);
        
        await _client.ExecuteAsync(new TdApi.SetPollAnswer
        {
            ChatId = _chatId,
            MessageId = _messageId,
            OptionIds = _pollOptionsIds.ToArray()
        });
        
        _pollOptionsIds.Clear();
    }

    private async void ButtonAnswer_OnClick(object sender, RoutedEventArgs e)
    {
        await _client.ExecuteAsync(new TdApi.SetPollAnswer
        {
            ChatId = _chatId,
            MessageId = _messageId,
            OptionIds = _pollOptionsIds.ToArray()
        });
        
        _pollOptionsIds.Clear();
    }
}