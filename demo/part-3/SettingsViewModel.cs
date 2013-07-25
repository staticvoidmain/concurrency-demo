namespace demo.part_3
{
	public class SettingsViewModel : ViewModel
	{
		private Engine _engine;
		public SettingsViewModel(Engine engine)
		{
			this.seed = "http://www.roadloans.com";
			this._engine = engine;
			this._engine.Seed = this.seed;
		}

		private string seed;
		public string Seed
		{
			get { return this.seed; }
			set
			{
				this.seed = value;
				this._engine.Seed = value;
				OnPropertyChanged("Seed");
			}
		}

		private int threads = 1;
		public int Threads
		{
			get { return this.threads; }
			set
			{
				this.threads = value;
				_engine.Threads = value;
				OnPropertyChanged("Threads");
			}
		}

		private bool useAsync;
		public bool UseAsync
		{
			get { return this.useAsync; }
			set
			{
				this.useAsync = value;
				this._engine.UseAsync = value;
				OnPropertyChanged("UseAsync");
			}
		}
	}
}