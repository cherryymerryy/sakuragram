using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private static readonly TdClient _client = App._client;
        private static TdApi.Chats _defaultChats;
        private static TdApi.Chats _defaultChatsInArchive;
        
        public Chat _currentChat;
        private bool _bInArchive = false;
        private bool _firstGenerate = true;
        private int _totalUnreadArchivedChatsCount = 0;
        private List<long> _chatsIds = [];
        
        public ChatsView()
        {
            InitializeComponent();
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
            if (_defaultChatsInArchive == null) return;

            ChatsList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () => {
                foreach (var chatId in _defaultChatsInArchive.ChatIds)
                {
                    var chat = _client.ExecuteAsync(new TdApi.GetChat {ChatId = chatId}).Result;
                    if (chat.UnreadCount > 0) _totalUnreadArchivedChatsCount++;
                }
            
                ArchiveUnreadChats.Value = _totalUnreadArchivedChatsCount;
            });
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
            Chat.Children.Remove(_currentChat);
        }
        
        private void GenerateChatEntries(TdApi.Chats chats)
        {
            if (chats == null) return;

            ChatsList.Children.Clear();
            _chatsIds.Clear();

            Task.Run(() =>
            {
                var chatEntries = new List<ChatEntry>();

                ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                Parallel.ForEach(chats.ChatIds, parallelOptions, chatId => {
                    DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                    {
                        var chatEntry = new ChatEntry
                        {
                            _ChatsView = this,
                            ChatPage = Chat,
                            ChatId = chatId
                        };
                        chatEntries.Add(chatEntry);
                    });
                });

                DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                {
                    foreach (var chatEntry in chatEntries)
                    {
                        ChatsList.Children.Add(chatEntry);
                        chatEntry.UpdateChatInfo();
                        _chatsIds.Add(chatEntry.ChatId);
                    }
                });
            });
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSearch.Text == "")
            {
                if (_bInArchive)
                {
                    ArchiveStatus.Text = "Archive";
                    ArchiveUnreadChats.Visibility = Visibility.Visible;
                    _bInArchive = false;
                }
                GenerateChatEntries(_defaultChats);
                return;
            }
            
            var foundedChats = _client.ExecuteAsync(new TdApi.SearchChats
            {
                Query = TextBoxSearch.Text,
                Limit = 100
            }).Result;
            
            GenerateChatEntries(foundedChats);
        }

        private void ButtonArchive_OnClick(object sender, RoutedEventArgs e)
        {
            ChatsList.Children.Clear();
            if (!_bInArchive)
            {
                ArchiveStatus.Text = "Back";
                ArchiveUnreadChats.Visibility = Visibility.Collapsed;
                _bInArchive = true;
                GenerateChatEntries(_defaultChatsInArchive);
            }
            else
            {
                ArchiveStatus.Text = "Archive";
                ArchiveUnreadChats.Visibility = Visibility.Visible;
                _bInArchive = false;
                GenerateChatEntries(_defaultChats);
            }
        }

        private void ButtonSavedMessages_OnClick(object sender, RoutedEventArgs e)
        {
            OpenChat(_client.ExecuteAsync(new TdApi.GetMe()).Result.Id);
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

        private void ChatsList_OnLoaded(object sender, RoutedEventArgs e)
        {
            _defaultChats = _client.ExecuteAsync(new TdApi.GetChats
            {
                Limit = 10000,
                ChatList = new TdApi.ChatList.ChatListMain()
            }).Result;
            GenerateChatEntries(_defaultChats);
        }

        private void ArchiveUnreadChats_OnLoaded(object sender, RoutedEventArgs e)
        {
            _defaultChatsInArchive = _client.ExecuteAsync(new TdApi.GetChats
            {
                Limit = 10000,
                ChatList = new TdApi.ChatList.ChatListArchive()
            }).Result;
            UpdateArchivedChatsCount();
        }
    }
}
