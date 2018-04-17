using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	public class AssignableTypesResult
	{
		public AssignableTypesResult (IReadOnlyList<ITypeInfo> assignableTypes)
		{
			if (assignableTypes == null)
				throw new ArgumentNullException (nameof(assignableTypes));

			AssignableTypes = assignableTypes;
		}

		public AssignableTypesResult (IReadOnlyList<ITypeInfo> suggestedTypes, IReadOnlyList<ITypeInfo> assignableTypes)
			: this (assignableTypes)
		{
			if (suggestedTypes == null)
				throw new ArgumentNullException (nameof(suggestedTypes));

			SuggestedTypes = suggestedTypes;
		}

		public IReadOnlyList<ITypeInfo> SuggestedTypes
		{
			get;
		}

		public IReadOnlyList<ITypeInfo> AssignableTypes
		{
			get;
		}
	}
}