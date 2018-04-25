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
		/// The base tile is drawn but not repeated. The remaining area is transparent.
		/// </summary>
		None = 0,
		/// <summary>
		/// The same as Tile except that alternate columns of tiles are flipped horizontally. The base tile itself is not flipped.
		/// </summary>
		FlipX = 1,
		/// <summary>
		/// The same as Tile except that alternate rows of tiles are flipped vertically. The base tile itself is not flipped.
		/// </summary>
		FlipY = 2,
		/// <summary>
		/// The combination of FlipX and FlipY. The base tile itself is not flipped.
		/// </summary>
		FlipXY = 3,
		/// <summary>
		/// The base tile is drawn and the remaining area is filled by repeating the base tile.
		/// The right edge of one tile meets the left edge of the next, and similarly for the bottom and top edges.
		/// </summary>
		Tile = 4
	}
}
