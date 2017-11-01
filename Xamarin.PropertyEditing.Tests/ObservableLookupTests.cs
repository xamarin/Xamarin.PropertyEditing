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
	}
}
