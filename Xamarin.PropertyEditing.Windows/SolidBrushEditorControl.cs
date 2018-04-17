using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

			this.eyedropperButton = GetTemplateChild ("eyedropper") as ToggleButton;
			if (this.eyedropperButton != null)
				this.eyedropperButton.Checked += OnEyedropper;

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

		private ToggleButton eyedropperButton;
		private Eyedropper eyedropper;
		private ComboBox colorSpacePicker;

		private BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;

		private void OnEyedropper (object sender, RoutedEventArgs e)
		{
			var eye = new Eyedropper ();
			eye.ColorChanged += (o, args) => ViewModel.Solid.Color = args.Color;
			eye.ColorComitted += (o, args) => {
				if (args.Exception == null) {
					ViewModel.Solid.Color = args.Color;
					ViewModel.Solid.CommitLastColor ();
				}

				((Eyedropper)o).Dispose ();
				this.eyedropperButton.IsChecked = false;
			};

			this.eyedropper = eye;
		}
	}
}
