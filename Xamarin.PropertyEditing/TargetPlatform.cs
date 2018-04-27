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
		public TargetPlatform (IEditorProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException (nameof(provider));

			EditorProvider = provider;
		}

		/// <summary>
		/// Gets the <see cref="IEditorProvider"/> associated with this platform.
		/// </summary>
		public IEditorProvider EditorProvider
		{
			get;
		}

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
		/// Gets or sets a dictionary defining the property types will be grouped into a single editor and their groups key.
		/// </summary>
		public IReadOnlyDictionary<Type, string> GroupedTypes
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a collection of group keys that should be automatically expanded in grouped mode at first load.
		/// </summary>
		public IReadOnlyCollection<string> AutoExpandGroups
		{
			get;
			set;
		}
	}
}
