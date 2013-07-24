using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace demo.part_3
{
	public class SettingsViewModel : ViewModel
	{
		public int Threads { get; set; }
		public bool UseAsync { get; set; }
	}
}
