using System.Windows.Controls;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	public class BrushEditorControl : PropertyEditorControl
	{
		public BrushEditorControl()
		{
			DefaultStyleKey = typeof (BrushEditorControl);
		}

		BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;

		TabControl tabs;
		TabItem noBrushTab;
		TabItem solidColorTab;

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			tabs = GetTemplateChild ("brushTabs") as TabControl;
			noBrushTab = GetTemplateChild ("noBrushTab") as TabItem;
			solidColorTab = GetTemplateChild ("solidColorTab") as TabItem;

			if (ViewModel.Value == null) {
				tabs.SelectedItem = noBrushTab;
			}
			else if (ViewModel.Value is CommonSolidBrush solidBrush) {
				tabs.SelectedItem = solidColorTab;
			}

			tabs.SelectionChanged += (s, e) => {
				if (ViewModel == null) return;
				if (tabs.Items[tabs.SelectedIndex] is TabItem tab) {
					StorePreviousBrush ();
					if (tab == noBrushTab) {
						ViewModel.Value = null;
					}
					else if (tab == solidColorTab) {
						ViewModel.Value = ViewModel.PreviousSolidBrush ?? new CommonSolidBrush (new CommonColor (0, 0, 0));
					}
				}
			};
		}

		void StorePreviousBrush()
		{
			if (ViewModel == null) return;
			if (ViewModel.Value is CommonSolidBrush solidBrush) {
				ViewModel.PreviousSolidBrush = solidBrush;
			}
		}
	}
}
