using System.Linq;
using System.Windows.Controls;
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

		BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;

		ButtonBase brushBoxButton;
		Popup brushBoxPopup;

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			brushBoxButton = GetTemplateChild ("brushBoxButton") as ButtonBase;
			brushBoxPopup = GetTemplateChild ("brushBoxPopup") as Popup;

			if (IsEnabled && brushBoxButton != null && brushBoxPopup != null) {
				brushBoxPopup.PlacementTarget = brushBoxButton;
				brushBoxPopup.Opened += (s, e) => {
					TabControl brushTabs = brushBoxPopup.Child?.GetDescendants<TabControl> ().FirstOrDefault ();
					(brushTabs?.SelectedItem as TabItem)?.Focus ();
				};
				brushBoxPopup.Closed += (s, e) => {
					brushBoxButton.Focus ();
				};
				brushBoxPopup.KeyUp += (s, e) => {
					if (e.Key == Key.Escape) {
						brushBoxPopup.IsOpen = false;
					}
				};
			}
		}
	}
}
