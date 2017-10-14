using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class ColorViewModel : NotifyingObject
	{
		CommonColor color;
		public CommonColor Value {
			get => color;
			set {
				if (!color.Equals(value)) {
					color = value;
					OnPropertyChanged ();
				}
			}
		}
	}
}
