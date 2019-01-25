using System;
using AppKit;
using CoreGraphics;
using Foundation;

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

		public virtual NSBackgroundStyle BackgroundStyle
		{
			[Export ("backgroundStyle")] get => this.label.Cell.BackgroundStyle;
			[Export ("setBackgroundStyle:")] set => this.label.Cell.BackgroundStyle = value;
		}

		public UnfocusableTextField ()
		{
			SetDefaultTextProperties ();
		}

		public UnfocusableTextField (CGRect frameRect, string text) : base (frameRect)
		{
			SetDefaultTextProperties ();

			StringValue = text;
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
	}
}