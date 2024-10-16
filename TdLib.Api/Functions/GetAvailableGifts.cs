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
        /// Returns gifts that can be sent to other users
        /// </summary>
        public class GetAvailableGifts : Function<Gifts>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getAvailableGifts";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Returns gifts that can be sent to other users
        /// </summary>
        public static Task<Gifts> GetAvailableGiftsAsync(
            this Client client)
        {
            return client.ExecuteAsync(new GetAvailableGifts
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd