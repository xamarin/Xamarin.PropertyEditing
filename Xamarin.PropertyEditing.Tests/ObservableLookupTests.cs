using System;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class ObservableLookupTests
	{
		[TestCase ("key")]
		[TestCase (null)]
		public void RemoveGroup (string key)
		{
			const string value = "value";
			var lookup = new ObservableLookup<string, string> ();
			lookup.Add (key, value);
			Assume.That (lookup.Contains (key), Is.True);
			Assume.That (lookup[key], Contains.Item (value));

			bool groupRemoved = false;
			lookup.CollectionChanged += (sender, args) => {
				if (args.Action == NotifyCollectionChangedAction.Remove) {
					var g = args.OldItems[0] as IGrouping<string, string>;
					if (g != null && g.Key == key)
						groupRemoved = true;
				}
			};

			var grouping = lookup[key];
			lookup.Remove (key);

			Assert.That (groupRemoved, Is.True);
			Assert.That (lookup, Does.Not.Contain (grouping));
		}

		[TestCase ("key")]
		[TestCase (null)] // The reality is that null as a key receives special treatment
		public void RemoveLastItemInGroup (string key)
		{
			const string value = "value";
			var lookup = new ObservableLookup<string, string> ();
			lookup.Add (key, value);
			Assume.That (lookup.Contains (key), Is.True);
			Assume.That (lookup[key], Contains.Item (value));

			bool itemRemoved = false, groupRemoved = false;
			lookup.CollectionChanged += (sender, args) => {
				if (args.Action == NotifyCollectionChangedAction.Remove) {
					var g = args.OldItems[0] as IGrouping<string, string>;
					if (g != null && g.Key == key)
						groupRemoved = true;
				}
			};

			var grouping = lookup[key];
			((INotifyCollectionChanged) grouping).CollectionChanged += (sender, args) => {
				if (args.Action == NotifyCollectionChangedAction.Remove) {
					if (args.OldItems[0] == value)
						itemRemoved = true;
				}
			};

			lookup.Remove (key, value);

			Assert.That (itemRemoved, Is.True);
			Assert.That (groupRemoved, Is.True);
			Assert.That (grouping, Does.Not.Contains (value));
			Assert.That (lookup, Does.Not.Contain (grouping));
		}

		[TestCase ("group")]
		[TestCase (null)]
		[Description ("If removing the last item from a group removes the group, adding a group with no items shouldn't add it")]
		public void AddingEmptyElementsIsIgnored (string groupName)
		{
			var lookup = new ObservableLookup<string, string> ();
			bool changed = false;
			lookup.CollectionChanged += (sender, args) => {
				changed = true;
			};
			lookup.Add (groupName, Enumerable.Empty<string> ());

			Assert.That (lookup, Is.Empty);
			Assert.That (changed, Is.False);
		}

		[TestCase ("group")]
		[TestCase (null)]
		[Description ("If removing the last item from a group removes the group, adding a group with no items shouldn't add it")]
		public void AddingEmptyGroupingIsIgnored (string groupName)
		{
			var lookup = new ObservableLookup<string, string> ();
			bool changed = false;
			lookup.CollectionChanged += (sender, args) => {
				changed = true;
			};
			lookup.Add (new ObservableGrouping<string, string> (groupName));

			Assert.That (lookup, Is.Empty);
			Assert.That (changed, Is.False);
		}

		[TestCase ("group")]
		[TestCase (null)]
		[Description ("If removing the last item from a group removes the group, adding a group with no items shouldn't add it")]
		public void InsertingEmptyIsIgnored (string groupName)
		{
			var lookup = new ObservableLookup<string, string> ();
			bool changed = false;
			lookup.CollectionChanged += (sender, args) => {
				changed = true;
			};
			lookup.Insert (0, new ObservableGrouping<string, string> (groupName));

			Assert.That (lookup, Is.Empty);
			Assert.That (changed, Is.False);
		}

		[Test]
		public void RemoveLastElementFromNullGroup ()
		{
			const string element = "test";
			var lookup = new ObservableLookup<string, string> ();
			lookup.Add (null, element);

			int groupChanged = 0;
			var g = lookup[null];
			((INotifyCollectionChanged) g).CollectionChanged += (sender, args) => {
				groupChanged++;
				Assert.That (args.Action, Is.EqualTo (NotifyCollectionChangedAction.Remove));
				Assert.That (args.OldItems[0], Is.SameAs (element));
			};

			int lookupChanged = 0;
			lookup.CollectionChanged += (sender, args) => {
				lookupChanged++;
				Assert.That (args.Action, Is.EqualTo (NotifyCollectionChangedAction.Remove));
				Assert.That (args.OldItems[0], Is.SameAs (g));
			};

			lookup.Remove (null, element);
			Assert.That (lookupChanged, Is.EqualTo (1));
			Assert.That (groupChanged, Is.EqualTo (1));
		}
	}
}
