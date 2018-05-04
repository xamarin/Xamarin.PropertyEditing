using System;
using System.Collections.Generic;
using System.Linq;
using Cadenza.Collections;

namespace Xamarin.PropertyEditing
{
	public class AssignableTypesResult
	{
		public AssignableTypesResult (IReadOnlyCollection<ITypeInfo> assignableTypes)
		{
			if (assignableTypes == null)
				throw new ArgumentNullException (nameof(assignableTypes));

			AssignableTypes = assignableTypes;
			SuggestedTypes = new ITypeInfo[0];
		}

		public AssignableTypesResult (IReadOnlyList<ITypeInfo> suggestedTypes, IReadOnlyCollection<ITypeInfo> assignableTypes)
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

		public IReadOnlyCollection<ITypeInfo> AssignableTypes
		{
			get;
		}

		internal IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>> GetTypeTree ()
		{
			var assemblies = new Dictionary<IAssemblyInfo, ILookup<string, ITypeInfo>> ();
			foreach (ITypeInfo type in AssignableTypes) {
				if (!assemblies.TryGetValue (type.Assembly, out ILookup<string, ITypeInfo> types)) {
					assemblies[type.Assembly] = types = new ObservableLookup<string, ITypeInfo> ();
				}

				((IMutableLookup<string, ITypeInfo>) types).Add (type.NameSpace, type);
			}

			return assemblies;
		}
	}
}