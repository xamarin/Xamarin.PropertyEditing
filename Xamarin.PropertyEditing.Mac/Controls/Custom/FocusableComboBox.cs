using Foundation;
using AppKit;
using System;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FocusableComboBox : NSComboBox
	{
		public FocusableComboBox (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;
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

		public override bool BecomeFirstResponder ()
		{
			var willBecomeFirstResponder = base.BecomeFirstResponder ();
			if (willBecomeFirstResponder) {
				ScrollRectToVisible (Bounds);
			}
			return willBecomeFirstResponder;
		}

		public override bool ShouldBeginEditing (NSText textObject)
		{
			textObject.Delegate = new FocusableComboBoxDelegate ();
			return false;
		}

		public override void DidBeginEditing (NSNotification notification)
		{
			base.DidBeginEditing (notification);
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();
			AppearanceChanged ();
		}

		protected virtual void AppearanceChanged()
		{
			if (this.hostResources == null)
				return;

			TextColor = this.hostResources.GetNamedColor (TextColorName);
		}

		private readonly IHostResourceProvider hostResources;
		private string textColorName = NamedResources.ForegroundColor;

		class FocusableComboBoxDelegate : NSTextDelegate
		{
			public override bool TextShouldEndEditing (NSText textObject)
			{
				return false;
			}

			public override bool TextShouldBeginEditing (NSText textObject)
			{
				return false;
			}
		}
	}
}