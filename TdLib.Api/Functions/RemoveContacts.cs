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
        /// Removes users from the contact list
        /// </summary>
        public class RemoveContacts : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "removeContacts";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Identifiers of users to be deleted
            /// </summary>
            [JsonProperty("user_ids", ItemConverterType = typeof(Converter))]
            public long[] UserIds { get; set; }
        }

        /// <summary>
        /// Removes users from the contact list
        /// </summary>
        public static Task<Ok> RemoveContactsAsync(
            this Client client, long[] userIds = default)
        {
            return client.ExecuteAsync(new RemoveContacts
            {
                UserIds = userIds
            });
        }
    }
}
// REUSE-IgnoreEnd