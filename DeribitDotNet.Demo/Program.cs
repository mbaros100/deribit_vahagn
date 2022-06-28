using System;
using System.Linq;
using System.Threading.Tasks;
using DeribitDotNet.Requests;
using DeribitDotNet.Responses;
using Serilog;
using Serilog.Events;

using System.Collections.Generic;

namespace DeribitDotNet.Demo
{
	public class Program
	{
		static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration().MinimumLevel.Is(LogEventLevel.Debug).WriteTo.Console().CreateLogger();

			Run(args[0], args[1]).Wait();
		}

		private static async Task Run(string key, string secret)
		{
			// Create a checking test api and initialize
			var deribitApi = new DeribitApi(key, secret, false);
			await deribitApi.Initialise();

			// Send test request and show parsed response
			Console.WriteLine("\nTest response: " + await deribitApi.Send(new TestRequest()));
			Console.WriteLine();

			// Send time request and show parsed response
			Console.WriteLine("Time response: " + await deribitApi.Send(new TimeRequest()));
			Console.WriteLine();

			// Get all active instruments settled in BTC and ETH
			var instrumentsResponseBTC = await deribitApi.Send(new InstrumentsRequest("BTC", InstrumentType.Future, false));
			var instrumentsResponseETH = await deribitApi.Send(new InstrumentsRequest("ETH", InstrumentType.Future, false));

			Console.WriteLine(@$"Active BTC futures instruments: {string.Join(", ",
			  instrumentsResponseBTC.Instruments.Select(i => i.InstrumentName))}");
			Console.WriteLine(@$"Active ETH futures instruments: {string.Join(", ",
			  instrumentsResponseETH.Instruments.Select(i => i.InstrumentName))}");

			// Join BTC and ETH instruments
			var totalInstrumentCount = instrumentsResponseBTC.Instruments.Length + instrumentsResponseETH.Instruments.Length;
			var allInstruments = new List<InstrumentsResponse> { instrumentsResponseBTC, instrumentsResponseETH };

			Console.WriteLine(@$"Total active instrument count: {totalInstrumentCount}");
			Console.WriteLine("\nStreaming top 20 levels for the available instruments");

			// TODO: Can be parallelised, or combined into a single socket. Needs research to understand which one has better performance.
			var apis = new Dictionary<string, DeribitApi>();
			// Shared memory for each of the api stored inside the OrderBook
			var sharedMemory = new Dictionary<string, OrderBook>();
			foreach (var instrumentype in allInstruments)
			{
				foreach (var instrument in instrumentype.Instruments)
				{
					// Create and initialize an api(ws) for the current instrument
					apis.Add(instrument.InstrumentName, new DeribitApi(key, secret, false));
					await apis[instrument.InstrumentName].Initialise();

					// Create an BorderBook with shared memory for the current instrument
					sharedMemory.Add(instrument.InstrumentName, new OrderBook(instrument.InstrumentName, 20));
					// Attach the current OrderBook to the current api(ws)
					apis[instrument.InstrumentName].OrderBooks.Subscribe(sharedMemory[instrument.InstrumentName].Update);
					// Subscribe to get quotes output on the console
					sharedMemory[instrument.InstrumentName].Quotes.Subscribe(Console.WriteLine);
				}
			}

			foreach (var api in apis)
			{
				// Subscribe for the current instrument to stream the order book
				var apiRes = await api.Value.Send(SubscribeRequest.OrderBook(true, api.Key));
			}

			Console.ReadLine();
		}
	}
}
