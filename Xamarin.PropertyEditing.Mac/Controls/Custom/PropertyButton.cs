using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyButton
		: UnfocusableButton
	{
		public const int DefaultSize = 20;

		NSMenu popUpContextMenu;

		PropertyViewModel viewModel;
		internal PropertyViewModel ViewModel
		{
			get { return viewModel; }
			set {
				if (this.viewModel != null) {
					this.viewModel.PropertyChanged -= OnPropertyChanged;
				}

				this.viewModel = value;
				if (this.viewModel != null) {
					this.viewModel.PropertyChanged += OnPropertyChanged;
					ValueSourceChanged (this.viewModel.ValueSource);
				}
			}
		}

		public PropertyButton (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			AccessibilityTitle = LocalizationResources.AccessibilityPropertiesButton;
			AccessibilityHelp = LocalizationResources.AccessibilityPropertiesButtonDescription;
			Enabled = true;
			Image = this.hostResources.GetNamedImage ("property-button-default-mac-10");
			ImageScaling = NSImageScale.None;
			ToolTip = Properties.Resources.Default;

			OnMouseEntered += (sender, e) => {
				ToggleFocusImage (true);
			};

			OnMouseExited += (sender, e) => {
				ToggleFocusImage ();
			};

			OnMouseLeftDown += (sender, e) => {
				PopUpContextMenu ();
			};
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();
			ToggleFocusImage ();
		}

		private readonly IHostResourceProvider hostResources;

		private void PopUpContextMenu ()
		{
			if (this.popUpContextMenu == null) {
				this.popUpContextMenu = new NSMenu ();

				if (this.viewModel.TargetPlatform.SupportsCustomExpressions) {
					var mi = new NSMenuItem (Properties.Resources.CustomExpressionEllipsis) {
						AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.CustomExpressionEllipsis,
						new CoreText.CTStringAttributes {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize + 1),
						})
					};

					mi.Activated += OnCustomExpression;

					this.popUpContextMenu.AddItem (mi);
					this.popUpContextMenu.AddItem (NSMenuItem.SeparatorItem);
				}

				if (this.viewModel.SupportsResources) {
					this.popUpContextMenu.AddItem (NSMenuItem.SeparatorItem);

					var mi2 = new NSMenuItem (Properties.Resources.ResourceEllipsis) {
						AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.ResourceEllipsis,
						new CoreText.CTStringAttributes {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize + 1),
						})
					};

					mi2.Activated += OnResourceRequested;
					this.popUpContextMenu.AddItem (mi2);
				}

				this.popUpContextMenu.AddItem (NSMenuItem.SeparatorItem);

				// TODO If we add more menu items consider making the Label/Command a dictionary that we can iterate over to populate everything.
				this.popUpContextMenu.AddItem (new CommandMenuItem (Properties.Resources.Reset, viewModel.ClearValueCommand) {
					AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.Reset,
						new CoreText.CTStringAttributes {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize + 1),
						})
				});
			}

			var menuOrigin = this.Superview.ConvertPointToView (new CGPoint (Frame.Location.X - 1, Frame.Location.Y + Frame.Size.Height + 4), null);

			var popupMenuEvent = NSEvent.MouseEvent (NSEventType.LeftMouseDown, menuOrigin, (NSEventModifierMask)0x100, 0, this.Window.WindowNumber, this.Window.GraphicsContext, 0, 1, 1);

			NSMenu.PopUpContextMenu (popUpContextMenu, popupMenuEvent, this);
		}

		private void ToggleFocusImage (bool focused = false)
		{
			if (viewModel != null) {
				
				switch (viewModel.ValueSource) {
				case ValueSource.Binding:
					Image = focused ? this.hostResources.GetNamedImage ("property-button-bound-mac-active-10") : this.hostResources.GetNamedImage ("property-button-bound-mac-10");
					break;

				case ValueSource.Default:
					Image = focused ? this.hostResources.GetNamedImage ("property-button-default-mac-active-10") : this.hostResources.GetNamedImage ("property-button-default-mac-10");
					break;

				case ValueSource.Local:
					Image = focused ? this.hostResources.GetNamedImage ("property-button-local-mac-active-10") : this.hostResources.GetNamedImage ("property-button-local-mac-10");
					break;

				case ValueSource.Inherited:
					Image = focused ? this.hostResources.GetNamedImage ("property-button-inherited-mac-active-10") : this.hostResources.GetNamedImage ("property-button-inherited-mac-10");
					break;

				case ValueSource.Resource:
					Image = focused ? this.hostResources.GetNamedImage ("property-button-inherited-mac-active-10") : this.hostResources.GetNamedImage ("property-button-inherited-mac-10");
					break;

				case ValueSource.Unset:
					Image = focused ? this.hostResources.GetNamedImage ("property-button-default-mac-active-10") : this.hostResources.GetNamedImage ("property-button-default-mac-10");
					break;

				default:
					// To Handle ValueSource.DefaultStyle, ValueSource.Style etc.
					Image = null;
					break;
				}
			}
		}

		private void ValueSourceChanged (ValueSource valueSource)
		{
			switch (valueSource) {
				case ValueSource.Binding:
					ToolTip = Properties.Resources.Binding;
					break;

				case ValueSource.Default:
					ToolTip = Properties.Resources.Default;
					return;

				case ValueSource.Local:
					ToolTip = Properties.Resources.Local;
					break;

				case ValueSource.Inherited:
					ToolTip = Properties.Resources.Inherited;
					break;

				case ValueSource.Resource:
					ToolTip = (this.viewModel?.Resource?.Name != null) ? String.Format (Properties.Resources.ResourceWithName, this.viewModel.Resource.Name) : Properties.Resources.Resource;
					break;

				case ValueSource.Unset:
					ToolTip = Properties.Resources.Unset;
					break;

				default:
					// To Handle ValueSource.DefaultStyle, ValueSource.Style etc.
					ToolTip = string.Empty;
					break;
			}

			ToggleFocusImage ();
		}

		private void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (viewModel.ValueSource)) {
				ValueSourceChanged (viewModel.ValueSource);
			}
		}

		private void OnCustomExpression (object sender, EventArgs e)
		{
			var customExpressionView = new CustomExpressionView (this.hostResources, viewModel) {
				Appearance = EffectiveAppearance
			};

			var customExpressionPopOver = new AutoClosePopOver {
				ContentViewController = new NSViewController (null, null) { View = customExpressionView },
			};
			customExpressionPopOver.SetAppearance (this.hostResources.GetVibrantAppearance (EffectiveAppearance));

			customExpressionPopOver.Show (customExpressionView.Frame, (NSView)this, NSRectEdge.MinYEdge);
		}

		private void OnResourceRequested (object sender, EventArgs e)
		{
			var requestResourceView = new RequestResourceView (this.hostResources, this.viewModel) {
				Appearance = EffectiveAppearance
			};

			var resourceSelectorPopOver = new AutoClosePopOver {
				ContentViewController = new NSViewController (null, null) { View = requestResourceView },
			};
			resourceSelectorPopOver.SetAppearance (this.hostResources.GetVibrantAppearance (EffectiveAppearance));

			requestResourceView.PopOver = resourceSelectorPopOver;

			resourceSelectorPopOver.Show (requestResourceView.Frame, (NSView)this, NSRectEdge.MinYEdge);
		}
	}
}
