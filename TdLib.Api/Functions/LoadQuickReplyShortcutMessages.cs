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
        /// Loads quick reply messages that can be sent by a given quick reply shortcut. The loaded messages will be sent through updateQuickReplyShortcutMessages
        /// </summary>
        public class LoadQuickReplyShortcutMessages : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "loadQuickReplyShortcutMessages";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Unique identifier of the quick reply shortcut
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("shortcut_id")]
            public int ShortcutId { get; set; }
        }

        /// <summary>
        /// Loads quick reply messages that can be sent by a given quick reply shortcut. The loaded messages will be sent through updateQuickReplyShortcutMessages
        /// </summary>
        public static Task<Ok> LoadQuickReplyShortcutMessagesAsync(
            this Client client, int shortcutId = default)
        {
            return client.ExecuteAsync(new LoadQuickReplyShortcutMessages
            {
                ShortcutId = shortcutId
            });
        }
    }
}
// REUSE-IgnoreEnd