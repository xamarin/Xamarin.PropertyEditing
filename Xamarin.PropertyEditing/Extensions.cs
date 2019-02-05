using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

		public static int IndexOf<T> (this IEnumerable<T> self, T element)
		{
			if (self == null)
				throw new ArgumentNullException (nameof (self));

			if (self is IList list)
				return list.IndexOf (element);

			int i = 0;
			foreach (T current in self) {
				if (Equals (current, element))
					return i;

				i++;
			}

			return -1;
		}

		public static object ElementAt (this IEnumerable self, int index)
		{
			if (self == null)
				throw new ArgumentNullException (nameof (self));
			if (index < 0)
				throw new ArgumentOutOfRangeException (nameof(index));

			if (self is IList list)
				return list[index];

			int i = 0;
			foreach (object element in self) {
				if (i++ == index)
					return element;
			}

			throw new ArgumentOutOfRangeException (nameof(index));
		}

		public static int Count (this IEnumerable self)
		{
			if (self == null)
				throw new ArgumentNullException (nameof (self));

			if (self is ICollection collection)
				return collection.Count;

			int count = 0;
			foreach (object element in self)
				count++;

			return count;
		}

		/// <summary>
		/// Replaces the first instance of <paramref name="replace"/> with <paramref name="with"/>, or adds it if <paramref name="replace"/> isn't found.
		/// </summary>
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

		public static void Reset<T> (this ICollection<T> self, IEnumerable<T> newContents)
		{
			if (self == null)
				throw new ArgumentNullException (nameof(self));
			if (newContents == null)
				throw new ArgumentNullException (nameof(newContents));

			if (self is ObservableCollectionEx<T> oce) {
				oce.Reset (newContents);
			} else {
				self.Clear();
				self.AddItems (newContents);
			}
		}

		public static void Move (this IList self, int index, int moveTo)
		{
			if (self == null)
				throw new ArgumentNullException (nameof(self));

			if (index < moveTo)
				moveTo--;

			object item = self[index];
			self.RemoveAt (index);
			self.Insert (moveTo, item);
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

		public static async Task<AssignableTypesResult> GetCommonAssignableTypes (this IEnumerable<IObjectEditor> editors, IPropertyInfo property, bool childTypes)
		{
			if (editors == null)
				throw new ArgumentNullException (nameof(editors));
			if (property == null)
				throw new ArgumentNullException (nameof(property));

			List<ITypeInfo> suggested = null;
			HashSet<ITypeInfo> all = null;
			var tasks = new HashSet<Task<AssignableTypesResult>> (editors.Select (oe => oe.GetAssignableTypesAsync (property, childTypes)));

			while (tasks.Count > 0) {
				var task = await Task.WhenAny (tasks).ConfigureAwait (false);
				tasks.Remove (task);

				if (suggested == null) {
					suggested = new List<ITypeInfo> (task.Result.SuggestedTypes ?? Enumerable.Empty<ITypeInfo>());
					all = new HashSet<ITypeInfo> (task.Result.AssignableTypes);
					continue;
				}

				all.IntersectWith (task.Result.AssignableTypes);

				foreach (ITypeInfo type in suggested.ToArray()) {
					if (!task.Result.AssignableTypes.Contains (type))
						suggested.Remove (type);
				}
			}

			return new AssignableTypesResult (suggested, all);
		}

		public static ITypeInfo ToTypeInfo (this Type type, bool isRelevant = true)
		{
			var asm = type.Assembly.GetName ().Name;
			return new TypeInfo (new AssemblyInfo (asm, isRelevant), type.Namespace, type.Name);
		}

		public static bool HasVariations (this IPropertyInfo property)
		{
			return (property.Variations?.Count ?? 0) > 0;
		}
	}
}
