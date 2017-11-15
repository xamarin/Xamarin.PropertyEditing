using System.Windows.Controls;
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

			brushChoice = GetTemplateChild ("brushChoice") as ChoiceControl;

			if (brushChoice != null) {
				StorePreviousBrush ();
				SelectTabFromBrush ();

				brushChoice.SelectedItemChanged += (s, e) => {
					if (ViewModel == null) return;
					StorePreviousBrush ();
					switch ((string)brushChoice.SelectedValue) {
					case none:
						ViewModel.Value = null;
						break;
					case solid:
						ViewModel.Value = ViewModel.Solid.PreviousSolidBrush ?? new CommonSolidBrush (new CommonColor (0, 0, 0));
						break;
					}
				};
			}
		}

		public static readonly string None = none;
		public static readonly string Solid = solid;

		internal void FocusFirstChild()
		{
			brushChoice?.FocusSelectedItem();
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
			if (brushChoice == null) return;
			switch (ViewModel?.Value) {
			case null:
				brushChoice.SelectedValue = none;
				break;
			case CommonSolidBrush _:
				brushChoice.SelectedValue = solid;
				break;
			}
		}
	}
}
