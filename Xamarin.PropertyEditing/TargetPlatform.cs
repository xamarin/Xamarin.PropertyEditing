using System;
using System.Collections.Generic;

using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing
{
	/// <summary>
	/// Represents a target platform by defining top level feature support and presentation preferences 
	/// </summary>
	public sealed class TargetPlatform
	{
		/// <summary>
		/// Gets a dictionary defining the property types will be grouped into a single editor and their groups resource name.
		/// </summary>
		public IReadOnlyDictionary<Type, string> GroupedTypes
		{
			get;
			set;
		}

		public static readonly TargetPlatform Default = new TargetPlatform {
			GroupedTypes = new Dictionary<Type, string> {
				{ typeof(CommonBrush), "Brush" }
			}
		};
	}
}
