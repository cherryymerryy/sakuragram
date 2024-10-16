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
        /// Adds or removes a bot to attachment and side menu. Bot can be added to the menu, only if userTypeBot.can_be_added_to_attachment_menu == true
        /// </summary>
        public class ToggleBotIsAddedToAttachmentMenu : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "toggleBotIsAddedToAttachmentMenu";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Bot's user identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("bot_user_id")]
            public long BotUserId { get; set; }

            /// <summary>
            /// Pass true to add the bot to attachment menu; pass false to remove the bot from attachment menu
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_added")]
            public bool IsAdded { get; set; }

            /// <summary>
            /// Pass true if the current user allowed the bot to send them messages. Ignored if is_added is false
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("allow_write_access")]
            public bool AllowWriteAccess { get; set; }
        }

        /// <summary>
        /// Adds or removes a bot to attachment and side menu. Bot can be added to the menu, only if userTypeBot.can_be_added_to_attachment_menu == true
        /// </summary>
        public static Task<Ok> ToggleBotIsAddedToAttachmentMenuAsync(
            this Client client, long botUserId = default, bool isAdded = default, bool allowWriteAccess = default)
        {
            return client.ExecuteAsync(new ToggleBotIsAddedToAttachmentMenu
            {
                BotUserId = botUserId, IsAdded = isAdded, AllowWriteAccess = allowWriteAccess
            });
        }
    }
}
// REUSE-IgnoreEnd