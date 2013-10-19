using System;
using System.Diagnostics;
using NUnit.Framework;

namespace nimbus.tests
{
	[TestFixture]
	public class Performance
	{
		private const int Iterations = 10*1000*1000;

		[Test]
		public void Examine()
		{
			Console.WriteLine("--------------------------------------");
			Console.WriteLine("{0} | {1} iterations", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"), Iterations);
			Console.WriteLine("--------------------------------------");
			var sw = Stopwatch.StartNew();
			for (var i = 0; i < Iterations; i++)
			{
				
			}
			sw.Stop();
			Console.WriteLine("Baseline: {0}s | {1:n}/ms", sw.Elapsed.TotalSeconds, Math.Round(Iterations/sw.Elapsed.TotalMilliseconds, 2));

			sw = Stopwatch.StartNew();
			for (var i = 0; i < Iterations; i++)
			{
				new Counter().Handle(new DoIteration());
			}
			sw.Stop();
			Console.WriteLine("Manual: {0}s | {1:n}/ms", sw.Elapsed.TotalSeconds, Math.Round(Iterations / sw.Elapsed.TotalMilliseconds, 2));

			var mediator = new Mediator();
			mediator.Subscribe<DoIteration>(() => new[]{new Counter() });

			sw = Stopwatch.StartNew();
			for (var i = 0; i < Iterations; i++)
			{
				mediator.Send(new DoIteration());
			}
			sw.Stop();
			Console.WriteLine("nimbus: {0}s | {1:n}/ms", sw.Elapsed.TotalSeconds, Math.Round(Iterations / sw.Elapsed.TotalMilliseconds, 2));
		}

		public class DoIteration { }

		public class Counter : IHandle<DoIteration>
		{
			public int Count { get; set; }

			public void Handle(DoIteration message)
			{
				Count++;
			}
		}
	}
}

/*
--------------------------------------
2013-10-19 02:31 | 10000000 iterations
--------------------------------------
Baseline: 0.024223s | 412,830.78/ms
Manual: 0.3331874s | 30,013.14/ms
nimbus: 3.3727589s | 2,964.93/ms



*/