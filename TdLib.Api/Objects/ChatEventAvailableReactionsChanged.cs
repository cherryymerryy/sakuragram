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
        public partial class ChatEventAction : Object
        {
            /// <summary>
            /// The chat available reactions were changed
            /// </summary>
            public class ChatEventAvailableReactionsChanged : ChatEventAction
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "chatEventAvailableReactionsChanged";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Previous chat available reactions
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("old_available_reactions")]
                public ChatAvailableReactions OldAvailableReactions { get; set; }

                /// <summary>
                /// New chat available reactions
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("new_available_reactions")]
                public ChatAvailableReactions NewAvailableReactions { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd