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
        public partial class InputPassportElement : Object
        {
            /// <summary>
            /// A Telegram Passport element to be saved containing the user's identity card
            /// </summary>
            public class InputPassportElementIdentityCard : InputPassportElement
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "inputPassportElementIdentityCard";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// The identity card to be saved
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("identity_card")]
                public InputIdentityDocument IdentityCard { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd