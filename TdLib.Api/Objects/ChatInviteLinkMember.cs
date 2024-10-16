using System;
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
        /// Describes a chat member joined a chat via an invite link
        /// </summary>
        public partial class ChatInviteLinkMember : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "chatInviteLinkMember";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// User identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("user_id")]
            public long UserId { get; set; }

            /// <summary>
            /// Point in time (Unix timestamp) when the user joined the chat
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("joined_chat_date")]
            public int JoinedChatDate { get; set; }

            /// <summary>
            /// True, if the user has joined the chat using an invite link for a chat folder
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("via_chat_folder_invite_link")]
            public bool ViaChatFolderInviteLink { get; set; }

            /// <summary>
            /// User identifier of the chat administrator, approved user join request
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("approver_user_id")]
            public long ApproverUserId { get; set; }
        }
    }
}
// REUSE-IgnoreEnd