using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BaseEditorControl : NSView
	{
		IEnumerable errorList;
		const int DefaultPropertyButtonSize = 10;
		const int DefaultActioButtonSize = 16;

		public event EventHandler ActionButtonClicked;
		NSButton actionButton;
		public NSButton ActionButton
		{
			get { return actionButton; }
		}

		public event EventHandler PropertyButtonClicked;
		PropertyButton propertyButton;
		public PropertyButton PropertyButton
		{
			get { return propertyButton; }
		}

		public BaseEditorControl ()
		{
			propertyButton = new PropertyButton ();

			AddSubview (propertyButton);

			actionButton = new NSButton {
				Bordered = false,
				Enabled = false,
				ImageScaling = NSImageScale.AxesIndependently,
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

#if DESIGNER_DEBUG
			actionButton.Image = NSImage.ImageNamed ("action-warning-16");
#endif

			actionButton.Activated += (object sender, EventArgs e) => {
				if (errorList != null) {
					var Container = new ErrorMessageView (errorList);

					var errorMessagePopUp = new NSPopover {
						Behavior = NSPopoverBehavior.Semitransient,
						ContentViewController = new NSViewController (null, null) { View = Container },
					};

					errorMessagePopUp.Show (default (CGRect), actionButton, NSRectEdge.MinYEdge);
				}

				NotifyActioButtonClicked ();
			};

			AddSubview (actionButton);

			this.DoConstraints (new[] {
				propertyButton.ConstraintTo (this, (ab, c) => ab.Width == DefaultPropertyButtonSize),
				propertyButton.ConstraintTo (this, (ab, c) => ab.Height == DefaultPropertyButtonSize),
				propertyButton.ConstraintTo (this, (ab, c) => ab.Top == c.Top + 6), // TODO: Better centering based on the icon height
				propertyButton.ConstraintTo (this, (ab, c) => ab.Left == c.Right - 28),
				actionButton.ConstraintTo (this, (eb, c) => eb.Width == DefaultActioButtonSize),
				actionButton.ConstraintTo (this, (eb, c) => eb.Height == DefaultActioButtonSize),
				actionButton.ConstraintTo (propertyButton, (eb, ab) => eb.Left == ab.Left + 10),
				actionButton.ConstraintTo (this, (eb, c) => eb.Top == c.Top + 3), // TODO: Better centering based on the icon height
			});

			PropertyEditorPanel.ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				PropertyEditorPanel.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
			}
		}

		void ThemeManager_ThemeChanged (object sender, EventArgs e)
		{
			UpdateTheme ();
		}

		protected void UpdateTheme ()
		{
			this.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
		}

		protected void SetErrors (IEnumerable errors)
		{
			errorList = errors;

			actionButton.Enabled = errors != null;

			// Using NSImageName.Caution for now, we can change this later at the designers behest
			actionButton.Image = actionButton.Enabled ? NSImage.ImageNamed ("action-warning-16") : null;
		}

		void NotifyPropertyButtonClicked ()
		{
			PropertyButtonClicked?.Invoke (this, EventArgs.Empty);
		}

		void NotifyActioButtonClicked ()
		{
			ActionButtonClicked?.Invoke (this, EventArgs.Empty);
		}
	}
}
