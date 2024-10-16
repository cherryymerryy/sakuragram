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
        public partial class InputMessageContent : Object
        {
            /// <summary>
            /// A document message (general file)
            /// </summary>
            public class InputMessageDocument : InputMessageContent
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "inputMessageDocument";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Document to be sent
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("document")]
                public InputFile Document { get; set; }

                /// <summary>
                /// Document thumbnail; pass null to skip thumbnail uploading
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("thumbnail")]
                public InputThumbnail Thumbnail { get; set; }

                /// <summary>
                /// Pass true to disable automatic file type detection and send the document as a file. Always true for files sent to secret chats
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("disable_content_type_detection")]
                public bool DisableContentTypeDetection { get; set; }

                /// <summary>
                /// Document caption; pass null to use an empty caption; 0-getOption("message_caption_length_max") characters
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("caption")]
                public FormattedText Caption { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd