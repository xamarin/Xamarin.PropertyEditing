using System.Windows.Controls;
using System.Windows.Input;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	public class BrushTabbedEditorControl
		: Control
	{
		public BrushTabbedEditorControl()
		{
			DefaultStyleKey = typeof (BrushTabbedEditorControl);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.brushChoice = GetTemplateChild ("brushChoice") as ChoiceControl;

			if (this.brushChoice != null) {
				StorePreviousBrush ();
				SelectTabFromBrush ();

				this.brushChoice.SelectedItemChanged += (s, e) => {
					if (ViewModel == null) return;
					StorePreviousBrush ();
					switch ((string)((ChoiceItem)(this.brushChoice.SelectedItem)).Value) {
					case none:
						ViewModel.Value = null;
						break;
					case solid:
						ViewModel.Value = ViewModel.Solid.PreviousSolidBrush ?? new CommonSolidBrush (new CommonColor (0, 0, 0));
						break;
					}
				};

				this.brushChoice.KeyUp += (s, e) => {
					if (ViewModel == null) return;
					StorePreviousBrush ();
					switch (e.Key) {
					case Key.N:
						e.Handled = true;
						this.brushChoice.SelectedValue = none;
						break;
					case Key.S:
						e.Handled = true;
						this.brushChoice.SelectedValue = solid;
						break;
					// TODO: add G, T, R for the other brush types when they are available.
					}
				};
			}
		}

		public static readonly string None = none;
		public static readonly string Solid = solid;

		internal void FocusFirstChild()
		{
			this.brushChoice?.FocusSelectedItem();
		}

		private const string none = nameof (none);
		private const string solid = nameof (solid);

		private ChoiceControl brushChoice;

		private BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;

		private void StorePreviousBrush ()
		{
			if (ViewModel == null) return;
			if (ViewModel.Value is CommonSolidBrush solidBrush) {
				ViewModel.Solid.PreviousSolidBrush = solidBrush;
			}
		}

		private void SelectTabFromBrush ()
		{
			if (this.brushChoice == null) return;
			switch (ViewModel?.Value) {
			case null:
				this.brushChoice.SelectedValue = none;
				break;
			case CommonSolidBrush _:
				this.brushChoice.SelectedValue = solid;
				break;
			}
		}
	}
}
