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
            /// A video message
            /// </summary>
            public class MessageVideo : MessageContent
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "messageVideo";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// The video description
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("video")]
                public Video Video { get; set; }

                /// <summary>
                /// Alternative qualities of the video
                /// </summary>
                [JsonProperty("alternative_videos", ItemConverterType = typeof(Converter))]
                public AlternativeVideo[] AlternativeVideos { get; set; }

                /// <summary>
                /// Video caption
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("caption")]
                public FormattedText Caption { get; set; }

                /// <summary>
                /// True, if the caption must be shown above the video; otherwise, the caption must be shown below the video
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("show_caption_above_media")]
                public bool ShowCaptionAboveMedia { get; set; }

                /// <summary>
                /// True, if the video preview must be covered by a spoiler animation
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("has_spoiler")]
                public bool HasSpoiler { get; set; }

                /// <summary>
                /// True, if the video thumbnail must be blurred and the video must be shown only while tapped
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("is_secret")]
                public bool IsSecret { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd