namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes the width, height, and location of a rectangle.
	/// </summary>
	public struct CommonRectangle
	{
		/// <param name="x">The horizontal coordinate of left border of the rectangle.</param>
		/// <param name="y">The vertical coordinate of the top border of the rectangle.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		public CommonRectangle (double x, double y, double width, double height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		/// <summary>
		/// The horizontal coordinate of left border of the rectangle.
		/// </summary>
		public double X { get; set; }
		/// <summary>
		/// The vertical coordinate of the top border of the rectangle.
		/// </summary>
		public double Y { get; set; }
		/// <summary>
		/// The width of the rectangle.
		/// </summary>
		public double Width { get; set; }
		/// <summary>
		/// The height of the rectangle.
		/// </summary>
		public double Height { get; set; }
	}
}
