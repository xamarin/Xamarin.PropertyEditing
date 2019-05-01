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

			this.fillColor = fillName;
			if (fillName == null)
				FillColor = NSColor.Clear;

			this.borderColor = borderName;
			if (borderName == null)
				BorderColor = NSColor.Clear;

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
					FillColor = NSColor.Clear;

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
					BorderColor = NSColor.Clear;

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

				FillColor = color;
			}

			if (this.borderColor != null) {
				NSColor color = this.hostResources.GetNamedColor (this.borderColor);
				if (color == null)
					return;

				BorderColor = color;
			}
		}
	}
}
