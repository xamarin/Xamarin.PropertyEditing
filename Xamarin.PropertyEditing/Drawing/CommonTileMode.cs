using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes how a TileBrush paints tiles onto an output area.
	/// </summary>
	[Serializable]
	public enum CommonTileMode
	{
		/// <summary>
		/// The same as Tile except that alternate columns of tiles are flipped horizontally. The base tile itself is not flipped.
		/// </summary>
		FlipX,
		/// <summary>
		/// The combination of FlipX and FlipY. The base tile itself is not flipped.
		/// </summary>
		FlipXY,
		/// <summary>
		/// The same as Tile except that alternate rows of tiles are flipped vertically. The base tile itself is not flipped.
		/// </summary>
		FlipY,
		/// <summary>
		/// The base tile is drawn but not repeated. The remaining area is transparent.
		/// </summary>
		None,
		/// <summary>
		/// The base tile is drawn and the remaining area is filled by repeating the base tile.
		/// The right edge of one tile meets the left edge of the next, and similarly for the bottom and top edges.
		/// </summary>
		Tile
	}
}
