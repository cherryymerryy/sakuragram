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
        /// Returns available options for Telegram Stars gifting
        /// </summary>
        public class GetStarGiftPaymentOptions : Function<StarPaymentOptions>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getStarGiftPaymentOptions";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Identifier of the user that will receive Telegram Stars; pass 0 to get options for an unspecified user
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("user_id")]
            public long UserId { get; set; }
        }

        /// <summary>
        /// Returns available options for Telegram Stars gifting
        /// </summary>
        public static Task<StarPaymentOptions> GetStarGiftPaymentOptionsAsync(
            this Client client, long userId = default)
        {
            return client.ExecuteAsync(new GetStarGiftPaymentOptions
            {
                UserId = userId
            });
        }
    }
}
// REUSE-IgnoreEnd