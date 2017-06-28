//
// MutableLookup.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2011 Eric Maupin (http://www.ermau.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Cadenza.Collections;

namespace Xamarin.PropertyEditing
{
	/// <summary>
	/// A mutable lookup implementing <see cref="ILookup{TKey,TElement}"/>
	/// </summary>
	/// <typeparam name="TKey">The lookup key.</typeparam>
	/// <typeparam name="TElement">The elements under each <typeparamref name="TKey"/>.</typeparam>
	internal class ObservableLookup<TKey, TElement>
		: IMutableLookup<TKey, TElement>, INotifyCollectionChanged, IReadOnlyList<IGroupingList<TKey, TElement>>
	{
		public ObservableLookup ()
			: this (EqualityComparer<TKey>.Default)
		{
		}

		public ObservableLookup (IEqualityComparer<TKey> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException ("comparer");

			this.groupings = new OrderedDictionary<TKey, ObservableGrouping<TKey, TElement>> (comparer);
			if (!typeof (TKey).IsValueType)
				this.nullGrouping = new ObservableGrouping<TKey, TElement> (default (TKey));
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public bool ReuseGroups
		{
			get { return this.reuseGroups; }
			set
			{
				if (this.reuseGroups == value)
					return;

				this.reuseGroups = value;
				if (value)
					this.oldGroups = new Dictionary<TKey, ObservableGrouping<TKey, TElement>> ();
				else
					this.oldGroups = null;
			}
		}

		/// <summary>
		/// Adds <paramref name="element"/> under the specified <paramref name="key"/>. <paramref name="key"/> does not need to exist.
		/// </summary>
		/// <param name="key">The key to add <paramref name="element"/> under.</param>
		/// <param name="element">The element to add.</param>
		public void Add (TKey key, TElement element)
		{
			ObservableGrouping<TKey, TElement> grouping;
			if (key == null) {
				grouping = nullGrouping;
				if (grouping.Count == 0)
					OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, grouping, 0));
			} else if (!this.groupings.TryGetValue (key, out grouping)) {
				if (!ReuseGroups || !this.oldGroups.TryRemove (key, out grouping))
					grouping = new ObservableGrouping<TKey, TElement> (key);
				
				this.groupings.Add (key, grouping);
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, (object)grouping, (this.groupings.Count - 1 + ((this.nullGrouping.Count > 0) ? 1 : 0))));
			}

			grouping.Add (element);
		}

		public void Add (TKey key, IEnumerable<TElement> elements)
		{
			if (elements == null)
				throw new ArgumentNullException ("elements");

			ObservableGrouping<TKey, TElement> grouping;
			if (key == null) {
				bool wasEmpty = this.nullGrouping.Count == 0;
				grouping = nullGrouping;
				grouping.AddRange (elements);
				if (wasEmpty)
					OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, grouping, 0));
			} else if (!this.groupings.TryGetValue (key, out grouping)) {
				if (!ReuseGroups || !this.oldGroups.TryRemove (key, out grouping))
					grouping = new ObservableGrouping<TKey, TElement> (key);

				grouping.AddRange (elements);

				this.groupings.Add (key, grouping);
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, (object)grouping, (this.groupings.Count - 1 + ((this.nullGrouping.Count > 0) ? 1 : 0))));
			}
		}

		public void Add (IGrouping<TKey, TElement> grouping)
		{
			if (grouping.Key == null) {
				bool wasEmpty = this.nullGrouping.Count == 0;
				this.nullGrouping.AddRange (grouping);
				if (wasEmpty)
					OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, grouping, 0));

				return;
			}
			
			ObservableGrouping<TKey, TElement> og;
			if (!ReuseGroups || !this.oldGroups.TryRemove (grouping.Key, out og)) {
				og = new ObservableGrouping<TKey, TElement> (grouping.Key);
			}

			og.AddRange (grouping);

			this.groupings.Add (grouping.Key, og);
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, (object)og, this.groupings.Count - 1));
		}

		public void Insert (int index, IGrouping<TKey, TElement> grouping)
		{
			ObservableGrouping<TKey, TElement> og;
			if (!ReuseGroups || !this.oldGroups.TryRemove (grouping.Key, out og)) {
				og = new ObservableGrouping<TKey, TElement> (grouping.Key);
			}

			og.AddRange (grouping);
			this.groupings.Insert (index, og.Key, og);
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, (object)og, index));
		}

		/// <summary>
		/// Removes <paramref name="element"/> from the <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key that <paramref name="element"/> is located under.</param>
		/// <param name="element">The element to remove from <paramref name="key"/>. </param>
		/// <returns><c>true</c> if <paramref name="key"/> and <paramref name="element"/> existed, <c>false</c> if not.</returns>
		public bool Remove (TKey key, TElement element)
		{
			ObservableGrouping<TKey, TElement> group;
			if (key == null) {
				group = this.nullGrouping;
			} else {
				if (!this.groupings.TryGetValue (key, out group))
					return false;
			}

			if (group.Remove (element)) {
				if (group.Count == 0) {
					Remove (key);
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Removes <paramref name="key"/> from the lookup.
		/// </summary>
		/// <param name="key">They to remove.</param>
		/// <returns><c>true</c> if <paramref name="key"/> existed.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
		public bool Remove (TKey key)
		{
			if (key == null) {
				bool removed = (this.nullGrouping.Count > 0);
				this.nullGrouping.Clear ();
				if (removed)
					OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, this.nullGrouping, 0));

				return removed;
			}

			int index = this.groupings.IndexOf (key);
			if (index >= 0) {
				var g = this.groupings[index];
				this.groupings.Remove (key);
				if (ReuseGroups) {
					g.QuietClear ();
					this.groupings.Add (key, g);
				}
				
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, g, index));
				return true;
			}

			return false;
		}

		public void Clear ()
		{
			if (this.nullGrouping != null)
				this.nullGrouping.Clear ();

			if (ReuseGroups) {
				foreach (var g in this.groupings.Values) {
					this.oldGroups.Add (g.Key, g);
					g.QuietClear ();
				}
			}

			this.groupings.Clear ();
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		public bool TryGetValues (TKey key, out IEnumerable<TElement> values)
		{
			values = null;

			if (key == null) {
				if (this.nullGrouping.Count != 0) {
					values = nullGrouping;
					return true;
				} else
					return false;
			}

			ObservableGrouping<TKey, TElement> grouping;
			if (!this.groupings.TryGetValue (key, out grouping))
				return false;

			values = grouping;
			return true;
		}

		#region ILookup Members
		/// <summary>
		/// Gets the number of groupings.
		/// </summary>
		public int Count
		{
			get { return this.groupings.Count + ((this.nullGrouping != null && this.nullGrouping.Count > 0) ? 1 : 0); }
		}

		IGroupingList<TKey, TElement> IReadOnlyList<IGroupingList<TKey, TElement>>.this[int index] => (IGroupingList<TKey, TElement>)this[index];

		/// <summary>
		/// Gets the elements for <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to get the elements for.</param>
		/// <returns>The elements under <paramref name="key"/>.</returns>
		public IEnumerable<TElement> this[TKey key]
		{
			get
			{
				if (key == null)
					return this.nullGrouping;

				ObservableGrouping<TKey, TElement> grouping;
				if (this.groupings.TryGetValue (key, out grouping))
					return grouping;

				return new TElement[0];
			}
		}

		public IEnumerable<TElement> this[int index]
		{
			get
			{
				if (index == this.groupings.Count) {
					return this.nullGrouping;
				}

				return this.groupings[index];
			}
		}

		/// <summary>
		/// Gets whether or not there's a grouping for <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to check for.</param>
		/// <returns><c>true</c> if <paramref name="key"/> is present.</returns>
		public bool Contains (TKey key)
		{
			if (key == null)
				return (this.nullGrouping.Count > 0);

			return this.groupings.ContainsKey (key);
		}

		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator ()
		{
			foreach (var g in this.groupings.Values)
				yield return g;

			if (this.nullGrouping != null && this.nullGrouping.Count > 0)
				yield return this.nullGrouping;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();
		}
		#endregion

		private readonly OrderedDictionary<TKey, ObservableGrouping<TKey, TElement>> groupings;
		private readonly ObservableGrouping<TKey, TElement> nullGrouping;

		private bool reuseGroups;
		private Dictionary<TKey, ObservableGrouping<TKey, TElement>> oldGroups;

		private void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke (this, e);
		}

		IEnumerator<IGroupingList<TKey, TElement>> IEnumerable<IGroupingList<TKey, TElement>>.GetEnumerator ()
		{
			foreach (var g in this.groupings.Values)
				yield return g;

			if (this.nullGrouping != null && this.nullGrouping.Count > 0)
				yield return this.nullGrouping;
		}
	}
}