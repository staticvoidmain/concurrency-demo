using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace demo.part_3
{
	public class GoogleKillerViewModel : ViewModel
	{
		private static readonly Engine _engine = Engine.Instance;

		public GoogleKillerViewModel()
		{
			Settings = new SettingsViewModel();
			Stats = new StatsViewModel();
		}

		public SettingsViewModel Settings { get; set; }
		public StatsViewModel Stats { get; set; }

		// I've forgotten a lot of this stuff.
		public ICommand Start { get; }
		public ICommand Stop { get; }
	}
}
