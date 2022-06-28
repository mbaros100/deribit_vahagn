using System;
using DeribitDotNet.JsonConverters;
using DeribitDotNet.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DeribitDotNet.Responses
{
    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum OrderStatus
    {
        Open,
        Filled,
        Cancelled
    }

    public class Order
    {
        [JsonProperty("amount")]
        public double Quantity;

        [JsonProperty("api")]
        public bool IsApiOrder;

        [JsonProperty("average_price")]
        public double AvgPrice;

        public double Commission;

        [JsonProperty("creation_timestamp")]
        [JsonConverter(typeof(CountToDateTimeConverter), false)]
        public DateTime CreatedTime;

        [JsonProperty("filled_amount")]
        public double FilledQuantity;

        [JsonProperty("instrument_name")]
        public string Instrument;

        [JsonProperty("is_liquidation")]
        public bool IsLiquidation;

        public string Label;

        [JsonProperty("last_update_timestamp")]
        [JsonConverter(typeof(CountToDateTimeConverter), false)]
        public DateTime LastUpdateTime;

        [JsonProperty("max_show")]
        public int ShowSize;

        [JsonProperty("order_id")]
        public string OrderId;

        [JsonProperty("order_state")]
        public OrderStatus Status;

        [JsonProperty("post_only")]
        public bool PostOnly;

        public double Price;

        [JsonProperty("profit_loss")]
        public double Pnl;

        [JsonProperty("reduce_only")]
        public bool ReduceOnly;

        [JsonProperty("stop_price")]
        public double? StopPrice;

        [JsonProperty("triggered")]
        public bool HasTriggered;

        public override string ToString() =>
          "";
    }
}