using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;

namespace part_1
{
	public class Helpers
	{
		private static readonly long[] cache = new long[256];

		/// <summary>
		/// We're not memoizing because this is meant to take some time.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		private static long Fib(int n, bool useCache = false)
		{
			if (n < 2) { return n; }

			if (useCache)
			{
				long cached = FromCache(n);

				if (cached != 0L) { return cached; }
			}

			long val = Fib(n - 1) + Fib(n - 2);

			if (useCache)
			{
				if (cache.Length > n)
				{
					cache[n] = val;
				}
			}

			return val;
		}

		private static long FromCache(int n)
		{
			long cached = 0L;
			
			if (cache.Length > n)
			{
				cached = cache[n];
			}

			return cached;
		}

		public static long[] FibonacciNumbers(int count)
		{
			var fibs = new long[count];

			for (int i = 0; i < count; i++)
			{
				fibs[i] = Fib(i);
			}

			return fibs;
		}

		internal static BigInteger Sum(long[] seq)
		{
			BigInteger sum = BigInteger.Zero;

			foreach (long item in seq)
			{
				sum += item;
			}

			return sum;
		}
	}

	/// <summary>
	/// Lets learn about tasks.
	/// </summary>
	public class Program
	{
		public static void Main(string[] args)
		{
			HelloWorld();
			// TaskCancellation();
			// ContinueWith();
			// FunkyClosures();

			Console.WriteLine("the end");
			Console.Read();
		}

		#region Step 1

		/// <summary>
		/// Simple introduction to the task api.
		/// </summary>
		private static void HelloWorld()
		{
			Task t = Task.Factory.StartNew(() =>
			{
				// just to make sure I don't look silly.
				Thread.Sleep(50);
				Console.WriteLine("Hello from another thread!");
			});

			Console.WriteLine("Hello from the main thread!");

			Console.ReadLine();
		}

		#endregion

		#region Step 2

		private static void TaskCancellation()
		{
			var source = new CancellationTokenSource();
			var token = source.Token;
			var tasks = new Task[5];

			Action<object> doStuff = (i) =>
			{
				int id = (int)i;
				int timeout = 50 + (50 * id);

				while (true)
				{
					if (token.IsCancellationRequested)
					{
						Console.Write("\r\n{0} cancelled!", id);
						token.ThrowIfCancellationRequested();
					}

					Console.Write(id);
					Thread.Sleep(timeout);
				}
			};

			for (int i = 0; i < 5; i++)
			{
				// reminder: funky closures
				tasks[i] = Task.Factory.StartNew(doStuff, i, token);
			}

			source.CancelAfter(2000);

			Console.Read();
		}

		#endregion

		#region Step 3

		private static void ContinueWith()
		{
			const int numberOfDesiredFibs = 50;

			// we can chain tasks together to do more interesting stuff.
			var task = Task.Factory.StartNew(() =>
			{
				return Helpers.FibonacciNumbers(numberOfDesiredFibs);
			})
			.ContinueWith(fibs =>
			{
				Console.WriteLine("Sum of first {0} fibonacci numbers = {1}", numberOfDesiredFibs, Helpers.Sum(fibs.Result));
			});

			// now we can do anything else! if only I were more creative...
			while (!task.IsCompleted)
			{
				Console.WriteLine("Still waiting...");
				Thread.Sleep(500);
			}

			Console.WriteLine("Finally!");
		}

		#endregion

		#region BONUS: Funky Closures

		private static void FunkyClosures()
		{
			for (int i = 0; i < 10; i++)
			{
				Task.Factory.StartNew(() =>
				{
					Console.WriteLine(i);
				});
			}

			Thread.Sleep(500);
		}

		#endregion
	}
}
