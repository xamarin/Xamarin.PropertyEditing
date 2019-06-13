using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class UnfocusableTextField : NSView
	{
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

		public string TextColorName {
			get { return this.label.TextColorName; }
			internal set { this.label.TextColorName = value; }
		}

		public virtual NSBackgroundStyle BackgroundStyle
		{
			[Export ("backgroundStyle")] get => this.label.Cell.BackgroundStyle;
			[Export ("setBackgroundStyle:")] set => this.label.Cell.BackgroundStyle = value;
		}

		public UnfocusableTextField (IHostResourceProvider hostResources)
		{
			SetDefaultTextProperties (hostResources);
		}

		public UnfocusableTextField (IHostResourceProvider hostResources, CGRect frameRect, string text) : base (frameRect)
		{
			SetDefaultTextProperties (hostResources);

			StringValue = text;
		}

		private PropertyTextField label;

		private void SetDefaultTextProperties (IHostResourceProvider hostResources)
		{
			this.label = new PropertyTextField (hostResources) {
				AccessibilityElement = false,
				BackgroundColor = NSColor.Clear,
				Bordered = false,
				ControlSize = NSControlSize.Small,
				Editable = false,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
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