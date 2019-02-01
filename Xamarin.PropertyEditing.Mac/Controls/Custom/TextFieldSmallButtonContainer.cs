using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class SmallButton : NSButton
	{
		private NSView previousKeyView;
		public override NSView PreviousKeyView => this.previousKeyView ?? base.PreviousKeyView;

		internal void SetPreviousKeyView (NSView view)
		{
			this.previousKeyView = view;
		}

		public SmallButton ()
		{
			BezelStyle = NSBezelStyle.RegularSquare;
			ControlSize = NSControlSize.Small;
			Bordered = false;
			TranslatesAutoresizingMaskIntoConstraints = false;
			Cell.HighlightsBy = (int)NSCellStyleMask.ContentsCell;
		}
	}

	internal class TextFieldSmallButtonContainer : NSTextField
	{
		public List<SmallButton> Buttons { get; } = new List<SmallButton> ();
		private readonly ButtonTextFieldCell cell;
		internal NSView lastView;

		public override bool Editable { 
			get => base.Editable;
			set
			{
				base.Editable = value;
				foreach (var item in Buttons) {
					item.Enabled = value;
				}
			}
		}

		public TextFieldSmallButtonContainer ()
		{
			Cell = this.cell = new ButtonTextFieldCell (this);
			Cell.Editable = true;
			Cell.Bezeled = true;
			Cell.ControlSize = NSControlSize.Regular;
			Cell.BezelStyle = NSTextFieldBezelStyle.Square;
			TranslatesAutoresizingMaskIntoConstraints = false;
			UsesSingleLineMode = true;
			LineBreakMode = NSLineBreakMode.CharWrapping;
			this.lastView = this;
		}
		public override bool AllowsVibrancy => false;

		public override void DrawRect (CGRect dirtyRect)
		{
			// There appears to be a bug in AppKit in light theme when NSTextField is a subview
			// of a layer-backed view, the background of the field does not get properly redrawn
			// becase the dirtyRect is seemingly invalid. Fix this by redrawing the entire control.
			// https://www.cocoanetics.com/2013/01/radar-nstextfield-redrawing-of-background-with-layer-backing/
			// http://www.openradar.me/radar?id=2552401
			base.DrawRect (Bounds);
		}
		public int ButtonSize { get; set; } = 16;

		public int ButtonRightBorder { get; set; } = 3;

		public int ButtonSeparation { get; set; } = 1;

		public void AddButton (SmallButton button)
		{
			AddSubview (button);

			var separation = Buttons.Count == 0 ? ButtonRightBorder : (ButtonSeparation + ButtonSize);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (button, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, lastView, NSLayoutAttribute.CenterY, 1f, 0),
				NSLayoutConstraint.Create (button, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, ButtonSize),
				NSLayoutConstraint.Create (button, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, ButtonSize),
				NSLayoutConstraint.Create (button, NSLayoutAttribute.Right, NSLayoutRelation.Equal, lastView, NSLayoutAttribute.Right, 1f, -separation),
			});

			Buttons.Add (button);
			this.lastView = Buttons[Buttons.Count - 1];

			//preview keyview calculation
			button.SetPreviousKeyView (this);
			if (Buttons.Count > 1) {
				for (var i = Buttons.Count - 2; i >= 0; i--) {
					Buttons[i].SetPreviousKeyView (Buttons[i+1]);
				}
			}
		}

		private sealed class ButtonTextFieldCell : NSTextFieldCell
		{
			private readonly TextFieldSmallButtonContainer field;

			public ButtonTextFieldCell (TextFieldSmallButtonContainer field)
			{
				this.field = field;
			}

			public override void DrawInteriorWithFrame (CGRect cellFrame, NSView inView)
			{
				base.DrawInteriorWithFrame (DrawingRectForBounds (cellFrame), inView);
			}

			public override void EditWithFrame (CGRect aRect, NSView inView, NSText editor, NSObject delegateObject, NSEvent theEvent)
			{
				base.EditWithFrame (DrawingRectForBounds (aRect), inView, editor, delegateObject, theEvent);
			}

			public override void SelectWithFrame (CGRect aRect, NSView inView, NSText editor, NSObject delegateObject, nint selStart, nint selLength)
			{
				base.SelectWithFrame (DrawingRectForBounds (aRect), inView, editor, delegateObject, selStart, selLength);
			}
		
			public override void ResetCursorRect (CGRect cellFrame, NSView inView)
			{
				base.ResetCursorRect (DrawingRectForBounds (cellFrame), inView);
			}

			public override CGRect DrawingRectForBounds (CGRect theRect)
			{
				CGRect baseRect = base.DrawingRectForBounds (theRect);
				if (this.field.Buttons.Count == 0) {
					return baseRect;
				}
				return new CGRect (baseRect.X, baseRect.Y - 2, baseRect.Width - (this.field.ButtonSize * this.field.Buttons.Count), 20);
			}
		}
	}
}
