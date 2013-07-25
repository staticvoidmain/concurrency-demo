using System.Windows.Input;

namespace demo.part_3
{
	public class GoogleKillerViewModel : ViewModel
	{
		private static readonly Engine _engine = Engine.Instance;

		public GoogleKillerViewModel()
		{
			Settings = new SettingsViewModel(_engine);
			Stats = new StatsViewModel(_engine);
		}

		public SettingsViewModel Settings { get; set; }
		public StatsViewModel Stats { get; set; }

		// I've forgotten a lot of this stuff.
		public ICommand Start 
		{ 
			get 
			{
				return new StartEngineCommand(_engine);
			} 
		}

		public ICommand Stop 
		{ 
			get 
			{	
				return new StopEngineCommand(_engine);
			} 
		}
	}
}
