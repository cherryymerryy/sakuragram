using System.Collections.Generic;
using CherryMerryGramDesktop;
using TdLib;

namespace CherryMerryGramDesktop.Services;

public class ForwardService
{
    private readonly TdApi.Client _client = App._client;
    private readonly List<long> _messageIds = [];

    public void ForwardMessages(long chatId, long fromChatId)
    {
        var messagesToForward = _messageIds.ToArray();
        
        _client.ExecuteAsync(new TdApi.ForwardMessages
        {
            ChatId = chatId,
            MessageIds = messagesToForward,
            FromChatId = fromChatId
        });
        
        ClearMessagesList();
    }

    public void SelectMessageToForward(long messageId)
    {
        if (_messageIds.Contains(messageId)) return;
        if (_messageIds.Count == 100) return;
        _messageIds.Add(messageId);
    }

    public void ClearMessagesList()
    {
        if (_messageIds.Count <= 0) return;
        _messageIds.Clear();
    }

    public long GetSelectedMessages()
    {
        return _messageIds.Count;
    }
}