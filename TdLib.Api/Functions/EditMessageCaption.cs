using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

// REUSE-IgnoreStart
namespace TdLib
{
    /// <summary>
    /// Autogenerated TDLib APIs
    /// </summary>
    public static partial class TdApi
    {
        /// <summary>
        /// Edits the message content caption. Returns the edited message after the edit is completed on the server side
        /// </summary>
        public class EditMessageCaption : Function<Message>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "editMessageCaption";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// The chat the message belongs to
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("chat_id")]
            public long ChatId { get; set; }

            /// <summary>
            /// Identifier of the message. Use messageProperties.can_be_edited to check whether the message can be edited
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("message_id")]
            public long MessageId { get; set; }

            /// <summary>
            /// The new message reply markup; pass null if none; for bots only
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("reply_markup")]
            public ReplyMarkup ReplyMarkup { get; set; }

            /// <summary>
            /// New message content caption; 0-getOption("message_caption_length_max") characters; pass null to remove caption
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("caption")]
            public FormattedText Caption { get; set; }

            /// <summary>
            /// Pass true to show the caption above the media; otherwise, the caption will be shown below the media. May be true only for animation, photo, and video messages
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("show_caption_above_media")]
            public bool ShowCaptionAboveMedia { get; set; }
        }

        /// <summary>
        /// Edits the message content caption. Returns the edited message after the edit is completed on the server side
        /// </summary>
        public static Task<Message> EditMessageCaptionAsync(
            this Client client, long chatId = default, long messageId = default, ReplyMarkup replyMarkup = default, FormattedText caption = default, bool showCaptionAboveMedia = default)
        {
            return client.ExecuteAsync(new EditMessageCaption
            {
                ChatId = chatId, MessageId = messageId, ReplyMarkup = replyMarkup, Caption = caption, ShowCaptionAboveMedia = showCaptionAboveMedia
            });
        }
    }
}
// REUSE-IgnoreEnd