using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes a way to paint a region by using one or more tiles.
	/// </summary>
	[Serializable]
	public abstract class CommonTileBrush : CommonBrush
	{
		public CommonTileBrush(
			CommonAlignmentX alignmentX,
			CommonAlignmentY alignmentY,
			CommonStretch stretch,
			CommonTileMode tileMode,
			CommonRectangle viewBox,
			CommonBrushMappingMode viewBoxUnits,
			CommonRectangle viewPort,
			CommonBrushMappingMode viewPortUnits,
			double opacity = 1.0)
			: base(opacity)
		{
			AlignmentX = alignmentX;
			AlignmentY = alignmentY;
			Stretch = stretch;
			TileMode = tileMode;
			ViewBox = viewBox;
			ViewBoxUnits = viewBoxUnits;
			ViewPort = viewPort;
			ViewPortUnits = viewPortUnits;
		}

		/// <summary>
		/// The horizontal alignment of content in the brush tile.
		/// </summary>
		public CommonAlignmentX AlignmentX { get; }
		/// <summary>
		/// The vertical alignment of content in the brush tile.
		/// </summary>
		public CommonAlignmentY AlignmentY { get; }
		/// <summary>
		/// Describes how the content of this TileBrush stretches to fit its tiles.
		/// </summary>
		public CommonStretch Stretch { get; }
		/// <summary>
		/// Describes how a TileBrush paints tiles onto an output area.
		/// </summary>
		public CommonTileMode TileMode { get; }
		/// <summary>
		/// The position and dimensions of the content in a TileBrush tile.
		/// </summary>
		public CommonRectangle ViewBox { get; }
		/// <summary>
		/// Specifies whether the Viewbox value is relative
		/// to the bounding box of the TileBrush contents or whether the value is absolute.
		/// </summary>
		public CommonBrushMappingMode ViewBoxUnits { get; }
		/// <summary>
		/// The position and dimensions of the base tile for a TileBrush.
		/// </summary>
		public CommonRectangle ViewPort { get; }
		/// <summary>
		/// Specifies whether the value
		/// of the Viewport, which indicates the size and position of the TileBrush base tile,
		/// is relative to the size of the output area.
		/// </summary>
		public CommonBrushMappingMode ViewPortUnits { get; }

		public override bool Equals (object obj)
		{
			var brush = obj as CommonTileBrush;
			if (brush == null) return false;
			return Equals (brush);
		}

		protected bool Equals (CommonTileBrush other)
		{
			return other != null &&
				   base.Equals (other) &&
				   AlignmentX == other.AlignmentX &&
				   AlignmentY == other.AlignmentY &&
				   Stretch == other.Stretch &&
				   TileMode == other.TileMode &&
				   ViewBox.Equals (other.ViewBox) &&
				   ViewBoxUnits == other.ViewBoxUnits &&
				   ViewPort.Equals (other.ViewPort) &&
				   ViewPortUnits == other.ViewPortUnits;
		}

		public override int GetHashCode ()
		{
			var hashCode = base.GetHashCode ();
			unchecked {
				hashCode = hashCode * -1521134295 + AlignmentX.GetHashCode ();
				hashCode = hashCode * -1521134295 + AlignmentY.GetHashCode ();
				hashCode = hashCode * -1521134295 + Stretch.GetHashCode ();
				hashCode = hashCode * -1521134295 + TileMode.GetHashCode ();
				hashCode = hashCode * -1521134295 + ViewBox.GetHashCode ();
				hashCode = hashCode * -1521134295 + ViewBoxUnits.GetHashCode ();
				hashCode = hashCode * -1521134295 + ViewPort.GetHashCode ();
				hashCode = hashCode * -1521134295 + ViewPortUnits.GetHashCode ();
			}
			return hashCode;
		}
	}
}
