using System.Collections.Generic;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal static class Extensions
	{
		public static void AddRange<T> (this ICollection<T> self, IEnumerable<T> range)
		{
			foreach (T item in range)
				self.Add (item);
		}
	}
}
