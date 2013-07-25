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
}
