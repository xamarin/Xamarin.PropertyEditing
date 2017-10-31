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

		BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			if (ViewModel == null) return;

			colorSpacePicker = (ComboBox)GetTemplateChild ("colorSpacePicker");
			if (ViewModel.ColorSpaces == null || ViewModel.ColorSpaces.Count == 0) {
				colorSpacePicker.Visibility = Visibility.Collapsed;
			}

			if (ViewModel.Property.CanWrite) {
				// Handle color space changes
				colorSpacePicker.SelectionChanged += (s, e) => {
					if (ViewModel != null && ViewModel.Value != null) {
						ViewModel.Value = new CommonSolidBrush (ViewModel.Color, (string)e.AddedItems[0]);
					}
				};
			}

			AddHandler (CurrentColorCommitterControlBase.CommitCurrentColorEvent, new RoutedEventHandler((s, e) => {
				ViewModel.CommitLastColor ();
			}));

			AddHandler (CurrentColorCommitterControlBase.CommitShadeEvent, new RoutedEventHandler ((s, e) => {
				ViewModel.CommitShade ();
			}));
		}
	}
}
