using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CherryMerryGramDesktop.Views.Chats;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGramDesktop.Views
{
    public sealed partial class ChatsView : Page
    {
        private static TdClient _client = App._client;

        public ChatsView()
        {
            InitializeComponent();
            
            GenerateChatEntries();
            //_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); }; 
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateNewMessage:
                {
                    Debug.WriteLine("UpdateNewMessage");
                    GenerateChatEntries();
                    break;
                }
            }

            return Task.CompletedTask;
        }

        private async void GenerateChatEntries()
        {
            try
            {
                var chats = GetChats(10000);

                await foreach (var chat in chats)
                {
                    var chatEntry = new ChatEntry
                    {
                        ChatPage = Chat,
                        Chat = chat,
                        ChatId = chat.Id
                    };
                    
                    chatEntry.UpdateChatInfo();
                    ChatsList.Children.Add(chatEntry);
                }
            }
            catch (Exception chatGenerationException)
            {
                Console.WriteLine(chatGenerationException);
                throw;
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
