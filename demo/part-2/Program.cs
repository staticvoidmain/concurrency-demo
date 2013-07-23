using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace part_2
{
	public class ConcurrencyProblems
	{
		static void Main(string[] args)
		{
			DataRaces();
			// Deadlocks();

			Console.WriteLine("the end");
			Console.Read();
		}

		#region Data Races

		private static int value = 0;

		// who moved my cheese?
		private static void DataRaces()
		{
			for (int i = 0; i < 5; i++)
			{
				Setup();

				Parallel.For(1, 10001, (_) =>
				{
					IncrementValue();
				});

				Console.WriteLine("The answer is... {0}", ConcurrencyProblems.value);
				Console.WriteLine(ConcurrencyProblems.value != 10000 ? "Wait what?" : "We got lucky this time!");

				Reset();
			}
		}

		private static void Setup()
		{
			Console.WriteLine("Press any key to continue...");
			Console.ReadLine();
			Console.WriteLine("Harmless looking parallel loop...");
		}

		private static void Reset()
		{
			ConcurrencyProblems.value = 0;
			Thread.Sleep(50);
		}

		private static void IncrementValue()
		{
			value += 1;
		}

		#endregion

		#region Deadlocks

		private static object lockOne = new object();
		private static object lockTwo = new object();

		private static void Deadlocks()
		{
			ManualResetEvent mre = new ManualResetEvent(false);

			var task1 = Task.Factory.StartNew(() =>
			{
				mre.WaitOne();

				lock (lockTwo)
				{
					Console.WriteLine("Task-1 entered lock#2");

					Thread.Sleep(10);

					lock (lockOne)
					{
						Console.WriteLine("Task-1 entered lock#1!");
					}
				}
			});

			var task2 = Task.Factory.StartNew(() =>
			{
				mre.WaitOne();

				lock (lockOne)
				{
					Console.WriteLine("Task-2 entered lock#1");

					Thread.Sleep(10);

					lock (lockTwo)
					{
						Console.WriteLine("Task-2 entered lock#2!");
					}
				}
			});

			mre.Set();

			Task.WaitAll(task1, task2);
		}

		#endregion
	}
}
