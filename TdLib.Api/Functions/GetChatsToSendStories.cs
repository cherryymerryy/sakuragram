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
        /// Returns supergroup and channel chats in which the current user has the right to post stories. The chats must be rechecked with canSendStory before actually trying to post a story there
        /// </summary>
        public class GetChatsToSendStories : Function<Chats>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getChatsToSendStories";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Returns supergroup and channel chats in which the current user has the right to post stories. The chats must be rechecked with canSendStory before actually trying to post a story there
        /// </summary>
        public static Task<Chats> GetChatsToSendStoriesAsync(
            this Client client)
        {
            return client.ExecuteAsync(new GetChatsToSendStories
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd