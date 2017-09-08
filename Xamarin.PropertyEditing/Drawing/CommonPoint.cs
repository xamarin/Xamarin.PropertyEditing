namespace Xamarin.PropertyEditing.Drawing
{
	public struct CommonPoint
	{
		/// <summary>
		/// A point in two-dimensional space.
		/// </summary>
		/// <param name="x">The horizontal coordinate of the point.</param>
		/// <param name="y">The vertical coordinate of the point.</param>
		public CommonPoint(double x, double y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// The horizontal coordiante of the point.
		/// </summary>
		public double X { get; set; }
		/// <summary>
		/// The vertical coordinate of the point.
		/// </summary>
		public double Y { get; set; }
	}
}
