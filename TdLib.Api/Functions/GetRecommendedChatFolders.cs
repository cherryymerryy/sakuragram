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
        /// Returns recommended chat folders for the current user
        /// </summary>
        public class GetRecommendedChatFolders : Function<RecommendedChatFolders>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getRecommendedChatFolders";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Returns recommended chat folders for the current user
        /// </summary>
        public static Task<RecommendedChatFolders> GetRecommendedChatFoldersAsync(
            this Client client)
        {
            return client.ExecuteAsync(new GetRecommendedChatFolders
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd