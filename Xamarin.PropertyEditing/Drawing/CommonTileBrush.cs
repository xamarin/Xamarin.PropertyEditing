namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes a way to paint a region by using one or more tiles.
	/// </summary>
	public abstract class CommonTileBrush : CommonBrush
	{
		/// <summary>
		/// Gets or sets the horizontal alignment of content in the brush tile.
		/// </summary>
		public CommonAlignmentX AlignmentX { get; set; }
		/// <summary>
		/// Gets or sets the vertical alignment of content in the brush tile.
		/// </summary>
		public CommonAlignmentY AlignmentY { get; set; }
		/// <summary>
		/// Gets or sets a value that specifies how the content of this TileBrush stretches to fit its tiles.
		/// </summary>
		public CommonStretch Stretch { get; set; }
		/// <summary>
		/// Describes how a TileBrush paints tiles onto an output area.
		/// </summary>
		public CommonTileMode TileMode { get; set; }
		/// <summary>
		/// Gets or sets the position and dimensions of the content in a TileBrush tile.
		/// </summary>
		public CommonRectangle ViewBox { get; set; }
		/// <summary>
		/// Gets or sets a value that specifies whether the Viewbox value is relative
		/// to the bounding box of the TileBrush contents or whether the value is absolute.
		/// </summary>
		public CommonBrushMappingMode ViewBoxUnits { get; set; }
		/// <summary>
		/// Gets or sets the position and dimensions of the base tile for a TileBrush.
		/// </summary>
		public CommonRectangle ViewPort { get; set; }
		/// <summary>
		/// Gets or sets a BrushMappingMode enumeration that specifies whether the value
		/// of the Viewport, which indicates the size and position of the TileBrush base tile,
		/// is relative to the size of the output area.
		/// </summary>
		public CommonBrushMappingMode ViewPortUnits { get; set; }
	}
}
