using System;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing
{
	public interface IOrigin
	{
		/// <summary>
		/// Gets whether this property supports Origins or not
		/// </summary>
		/// <remarks>
		/// If this is <c>true</c>, <see cref="CommonOrigin Origin"/> should contain a value.
		/// </remarks>
		bool HasOrigin { get; }
	}
}
