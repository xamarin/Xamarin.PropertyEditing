using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class DynamicFillBox
		: NSBox
	{
		public DynamicFillBox (IHostResourceProvider hostResources, string colorName)
		{
			this.hostResources = hostResources;
			BorderWidth = 0;
			BoxType = NSBoxType.NSBoxCustom;
			TranslatesAutoresizingMaskIntoConstraints = false;
			this.colorName = colorName;
			if (colorName == null)
				FillColor = NSColor.Clear;

			ViewDidChangeEffectiveAppearance ();
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

			FillColor = this.hostResources.GetNamedColor (this.colorName);
		}

		private readonly IHostResourceProvider hostResources;
		private string colorName;
	}
}
