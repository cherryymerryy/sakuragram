using System;
using Newtonsoft.Json;

// REUSE-IgnoreStart
namespace TdLib
{
    /// <summary>
    /// Autogenerated TDLib APIs
    /// </summary>
    public static partial class TdApi
    {
        public partial class ReactionUnavailabilityReason : Object
        {
            /// <summary>
            /// Describes why the current user can't add reactions to the message, despite some other users can
            /// </summary>
            public class ReactionUnavailabilityReasonAnonymousAdministrator : ReactionUnavailabilityReason
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "reactionUnavailabilityReasonAnonymousAdministrator";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }


            }
        }
    }
}
// REUSE-IgnoreEnd