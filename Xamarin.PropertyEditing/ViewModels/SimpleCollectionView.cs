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
	{
		public event EventHandler OptionsChanged;

		public SimpleCollectionViewOptions ChildOptions
		{
			get { return this.childOptions; }
			set
			{
				if (this.childOptions == value)
					return;

				if (this.childOptions != null) {
					this.childOptions.OptionsChanged -= OnChildOptionsChanged;
				}

				this.childOptions = value;
				if (this.childOptions != null)
					this.childOptions.OptionsChanged += OnChildOptionsChanged;

				OnOptionsChanged();
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
				OnOptionsChanged();
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
				OnOptionsChanged();
			}
		}

		private Func<object, IEnumerable> childrenSelector;
		private Func<object, string> displaySelector;
		private SimpleCollectionViewOptions childOptions;

		private void OnChildOptionsChanged (object sender, EventArgs e)
		{
			OnOptionsChanged();
		}

		private void OnOptionsChanged()
		{
			OptionsChanged?.Invoke (this, EventArgs.Empty);
		}
	}

	internal class SimpleCollectionView
		: NotifyingObject, IEnumerable, INotifyCollectionChanged
	{
		public SimpleCollectionView (IEnumerable source, SimpleCollectionViewOptions options)
		{
			if (source == null)
				throw new ArgumentNullException (nameof (source));
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			this.source = source;
			Options = options;

			var changed = source as INotifyCollectionChanged;
			if (changed != null) {
				changed.CollectionChanged += OnSourceCollectionChanged;
			}
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public SimpleCollectionViewOptions Options
		{
			get { return this.options; }
			set
			{
				if (this.options == value)
					return;

				if (this.options != null)
					this.options.OptionsChanged -= OnOptionsPropertyChanged;

				this.options = value;
				if (this.options != null)
					this.options.OptionsChanged += OnOptionsPropertyChanged;

				Reset();
			}
		}

		public bool HasChildElements => (this.arranged.Count > 0);

		public bool IsFiltering => !String.IsNullOrWhiteSpace (FilterText);

		public string FilterText
		{
			get { return this.filterText; }
			set
			{
				if (this.filterText == value)
					return;
			
				Filter (value);
			}
		}

		public IEnumerator GetEnumerator ()
		{
			bool haveChildren = (Options.ChildOptions != null);

			foreach (var kvp in this.arranged) {
				if (!haveChildren)
					yield return kvp.Value.Item;
				else
					yield return new KeyValuePair<string, SimpleCollectionView> (kvp.Key, kvp.Value.ChildrenView);
			}
		}

		private struct Element
		{
			public object Item;
			public SimpleCollectionView ChildrenView;
		}

		private readonly OrderedDictionary<string, Element> arranged = new OrderedDictionary<string, Element>();
		private SimpleCollectionViewOptions options;
		private IEnumerable source;
		private string filterText;

		private void OnSourceCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
				default:
					Reset();
					break;
			}
		}

		private void OnOptionsPropertyChanged (object sender, EventArgs e)
		{
			Reset();
		}

		private bool MatchesFilter (Element element)
		{
			if (!IsFiltering)
				return true;

			string display = Options.DisplaySelector (element.Item);
			if (String.IsNullOrWhiteSpace (display))
				return false;
			if (display.StartsWith (FilterText))
				return true;

			return false;
		}

		private void Filter (string oldFilter, string newFilter, bool notify = true)
		{
			bool hadChildren = HasChildElements;

			if (IsFiltering && (String.IsNullOrWhiteSpace (oldFilter) || FilterText.StartsWith (oldFilter))) {
				var toRemove = new List<string>();
				foreach (var kvp in this.arranged) {
					var childView = kvp.Value.ChildrenView;
					if (childView != null) {
						childView.Filter (newFilter);
						if (!childView.HasChildElements) {
							toRemove.Add (kvp.Key);
						}

						continue;
					}

					if (!MatchesFilter (kvp.Value))
						toRemove.Add (kvp.Key);
				}

				Remove (toRemove, notify);
			} else {
				Reset();
			}

			if (hadChildren != HasChildElements)
				OnPropertyChanged (nameof(HasChildElements));
		}

		private void Remove (IReadOnlyList<string> keys, bool notify)
		{
			List<object> oldItems = new List<object> (keys.Count);
			foreach (string key in keys) {
				if (!this.arranged.TryRemove (key, out Element e))
					continue;

				oldItems.Add (Options.ChildOptions != null ? e.ChildrenView : e.Item);
			}

			if (oldItems.Count > 0) {
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, oldItems));
			}
		}

		private void Reset()
		{
			this.arranged.Clear();

			foreach (var element in this.source.Cast<object>().Select (o => new { Item = o, Key = options.DisplaySelector (o) }).OrderBy (e => e.Key)) {
				SimpleCollectionView childView = null;
				IEnumerable children = (Options.ChildrenSelector != null) ? options.ChildrenSelector (element.Item) : element.Item as IEnumerable;
				if (Options.ChildOptions != null) {
					if (children == null) {
						throw new InvalidOperationException ("ChildOptions specified, but element not enumerable or no selector");
					}

					childView = new SimpleCollectionView (children, Options.ChildOptions);
				}

				Element e = new Element {
					Item = element.Item,
					ChildrenView = childView
				};

				this.arranged.Add (element.Key, e);
			}

			if (IsFiltering)
				Filter (FilterText, FilterText, notify: false);

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		private void Filter (string newFilter)
		{
			string oldFilter = this.filterText;
			this.filterText = newFilter;

			Filter (oldFilter, newFilter);
			if (oldFilter != newFilter) {
				OnPropertyChanged (nameof (FilterText));
				if (String.IsNullOrWhiteSpace (oldFilter) != String.IsNullOrWhiteSpace (newFilter))
					OnPropertyChanged (nameof(IsFiltering));
			}
		}

		private void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke (this, e);
		}
	}
}
