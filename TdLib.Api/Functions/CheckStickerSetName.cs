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
        /// Checks whether a name can be used for a new sticker set
        /// </summary>
        public class CheckStickerSetName : Function<CheckStickerSetNameResult>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "checkStickerSetName";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Name to be checked
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Checks whether a name can be used for a new sticker set
        /// </summary>
        public static Task<CheckStickerSetNameResult> CheckStickerSetNameAsync(
            this Client client, string name = default)
        {
            return client.ExecuteAsync(new CheckStickerSetName
            {
                Name = name
            });
        }
    }
}
// REUSE-IgnoreEnd