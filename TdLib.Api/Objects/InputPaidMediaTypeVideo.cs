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
        public partial class InputPaidMediaType : Object
        {
            /// <summary>
            /// The media is a video
            /// </summary>
            public class InputPaidMediaTypeVideo : InputPaidMediaType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "inputPaidMediaTypeVideo";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Duration of the video, in seconds
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("duration")]
                public int Duration { get; set; }

                /// <summary>
                /// True, if the video is expected to be streamed
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("supports_streaming")]
                public bool SupportsStreaming { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd