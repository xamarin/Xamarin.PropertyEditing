using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using System.Windows.Input;

using Xamarin.PropertyEditing.Mac.Resources;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BaseEditorControl : NSView
	{
		private IEnumerable errorList;
		protected const int DefaultPropertyButtonSize = 20;
		protected const int DefaultActioButtonSize = 16;

		public event EventHandler ActionButtonClicked;
		private NSButton actionButton;
		public NSButton ActionButton
		{
			get { return this.actionButton; }
		}

		protected BaseEditorControl ()
		{
			this.actionButton = new NSButton {
				Bordered = false,
				Enabled = false,
				ImageScaling = NSImageScale.AxesIndependently,
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
				AccessibilityTitle = LocalizationResources.AccessibilityActionButton,
				AccessibilityHelp = LocalizationResources.AccessibilityActionButtonDescription,
			};

#if DESIGNER_DEBUG
			actionButton.Image = PropertyEditorPanel.ThemeManager.GetImageForTheme ("action-warning-16");
#endif

			this.actionButton.Activated += (object sender, EventArgs e) => {
				if (this.errorList != null) {
					var Container = new ErrorMessageView (this.errorList);

					var errorMessagePopUp = new NSPopover {
						Behavior = NSPopoverBehavior.Semitransient,
						ContentViewController = new NSViewController (null, null) { View = Container },
					};

					errorMessagePopUp.Show (default (CGRect), this.actionButton, NSRectEdge.MinYEdge);
				}

				NotifyActionButtonClicked ();
			};

			AddSubview (this.actionButton);

			this.DoConstraints (new[] {
				 this.actionButton.ConstraintTo (this, (eb, c) => eb.Width == DefaultActioButtonSize),
				 this.actionButton.ConstraintTo (this, (eb, c) => eb.Height == DefaultActioButtonSize),
				 this.actionButton.ConstraintTo (this, (eb, c) => eb.Left == c.Left + DefaultPropertyButtonSize),
				 this.actionButton.ConstraintTo (this, (eb, c) => eb.Top == c.Top + 3), // TODO: Better centering based on the icon height
			});

			PropertyEditorPanel.ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				PropertyEditorPanel.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
			}
		}

		private void ThemeManager_ThemeChanged (object sender, EventArgs e)
		{
			UpdateTheme ();
		}

		protected void UpdateTheme ()
		{
			this.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
		}

		protected void SetErrors (IEnumerable errors)
		{
			this.errorList = errors;

			this.actionButton.Enabled = errors != null;
			this.actionButton.Hidden = !this.actionButton.Enabled;

			// Using NSImageName.Caution for now, we can change this later at the designers behest
			this.actionButton.Image = this.actionButton.Enabled ? PropertyEditorPanel.ThemeManager.GetImageForTheme ("action-warning-16") : null;
		}

		void NotifyActionButtonClicked ()
		{
			ActionButtonClicked?.Invoke (this, EventArgs.Empty);
		}
	}
}
