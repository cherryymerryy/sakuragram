using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CherryMerryGramDesktop.Views.Chats;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGramDesktop.Views
{
    public sealed partial class ChatsView : Page
    {
        private static TdClient _client = App._client;
        
        private bool _bInArchive = false;
        private int _totalUnreadArchivedChatsCount = 0;
        
        public ChatsView()
        {
            InitializeComponent();
            
            GenerateChatEntries(new TdApi.ChatList.ChatListMain());
            UpdateArchivedChatsCount();
            
            //_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); }; 
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateNewMessage:
                {
                    if (_bInArchive)
                    {
                        ChatsList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => GenerateChatEntries(new TdApi.ChatList.ChatListArchive()));
                    }
                    else
                    {
                        ChatsList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => GenerateChatEntries(new TdApi.ChatList.ChatListMain()));
                    }
                    break;
                }
            }

            return Task.CompletedTask;
        }

        private void UpdateArchivedChatsCount()
        {
            var chatsIds = _client.ExecuteAsync(new TdApi.GetChats
            {
                Limit = 100, ChatList = new TdApi.ChatList.ChatListArchive()
            }).Result.ChatIds;
            
            foreach (var chatId in chatsIds)
            {
                var chat = _client.ExecuteAsync(new TdApi.GetChat {ChatId = chatId}).Result;
                if (chat.UnreadCount > 0) _totalUnreadArchivedChatsCount++;
            }
            
            ArchiveUnreadChats.Value = _totalUnreadArchivedChatsCount;
        }
        
        private void OpenChat(long chatId)
        {
            try
            {
                var chat = _client.ExecuteAsync(new TdApi.GetChat {ChatId = chatId}).Result;
            
                var chatWidget = new Chat
                {
                    _chatId = chat.Id
                };
                chatWidget.UpdateChat(chat.Id);
                _ = chatWidget.GetMessagesAsync(chat.Id);
                Chat.Children.Add(chatWidget);
            }
            catch
            {
            }
        }
        
        private async void GenerateChatEntries(TdApi.ChatList chatList)
        {
            ChatsList.Children.Clear();
            
            try
            {
                var chats = GetChats(_client.ExecuteAsync(new TdApi.GetChats
                {
                    Limit = 10000,
                    ChatList = chatList
                }).Result);
                
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
            if (TextBoxSearch.Text == "")
            {
                if (_bInArchive)
                {
                    ArchiveStatus.Text = "Archive";
                    ArchiveUnreadChats.Visibility = Visibility.Visible;
                    _bInArchive = false;
                }
                GenerateChatEntries(new TdApi.ChatList.ChatListMain());
                return;
            }
            
            ChatsList.Children.Clear();
            
            var foundedChats = _client.ExecuteAsync(new TdApi.SearchChats
            {
                Query = TextBoxSearch.Text,
                Limit = 100
            });

            var foundedMessages = _client.ExecuteAsync(new TdApi.SearchMessages()
            {
                ChatList = new TdApi.ChatList.ChatListMain(),
                Limit = 100,
                OnlyInChannels = true
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
            if (!_bInArchive)
            {
                ArchiveStatus.Text = "Back";
                ArchiveUnreadChats.Visibility = Visibility.Collapsed;
                _bInArchive = true;
                GenerateChatEntries(new TdApi.ChatList.ChatListArchive());
            }
            else
            {
                ArchiveStatus.Text = "Archive";
                ArchiveUnreadChats.Visibility = Visibility.Visible;
                _bInArchive = false;
                GenerateChatEntries(new TdApi.ChatList.ChatListMain());
            }
        }

        private void ButtonSavedMessages_OnClick(object sender, RoutedEventArgs e)
        {
            OpenChat(_client.ExecuteAsync(new TdApi.GetMe()).Result.Id);
        }
    }
}
