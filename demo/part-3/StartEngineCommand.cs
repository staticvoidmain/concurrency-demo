using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace demo.part_3
{
	internal class StartEngineCommand : ICommand
	{
		private readonly Engine target;

		public StartEngineCommand(Engine target)
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
			target.Start();
		}
	}
}
