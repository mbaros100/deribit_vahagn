using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DeribitDotNet.Notifications;
using DeribitDotNet.Responses;

using System.IO.MemoryMappedFiles;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Serilog;

namespace DeribitDotNet.Demo
{
	public class OrderBook
	{
		private readonly SortedDictionary<double, Level> _bids = new SortedDictionary<double, Level>(new ReverseComparer<double>());
		private readonly SortedDictionary<double, Level> _asks = new SortedDictionary<double, Level>();

		private readonly MemoryMappedFile _privateBytesBids;
		private readonly MemoryMappedFile _privateBytesAsks;

		private readonly Mutex _mutex;

		private readonly Subject<Quote> _subject = new Subject<Quote>();

		private readonly int _levels;

		private const int bidaskSize = 8;

		public IObservable<Quote> Quotes => _subject.DistinctUntilChanged().AsObservable();

		public OrderBook(string instrumentName, int levels)
		{
			_levels = levels;

			_privateBytesBids = MemoryMappedFile.CreateNew(instrumentName + "_bids", levels * bidaskSize);
			_privateBytesAsks = MemoryMappedFile.CreateNew(instrumentName + "_asks", levels * bidaskSize);
			_mutex = new Mutex(false, instrumentName + "_mutex");
		}

		public void Update(OrderBookNotification notification)
		{
			if (notification.PreviousChangeId == 0)
			{
				_bids.Clear();
				_asks.Clear();
			}

			var msStart = DateTime.Now.Ticks;

			ProcessItems(notification.Bids, Direction.Buy);
			ProcessItems(notification.Asks, Direction.Sell);

			Console.WriteLine("Time to write into shared memory: " + (DateTime.Now.Ticks - msStart) + " ticks");

			_subject.OnNext(GetQuote(this, notification.ArrivalTime));
		}

		private void ProcessItems(LevelEvent[] levelEvents, Direction direction)
		{
			var levels = direction == Direction.Buy ? _bids : _asks;
			foreach (var levelEvent in levelEvents)
			{
				switch (levelEvent.Type)
				{
					case EventType.New:
					case EventType.Change:
						levels[levelEvent.Price] = new Level((float)levelEvent.Price, levelEvent.Amount);
						break;

					case EventType.Delete:
						levels.Remove(levelEvent.Price);
						break;
				}
			}
			var tmpBuffer = new Level[_levels];
			int index = 0;
			foreach (var level in levels)
			{
				if (index == _levels)
				{
					break;
				}
				tmpBuffer[index++] = level.Value;
			}

			_mutex.WaitOne();
			var _privateBytes = direction == Direction.Buy ? _privateBytesBids : _privateBytesAsks;
			using (MemoryMappedViewStream stream = _privateBytes.CreateViewStream())
			{
				BinaryWriter writer = new BinaryWriter(stream);
				BinaryFormatter bf = new BinaryFormatter();
				using (MemoryStream ms = new MemoryStream())
				{
					bf.Serialize(ms, tmpBuffer);
					writer.Write(ms.ToArray(), 0, _levels * bidaskSize);
				}
			}
			_mutex.ReleaseMutex();
		}

		private Quote GetQuote(OrderBook ob, DateTime time) => new Quote(time, GetList(ob._bids.Values), GetList(ob._asks.Values));

		private IList<Level> GetList(IEnumerable<Level> levels) => levels.Take(_levels).ToList();

		private sealed class ReverseComparer<T> : IComparer<T>
		{
			int IComparer<T>.Compare(T x, T y) => ((IComparer<T>)Comparer<T>.Default).Compare(y, x);
		}
	}
}
