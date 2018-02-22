using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal struct MaterialColorScale : IEquatable<MaterialColorScale>
	{
		public MaterialColorScale (string name, bool isAccent, int lightIndex, params CommonColor[] colors) : this ()
		{
			Name = name;
			IsAccent = isAccent;
			LightScriptureIndex = lightIndex;
			Colors = colors ?? new CommonColor[0];
		}

		public IReadOnlyList<CommonColor> Colors { get; set; }
		// At which index in the scale the rendering should switch to use a light color for the color label
		public int LightScriptureIndex { get; set; }
		public double LightScriptureLightnessThreshold => Colors == null ? 0.667 :
			Colors.Count <= LightScriptureIndex ? 0 : Colors[LightScriptureIndex].Lightness + 0.001;
		public bool IsAccent { get; set; }
		public string Name { get; set; }

		// 500 for main color, A200 for accents
		public CommonColor MainColor
			=> IsAccent ?
				(Colors != null && Colors.Count > 1 ? Colors[1] : CommonColor.Black) :
				(Colors != null && Colors.Count > 5 ? Colors[5] : Colors.Count > 1 ? Colors[1] : CommonColor.Black);

		public bool Equals (MaterialColorScale other)
		{
			if (ReferenceEquals (this, other))
				return true;

			if (other.Name != Name ||
				other.IsAccent != IsAccent ||
				other.LightScriptureIndex != LightScriptureIndex) return false;

			if (other.Colors is null) return Colors is null;

			if (Colors is null || other.Colors.Count != Colors.Count) return false;

			for (var i = 0; i < Colors.Count; i++) {
				if (other.Colors[i] != Colors[i]) return false;
			}

			return true;
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != GetType ())
				return false;
			return Equals ((MaterialColorScale)obj);
		}

		public static bool operator == (MaterialColorScale left, MaterialColorScale right)
		{
			return Equals (left, right);
		}

		public static bool operator != (MaterialColorScale left, MaterialColorScale right)
		{
			return !Equals (left, right);
		}

		public override string ToString () => Name;

		public override int GetHashCode ()
		{
			unchecked {
				var hashCode = LightScriptureIndex.GetHashCode () * -1521134295 + IsAccent.GetHashCode ();
				if (Name != null) {
					hashCode = hashCode * -1521134295 + Name.GetHashCode ();
				}
				if (Colors != null) {
					foreach (CommonColor color in Colors) {
						hashCode = hashCode * -1521134295 + color.GetHashCode ();
					}
				}
				return hashCode;
			}
		}
	}
}
