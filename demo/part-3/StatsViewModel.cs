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
		private int pagesIndexed;
		private int pagesPerSecond;
		private Engine _engine;
		private Timer _timer;
		
		public StatsViewModel(Engine _engine)
		{
			// TODO: Complete member initialization
			this._engine = _engine;

			this._engine.PageIndexed += _engine_PageIndexed;
			this._timer = new Timer(UpdateProcessedPerSecond, null, 1000, 1000);
		}

		void _engine_PageIndexed(object sender, EventArgs e)
		{
			int count = Interlocked.Increment(ref pagesIndexed);

			if (count % 2 == 0)
			{
				OnPropertyChanged("PagesIndexed");
			}
		}

		private void UpdateProcessedPerSecond(object state)
		{
			this.PagesPerSecond = (pagesIndexed - pagesPerSecond);
			this.pagesPerSecond = this.pagesIndexed;

			OnPropertyChanged("PagesPerSecond");
		}

		public int PagesIndexed 
		{
			get { return pagesIndexed; }
		}

		public int PagesPerSecond { get; set; }
	}
}
