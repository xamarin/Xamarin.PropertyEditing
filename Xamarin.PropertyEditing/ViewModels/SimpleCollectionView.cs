using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Cadenza.Collections;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class SimpleCollectionViewOptions
		: NotifyingObject
	{

		public SimpleCollectionViewOptions ChildOptions
		{
			get { return this.childOptions; }
			set
			{
				if (this.childOptions == value)
					return;

				this.childOptions = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets or sets the element selector with which elements will be arranged and grouped.
		/// </summary>
		public Func<object, string> DisplaySelector
		{
			get { return this.displaySelector; }
			set {
				if (this.displaySelector == value)
					return;

				this.displaySelector = value;
				OnPropertyChanged();
			}
		}

		public IComparer<string> DisplayComparer
		{
			get { return this.displayComparer; }
			set
			{
				if (this.displayComparer == value)
					return;

				this.displayComparer = value;
				OnPropertyChanged();
			}
		}

		public Func<object, IEnumerable> ChildrenSelector
		{
			get { return this.childrenSelector; }
			set
			{
				if (this.childrenSelector == value)
					return;

				this.childrenSelector = value;
				OnPropertyChanged();
			}
		}

		public Predicate<object> Filter
		{
			get { return this.filter; }
			set
			{
				if (this.filter == value)
					return;

				this.filter = value;
				OnPropertyChanged();
			}
		}

		private IComparer<string> displayComparer = Comparer<string>.Default;
		private Predicate<object> filter;
		private Func<object, IEnumerable> childrenSelector;
		private Func<object, string> displaySelector;
		private SimpleCollectionViewOptions childOptions;
	}

	internal class SimpleCollectionView
		: NotifyingObject, IEnumerable, INotifyCollectionChanged
	{
		public SimpleCollectionView (IEnumerable source, SimpleCollectionViewOptions options)
			: this (source, options, null, null)
		{
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public SimpleCollectionViewOptions Options
		{
			get { return this.options; }
			set
			{
				if (this.options == value)
					return;

				SetOptions (value);
			}
		}

		public bool HasChildElements => (this.filtered.Count > 0);

		public bool IsFiltering => Options?.Filter != null;

		public IEnumerator GetEnumerator ()
		{
			bool haveChildren = (Options.ChildOptions != null);

			foreach (var kvp in this.filtered) {
				if (!haveChildren)
					yield return kvp.Value.Item;
				else
					yield return new KeyValuePair<string, SimpleCollectionView> (kvp.Key, kvp.Value.ChildrenView);
			}
		}

		private class Element
		{
			public object Item;
			public SimpleCollectionView ChildrenView;
		}

		private readonly OrderedDictionary<string, Element> filtered = new OrderedDictionary<string, Element>();
		private readonly OrderedDictionary<string, Element> arranged = new OrderedDictionary<string, Element>();
		private readonly SimpleCollectionView parent;
		private readonly string key;
		private readonly object item;
		private SimpleCollectionViewOptions options;
		private bool wasFilterNull;
		private IEnumerable source;

		private IComparer<string> Comparer => Options?.DisplayComparer ?? Comparer<string>.Default;

		private SimpleCollectionView (IEnumerable source, SimpleCollectionViewOptions options, SimpleCollectionView parent = null, string key = null, object item = null)
		{
			if (source == null)
				throw new ArgumentNullException (nameof (source));
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			this.wasFilterNull = (options.Filter == null);
			this.source = source;
			this.parent = parent;
			this.item = item;
			this.key = key;
			SetOptions (options, notify: false);

			var changed = source as INotifyCollectionChanged;
			if (changed != null) {
				changed.CollectionChanged += OnSourceCollectionChanged;
			}
		}

		private void SetOptions (SimpleCollectionViewOptions newOptions, bool notify = true)
		{
			if (this.options != null)
				this.options.PropertyChanged -= OnOptionsPropertyChanged;

			this.options = newOptions;
			if (this.options != null)
				this.options.PropertyChanged += OnOptionsPropertyChanged;

			Reset (notify);
		}

		private void OnSourceCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			// TODO: More fine grained handling
			switch (e.Action) {
			/*case NotifyCollectionChangedAction.Remove:
				if (e.OldStartingIndex == -1 || e.OldItems == null) {
					Reset();
					return;
				}

				// broken for rearranged
				object firstElement = e.OldItems[0];
				for (int i = 0; i < e.OldItems.Count; i++) {
					this.arranged.RemoveAt (e.OldStartingIndex);
				}

				RemoveFilteredRange (firstElement, e.OldStartingIndex, e.OldItems.Count);
				break;
				*/
			case NotifyCollectionChangedAction.Reset:
			default:
				Reset();
				break;
			}
		}

		private void OnOptionsPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SimpleCollectionViewOptions.Filter)) {
				FilterCore (isPureSubset: this.wasFilterNull);
				this.wasFilterNull = Options.Filter == null;
			} else
				Reset();
		}

		private bool MatchesFilter (Element element)
		{
			if (!IsFiltering)
				return true;

			return Options.Filter (element.Item);
		}

		private void FilterCore (bool notify = true, bool isPureSubset = false)
		{
			if (!IsFiltering) {
				if (this.arranged.Count == this.filtered.Count)
					return;
			}

			bool hadChildren = HasChildElements;

			HashSet<string> filteredOut = null;
			if (!isPureSubset) {
				filteredOut = new HashSet<string> (this.arranged.Keys);
				filteredOut.ExceptWith (this.filtered.Keys);
			}

			var toRemove = new List<string>();
			foreach (var kvp in this.filtered) {
				var childView = kvp.Value.ChildrenView;
				if ((childView != null && !childView.HasChildElements) || !MatchesFilter (kvp.Value)) {
					toRemove.Add (kvp.Key);
				}
			}

			if (!isPureSubset) {
				var toAdd = new List<string> (filteredOut.Count);
				foreach (string key in filteredOut) {
					Element e = this.arranged[key];
					if (!IsFiltering || Options.Filter (e.Item))
						toAdd.Add (key);
				}
				toAdd.Sort (Comparer);

				AddFiltered (toAdd, notify);
			}

			RemoveFiltered (toRemove, notify);

			if (hadChildren != HasChildElements)
				OnPropertyChanged (nameof(HasChildElements));
		}

		private void AddFiltered (string key, bool notify = true)
		{
			int sourceIndex = this.arranged.BinarySearch (key);
			AddFiltered (key, sourceIndex, notify);
		}

		private void AddFiltered (string key, int sourceIndex, bool notify = true)
		{
			Element e = this.arranged[sourceIndex];
			int maxMoved = this.arranged.Count - this.filtered.Count;
			int index = Math.Max (0, sourceIndex - maxMoved);

			bool added = false;
			if (this.filtered.Count != 0) {
				for (int i = index; i < this.filtered.Count; i++) {
					string currentKey = this.filtered.KeyAt (i);
					
					if (Comparer.Compare (currentKey, key) > 0) {
						index = i;
						this.filtered.Insert (i, key, this.arranged[i]);
						added = true;
						break;
					}
				}
			}

			if (!added) {
				index = this.filtered.Count;
				this.filtered.Add (key, e);
			}

			if (notify)
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, new[] { GetChangeItem (e, key) }, index));
		}

		private void AddFiltered (IReadOnlyList<string> keys, bool notify)
		{
			// TODO optimizations: single index lookup, batch up notices
			foreach (string key in keys)
				AddFiltered (key, notify);
		}

		private void RemoveFilteredRange (object firstElement, int sourceIndex, int count)
		{
			string key = Options.DisplaySelector (firstElement);

			// Filtered is always a subset of arranged so if we know where we were in the original
			// the most out of place we can be is the difference.
			int maxMoved = this.arranged.Count - this.filtered.Count;
			int index = (maxMoved > 0) ? this.filtered.BinarySearch (key, Math.Max (0, sourceIndex - maxMoved), maxMoved) : sourceIndex;

			List<object> items = new List<object> (count);
			for (int i = 0; i < count; i++) {
				Element e = this.filtered[index];
				items.Add (e.Item);
				this.filtered.RemoveAt (index);
			}

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, items, index));
		}

		private object GetChangeItem (Element e, string itemKey)
		{
			return Options.ChildOptions != null ? new KeyValuePair<string, SimpleCollectionView> (itemKey, e.ChildrenView) : e.Item;
		}

		private void RemoveFiltered (IReadOnlyList<string> keys, bool notify = true)
		{
			if (keys.Count == 0)
				return;

			List<Tuple<object, int>> oldItems = new List<Tuple<object, int>> (keys.Count);
			foreach (string key in keys) {
				int find = this.filtered.IndexOf (key);
				if (find == -1)
					continue;

				Element e = this.filtered[find];
				this.filtered.RemoveAt (find);

				object item = GetChangeItem (e, key);
				oldItems.Add (new Tuple<object, int> (item, find));
			}

			if (oldItems.Count > 0) {
				List<object> currentSet = new List<object>();
				int currentIndex = 0;
				for (int i = 0; i < oldItems.Count; i++) {
					var t = oldItems[i];
					if (t.Item2 != currentIndex) {
						if (notify && currentSet.Count > 0)
							OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, currentSet.ToArray(), currentIndex));

						currentSet.Clear();
						currentIndex = t.Item2;
					}

					currentSet.Add (t.Item1);
				}

				if (notify && currentSet.Count > 0)
					OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, currentSet, currentIndex));
			}
		}

		private void Reset (bool notify = true)
		{
			this.arranged.Clear();
			this.filtered.Clear();

			bool filtering = IsFiltering;

			foreach (var sourceItem in this.source.Cast<object>().Select (o => new { Item = o, Key = this.options.DisplaySelector (o) }).OrderBy (e => e.Key, Comparer)) {
				Element e = new Element {
					Item = sourceItem.Item
				};

				this.arranged.Add (sourceItem.Key, e);

				SimpleCollectionView childView = null;
				IEnumerable children = (Options.ChildrenSelector != null) ? this.options.ChildrenSelector (sourceItem.Item) : sourceItem.Item as IEnumerable;
				if (Options.ChildOptions != null) {
					if (children == null) {
						throw new InvalidOperationException ("ChildOptions specified, but element not enumerable or no selector");
					}

					childView = new SimpleCollectionView (children, Options.ChildOptions, this, sourceItem.Key, sourceItem.Item);
					e.ChildrenView = childView;
				}

				if (!filtering || this.options.Filter (e.Item))
					this.filtered.Add (sourceItem.Key, e);
			}

			if (notify)
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		private void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke (this, e);
			if (this.parent == null || (e.Action == NotifyCollectionChangedAction.Remove && HasChildElements) || e.Action == NotifyCollectionChangedAction.Move || e.Action == NotifyCollectionChangedAction.Replace)
				return;

			bool isVisible = HasChildElements;
			SimpleCollectionView parent = this.parent;
			SimpleCollectionView child = this;
			if (isVisible) {
				var parents = new Stack<Tuple<SimpleCollectionView, SimpleCollectionView>>();
				while (parent != null && !parent.filtered.ContainsKey (child.key)) {
					if (parent.Options.Filter != null && !parent.options.Filter (child.item))
						break;

					parents.Push (new Tuple<SimpleCollectionView, SimpleCollectionView> (parent, child));
					child = parent;
					parent = parent.parent;
				}

				while (parents.Count > 0) {
					var pair = parents.Pop();
					pair.Item1.AddFiltered (pair.Item2.key);
				}
			} else {
				var parents = new List<Tuple<SimpleCollectionView, SimpleCollectionView>>();
				while (parent != null && parent.filtered.ContainsKey (child.key) && !child.HasChildElements) {
					parents.Add (new Tuple<SimpleCollectionView, SimpleCollectionView> (parent, child));
					child = parent;
					parent = parent.parent;
				}

				for (int i  = 0; i < parents.Count; i++) {
					var pair = parents[i];
					pair.Item1.RemoveFiltered (new[] { pair.Item2.key });
				}
			}
		}
	}
}
