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

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.colorSpacePicker = GetTemplateChild ("colorSpacePicker") as ComboBox;
			if (this.colorSpacePicker == null)
				throw new InvalidOperationException ($"{nameof (SolidBrushEditorControl)} is missing a child ComboBox named \"colorSpacePicker\"");

			// Handle color space changes
			this.colorSpacePicker.SelectionChanged += (s, e) => {
				if (ViewModel?.Value != null && ViewModel.Property.CanWrite) {
					ViewModel.Value = new CommonSolidBrush (ViewModel.Solid.Color, (string)e.AddedItems[0]);
				}
			};

			AddHandler (CurrentColorCommitterControlBase.CommitCurrentColorEvent,
				new RoutedEventHandler((s, e) => ViewModel?.Solid.CommitLastColor ()));

			AddHandler (CurrentColorCommitterControlBase.CommitHueEvent,
				new RoutedEventHandler ((s, e) => ViewModel?.Solid.CommitHue ()));
		}
		
		private ComboBox colorSpacePicker;

		private BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;
	}
}
