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
        /// Sets menu button for the given user or for all users; for bots only
        /// </summary>
        public class SetMenuButton : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "setMenuButton";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Identifier of the user or 0 to set menu button for all users
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("user_id")]
            public long UserId { get; set; }

            /// <summary>
            /// New menu button
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("menu_button")]
            public BotMenuButton MenuButton { get; set; }
        }

        /// <summary>
        /// Sets menu button for the given user or for all users; for bots only
        /// </summary>
        public static Task<Ok> SetMenuButtonAsync(
            this Client client, long userId = default, BotMenuButton menuButton = default)
        {
            return client.ExecuteAsync(new SetMenuButton
            {
                UserId = userId, MenuButton = menuButton
            });
        }
    }
}
// REUSE-IgnoreEnd