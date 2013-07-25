using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace demo.part_3
{
	internal class StopEngineCommand : ICommand
	{
		private readonly Engine target;

		public StopEngineCommand(Engine target)
		{
			this.target = target;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			this.target.Stop();
		}
	}
}
