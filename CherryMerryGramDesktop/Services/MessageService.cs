using System;
using System.Collections.Generic;
using CherryMerryGramDesktop.Views.Chats;
using TdLib;

namespace CherryMerryGramDesktop.Services;

public class MessageService
{
    private static TdClient _client = App._client;
    private List<long> _selectedMessages = [];

    public long[] GetSelectedMessages()
    {
        return _selectedMessages.ToArray();
    }

    public void SelectMessage(long messageId)
    {
        if (_selectedMessages.Contains(messageId)) return;
        _selectedMessages.Add(messageId);
    }
    
    public void DeselectMessage(long messageId)
    {
        if (!_selectedMessages.Contains(messageId)) return;
        _selectedMessages.Remove(messageId);
    }

    public void ClearSelectedMessages()
    {
        if (_selectedMessages.Count <= 0) return;
        _selectedMessages.Clear();
    }
    
    public string GetLastMessageContent(TdApi.Message message)
    {
        var user = _client.ExecuteAsync(new TdApi.GetUser
        {
            UserId = message.SenderId switch
            {
                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                _ => 0
            }
        }).Result;
        
        var lastMessage = message.Content switch
        {
            TdApi.MessageContent.MessageText messageText => $"{user.FirstName}: {messageText.Text.Text}",
            
            TdApi.MessageContent.MessageAudio messageAudio =>
                $"{user.FirstName} Audio message ({messageAudio.Audio.Duration})",
            
            TdApi.MessageContent.MessageVoiceNote messageVoiceNote =>
                $"{user.FirstName} Voice message ({messageVoiceNote.VoiceNote.Duration})",
            
            TdApi.MessageContent.MessageVideo messageVideo =>
                $"{user.FirstName} Video message ({messageVideo.Video.Duration} sec)",
            
            TdApi.MessageContent.MessagePhoto messagePhoto =>
                $"{user.FirstName} Photo message ({messagePhoto.Photo.Minithumbnail.Width}x{messagePhoto.Photo.Minithumbnail.Height})",
            
            TdApi.MessageContent.MessageSticker messageSticker =>
                $"{user.FirstName} {messageSticker.Sticker.Emoji} Sticker message",
            TdApi.MessageContent.MessagePoll messagePoll => $"{user.FirstName}: {messagePoll.Poll.Question}",
            
            TdApi.MessageContent.MessagePinMessage messagePinMessage =>
                $"{user.FirstName} pinned {_client.ExecuteAsync(new TdApi.GetMessage
                {
                    ChatId = message.ChatId,
                    MessageId = messagePinMessage.MessageId
                })}",
            
            TdApi.MessageContent.MessageGame messageGame => $"{user.FirstName} {messageGame.Game.Title}",
            
            _ => throw new ArgumentOutOfRangeException()
        };
        
        return lastMessage;
    }
}