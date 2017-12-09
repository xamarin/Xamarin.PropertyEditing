using System.Linq;
using System.Windows;
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
				this.brushBoxPopup.PlacementTarget = this.brushBoxButton.FindParent<PropertyPresenter>();
				this.brushBoxPopup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(PlacePopup);
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

		public static CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset) {
			return new[] {
				new CustomPopupPlacement (new Point(offset.X, offset.Y + targetSize.Height), PopupPrimaryAxis.Horizontal),
				new CustomPopupPlacement (new Point(offset.X, offset.Y - popupSize.Height), PopupPrimaryAxis.Horizontal),
				new CustomPopupPlacement (new Point(offset.X - popupSize.Width, offset.Y + targetSize.Height), PopupPrimaryAxis.Horizontal),
			};
		}

		private ButtonBase brushBoxButton;
		private Popup brushBoxPopup;
		private BrushTabbedEditorControl brushTabs;

		private BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;
	}
}
