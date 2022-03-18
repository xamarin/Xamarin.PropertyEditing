using System;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	public class BrushEditorControl : PropertyEditorControl
	{
		public BrushEditorControl()
		{
			DefaultStyleKey = typeof (BrushEditorControl);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.brushBoxButton = GetTemplateChild ("brushBoxButton") as ButtonBase;
			this.brushBoxPopup = GetTemplateChild ("brushBoxPopup") as Popup;

			if (this.brushBoxButton == null)
				throw new InvalidOperationException ($"{nameof(BrushEditorControl)} is missing a child ButtonBase named \"brushBoxButton\"");
			if (this.brushBoxPopup == null)
				throw new InvalidOperationException ("BrushEditorControl is missing a child Popup named \"brushBoxPopup\"");

			this.brushTabs = this.brushBoxPopup.Child?.GetDescendants<BrushTabbedEditorControl>().FirstOrDefault();

			this.brushBoxPopup.Opened += (s, e) => {
				this.brushTabs?.FocusFirstChild ();
				if (DataContext is BrushPropertyViewModel viewModel) {
					viewModel.Solid?.ResetInitialColor ();
				}
			};
			this.brushBoxPopup.Closed += (s, e) => {
				this.brushBoxButton.Focus ();
			};
			this.brushBoxPopup.PreviewKeyDown += (s, e) => {
				if (e.Key == Key.Escape) {
					this.brushBoxPopup.IsOpen = false;
					e.Handled = true;
				}
			};
		}

		private ButtonBase brushBoxButton;
		private Popup brushBoxPopup;
		private BrushTabbedEditorControl brushTabs;
	}
}
