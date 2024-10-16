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
        public partial class CallServerType : Object
        {
            /// <summary>
            /// Describes the type of call server
            /// </summary>
            public class CallServerTypeTelegramReflector : CallServerType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "callServerTypeTelegramReflector";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// A peer tag to be used with the reflector
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("peer_tag")]
                public byte[] PeerTag { get; set; }

                /// <summary>
                /// True, if the server uses TCP instead of UDP
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("is_tcp")]
                public bool IsTcp { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd