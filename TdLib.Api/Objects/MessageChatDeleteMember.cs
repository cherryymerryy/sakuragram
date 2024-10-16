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
        public partial class MessageContent : Object
        {
            /// <summary>
            /// A chat member was deleted
            /// </summary>
            public class MessageChatDeleteMember : MessageContent
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "messageChatDeleteMember";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// User identifier of the deleted chat member
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("user_id")]
                public long UserId { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd