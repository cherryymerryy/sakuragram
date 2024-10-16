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
        /// <summary>
        /// A detailed statistics about Telegram Stars earned by a bot or a chat
        /// </summary>
        public partial class StarRevenueStatistics : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "starRevenueStatistics";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// A graph containing amount of revenue in a given day
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("revenue_by_day_graph")]
            public StatisticalGraph RevenueByDayGraph { get; set; }

            /// <summary>
            /// Telegram Star revenue status
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("status")]
            public StarRevenueStatus Status { get; set; }

            /// <summary>
            /// Current conversion rate of a Telegram Star to USD
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("usd_rate")]
            public double? UsdRate { get; set; }
        }
    }
}
// REUSE-IgnoreEnd