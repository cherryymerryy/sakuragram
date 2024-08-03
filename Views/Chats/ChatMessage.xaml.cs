using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using TdLib;
using CherryMerryGram;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CherryMerryGram.Views.Chats
{
    public sealed partial class ChatMessage : Page
    {
        private static TdClient _client = MainWindow._client;
        
        public ChatMessage()
        {
            this.InitializeComponent();
        }
        
        public async void  UpdateMessage(TdApi.Message message)
        {
            var user = GetUser(message);
            var chatMember = GetChatMember(message.ChatId, message.SenderId);
            var chat = GetChat(message.ChatId);
            var chatType = chat.Result.Type;
            var userProfilePicture = user.Result.ProfilePhoto;

            if (chat.Result.Type is not TdApi.ChatType.ChatTypePrivate && chat.Result.Permissions.CanSendBasicMessages)
            {
                try
                {
                    var userProfilePictureFile = await _client.ExecuteAsync(new TdApi.DownloadFile
                    {
                        FileId = userProfilePicture.Small.Id,
                        Priority = 1
                    });

                    ProfilePicture.ImageSource = new BitmapImage(new Uri(userProfilePictureFile.Local.Path));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                ProfilePicture.ImageSource = null;
                BorderProfilePicture.Visibility = Visibility.Collapsed;
            }

            DisplayName.Text = chat.Result.Type switch
            {
                TdApi.ChatType.ChatTypePrivate => DisplayName.Text = $"{user.Result.FirstName} {user.Result.LastName}",
                TdApi.ChatType.ChatTypeSecret => DisplayName.Text = $"{user.Result.FirstName} {user.Result.LastName}",
                TdApi.ChatType.ChatTypeBasicGroup => DisplayName.Text = chat.Result.Title,
                _ => DisplayName.Text
            };

            if (chatType is TdApi.ChatType.ChatTypeSupergroup && chat.Result.Permissions.CanSendBasicMessages)
            {
                DisplayName.Text = $"{user.Result.FirstName} {user.Result.LastName}";
            }
            else if (chatType is TdApi.ChatType.ChatTypeSupergroup && !chat.Result.Permissions.CanSendBasicMessages)
            {
                DisplayName.Text = chat.Result.Title;
            }
            
            Status.Visibility = chat.Result.Type switch
            {
                TdApi.ChatType.ChatTypePrivate => Status.Visibility = Visibility.Collapsed,
                TdApi.ChatType.ChatTypeSecret => Status.Visibility = Visibility.Collapsed,
                TdApi.ChatType.ChatTypeSupergroup => Status.Visibility = Visibility.Visible,
                TdApi.ChatType.ChatTypeBasicGroup => Status.Visibility = Visibility.Collapsed,
                _ => Status.Visibility
            };

            if (chatType is TdApi.ChatType.ChatTypeSupergroup && chat.Result.Permissions.CanSendBasicMessages)
            {
                Status.Text = chatMember.Result.Status switch
                {
                    TdApi.ChatMemberStatus.ChatMemberStatusCreator => Status.Text += " (creator)",
                    TdApi.ChatMemberStatus.ChatMemberStatusAdministrator => Status.Text += " (admin)",
                    TdApi.ChatMemberStatus.ChatMemberStatusMember => Status.Text += " (member)",
                    TdApi.ChatMemberStatus.ChatMemberStatusLeft => Status.Text += " (left)",
                    TdApi.ChatMemberStatus.ChatMemberStatusBanned => Status.Text += " (banned)",
                    _ => Status.Text
                };
            }

            MessageContent.Text = message.Content switch
            {
                TdApi.MessageContent.MessageText messageText => MessageContent.Text = messageText.Text.Text,
                _ => MessageContent.Text
            };
        }

        private static Task<TdApi.User> GetUser(TdApi.Message message)
        {
            long userId = message.SenderId switch {
                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                _ => 0
            };
            
            var user = _client.ExecuteAsync(new TdApi.GetUser
            {
                UserId = userId
            });

            return user;
        }

        private static Task<TdApi.ChatMember> GetChatMember(long chatId, TdApi.MessageSender user)
        {
            var chatMember = _client.ExecuteAsync(new TdApi.GetChatMember
            {
                ChatId = chatId,
                MemberId = user
            });
            
            return chatMember;
        }

        private static Task<TdApi.Chat> GetChat(long chatId)
        {
            var chat = _client.ExecuteAsync(new TdApi.GetChat
            {
                ChatId = chatId
            });
            
            return chat;
        }

        private void UIElement_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void UIElement_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
