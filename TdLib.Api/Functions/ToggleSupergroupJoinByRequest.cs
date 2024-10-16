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
        /// Toggles whether all users directly joining the supergroup need to be approved by supergroup administrators; requires can_restrict_members administrator right
        /// </summary>
        public class ToggleSupergroupJoinByRequest : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "toggleSupergroupJoinByRequest";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Identifier of the supergroup that isn't a broadcast group
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("supergroup_id")]
            public long SupergroupId { get; set; }

            /// <summary>
            /// New value of join_by_request
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("join_by_request")]
            public bool JoinByRequest { get; set; }
        }

        /// <summary>
        /// Toggles whether all users directly joining the supergroup need to be approved by supergroup administrators; requires can_restrict_members administrator right
        /// </summary>
        public static Task<Ok> ToggleSupergroupJoinByRequestAsync(
            this Client client, long supergroupId = default, bool joinByRequest = default)
        {
            return client.ExecuteAsync(new ToggleSupergroupJoinByRequest
            {
                SupergroupId = supergroupId, JoinByRequest = joinByRequest
            });
        }
    }
}
// REUSE-IgnoreEnd