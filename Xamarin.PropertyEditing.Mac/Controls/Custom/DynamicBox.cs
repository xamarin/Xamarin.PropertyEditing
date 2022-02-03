using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class DynamicBox
		: NSBox
	{
		public DynamicBox (IHostResourceProvider hostResources, string fillName = null, string borderName = null)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			BorderWidth = 0;
			BoxType = NSBoxType.NSBoxCustom;
			TranslatesAutoresizingMaskIntoConstraints = false;
			this.WantsLayer = true;
			this.fillColor = fillName;
			if (fillName == null)
				this.Layer.BackgroundColor = NSColor.Clear.CGColor;

			this.borderColor = borderName;
			if (borderName == null)
				this.Layer.BorderColor = NSColor.Clear.CGColor;

			HostResourceProvider = hostResources;
		}

		public IHostResourceProvider HostResourceProvider
		{
			get { return this.hostResources; }
			set
			{
				this.hostResources = value;
				AppearanceChanged ();
			}
		}

		public string FillColorName
		{
			get => this.fillColor;
			set
			{
				this.fillColor = value;
				if (value == null)
					this.Layer.BackgroundColor = NSColor.Clear.CGColor;

				AppearanceChanged ();
			}
		}

		public string BorderColorName
		{
			get => this.borderColor;
			set
			{
				this.borderColor = value;
				if (value == null)
					this.Layer.BorderColor = NSColor.Clear.CGColor;

				AppearanceChanged ();
			}
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}

		private IHostResourceProvider hostResources;
		private string fillColor, borderColor;

		private void AppearanceChanged ()
		{
			if (this.fillColor != null) {
				NSColor color = this.hostResources.GetNamedColor (this.fillColor);
				if (color == null)
					return;
				this.Layer.BackgroundColor = color.CGColor;
			}

			if (this.borderColor != null) {
				NSColor color = this.hostResources.GetNamedColor (this.borderColor);
				if (color == null)
					return;
				this.Layer.BorderColor = color.CGColor;
			}
		}
	}
}
