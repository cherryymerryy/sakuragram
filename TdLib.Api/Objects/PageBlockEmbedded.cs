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
        public partial class PageBlock : Object
        {
            /// <summary>
            /// An embedded web page
            /// </summary>
            public class PageBlockEmbedded : PageBlock
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "pageBlockEmbedded";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// URL of the embedded page, if available
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("url")]
                public string Url { get; set; }

                /// <summary>
                /// HTML-markup of the embedded page
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("html")]
                public string Html { get; set; }

                /// <summary>
                /// Poster photo, if available; may be null
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("poster_photo")]
                public Photo PosterPhoto { get; set; }

                /// <summary>
                /// Block width; 0 if unknown
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("width")]
                public int Width { get; set; }

                /// <summary>
                /// Block height; 0 if unknown
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("height")]
                public int Height { get; set; }

                /// <summary>
                /// Block caption
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("caption")]
                public PageBlockCaption Caption { get; set; }

                /// <summary>
                /// True, if the block must be full width
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("is_full_width")]
                public bool IsFullWidth { get; set; }

                /// <summary>
                /// True, if scrolling needs to be allowed
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("allow_scrolling")]
                public bool AllowScrolling { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd