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
        /// Returns the high scores for a game and some part of the high score table in the range of the specified user; for bots only
        /// </summary>
        public class GetGameHighScores : Function<GameHighScores>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getGameHighScores";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// The chat that contains the message with the game
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("chat_id")]
            public long ChatId { get; set; }

            /// <summary>
            /// Identifier of the message
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("message_id")]
            public long MessageId { get; set; }

            /// <summary>
            /// User identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("user_id")]
            public long UserId { get; set; }
        }

        /// <summary>
        /// Returns the high scores for a game and some part of the high score table in the range of the specified user; for bots only
        /// </summary>
        public static Task<GameHighScores> GetGameHighScoresAsync(
            this Client client, long chatId = default, long messageId = default, long userId = default)
        {
            return client.ExecuteAsync(new GetGameHighScores
            {
                ChatId = chatId, MessageId = messageId, UserId = userId
            });
        }
    }
}
// REUSE-IgnoreEnd