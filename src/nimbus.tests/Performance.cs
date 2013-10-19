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
2013-10-19 02:31 | 10000000 iterations - Run full code
--------------------------------------
Baseline: 0.024223s | 412,830.78/ms
Manual: 0.3331874s | 30,013.14/ms
nimbus: 3.3727589s | 2,964.93/ms

--------------------------------------
2013-10-19 02:48 | 10000000 iterations - Comment out Send
--------------------------------------
Baseline: 0.0233534s | 428,203.17/ms
Manual: 0.3313247s | 30,181.87/ms
nimbus: 0.1523535s | 65,636.82/ms

--------------------------------------
2013-10-19 02:48 | 10000000 iterations - Comment out all code in Execute
--------------------------------------
Baseline: 0.0233888s | 427,555.07/ms
Manual: 0.3260729s | 30,667.99/ms
nimbus: 0.2040964s | 48,996.45/ms

--------------------------------------
2013-10-19 02:49 | 10000000 iterations - Just call containsKey in Execute
--------------------------------------
Baseline: 0.0239671s | 417,238.63/ms
Manual: 0.3282298s | 30,466.46/ms
nimbus: 0.5099901s | 19,608.22/ms
  
--------------------------------------
2013-10-19 02:50 | 10000000 iterations - Just derefence subscription from dictionary
--------------------------------------
Baseline: 0.0236529s | 422,781.14/ms
Manual: 0.3297096s | 30,329.72/ms
nimbus: 0.5202282s | 19,222.33/ms 
 
--------------------------------------
2013-10-19 02:51 | 10000000 iterations - ContainsKey _and_ dereference
--------------------------------------
Baseline: 0.0236916s | 422,090.53/ms
Manual: 0.3290083s | 30,394.37/ms
nimbus: 0.8243446s | 12,130.85/ms

--------------------------------------
2013-10-19 02:52 | 10000000 iterations - Just derefernce and create handlers
--------------------------------------
Baseline: 0.0236606s | 422,643.55/ms
Manual: 0.3293105s | 30,366.48/ms
nimbus: 0.7905208s | 12,649.89/ms 

--------------------------------------
2013-10-19 02:53 | 10000000 iterations - Create handlers and result
--------------------------------------
Baseline: 0.023544s | 424,736.66/ms
Manual: 0.3302363s | 30,281.35/ms
nimbus: 0.9960391s | 10,039.77/ms
 
--------------------------------------
2013-10-19 02:54 | 10000000 iterations - Create handlers, result, empty loop
--------------------------------------
Baseline: 0.023635s | 423,101.33/ms
Manual: 0.3305185s | 30,255.49/ms
nimbus: 1.0838265s | 9,226.57/ms
 
--------------------------------------
2013-10-19 02:55 | 10000000 iterations - handle in the loop
--------------------------------------
Baseline: 0.0234184s | 427,014.66/ms
Manual: 0.3288154s | 30,412.20/ms
nimbus: 2.4817943s | 4,029.34/ms
 
--------------------------------------
2013-10-19 02:55 | 10000000 iterations - Call Dispatch in loop (still not doing containsKey)
--------------------------------------
Baseline: 0.0233748s | 427,811.15/ms
Manual: 0.3302466s | 30,280.40/ms
nimbus: 2.8046201s | 3,565.55/ms

--------------------------------------
2013-10-19 03:02 | 10000000 iterations - Full code
--------------------------------------
Baseline: 0.0233974s | 427,397.92/ms
Manual: 0.3269618s | 30,584.61/ms
nimbus: 3.6025146s | 2,775.84/ms
 
--------------------------------------
2013-10-19 03:01 | 10000000 iterations - TryGetValue instead of containsKey and dereference
--------------------------------------
Baseline: 0.0234527s | 426,390.14/ms
Manual: 0.3292778s | 30,369.49/ms
nimbus: 3.2379474s | 3,088.38/ms

--------------------------------------
2013-10-19 03:03 | 10000000 iterations - Just TryGetValue
--------------------------------------
Baseline: 0.0236949s | 422,031.75/ms
Manual: 0.3307607s | 30,233.34/ms
nimbus: 0.5440601s | 18,380.32/ms - Beats containsKey and dereference @12,130.85/ms

--------------------------------------
2013-10-19 03:14 | 10000000 iterations - Use string for dictionary key instead of type - a bit slower
--------------------------------------
Baseline: 0.0236549s | 422,745.39/ms
Manual: 0.3332956s | 30,003.40/ms
nimbus: 3.4032898s | 2,938.33/ms
 
--------------------------------------
2013-10-19 03:16 | 10000000 iterations - use int for dictionary key - a bit faster
--------------------------------------
Baseline: 0.0234211s | 426,965.43/ms
Manual: 0.328381s | 30,452.43/ms
nimbus: 3.0904552s | 3,235.77/ms
 
*/