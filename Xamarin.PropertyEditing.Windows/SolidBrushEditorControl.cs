using System;
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

			this.colorSpacePicker = GetTemplateChild ("colorSpacePicker") as ComboBox;

			if (this.colorSpacePicker == null)
				throw new InvalidOperationException ($"{nameof (SolidBrushEditorControl)} is missing a child ComboBox named \"colorSpacePicker\"");

			if (ViewModel.Solid.ColorSpaces == null || ViewModel.Solid.ColorSpaces.Count == 0) {
				this.colorSpacePicker.Visibility = Visibility.Collapsed;
			}

			if (ViewModel.Property.CanWrite) {
				// Handle color space changes
				this.colorSpacePicker.SelectionChanged += (s, e) => {
					if (ViewModel != null && ViewModel.Value != null) {
						ViewModel.Value = new CommonSolidBrush (ViewModel.Solid.Color, (string)e.AddedItems[0]);
					}
				};
			}

			AddHandler (CurrentColorCommitterControlBase.CommitCurrentColorEvent, new RoutedEventHandler((s, e) => {
				ViewModel.Solid.CommitLastColor ();
			}));
		}
	}
}
