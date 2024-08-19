using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CherryMerryGramDesktop.Views.Chats;
using Microsoft.UI.Xaml;
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

        private void OpenChat(long chatId)
        {
            try
            {
                var chat = _client.ExecuteAsync(new TdApi.GetChat {ChatId = chatId}).Result;
            
                var _chatWidget = new Chat();
                _chatWidget.ChatId = chat.Id;
                _chatWidget.UpdateChat(chat.Id);
                _chatWidget.GetMessages(chat.Id);
                Chat.Children.Add(_chatWidget);
            }
            catch
            {
            }
        }
        
        private async void GenerateChatEntries()
        {
            try
            {
                var chats = GetChats(_client.ExecuteAsync(new TdApi.GetChats {Limit = 10000}).Result);
                
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

        private static async IAsyncEnumerable<TdApi.Chat> GetChats(TdApi.Chats chats)
        {
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

        private async void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSearch.Text == "") GenerateChatEntries();
            
            ChatsList.Children.Clear();
            
            var foundedChats = _client.ExecuteAsync(new TdApi.SearchChats
            {
                Query = TextBoxSearch.Text,
                Limit = 100
            });
            
            var chats = GetChats(foundedChats.Result);
            
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

        private void ButtonArchive_OnClick(object sender, RoutedEventArgs e)
        {
            ChatsList.Children.Clear();
            GenerateChatEntries();
        }

        private void ButtonSavedMessages_OnClick(object sender, RoutedEventArgs e)
        {
            OpenChat(_client.ExecuteAsync(new TdApi.GetMe()).Result.Id);
        }
    }
}
