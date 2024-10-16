using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

// REUSE-IgnoreStart
namespace TdLib
{
    /// <summary>
    /// Autogenerated TDLib APIs
    /// </summary>
    public static partial class TdApi
    {
        /// <summary>
        /// Edits a subscription invite link for a channel chat. Requires can_invite_users right in the chat for own links and owner privileges for other links
        /// </summary>
        public class EditChatSubscriptionInviteLink : Function<ChatInviteLink>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "editChatSubscriptionInviteLink";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Chat identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("chat_id")]
            public long ChatId { get; set; }

            /// <summary>
            /// Invite link to be edited
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("invite_link")]
            public string InviteLink { get; set; }

            /// <summary>
            /// Invite link name; 0-32 characters
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Edits a subscription invite link for a channel chat. Requires can_invite_users right in the chat for own links and owner privileges for other links
        /// </summary>
        public static Task<ChatInviteLink> EditChatSubscriptionInviteLinkAsync(
            this Client client, long chatId = default, string inviteLink = default, string name = default)
        {
            return client.ExecuteAsync(new EditChatSubscriptionInviteLink
            {
                ChatId = chatId, InviteLink = inviteLink, Name = name
            });
        }
    }
}
// REUSE-IgnoreEnd