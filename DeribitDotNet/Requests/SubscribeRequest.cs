using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeribitDotNet.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DeribitDotNet.Requests
{
    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum InstrumentKind
    {
        Any,
        Future,
        Option,
    }

    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum DeribitCurrency
    {
        Btc,
        Eth
    }

    public class SubscribeRequest : Request<SubscribeInstrumentsResponse>
    {
        public readonly string[] Channels;

        public static SubscribeRequest OrderBook(bool batched = false, params string[] symbols) =>
            new SubscribeRequest("book", batched, true, symbols);

        public override bool Equals(object obj) =>
            base.Equals(obj) && obj is SubscribeRequest other && Channels.SequenceEqual(other.Channels);

        public override int GetHashCode() =>
            base.GetHashCode() * 13 ^ ((IStructuralEquatable)Channels).GetHashCode(EqualityComparer<string>.Default);

        private SubscribeRequest(string eventType, bool batched, bool isPublic, params string[] symbols) : base("subscribe", isPublic) =>
            Channels = symbols.Select(s => $"{eventType}.{s}.{GetBatched(batched)}").ToArray();
        private static string GetBatched(bool batched) => batched ? "100ms" : "raw";
    }
}