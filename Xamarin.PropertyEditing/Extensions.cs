using System.Collections;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	internal static class Extensions
	{
		public static void AddRange<T> (this ICollection<T> self, IEnumerable range)
		{
			List<T> list = self as List<T>;
			if (list != null) {
				list.AddRange (range);
				return;
			}

			ObservableCollectionEx<T> observable = self as ObservableCollectionEx<T>;
			if (observable != null) {
				observable.AddRange (range);
				return;
			}

			foreach (T item in range)
				self.Add (item);
		}
	}
}
