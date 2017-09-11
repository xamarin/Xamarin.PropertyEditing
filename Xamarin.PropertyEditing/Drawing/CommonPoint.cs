namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// A point in two-dimensional space.
	/// </summary>
	public struct CommonPoint
	{
		/// <param name="x">The horizontal coordinate of the point.</param>
		/// <param name="y">The vertical coordinate of the point.</param>
		public CommonPoint(double x, double y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// The horizontal coordinate of the point.
		/// </summary>
		public double X { get; set; }
		/// <summary>
		/// The vertical coordinate of the point.
		/// </summary>
		public double Y { get; set; }
	}
}
