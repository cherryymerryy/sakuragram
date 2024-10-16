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
        /// Forces an updates.getDifference call to the Telegram servers; for testing only
        /// </summary>
        public class TestGetDifference : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "testGetDifference";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Forces an updates.getDifference call to the Telegram servers; for testing only
        /// </summary>
        public static Task<Ok> TestGetDifferenceAsync(
            this Client client)
        {
            return client.ExecuteAsync(new TestGetDifference
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd