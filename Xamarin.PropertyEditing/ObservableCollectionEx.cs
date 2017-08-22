using System;
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
			if (range == null)
				throw new ArgumentNullException (nameof(range));

			foreach (T element in range)
				Add (element);

			return;

			int index = Count;
			var list = range.ToList ();
			for (int i = 0; i < list.Count; i++) {
				Items.Add (list[i]);
			}

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, list, index));
		}

		public void RemoveRange (IEnumerable<T> range)
		{
			foreach (T element in range)
				Remove (element);
		}

		public void Reset (IEnumerable<T> newContents)
		{
			if (newContents == null)
				throw new ArgumentNullException (nameof(newContents));

			Items.Clear();
			Items.AddItems (newContents);
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}
	}
}
