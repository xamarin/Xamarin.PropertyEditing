using System;
using System.IO;
using AppKit;
using Foundation;
using ObjCRuntime;

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

		public HostResourceProvider ()
		{
			var bundlePath = NSBundle.MainBundle.PathForResource ("PropertyEditingResource", "bundle");
			if (!Directory.Exists (bundlePath))
			{
				//if the bundle resource directory is not in place we fallback into the assembly location
				var containingDir = Path.GetDirectoryName (typeof (HostResourceProvider).Assembly.Location);
				bundlePath = Path.Combine (containingDir, "PropertyEditingResource.bundle");
			}
			
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
			NSAppearance currentAppearance = CurrentAppearance ?? NSAppearance.CurrentAppearance;
			if (currentAppearance != null && currentAppearance.Name.ToLower ().Contains ("dark")) {
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

		private readonly NSBundle resourceBundle;
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
		public const string FrameBoxBorderColor = "FrameBoxBorderColor";
		public const string FrameBoxBackgroundColor = "FrameBoxBackgroundColor";
		public const string FrameBoxButtonBorderColor = "FrameBoxButtonBorderColor";
		public const string FrameBoxButtonBackgroundColor = "FrameBoxButtonBackgroundColor";
		public const string ListHeaderSeparatorColor = "ListHeaderSeparatorColor";
		public const string PanelTabBackground = "PanelTabBackground";
		public const string ControlBackground = "ControlBackground";
	}
}
