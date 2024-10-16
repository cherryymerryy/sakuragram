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
        /// Changes the default message auto-delete time for new chats
        /// </summary>
        public class SetDefaultMessageAutoDeleteTime : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "setDefaultMessageAutoDeleteTime";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// New default message auto-delete time; must be from 0 up to 365 * 86400 and be divisible by 86400. If 0, then messages aren't deleted automatically
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("message_auto_delete_time")]
            public MessageAutoDeleteTime MessageAutoDeleteTime { get; set; }
        }

        /// <summary>
        /// Changes the default message auto-delete time for new chats
        /// </summary>
        public static Task<Ok> SetDefaultMessageAutoDeleteTimeAsync(
            this Client client, MessageAutoDeleteTime messageAutoDeleteTime = default)
        {
            return client.ExecuteAsync(new SetDefaultMessageAutoDeleteTime
            {
                MessageAutoDeleteTime = messageAutoDeleteTime
            });
        }
    }
}
// REUSE-IgnoreEnd