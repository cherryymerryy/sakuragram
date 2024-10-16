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
            /// A newly created basic group
            /// </summary>
            public class MessageBasicGroupChatCreate : MessageContent
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "messageBasicGroupChatCreate";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Title of the basic group
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("title")]
                public string Title { get; set; }

                /// <summary>
                /// User identifiers of members in the basic group
                /// </summary>
                [JsonProperty("member_user_ids", ItemConverterType = typeof(Converter))]
                public long[] MemberUserIds { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd