using System;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyButton
		: UnfocusableButton
	{
		public const int DefaultSize = 6;

		NSMenu popUpContextMenu;

		PropertyViewModel viewModel;
		internal PropertyViewModel ViewModel
		{
			get { return this.viewModel; }
			set {
				if (this.viewModel != null) {
					this.viewModel.PropertyChanged -= OnPropertyChanged;

					if (this.viewModel.SupportsBindings)
						this.viewModel.CreateBindingRequested -= OnBindingRequested;

					if (this.viewModel.HasVariations) 
						this.viewModel.CreateVariantRequested -= OnCreateVariantRequested;
				}

				this.viewModel = value;

				if (this.viewModel != null) {
					this.viewModel.PropertyChanged += OnPropertyChanged;

					if (this.viewModel.SupportsBindings)
						this.viewModel.CreateBindingRequested += OnBindingRequested;

					if (this.viewModel.HasVariations)
						this.viewModel.CreateVariantRequested += OnCreateVariantRequested;

					ValueSourceChanged (this.viewModel.ValueSource);

					AccessibilityTitle = string.Format (Properties.Resources.AccessibilityPropertiesButton, ViewModel.Property.Name);
				}

				if (this.popUpContextMenu != null) {
					this.popUpContextMenu.RemoveAllItems ();
					this.popUpContextMenu = null;
				}
			}
		}

		public PropertyButton (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			AccessibilityHelp = Properties.Resources.AccessibilityPropertiesButtonDescription;
			Enabled = true;
			Image = this.hostResources.GetNamedImage ("pe-property-button-default-mac-10");
			ImageScaling = NSImageScale.None;
			ToolTip = Properties.Resources.Default;

			OnMouseEntered += (sender, e) => {
				UpdateImage (true);
			};

			OnMouseExited += (sender, e) => {
				UpdateImage ();
			};

			OnMouseLeftDown += (sender, e) => {
				PopUpContextMenu ();
			};

			AppearanceChanged ();
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}

		private void AppearanceChanged ()
		{
			UpdateImage ();
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
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
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
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
						})
					};

					mi2.Activated += OnResourceRequested;
					this.popUpContextMenu.AddItem (mi2);
				}

				if (this.viewModel.SupportsBindings) {
					this.popUpContextMenu.AddItem (NSMenuItem.SeparatorItem);

					this.popUpContextMenu.AddItem (new CommandMenuItem (Properties.Resources.CreateDataBindingMenuItem, this.viewModel.RequestCreateBindingCommand) {
						AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.CreateDataBindingMenuItem,
						new CoreText.CTStringAttributes {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
						})
					});
				}

				this.popUpContextMenu.AddItem (NSMenuItem.SeparatorItem);

				// TODO If we add more menu items consider making the Label/Command a dictionary that we can iterate over to populate everything.
				this.popUpContextMenu.AddItem (new CommandMenuItem (Properties.Resources.Reset, this.viewModel.ClearValueCommand) {
					AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.Reset,
						new CoreText.CTStringAttributes {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
						})
				});
			}

			FocusClickedRow ();

			var menuOrigin = this.Superview.ConvertPointToView (new CGPoint (Frame.Location.X - 1, Frame.Location.Y + Frame.Size.Height + 4), null);

			var popupMenuEvent = NSEvent.MouseEvent (NSEventType.LeftMouseDown, menuOrigin, (NSEventModifierMask)0x100, 0, this.Window.WindowNumber, this.Window.GraphicsContext, 0, 1, 1);

			NSMenu.PopUpContextMenu (popUpContextMenu, popupMenuEvent, this);
		}

		private void FocusClickedRow ()
		{
			if (Superview is EditorContainer ec) {
				MakeFocusableKeyViewFirstResponder (ec.EditorView.NativeView.Subviews);
			}
		}

		private void MakeFocusableKeyViewFirstResponder (NSView[] subViews)
		{
			foreach (NSView item in subViews) {
				if (item.CanBecomeKeyView
					&& item.Tag == 1) {
					Window?.MakeFirstResponder (item);
					break;
				} else {
					MakeFocusableKeyViewFirstResponder (item.Subviews);
				}
			}
		}

		private void UpdateImage (bool focused = false)
		{
			if (this.viewModel != null) {
				
				switch (this.viewModel.ValueSource) {
					case ValueSource.Binding:
						Image = focused ? this.hostResources.GetNamedImage ("pe-property-button-bound-mac-active-10") : this.hostResources.GetNamedImage ("pe-property-button-bound-mac-10");
						break;

					case ValueSource.Default:
						Image = focused ? this.hostResources.GetNamedImage ("pe-property-button-default-mac-active-10") : this.hostResources.GetNamedImage ("pe-property-button-default-mac-10");
						break;

					case ValueSource.Local:
						Image = focused ? this.hostResources.GetNamedImage ("pe-property-button-local-mac-active-10") : this.hostResources.GetNamedImage ("pe-property-button-local-mac-10");
						break;

					case ValueSource.Inherited:
						Image = focused ? this.hostResources.GetNamedImage ("pe-property-button-inherited-mac-active-10") : this.hostResources.GetNamedImage ("pe-property-button-inherited-mac-10");
						break;

					case ValueSource.Resource:
						Image = focused ? this.hostResources.GetNamedImage ("pe-property-button-inherited-mac-active-10") : this.hostResources.GetNamedImage ("pe-property-button-inherited-mac-10");
						break;

					case ValueSource.Unset:
						Image = focused ? this.hostResources.GetNamedImage ("pe-property-button-default-mac-active-10") : this.hostResources.GetNamedImage ("pe-property-button-default-mac-10");
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
					break;

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

			UpdateImage ();
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

			var customExpressionPopOver = new AutoClosePopOver (this.hostResources, EffectiveAppearance) {
				CloseOnEnter = false,
				ContentViewController = new NSViewController (null, null) { View = customExpressionView },
			};

			customExpressionView.PopOver = customExpressionPopOver;

			customExpressionPopOver.Show (customExpressionView.Frame, (NSView)this, NSRectEdge.MinYEdge);
		}

		private void OnResourceRequested (object sender, EventArgs e)
		{
			var requestResourceView = new RequestResourceView (this.hostResources, this.viewModel) {
				Appearance = EffectiveAppearance
			};

			var resourceSelectorPopOver = new AutoClosePopOver (this.hostResources, EffectiveAppearance) {
				ContentViewController = new NSViewController (null, null) { View = requestResourceView },
			};

			requestResourceView.PopOver = resourceSelectorPopOver;

			resourceSelectorPopOver.Show (requestResourceView.Frame, (NSView)this, NSRectEdge.MinYEdge);
		}

		private void OnBindingRequested (object sender, CreateBindingRequestedEventArgs e)
		{
			var bindingEditorWindow = new BindingEditorWindow (this.hostResources, this.viewModel) {
				Appearance = EffectiveAppearance,
			};

			var result = (NSModalResponse)(int)NSApplication.SharedApplication.RunModalForWindow (bindingEditorWindow);
			if (result == NSModalResponse.OK) {
				e.BindingObject = bindingEditorWindow.ViewModel.SelectedObjects.Single ();
			}
		}

		private void OnCreateVariantRequested (object sender, CreateVariantEventArgs e)
		{
			using (var createVariantWindow = new CreateVariantWindow (this.hostResources, this.viewModel) {
				Appearance = EffectiveAppearance,
				})
				{
					var result = (NSModalResponse)(int)NSApplication.SharedApplication.RunModalForWindow (createVariantWindow);
					if (result == NSModalResponse.OK) {
						e.Variation = Task.FromResult (createVariantWindow.ViewModel.Variation);
				}
			}
		}
	}
}
