using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name = "sizingButton", Type = typeof(ButtonBase))]
	[TemplatePart (Name = "windowRect", Type = typeof (FrameworkElement))]
	[TemplatePart (Name = "elementRect", Type = typeof (FrameworkElement))]
	internal class AutoResizingMaskEditorControl
		: PropertyEditorControl
	{
		public AutoResizingMaskEditorControl ()
		{
			DataContextChanged += OnDataContextChanged;
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.sizingButton = GetTemplateChild ("sizingButton") as ButtonBase;
			if (this.sizingButton == null)
				throw new InvalidOperationException ($"Template for {nameof(AutoResizingMaskEditorControl)} must have a sizingButton");

			this.window = GetTemplateChild ("windowRect") as FrameworkElement;
			if (this.window == null)
				throw new InvalidOperationException ($"Template for {nameof(AutoResizingMaskEditorControl)} must have a windowRect");

			this.elementVisual = GetTemplateChild ("elementRect") as FrameworkElement;
			if (this.elementVisual == null)
				throw new InvalidOperationException ($"Template for {nameof(AutoResizingMaskEditorControl)} must have a elementRect");

			this.window.SizeChanged += OnPreviewWindowSizeChanged;

			UpdateSizeName();
			UpdateVisual();
		}

		private AutoResizingPropertyViewModel vm;
		private ButtonBase sizingButton;
		private FrameworkElement elementVisual, window;

		private void OnDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.vm != null) {
				this.vm.PropertyChanged -= OnViewModelPropertyChanged;
			}

			this.vm = e.NewValue as AutoResizingPropertyViewModel;
			if (this.vm != null) {
				this.vm.PropertyChanged += OnViewModelPropertyChanged;
			}

			UpdateSizeName();
		}

		private void OnViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AutoResizingPropertyViewModel.WidthSizable) || e.PropertyName == nameof(AutoResizingPropertyViewModel.HeightSizable)) {
				UpdateSizeName ();
				UpdateVisual();
			} else if (e.PropertyName == nameof(AutoResizingPropertyViewModel.LeftMarginFixed)
			           || e.PropertyName == nameof(AutoResizingPropertyViewModel.RightMarginFixed)
			           || e.PropertyName == nameof(AutoResizingPropertyViewModel.TopMarginFixed)
			           || e.PropertyName == nameof(AutoResizingPropertyViewModel.BottomMarginFixed)) {
				UpdateVisual();
			}
		}

		private void OnPreviewWindowSizeChanged (object sender, SizeChangedEventArgs e)
		{
			UpdateVisual ();
		}

		private void UpdateVisual ()
		{
			if (this.vm == null)
				return;

			var elementRect = this.vm.GetPreviewElementRectangle (new CommonSize (this.window.ActualWidth, this.window.ActualHeight), this.vm.Value);
			Canvas.SetLeft (this.elementVisual, elementRect.X);
			Canvas.SetTop (this.elementVisual, elementRect.Y);
			this.elementVisual.Width = elementRect.Width;
			this.elementVisual.Height = elementRect.Height;
		}

		private void UpdateSizeName ()
		{
			if (this.sizingButton == null)
				return;

			string name = null;
			if (this.vm != null) {
				string current;
				if (!this.vm.HeightSizable && !this.vm.WidthSizable)
					current = Properties.Resources.AutoresizingFixedSized;
				else if (this.vm.HeightSizable && !this.vm.WidthSizable)
					current = Properties.Resources.AutoresizingHeightSizable;
				else if (!this.vm.HeightSizable && this.vm.WidthSizable)
					current = Properties.Resources.AutoresizingWidthSizable;
				else
					current = Properties.Resources.AutoresizingWidthHeightSizable;

				name = String.Format (Properties.Resources.AutoresizingSizingName, current);
			}

			AutomationProperties.SetName (this.sizingButton, name ?? String.Empty);
		}
	}
}
