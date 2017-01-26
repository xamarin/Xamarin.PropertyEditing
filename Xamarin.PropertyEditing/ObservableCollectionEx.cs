using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.PropertyEditing
{
	internal class ObservableCollectionEx<T>
		: ObservableCollection<T>
	{
		public void AddRange (IEnumerable<T> range)
		{
			int index = Count;
			var list = range.ToList ();
			for (int i = 0; i < list.Count; i++) {
				Items.Add (list[i]);
			}

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, list, index));
		}
	}
}
