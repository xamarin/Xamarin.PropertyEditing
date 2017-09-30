using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Baml2006;
using System.Xaml;

namespace Xamarin.PropertyEditing.Themes
{

	public class WinThemeManager : BaseThemeManager
	{
		Dictionary<string, ResourceDictionary> themeList;

		public WinThemeManager ()
		{
			themeList = new Dictionary<string, ResourceDictionary> ();

			var asm = Assembly.GetExecutingAssembly ();
			var resourcenames = asm.GetManifestResourceNames ();
			foreach (var item in resourcenames) {
				ManifestResourceInfo info = asm.GetManifestResourceInfo (item);
				if (info.ResourceLocation != ResourceLocation.ContainedInAnotherAssembly) {
					using (var stream = asm.GetManifestResourceStream (item)) {
						using (var reader = new ResourceReader (stream)) {
							foreach (DictionaryEntry entry in reader) {
								if ((entry.Key.ToString ().EndsWith ("baml") && entry.Value is Stream)) {
									if (entry.Key.ToString ().Contains ("vs.")) {
										themeList[Path.GetFileName (entry.Key.ToString ())] = ExtractResourceDictionary (entry.Value as Stream);
									}
								}
							}
						}
					}
				}
			}
		}

		private ResourceDictionary ExtractResourceDictionary (Stream stream)
		{
			// We need the Baml reader to de compile the xaml back for later use.
			using (var breader = new Baml2006Reader (stream)) {
				using (var writer = new XamlObjectWriter (breader.SchemaContext)) {
					while (breader.Read ()) {
						writer.WriteNode (breader);
					}
					return writer.Result as ResourceDictionary;
				}
			}
		}

		protected override void SetTheme ()
		{
			if (themeList != null) {
				// Making sure none of the themes are loaded, then we can just switch appropriately.
				Application.Current.Resources.MergedDictionaries.Remove (themeList["vs.dark.baml"]);
				Application.Current.Resources.MergedDictionaries.Remove (themeList["vs.light.baml"]);

				switch (Theme) {
					case PropertyEditorTheme.Dark:
						Application.Current.Resources.MergedDictionaries.Add (themeList["vs.dark.baml"]);
						break;

					case PropertyEditorTheme.Light:
						Application.Current.Resources.MergedDictionaries.Add (themeList["vs.light.baml"]);
						break;
				}
			}
		}
	}
}
