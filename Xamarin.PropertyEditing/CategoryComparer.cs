using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	internal class CategoryComparer
		: IComparer<string>
	{
		public static readonly CategoryComparer Instance = new CategoryComparer();

		public int Compare (string x, string y)
		{
			int result = Comparer<string>.Default.Compare (x, y);
			if (result != 0 && (String.IsNullOrEmpty (x) || String.IsNullOrEmpty (y)))
				result *= -1;

			return result;
		}
	}
}
