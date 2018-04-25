using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes how content is positioned horizontally in a container.
	/// </summary>
	[Serializable]
	public enum CommonAlignmentX
	{
		/// <summary>
		/// The contents align toward the left of the container.
		/// </summary>
		Left = 0,
		/// <summary>
		/// The contents align toward the center of the container.
		/// </summary>
		Center = 1,
		/// <summary>
		/// The contents align toward the right of the container.
		/// </summary>
		Right = 2
	}

	/// <summary>
	/// Describes how content is positioned vertically in a container.
	/// </summary>
	[Serializable]
	public enum CommonAlignmentY
	{
		/// <summary>
		/// The contents align toward the upper edge of the container.
		/// </summary>
		Top = 0,
		/// <summary>
		/// The contents align toward the center of the container.
		/// </summary>
		Center = 1,
		/// <summary>
		/// The contents align toward the lower edge of the container.
		/// </summary>
		Bottom = 2
	}
}
