using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal struct MaterialColorScale
	{
		public MaterialColorScale (string name, bool isAccent, int lightIndex, params CommonColor[] colors) : this ()
		{
			Name = name;
			IsAccent = isAccent;
			LightScriptureIndex = lightIndex;
			Colors = colors;
		}

		public IReadOnlyList<CommonColor> Colors { get; set; }
		// At which index in the scale the rendering should switch to use a light color for the color label
		public int LightScriptureIndex { get; set; }
		public bool IsAccent { get; set; }
		public string Name { get; set; }

		// 500 for main color, A200 for accents
		public CommonColor MainColor
		{
			get {
				return IsAccent ? Colors[1] : Colors[5];
			}
		}

		public override string ToString () => Name;
	}
}
