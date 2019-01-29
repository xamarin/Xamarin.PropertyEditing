using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class DynamicFillBox
		: NSBox
	{
		public DynamicFillBox (IHostResourceProvider hostResources, string colorName)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			BorderWidth = 0;
			BoxType = NSBoxType.NSBoxCustom;
			TranslatesAutoresizingMaskIntoConstraints = false;
			this.colorName = colorName;
			if (colorName == null)
				FillColor = NSColor.Clear;

			HostResourceProvider = hostResources;
		}

		public IHostResourceProvider HostResourceProvider
		{
			get { return this.hostResources; }
			set
			{
				this.hostResources = value;
				ViewDidChangeEffectiveAppearance ();
			}
		}

		public string FillColorName
		{
			get => this.colorName;
			set
			{
				this.colorName = value;
				if (value == null)
					FillColor = NSColor.Clear;

				ViewDidChangeEffectiveAppearance ();
			}
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			if (this.colorName == null)
				return;

			NSColor color = this.hostResources.GetNamedColor (this.colorName);
			if (color == null)
				return;

			FillColor = color;
		}

		private IHostResourceProvider hostResources;
		private string colorName;
	}
}
