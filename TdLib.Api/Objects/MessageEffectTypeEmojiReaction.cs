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
        public partial class MessageEffectType : Object
        {
            /// <summary>
            /// Describes type of emoji effect
            /// </summary>
            public class MessageEffectTypeEmojiReaction : MessageEffectType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "messageEffectTypeEmojiReaction";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Select animation for the effect in TGS format
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("select_animation")]
                public Sticker SelectAnimation { get; set; }

                /// <summary>
                /// Effect animation for the effect in TGS format
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("effect_animation")]
                public Sticker EffectAnimation { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd