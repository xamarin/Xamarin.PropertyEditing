using System;
namespace Xamarin.PropertyEditing.Mac
{
	[Serializable]
	public struct PredefinedColor
	{
		public static PredefinedColor New (string name, string displayName, string colorDesc)
		{
			return new PredefinedColor {
				Name = name,
				DisplayName = displayName,
				ColorDescription = colorDesc
			};
		}

		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string ColorDescription { get; set; }
	}
}
