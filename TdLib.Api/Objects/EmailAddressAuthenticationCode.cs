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
        public partial class EmailAddressAuthentication : Object
        {
            /// <summary>
            /// Contains authentication data for an email address
            /// </summary>
            public class EmailAddressAuthenticationCode : EmailAddressAuthentication
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "emailAddressAuthenticationCode";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// The code
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("code")]
                public string Code { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd