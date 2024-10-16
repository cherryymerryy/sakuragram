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
        /// Disconnects all websites from the current user's Telegram account
        /// </summary>
        public class DisconnectAllWebsites : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "disconnectAllWebsites";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Disconnects all websites from the current user's Telegram account
        /// </summary>
        public static Task<Ok> DisconnectAllWebsitesAsync(
            this Client client)
        {
            return client.ExecuteAsync(new DisconnectAllWebsites
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd