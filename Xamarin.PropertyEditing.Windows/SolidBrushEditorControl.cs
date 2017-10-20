using System.Windows;
using System.Windows.Controls;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class SolidBrushEditorControl
		: PropertyEditorControl
	{
		public SolidBrushEditorControl ()
		{
			DefaultStyleKey = typeof (SolidBrushEditorControl);
		}

		ComboBox colorSpacePicker;

		SolidBrushPropertyViewModel ViewModel => DataContext as SolidBrushPropertyViewModel;

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			colorSpacePicker = (ComboBox)GetTemplateChild ("colorSpacePicker");
			if (ViewModel.ColorSpaces == null || ViewModel.ColorSpaces.Count == 0) {
				colorSpacePicker.Visibility = Visibility.Collapsed;
			}

			if (ViewModel.Property.CanWrite) {
				// Handle color space changes
				colorSpacePicker.SelectionChanged += (s, e) => {
					if (ViewModel != null && ViewModel.Value != null) {
						ViewModel.Value = new CommonSolidBrush (ViewModel.Value.Color, (string)e.AddedItems[0]);
					}
				};
			}

			AddHandler (CurrentColorCommitterControlBase.CommitCurrentColorEvent, new RoutedEventHandler((s, e) => {
				ViewModel.CommitLastColor ();
			}));
		}
	}
}
