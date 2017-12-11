using System.IO;

using static System.Environment;

namespace Xamarin.PropertyEditing
{
	public static class FeatureHelper
	{
		#region Xamarin Property Editor Flags
		const string ShowActionIconString = "xamarin-proppy-show-action-icon";
		const string ShowGuideLinesString = "xamarin-proppy-show-guidelines";
		const string ShowPropertyIconString = "xamarin-proppy-show-property-icon";
		#endregion

		public static bool ShowActionIcon
		{
			get => IsFeatureEnabled (ShowActionIconString);
		}

		public static bool ShowGuideLines
		{
			get => IsFeatureEnabled (ShowGuideLinesString);
		}

		public static bool ShowPropertyIcon
		{
			get => IsFeatureEnabled (ShowPropertyIconString);
		}

		static bool IsFeatureEnabled (string feature)
		{
			return File.Exists (GetFeatureFilePath (feature));
		}

		static string GetFeatureFilePath (string fileName)
		{
			return Path.Combine (GetFolderPath (SpecialFolder.Personal), fileName);
		}
	}
}
