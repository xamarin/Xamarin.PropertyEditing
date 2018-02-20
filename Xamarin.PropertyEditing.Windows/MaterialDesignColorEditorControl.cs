using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class MaterialDesignColorEditorControl
		: PropertyEditorControl
	{
		static MaterialDesignColorEditorControl ()
		{
			FocusableProperty.OverrideMetadata (typeof (MaterialDesignColorEditorControl), new FrameworkPropertyMetadata (false));
			DefaultStyleKeyProperty.OverrideMetadata (typeof (MaterialDesignColorEditorControl), new FrameworkPropertyMetadata (typeof (MaterialDesignColorEditorControl)));
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.normalColorPicker = GetTemplateChild ("normalColorPicker") as ChoiceControl;
			this.accentColorPicker = GetTemplateChild ("accentColorPicker") as ChoiceControl;

			if (this.normalColorPicker == null)
				throw new InvalidOperationException ($"{nameof (MaterialDesignColorEditorControl)} is missing a child ChoiceControl named \"normalColorPicker\"");
			if (this.accentColorPicker == null)
				throw new InvalidOperationException ($"{nameof (MaterialDesignColorEditorControl)} is missing a child ChoiceControl named \"accentColorPicker\"");

			if (ViewModel == null)
				throw new InvalidOperationException ($"{nameof (MaterialDesignColorEditorControl)} is missing a data context with a non-null MaterialDesign part.");

			this.normalColorPicker.SelectedItemChanged += (s, e) => {
				ViewModel.NormalColor = this.normalColorPicker.SelectedItem as CommonColor?;
				EnsureNormalAndAccentState ();
			};
			this.accentColorPicker.SelectedItemChanged += (s, e) => {
				ViewModel.AccentColor = this.accentColorPicker.SelectedItem as CommonColor?;
				EnsureNormalAndAccentState ();
			};

			this.normalColorPicker.LayoutUpdated += (s, e) => {
				EnsureCheckBoxState (this.normalColorPicker, ViewModel?.NormalColor);
			};

			this.accentColorPicker.LayoutUpdated += (s, e) => {
				EnsureCheckBoxState (this.accentColorPicker, ViewModel?.AccentColor);
			};
		}

		protected override void OnRender (DrawingContext drawingContext)
		{
			base.OnRender (drawingContext);

			EnsureNormalAndAccentState ();
		}

		private ChoiceControl normalColorPicker;
		private ChoiceControl accentColorPicker;

		private void EnsureCheckBoxState(ChoiceControl control, CommonColor? value)
		{
			for (int i = 0; i < control.Items.Count; i++) {
				var container = control.ItemContainerGenerator.ContainerFromIndex (i) as ContentPresenter;
				if (container == null)
					throw new InvalidOperationException ("Unexpected visual tree");

				var child = VisualTreeHelper.GetChild (container, 0);
				var toggle = child as ToggleButton;
				if (toggle == null)
					throw new InvalidOperationException ("Children must be of ToggleButton");

				if (value.HasValue && value.Value.Equals ((CommonColor)container.DataContext))
					toggle.IsChecked = true;
			}
		}

		private void EnsureNormalAndAccentState ()
		{
			if (ViewModel == null) return;
			EnsureCheckBoxState (this.normalColorPicker, ViewModel.NormalColor);
			EnsureCheckBoxState (this.accentColorPicker, ViewModel.AccentColor);
		}

		private MaterialDesignColorViewModel ViewModel => (DataContext as BrushPropertyViewModel)?.MaterialDesign;
	}
}