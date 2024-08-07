using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using TdLib;
using CherryMerryGram;
using CherryMerryGram.Views.Chats;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CherryMerryGram.Views
{
    public sealed partial class ChatsView : Page
    {
        private static TdClient _client = MainWindow._client;

        public ChatsView()
        {
            this.InitializeComponent();
            
            GenerateChatEntries(); 
            //_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); }; 
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateNewMessage: { GenerateChatEntries(); break; }
            }

            return Task.CompletedTask;
        }

        private async void GenerateChatEntries()
        {
            ChatsList.Children.Clear();
            var chats = GetChats(4000);

            await foreach (var chat in chats)
            {
                var chatEntry = new ChatEntry();
                chatEntry.ChatPage = Chat;
                chatEntry.UpdateChat(chat);
                ChatsList.Children.Add(chatEntry);
            }
        }

        private static async IAsyncEnumerable<TdApi.Chat> GetChats(int limit)
        {
            var chats = await _client.ExecuteAsync(new TdApi.GetChats
            {
                Limit = limit
            });

            foreach (var chatId in chats.ChatIds)
            {
                var chat = await _client.ExecuteAsync(new TdApi.GetChat
                {
                    ChatId = chatId
                });

                if (chat.Type is TdApi.ChatType.ChatTypeSupergroup or TdApi.ChatType.ChatTypeBasicGroup or TdApi.ChatType.ChatTypePrivate)
                {
                    yield return chat;
                }
            }
        }
    }
}
