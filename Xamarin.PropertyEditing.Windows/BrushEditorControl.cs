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

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.brushBoxButton = GetTemplateChild ("brushBoxButton") as ButtonBase;
			this.brushBoxPopup = GetTemplateChild ("brushBoxPopup") as Popup;
			this.brushTabs = this.brushBoxPopup?.Child?.GetDescendants<BrushTabbedEditorControl>().FirstOrDefault();

			if (IsEnabled && this.brushBoxButton != null && this.brushBoxPopup != null) {
				this.brushBoxPopup.PlacementTarget = this.brushBoxButton;
				this.brushBoxPopup.Opened += (s, e) => {
					this.brushTabs?.FocusFirstChild ();
				};
				this.brushBoxPopup.Closed += (s, e) => {
					this.brushBoxButton.Focus ();
				};
				this.brushBoxPopup.KeyUp += (s, e) => {
					if (e.Key == Key.Escape) {
						this.brushBoxPopup.IsOpen = false;
					}
				};
			}
		}

		private ButtonBase brushBoxButton;
		private Popup brushBoxPopup;
		private BrushTabbedEditorControl brushTabs;

		private BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;
	}
}
