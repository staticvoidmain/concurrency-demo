using System.ComponentModel;

namespace demo.part_3
{
	public abstract class ViewModel : INotifyPropertyChanged
	{
		protected virtual void OnPropertyChanged(string property)
		{
			var handler = PropertyChanged;

			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}