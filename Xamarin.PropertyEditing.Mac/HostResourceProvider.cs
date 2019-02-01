using System;
using System.IO;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	public class HostResourceProvider
		: IHostResourceProvider
	{
		public NSAppearance CurrentAppearance
		{
			get;
			set;
		}

		private readonly NSBundle resourceBundle;

		public HostResourceProvider ()
		{
			var containingDir = Path.GetDirectoryName (typeof (HostResourceProvider).Assembly.Location);
			var bundlePath = Path.Combine (containingDir, "PropertyEditingResource.bundle");
			this.resourceBundle = new NSBundle (bundlePath);
		}

		public virtual NSAppearance GetVibrantAppearance (NSAppearance appearance)
		{
			if (appearance == null)
				throw new ArgumentNullException (nameof (appearance));

			if (appearance.Name == NSAppearance.NameDarkAqua || appearance.Name == NSAppearance.NameVibrantDark)
				return NSAppearance.GetAppearance (NSAppearance.NameVibrantDark);

			return NSAppearance.GetAppearance (NSAppearance.NameVibrantLight);
		}

		public virtual NSColor GetNamedColor (string name)
		{
			return NSColor.FromName (name);
		}

		public virtual NSImage GetNamedImage (string name)
		{
			if ((CurrentAppearance ?? NSAppearance.CurrentAppearance).Name.ToLower ().Contains ("dark")) {
				bool sel = name.EndsWith ("~sel");
				if (sel)
					name = name.Substring (0, name.Length - 4);

				name += "~dark";

				if (sel)
					name += "~sel";
			}

			return this.resourceBundle.ImageForResource (name);
		}

		public virtual NSFont GetNamedFont (string name, nfloat fontSize)
		{
			return NSFont.FromFontName (name, fontSize);
		}
	}

	public static class NamedResources
	{
		public const string Checkerboard0Color = "Checkerboard0";
		public const string Checkerboard1Color = "Checkerboard1";
		public const string ForegroundColor = "ForegroundColor";
		public const string PadBackgroundColor = "PadBackgroundColor";
		public const string DescriptionLabelColor = "DescriptionLabelColor";
		public const string ValueBlockBackgroundColor = "ValueBlockBackgroundColor";
		public const string TabBorderColor = "TabBorderColor";
		public const string PanelTabBackground = "PanelTabBackground";
	}
}
