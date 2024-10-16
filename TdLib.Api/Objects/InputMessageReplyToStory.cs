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
        public partial class InputMessageReplyTo : Object
        {
            /// <summary>
            /// Describes a story to be replied
            /// </summary>
            public class InputMessageReplyToStory : InputMessageReplyTo
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "inputMessageReplyToStory";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// The identifier of the sender of the story. Currently, stories can be replied only in the sender's chat and channel stories can't be replied
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("story_sender_chat_id")]
                public long StorySenderChatId { get; set; }

                /// <summary>
                /// The identifier of the story
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("story_id")]
                public int StoryId { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd