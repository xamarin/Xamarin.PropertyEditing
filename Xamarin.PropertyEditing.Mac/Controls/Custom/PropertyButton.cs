using System;
using System.Collections.Generic;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyButton
		: NSButton
	{
		NSMenu popUpContextMenu;

		PropertyViewModel viewModel;
		internal PropertyViewModel ViewModel
		{
			get { return viewModel; }
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= OnPropertyChanged;
				}

				viewModel = value;
				viewModel.PropertyChanged += OnPropertyChanged;

				// No point showing myself if you can't do anything with me.
				Hidden = !viewModel.Property.CanWrite;

				ValueSourceChanged (viewModel.ValueSource);
			}
		}

		private static readonly Dictionary<ValueSource, Tuple<string, NSImage, NSImage>> stateImages = new Dictionary<ValueSource, Tuple<string, NSImage, NSImage>> {
			{ ValueSource.Default, new Tuple<string, NSImage, NSImage> (Properties.Resources.Default, NSImage.ImageNamed ("property-button-default-mac-10"), NSImage.ImageNamed ("property-button-default-mac-active-10") )},
			{ ValueSource.Binding, new Tuple<string, NSImage, NSImage> (Properties.Resources.Binding, NSImage.ImageNamed ("property-button-bound-mac-10"), NSImage.ImageNamed ("property-button-bound-mac-active-10")) }
		};

		public PropertyButton ()
		{
			using (Performance.StartNew ()) {
				Cell = new NSButtonCell {
					HighlightsBy = 1,
				};
				Bordered = false;
				Enabled = true;
				ImageScaling = NSImageScale.AxesIndependently;
				Title = string.Empty;
				ToolTip = Properties.Resources.Default;
				TranslatesAutoresizingMaskIntoConstraints = false;

				using (Performance.StartNew("ActivatedSub")) {
					Activated += OnActivated;
				}
				//Target = this;
				//Action = new ObjCRuntime.Selector ("Pressed");
			}
		}

		[Export ("Pressed")]
		private void Pushed()
		{
			OnActivated (this, EventArgs.Empty);
		}

		private void OnActivated (object sender, EventArgs e)
		{
			if (this.popUpContextMenu == null) {
				this.popUpContextMenu = new NSMenu ();

				if (this.viewModel.TargetPlatform.SupportsCustomExpressions) {
					var mi = new NSMenuItem (Properties.Resources.CustomExpressionEllipsis) {
						AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.CustomExpressionEllipsis,
						new CoreText.CTStringAttributes () {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize + 1),
						})
					};

					mi.Activated += OnCustomExpression;

					this.popUpContextMenu.AddItem (mi);
					this.popUpContextMenu.AddItem (NSMenuItem.SeparatorItem);
				}

				// TODO If we add more menu items consider making the Label/Command a dictionary that we can iterate over to populate everything.
				this.popUpContextMenu.AddItem (new CommandMenuItem (Properties.Resources.Reset, viewModel.ClearValueCommand) {
					AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.Reset,
						new CoreText.CTStringAttributes () {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize + 1),
						})
				});
			}

			var menuOrigin = this.Superview.ConvertPointToView (new CGPoint (Frame.Location.X - 1, Frame.Location.Y + Frame.Size.Height + 4), null);

			var popupMenuEvent = NSEvent.MouseEvent (NSEventType.LeftMouseDown, menuOrigin, (NSEventModifierMask)0x100, 0, this.Window.WindowNumber, this.Window.GraphicsContext, 0, 1, 1);

			NSMenu.PopUpContextMenu (popUpContextMenu, popupMenuEvent, this);
		}

		private void ValueSourceChanged (ValueSource valueSource)
		{
			if (stateImages.TryGetValue (valueSource, out Tuple<string, NSImage, NSImage> state)) {
				Image = state.Item2;
				AlternateImage = state.Item3;
				ToolTip = state.Item1;
				return;
			}

			switch (valueSource) {
				case ValueSource.Local:
					AlternateImage = NSImage.ImageNamed ("property-button-local-mac-active-10");
					Image = NSImage.ImageNamed ("property-button-local-mac-10");
					ToolTip = Properties.Resources.Local;
					break;

				case ValueSource.Inherited:
					AlternateImage = NSImage.ImageNamed ("property-button-inherited-mac-active-10");
					Image = NSImage.ImageNamed ("property-button-inherited-mac-10");
					ToolTip = Properties.Resources.Inherited;
					break;

				case ValueSource.Resource:
					AlternateImage = NSImage.ImageNamed ("property-button-inherited-mac-active-10");
					Image = NSImage.ImageNamed ("property-inherited-resource-mac-10");
					ToolTip = (this.viewModel?.Resource?.Name != null) ? String.Format (Properties.Resources.ResourceWithName, this.viewModel.Resource.Name) : Properties.Resources.Resource;
					break;

				case ValueSource.Unset:
					AlternateImage = NSImage.ImageNamed ("property-button-default-mac-active-10");
					Image = NSImage.ImageNamed ("property-button-default-mac-10");
					ToolTip = Properties.Resources.Unset;
					break;

				default:
					// To Handle ValueSource.DefaultStyle, ValueSource.Style etc.
					AlternateImage = null;
					Image = null;
					ToolTip = string.Empty;
					break;
			}
		}

		private void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (viewModel.ValueSource)) {
				ValueSourceChanged (viewModel.ValueSource);
			}
		}

		private void OnCustomExpression (object sender, EventArgs e)
		{
			var customExpressionView = new CustomExpressionView (viewModel);

			var customExpressionPopOver = new AutoClosePopOver {
				ContentViewController = new NSViewController (null, null) { View = customExpressionView },
			};

			customExpressionPopOver.Show (customExpressionView.Frame, (NSView)this, NSRectEdge.MinYEdge);
		}
	}
}
