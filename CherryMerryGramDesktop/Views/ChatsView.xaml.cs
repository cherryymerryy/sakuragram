using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        
        public Chat _currentChat;
        private bool _bInArchive = false;
        private bool _firstGenerate = true;
        private int _totalUnreadArchivedChatsCount = 0;
        private List<long> _chatsIds = [];
        
        public ChatsView()
        {
            InitializeComponent();
            UpdateArchivedChatsCount();
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); }; 
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateNewMessage updateNewMessage:
                {
                    ChatsList.DispatcherQueue.TryEnqueue(() =>
                    {
                        var chats = ChatsList.Children;
                        var chatToMove = chats.OfType<ChatEntry>()
                            .FirstOrDefault(chat => chat.ChatId == updateNewMessage.Message.ChatId);

                        if (chatToMove != null && chatToMove.ChatId == updateNewMessage.Message.ChatId)
                        {
                            chats.Remove(chatToMove);
                            chats.Insert(0, chatToMove);
                            chatToMove.UpdateChatInfo();
                        }
                    });
                    break;
                }
                case TdApi.Update.UpdateNewChat updateNewChat:
                {
                    // if (!_firstGenerate && !_chatsIds.Contains(updateNewChat.Chat.Id))
                    // {
                    //     ChatsList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
                    //     {
                    //         var chatEntry = new ChatEntry
                    //         {
                    //             ChatPage = Chat,
                    //             Chat = updateNewChat.Chat,
                    //             ChatId = updateNewChat.Chat.Id
                    //         };
                    //         ChatsList.Children.Insert(0, chatEntry);
                    //         _chatsIds.Add(updateNewChat.Chat.Id);
                    //     });
                    // }
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
            
                _currentChat = new Chat
                {
                    _ChatsView = this,
                    _chatId = chat.Id
                };
                _currentChat.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => _currentChat.UpdateChat(chat.Id));
                Chat.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => Chat.Children.Add(_currentChat));
            }
            catch { }
        }

        public void CloseChat()
        {
            if (_currentChat == null) return;
            _currentChat.CloseChat();
            Chat.Children.Remove(_currentChat);
        }
        
        private async void GenerateChatEntries(TdApi.ChatList chatList)
        {
            ChatsList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => ChatsList.Children.Clear());
            
            var chats = GetChats(_client.ExecuteAsync(new TdApi.GetChats
            {
                Limit = 10000,
                ChatList = chatList
            }).Result);
            
            await foreach (var chat in chats)
            {
                var chatEntry = new ChatEntry
                {
                    _ChatsView = this,
                    ChatPage = Chat,
                    Chat = chat,
                    ChatId = chat.Id
                };
                _chatsIds.Add(chat.Id);
                
                chatEntry.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal,
                    () => chatEntry.UpdateChatInfo());
                ChatsList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                    () => ChatsList.Children.Add(chatEntry));
            }
            
            _firstGenerate = false;
        }

        private static async IAsyncEnumerable<TdApi.Chat> GetChats(TdApi.Chats chats)
        {
            foreach (var chatId in chats.ChatIds)
            {
                var chat = _client.ExecuteAsync(new TdApi.GetChat
                {
                    ChatId = chatId
                }).Result;

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
            
            ChatsList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => ChatsList.Children.Clear());
            
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
                ChatsList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
                {
                    ChatsList.Children.Add(chatEntry);
                });
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

        private void ChatList_OnLoaded(object sender, RoutedEventArgs e)
        {
            GenerateChatEntries(new TdApi.ChatList.ChatListMain());
        }

        private void ButtonNewMessage_OnClick(object sender, RoutedEventArgs e)
        {
            NewMessage.ShowAsync();
        }

        private async void NewGroup_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (TextBoxGroupName.Text == "") return;
            
            var newGroup = await _client.ExecuteAsync(new TdApi.CreateNewBasicGroupChat
            {
                Title = TextBoxGroupName.Text,
                UserIds = null,
            });
            
            // _client.ExecuteAsync(new TdApi.SetChatPhoto
            // {
            //     ChatId = newGroup.ChatId, 
            //     Photo = 
            // });
        }
        
        private void NewChannel_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (TextBoxChannelName.Text == "") return;

            _client.ExecuteAsync(new TdApi.CreateNewSupergroupChat
            {
                IsChannel = true,
                Title = TextBoxChannelName.Text,
                Description = TextBoxChannelDescription.Text
            });
        }
        
        private void CreateNewGroup_OnClick(object sender, RoutedEventArgs e)
        {
            NewMessage.Hide();
            NewGroup.ShowAsync();
        }

        private void CreateNewChannel_OnClick(object sender, RoutedEventArgs e)
        {
            NewMessage.Hide();
            NewChannel.ShowAsync();
        }
    }
}
