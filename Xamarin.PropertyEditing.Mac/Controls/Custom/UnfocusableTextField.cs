using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class UnfocusableTextField : NSView
	{
		private NSTextField label;

		public NSTextAlignment Alignment {
			get { return this.label.Alignment; }
			internal set { this.label.Alignment = value; }
		}

		public NSColor BackgroundColor {
			get { return this.label.BackgroundColor; }
			internal set { this.label.BackgroundColor = value; }
		}

		public NSTextFieldCell Cell {
			get { return this.label.Cell; }
			internal set { this.label.Cell = value; }
		}

		public NSFont Font {
			get { return this.label.Font; }
			internal set { this.label.Font = value; }
		}

		public string StringValue {
			get { return this.label.StringValue; }
			internal set { this.label.StringValue = value; }
		}

		public NSColor TextColor {
			get { return this.label.TextColor; }
			internal set { this.label.TextColor = value; }
		}

		public UnfocusableTextField ()
		{
			SetDefaultTextProperties ();
			SetTheming ();
		}

		public UnfocusableTextField (CGRect frameRect, string text) : base (frameRect)
		{
			SetDefaultTextProperties ();
			SetTheming ();

			StringValue = text;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				PropertyEditorPanel.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
			}
		}

		private void SetDefaultTextProperties ()
		{
			this.label = new NSTextField {
				AccessibilityElement = false,
				BackgroundColor = NSColor.Clear,
				Bordered = false,
				Cell = {
					LineBreakMode = NSLineBreakMode.TruncatingTail,
					UsesSingleLineMode = true,
				},
				ControlSize = NSControlSize.Small,
				Editable = false,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultPropertyLabelFontSize),
				Selectable = false,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (this.label);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this.label, NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this.label, NSLayoutAttribute.Leading, 1f, 0f),
				NSLayoutConstraint.Create (this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this.label, NSLayoutAttribute.Trailing, 1f, 0f)
			});
		}

		private void SetTheming ()
		{
			PropertyEditorPanel.ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;

			UpdateTheme ();
		}

		private void ThemeManager_ThemeChanged (object sender, EventArgs e)
		{
			UpdateTheme ();
		}

		protected void UpdateTheme ()
		{
			Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
		}
	}
}