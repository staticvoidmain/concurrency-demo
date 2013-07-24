using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace demo.part_3
{
	// todo: raise property changed
	public class StatsViewModel : ViewModel
	{
		public int TotalProcessed 
		{ get; set; }
		public int ProcessedPerSecond { get; set; }
	}
}
