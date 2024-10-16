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
        /// Creates a new supergroup or channel and sends a corresponding messageSupergroupChatCreate. Returns the newly created chat
        /// </summary>
        public class CreateNewSupergroupChat : Function<Chat>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "createNewSupergroupChat";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Title of the new chat; 1-128 characters
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("title")]
            public string Title { get; set; }

            /// <summary>
            /// Pass true to create a forum supergroup chat
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_forum")]
            public bool IsForum { get; set; }

            /// <summary>
            /// Pass true to create a channel chat; ignored if a forum is created
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_channel")]
            public bool IsChannel { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("description")]
            public string Description { get; set; }

            /// <summary>
            /// Chat location if a location-based supergroup is being created; pass null to create an ordinary supergroup chat
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("location")]
            public ChatLocation Location { get; set; }

            /// <summary>
            /// Message auto-delete time value, in seconds; must be from 0 up to 365 * 86400 and be divisible by 86400. If 0, then messages aren't deleted automatically
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("message_auto_delete_time")]
            public int MessageAutoDeleteTime { get; set; }

            /// <summary>
            /// Pass true to create a supergroup for importing messages using importMessages
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("for_import")]
            public bool ForImport { get; set; }
        }

        /// <summary>
        /// Creates a new supergroup or channel and sends a corresponding messageSupergroupChatCreate. Returns the newly created chat
        /// </summary>
        public static Task<Chat> CreateNewSupergroupChatAsync(
            this Client client, string title = default, bool isForum = default, bool isChannel = default, string description = default, ChatLocation location = default, int messageAutoDeleteTime = default, bool forImport = default)
        {
            return client.ExecuteAsync(new CreateNewSupergroupChat
            {
                Title = title, IsForum = isForum, IsChannel = isChannel, Description = description, Location = location, MessageAutoDeleteTime = messageAutoDeleteTime, ForImport = forImport
            });
        }
    }
}
// REUSE-IgnoreEnd