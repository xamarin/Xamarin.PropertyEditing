using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.PropertyEditing
{
	internal interface IGroupingList<TKey, TElement>
		: IGrouping<TKey, TElement>, IReadOnlyList<TElement>
	{
	}

	internal class ObservableGrouping<TKey, TElement>
		: ObservableCollectionEx<TElement>, IGroupingList<TKey, TElement>
	{
		public ObservableGrouping (TKey key)
		{
			Key = key;
		}

		public ObservableGrouping (IGrouping<TKey, TElement> grouping)
		{
			Key = grouping.Key;
			AddRange (grouping);
		}

		public TKey Key
		{
			get;
		}

		public void QuietClear()
		{
			Items.Clear ();
		}
	}
}
