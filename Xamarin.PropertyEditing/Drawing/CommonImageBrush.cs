using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Paints an area with an image.
	/// </summary>
	[Serializable]
	public sealed class CommonImageBrush : CommonTileBrush, IEquatable<CommonImageBrush>
	{
		public CommonImageBrush(
			string imageSource,
			CommonAlignmentX alignmentX,
			CommonAlignmentY alignmentY,
			CommonStretch stretch,
			CommonTileMode tileMode,
			CommonRectangle viewBox,
			CommonBrushMappingMode viewBoxUnits,
			CommonRectangle viewPort,
			CommonBrushMappingMode viewPortUnits,
			double opacity = 1.0)
			: base(alignmentX, alignmentY, stretch, tileMode, viewBox, viewBoxUnits, viewPort, viewPortUnits, opacity)
		{
			ImageSource = imageSource;
		}

		/// <summary>
		/// The image displayed by this ImageBrush.
		/// </summary>
		public string ImageSource { get; }

		public override bool Equals (object obj)
		{
			var brush = obj as CommonImageBrush;
			if (brush == null) return false;
			return Equals (brush);
		}

		public bool Equals (CommonImageBrush other)
		{
			return other != null &&
				   base.Equals (other) &&
				   ImageSource == other.ImageSource;
		}

		public static bool operator == (CommonImageBrush left, CommonImageBrush right) => Equals (left, right);
		public static bool operator != (CommonImageBrush left, CommonImageBrush right) => !Equals (left, right);

		public override int GetHashCode ()
		{
			var hashCode = base.GetHashCode ();
			hashCode = unchecked(hashCode * -1521134295 + ImageSource.GetHashCode());
			return hashCode;
		}
	}
}
