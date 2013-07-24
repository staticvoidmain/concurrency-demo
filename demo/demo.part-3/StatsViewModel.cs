using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace demo.part_3
{
	// todo: raise property changed
	public class StatsViewModel : ViewModel
	{
		private int totalProcessed;
		public int TotalProcessed 
		{
			get { return totalProcessed; }
		}

		public void IncrementTotalProcessed()
		{
			Interlocked.Increment(ref totalProcessed);
		}

		public int ProcessedPerSecond { get; set; }
	}
}
