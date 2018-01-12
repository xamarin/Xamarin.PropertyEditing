using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	public class SimpleCollectionViewTests
	{
		[Test]
		public void Source()
		{
			TestNode[] nodes = new TestNode[] {
				new TestNode { Key = "key" },
				new TestNode { Key = "key2" }
			};

			var view = new SimpleCollectionView (nodes, new SimpleCollectionViewOptions {
				DisplaySelector = TestNodeDisplaySelector
			});

			Assert.That (view, Contains.Item (nodes[0]));
			Assert.That (view, Contains.Item (nodes[1]));
		}

		[Test]
		public void PassThroughINCC()
		{
			var nodes = new ObservableCollection<TestNode> {
				new TestNode { Key = "key" },
				new TestNode { Key = "key2" }
			};

			var view = new SimpleCollectionView (nodes, new SimpleCollectionViewOptions {
				DisplaySelector = TestNodeDisplaySelector
			});
			bool changed = false;
			NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset;
			view.CollectionChanged += (sender, e) => {
				changed = true;
				action = e.Action;
			};

			nodes.Add (new TestNode { Key = "key3" });
			Assert.That (changed, Is.True);
			Assert.That (action, Is.EqualTo (NotifyCollectionChangedAction.Add).Or.EqualTo (NotifyCollectionChangedAction.Reset));
			changed = false;

			nodes.RemoveAt (0);
			Assert.That (changed, Is.True);
			Assert.That (action, Is.EqualTo (NotifyCollectionChangedAction.Remove).Or.EqualTo (NotifyCollectionChangedAction.Reset));
			changed = false;

			nodes.Move (0, 1);
			Assert.That (changed, Is.True);
			Assert.That (action, Is.EqualTo (NotifyCollectionChangedAction.Move).Or.EqualTo (NotifyCollectionChangedAction.Reset));
			changed = false;

			nodes[0] = new TestNode { Key = "replace" };
			Assert.That (changed, Is.True);
			Assert.That (action, Is.EqualTo (NotifyCollectionChangedAction.Replace).Or.EqualTo (NotifyCollectionChangedAction.Reset));
			changed = false;

			nodes.Clear();
			Assert.That (changed, Is.True);
			Assert.That (action, Is.EqualTo (NotifyCollectionChangedAction.Reset));
			changed = false;
		}

		[Test]
		public void Filter()
		{
			TestNode[] nodes = new TestNode[] {
				new TestNode { Key = "key" },
				new TestNode { Key = "key2" }
			};

			var view = new SimpleCollectionView (nodes, new SimpleCollectionViewOptions {
				DisplaySelector = TestNodeDisplaySelector
			});

			Assume.That (view, Contains.Item (nodes[0]));
			Assume.That (view, Contains.Item (nodes[1]));

			bool changed = false;
			view.CollectionChanged += (sender, e) => {
				changed = true;
				Assert.That (e.Action, Is.EqualTo (NotifyCollectionChangedAction.Remove));
				Assert.That (e.OldItems, Contains.Item (nodes[0]));
				Assert.That (e.OldStartingIndex, Is.EqualTo (0));
			};

			view.Filter (o => ((TestNode)o).Key.StartsWith ("key"), isSuperset: false);
			Assert.That (changed, Is.False);
			Assert.That (view, Contains.Item (nodes[0]));
			Assert.That (view, Contains.Item (nodes[1]));

			view.Filter (o => ((TestNode)o).Key.StartsWith ("key2"), isSuperset: false);
			Assert.That (changed, Is.True);
			Assert.That (view, Does.Not.Contain (nodes[0]));
			Assert.That (view, Contains.Item (nodes[1]));
		}

		[Test]
		public void FilterGroupedIndexes()
		{
			TestNode[] nodes = new TestNode[] {
				new TestNode ("A"),
				new TestNode ("Ab") { Flag = true },
				new TestNode ("Ac") { Flag = true },
				new TestNode ("Ad"),
				new TestNode ("Ae") { Flag = true },
				new TestNode ("Af") { Flag = true }
			};

			var view = new SimpleCollectionView (nodes, new SimpleCollectionViewOptions {
				DisplaySelector = TestNodeDisplaySelector
			});

			Assume.That (view.Cast<object>().Count(), Is.EqualTo (6));

			List<NotifyCollectionChangedEventArgs> args = new List<NotifyCollectionChangedEventArgs>();
			view.CollectionChanged += (sender, e) => {
				args.Add (e);
			};

			view.Filter (o => !((TestNode)o).Flag, isSuperset: false);

			Assert.That (view, Contains.Item (nodes[0]));
			Assert.That (view, Contains.Item (nodes[3]));
			Assert.That (view.Cast<object>().Count(), Is.EqualTo (2));

			Assert.That (args.Count, Is.EqualTo (2));
			Assert.That (args[0].OldItems, Contains.Item (nodes[1]));
			Assert.That (args[0].OldItems, Contains.Item (nodes[2]));
			Assert.That (args[0].OldStartingIndex, Is.EqualTo (1));
			Assert.That (args[1].OldItems, Contains.Item (nodes[4]));
			Assert.That (args[1].OldItems, Contains.Item (nodes[5]));
			Assert.That (args[1].OldStartingIndex, Is.EqualTo (2));
		}

		[Test]
		public void InOrder()
		{
			TestNode[] nodes = new TestNode[] {
				new TestNode { Key = "Z" },
				new TestNode { Key = "C" },
				new TestNode { Key = "E" },
				new TestNode { Key = "B" },
				new TestNode { Key = "A" }
			};

			var view = new SimpleCollectionView (nodes, new SimpleCollectionViewOptions {
				DisplaySelector = TestNodeDisplaySelector
			});

			Assume.That (view, Contains.Item (nodes[0]));
			Assume.That (view, Contains.Item (nodes[1]));
			Assume.That (view, Contains.Item (nodes[2]));
			Assume.That (view, Contains.Item (nodes[3]));
			Assume.That (view, Contains.Item (nodes[4]));

			Assert.That (view.Cast<object>().ElementAt (0), Is.EqualTo (nodes[4]));
			Assert.That (view.Cast<object>().ElementAt (1), Is.EqualTo (nodes[3]));
			Assert.That (view.Cast<object>().ElementAt (2), Is.EqualTo (nodes[1]));
			Assert.That (view.Cast<object>().ElementAt (3), Is.EqualTo (nodes[2]));
			Assert.That (view.Cast<object>().ElementAt (4), Is.EqualTo (nodes[0]));
		}

		[Test]
		public void AddedInOrder()
		{
			var nodes = new ObservableCollection<TestNode> {
				new TestNode { Key = "Z" },
				new TestNode { Key = "C" },
				// F
				new TestNode { Key = "E" },
				new TestNode { Key = "B" },
				new TestNode { Key = "A" }
			};

			var view = new SimpleCollectionView (nodes, new SimpleCollectionViewOptions {
				DisplaySelector = TestNodeDisplaySelector
			});

			Assume.That (view.Cast<object>().ElementAt (0), Is.EqualTo (nodes[4]));
			Assume.That (view.Cast<object>().ElementAt (1), Is.EqualTo (nodes[3]));
			Assume.That (view.Cast<object>().ElementAt (2), Is.EqualTo (nodes[1]));
			Assume.That (view.Cast<object>().ElementAt (3), Is.EqualTo (nodes[2]));
			Assume.That (view.Cast<object>().ElementAt (4), Is.EqualTo (nodes[0]));

			bool changed = false;
			NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset;
			view.CollectionChanged += (sender, e) => {
				changed = true;
				action = e.Action;
			};

			nodes.Insert (2, new TestNode ("F"));
			Assert.That (action, Is.EqualTo (NotifyCollectionChangedAction.Add).Or.EqualTo (NotifyCollectionChangedAction.Reset));

			Assert.That (changed, Is.True);
			Assert.That (view.Cast<object>().ElementAt (0), Is.EqualTo (nodes[5]));
			Assert.That (view.Cast<object>().ElementAt (1), Is.EqualTo (nodes[4]));
			Assert.That (view.Cast<object>().ElementAt (2), Is.EqualTo (nodes[1]));
			Assert.That (view.Cast<object>().ElementAt (3), Is.EqualTo (nodes[3]));
			Assert.That (view.Cast<object>().ElementAt (4), Is.EqualTo (nodes[2]));
			Assert.That (view.Cast<object>().ElementAt (5), Is.EqualTo (nodes[0]));
		}

		[Test]
		public void Grouping()
		{
			TestNode[] nodes = new TestNode[] {
				new TestNode {
					Key = "Group",
					Children = new List<TestNode> {
						new TestNode ("Child")
					}
				}
			};

			var view = new SimpleCollectionView (nodes, new SimpleCollectionViewOptions {
					DisplaySelector = TestNodeDisplaySelector,
					ChildrenSelector = TestNodeChildrenSelector,
					ChildOptions = new SimpleCollectionViewOptions {
						DisplaySelector = TestNodeDisplaySelector
					}
				});

			Assert.That (view, Is.Not.Empty);
			var kvp = (KeyValuePair<string, SimpleCollectionView>)view.Cast<object>().Single();
			Assert.That (kvp.Value, Is.Not.Empty);
			Assert.That (kvp.Value, Contains.Item (nodes[0].Children[0]));
		}

		[Test]
		public void FilterCategory()
		{
			TestNode[] nodes = new TestNode[] {
				new TestNode {
					Key = "Group",
					Children = new List<TestNode> {
						new TestNode ("Child")
					}
				}
			};

			var view = new SimpleCollectionView (nodes, new SimpleCollectionViewOptions {
					DisplaySelector = TestNodeDisplaySelector,
					ChildrenSelector = TestNodeChildrenSelector,
					ChildOptions = new SimpleCollectionViewOptions {
						DisplaySelector = TestNodeDisplaySelector
					}
				});

			Assume.That (view, Is.Not.Empty);

			view.Filter (o => ((TestNode)o).Key.StartsWith ("Chi"), isSuperset: false);

			Assert.That (view, Is.Not.Empty);
		}

		[Test]
		[Description ("Even if a filter matches the group name, if it's empty it should not be displayed")]
		public void FilterCategoryEmpty()
		{
			TestNode[] nodes = new TestNode[] {
				new TestNode {
					Key = "Group",
					Children = new List<TestNode> {
						new TestNode ("Child")
					}
				}
			};

			var view = new SimpleCollectionView (nodes, new SimpleCollectionViewOptions {
					DisplaySelector = TestNodeDisplaySelector,
					ChildrenSelector = TestNodeChildrenSelector,
					ChildOptions = new SimpleCollectionViewOptions {
						DisplaySelector = TestNodeDisplaySelector
					}
				});

			Assume.That (view, Is.Not.Empty);

			view.Filter (o => ((TestNode)o).Key.StartsWith ("Group"), isSuperset: false);

			Assert.That (view, Is.Empty);
		}

		private IEnumerable TestNodeChildrenSelector (object o)
		{
			return ((TestNode)o).Children;
		}

		private string TestNodeDisplaySelector (object o)
		{
			return ((TestNode)o).Key;
		}

		private class TestNode
		{
			public TestNode()
			{

			}

			public TestNode (string key)
			{
				Key = key;
			}

			public string Key;
			public List<TestNode> Children;
			public bool Flag;
		}
	}
}
