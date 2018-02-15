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
		/// Gets or sets whether the platform supports custom expressions (default false).
		/// </summary>
		public bool SupportsCustomExpressions
		{
			get;
			set;
		}

		/// <summary>
		/// Specifies whether Material Design is relevant to theplatform.
		/// </summary>
		public bool SupportsMaterialDesign
		{
			get;
			set;
		}

		public bool SupportsBrushOpacity
		{
			get;
			set;
		}

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
