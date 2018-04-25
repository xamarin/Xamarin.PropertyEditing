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
			CommonImageSource imageSource,
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
		public CommonImageBrush (
			string imageUri,
			CommonAlignmentX alignmentX,
			CommonAlignmentY alignmentY,
			CommonStretch stretch,
			CommonTileMode tileMode,
			CommonRectangle viewBox,
			CommonBrushMappingMode viewBoxUnits,
			CommonRectangle viewPort,
			CommonBrushMappingMode viewPortUnits,
			double opacity = 1.0)
			: this (string.IsNullOrEmpty(imageUri) ? null : new CommonImageSource { UriSource = new Uri (imageUri) },
				  alignmentX, alignmentY, stretch, tileMode, viewBox, viewBoxUnits, viewPort, viewPortUnits, opacity)
		{ }

		public CommonImageBrush (string imageUri = null) : this(
			imageUri,
			CommonAlignmentX.Center, CommonAlignmentY.Center, CommonStretch.None, CommonTileMode.None,
			new CommonRectangle(0, 0, 1, 1), CommonBrushMappingMode.RelativeToBoundingBox,
			new CommonRectangle(0, 0, 1, 1), CommonBrushMappingMode.RelativeToBoundingBox) { }

		/// <summary>
		/// The image displayed by this ImageBrush.
		/// </summary>
		public CommonImageSource ImageSource { get; }

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
			if (ImageSource != null) hashCode = unchecked(hashCode * -1521134295 + ImageSource.GetHashCode());
			return hashCode;
		}

		public CommonImageBrush CopyWith (
			CommonImageSource imageSource = null,
			CommonAlignmentX? alignmentX= null,
			CommonAlignmentY? alignmentY = null,
			CommonStretch? stretch = null,
			CommonTileMode? tileMode = null,
			CommonRectangle? viewBox = null,
			CommonBrushMappingMode? viewBoxUnits = null,
			CommonRectangle? viewPort = null,
			CommonBrushMappingMode? viewPortUnits = null,
			double? opacity = null)

			=> new CommonImageBrush (
				imageSource ?? ImageSource,
				alignmentX ?? AlignmentX,
				alignmentY ?? AlignmentY,
				stretch ?? Stretch,
				tileMode ?? TileMode,
				viewBox ?? ViewBox,
				viewBoxUnits ?? ViewBoxUnits,
				viewPort ?? ViewPort,
				viewPortUnits ?? ViewPortUnits,
				opacity ?? Opacity);
	}
}
