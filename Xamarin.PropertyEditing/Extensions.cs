using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.PropertyEditing
{
	internal static class Extensions
	{
		public static void AddItems<T> (this ICollection<T> self, IEnumerable items)
		{
			if (items == null)
				throw new ArgumentNullException (nameof (items));

			IEnumerable<T> enumerable = items as IEnumerable<T> ?? items.Cast<T>();

			List<T> list = self as List<T>;
			if (list != null) {
				list.AddRange (enumerable);
				return;
			}

			ObservableCollectionEx<T> observable = self as ObservableCollectionEx<T>;
			if (observable != null) {
				observable.AddRange (enumerable);
				return;
			}

			foreach (T item in enumerable)
				self.Add (item);
		}

		public static void ReplaceOrAdd<T> (this ICollection<T> self, T replace, T with)
		{
			if (self == null)
				throw new ArgumentNullException (nameof (self));

			IList<T> list = self as IList<T>;
			if (list != null) {
				int i = list.IndexOf (replace);
				if (i != -1)
					list[i] = with;
				else
					list.Add (with);

				return;
			}

			self.Remove (replace);
			self.Add (with);
		}

		public static bool TryRemove<TKey, TElement> (this IDictionary<TKey, TElement> self, TKey key, out TElement element)
		{
			if (!self.TryGetValue (key, out element))
				return false;

			self.Remove (key);
			return true;
		}

		public static bool Contains (this string self, string value, StringComparison comparison)
		{
			if (self == null)
				throw new ArgumentNullException (nameof(self));

			return self.IndexOf (value, comparison) >= 0;
		}
	}
}
