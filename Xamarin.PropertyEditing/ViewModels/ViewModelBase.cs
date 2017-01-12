using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal abstract class ViewModelBase
		: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}