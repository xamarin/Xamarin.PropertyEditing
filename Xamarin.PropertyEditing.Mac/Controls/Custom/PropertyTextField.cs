using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyTextField : NSTextField
	{
		public PropertyTextField (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			HostResources = hostResources;
			AllowsExpansionToolTips = true;
			Cell.LineBreakMode = NSLineBreakMode.TruncatingTail;
			Cell.UsesSingleLineMode = true;

			AppearanceChanged ();
		}

		public string TextColorName
		{
			get => this.textColorName;
			set
			{
				if (this.textColorName == value)
					return;

				this.textColorName = value;
				AppearanceChanged ();
			}
		}

		protected IHostResourceProvider HostResources
		{
			get;
		}

		public override bool BecomeFirstResponder ()
		{
			var willBecomeFirstResponder = base.BecomeFirstResponder ();
			if (willBecomeFirstResponder) {
				ScrollRectToVisible (Bounds);
			}
			return willBecomeFirstResponder;
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}

		protected virtual void AppearanceChanged ()
		{
			if (HostResources == null)
				return;

			TextColor = HostResources.GetNamedColor (TextColorName);
		}

		private string textColorName = NamedResources.ForegroundColor;
	}

	internal class PropertyTextFieldCell : NSTextFieldCell
	{
		public PropertyTextFieldCell ()
		{
			LineBreakMode = NSLineBreakMode.TruncatingTail;
			UsesSingleLineMode = true;
		}
	}
}
