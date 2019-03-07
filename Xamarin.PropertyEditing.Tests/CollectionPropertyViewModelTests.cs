using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Properties;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.Tests.MockControls;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class CollectionPropertyViewModelTests
	{
		private TestContext syncContext;

		[SetUp]
		public void Setup ()
		{
			this.syncContext = new TestContext ();
			SynchronizationContext.SetSynchronizationContext (this.syncContext);
		}

		[TearDown]
		public void TearDown ()
		{
			SynchronizationContext.SetSynchronizationContext (null);
			this.syncContext.ThrowPendingExceptions ();
		}

		[Test]
		public void AddType ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());
			var obj = new {
				Collection = new ArrayList ()
			};
			var editor = new ReflectionObjectEditor (obj);
			
			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First(), new[] { editor });

			var buttonType = GetTypeInfo (typeof(MockWpfButton));
			vm.TypeRequested += (o, e) => {
				e.SelectedType = Task.FromResult (buttonType);
			};

			vm.AssignableTypes.Task.Wait();
			vm.SelectedType = vm.SuggestedTypes.First ();

			Assert.That (vm.SuggestedTypes, Contains.Item (buttonType));
		}

		[Test]
		public void AddTarget ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);
			
			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First(), new[] { editor });
			vm.AssignableTypes.Task.Wait();

			vm.SelectedType = GetTypeInfo (typeof(MockWpfButton));
			vm.AddTargetCommand.Execute (null);

			Assert.That (vm.Targets, Is.Not.Empty, "Adding a target failed");
			Assume.That (vm.Targets.Single ().Item, Is.InstanceOf (typeof(MockWpfButton)));
		}

		[Test]
		public async Task AddTargetCanAdd ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider ());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);

			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First (), new[] { editor });
			await vm.AssignableTypes.Task;

			Assume.That (vm.SelectedType, Is.Null);
			Assert.That (vm.AddTargetCommand.CanExecute (null), Is.False);

			bool changed = false;
			vm.AddTargetCommand.CanExecuteChanged += (o, e) => { changed = true; };
			vm.SelectedType = GetTypeInfo (typeof (MockWpfButton));
			Assert.That (vm.AddTargetCommand.CanExecute (null), Is.True);
			Assert.That (changed, Is.True, "CanExecuteChanged did not fire");

			changed = false;
			vm.SelectedType = null;
			Assert.That (vm.AddTargetCommand.CanExecute (null), Is.False);
			Assert.That (changed, Is.True, "CanExecuteChanged did not fire");
		}

		[Test]
		[Description ("When selecting a new type, if it's canceled the suggested type should return to something else")]
		public void AddTypeCanceled ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider ());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);

			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First (), new[] { editor });
			vm.AssignableTypes.Task.Wait();

			var buttonType = GetTypeInfo (typeof (MockWpfButton));
			vm.TypeRequested += (o, e) => {
				e.SelectedType = Task.FromResult (buttonType);
			};

			vm.SelectedType = vm.SuggestedTypes.Last ();
			Assume.That (vm.SuggestedTypes, Contains.Item (buttonType));

			buttonType = null;
			vm.SelectedType = vm.SuggestedTypes.Last ();
			Assert.That (vm.SelectedType, Is.EqualTo (vm.SuggestedTypes.First ()));
		}

		[Test]
		public async Task RemoveTarget ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);
			
			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First(), new[] { editor });
			await vm.AssignableTypes.Task;

			vm.SelectedType = GetTypeInfo (typeof(MockWpfButton));
			vm.AddTargetCommand.Execute (null);

			Assume.That (vm.Targets, Is.Not.Empty);
			vm.SelectedTarget = vm.Targets.First ();

			Assert.That (vm.RemoveTargetCommand.CanExecute (null), Is.True);
			vm.RemoveTargetCommand.Execute (null);
			Assert.That (vm.Targets, Is.Empty);
		}

		[Test]
		public async Task RemoveEditor ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider ());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);

			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First (), new[] { editor });
			await vm.AssignableTypes.Task;

			vm.Editors.Remove (editor);
			await vm.AssignableTypes.Task;

			Assert.That (vm.AssignableTypes.Value, Is.Empty);
			Assert.That (vm.SuggestedTypes, Is.Empty);
			Assert.That (vm.SelectedType, Is.Null);
		}

		[Test]
		public async Task MoveUpCommand ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);
			
			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First(), new[] { editor });
			await vm.AssignableTypes.Task;

			vm.SelectedType = GetTypeInfo (typeof(MockWpfButton));
			vm.AddTargetCommand.Execute (null);
			vm.AddTargetCommand.Execute (null);
			Assume.That (vm.Targets.Count, Is.EqualTo (2));

			Assume.That (vm.Targets, Is.Not.Empty, "Adding a target failed");
			var target = vm.Targets.Skip (1).First ();
			vm.SelectedTarget = target;

			Assume.That (vm.Targets[0], Is.Not.SameAs (target));
			Assert.That (vm.MoveUpCommand.CanExecute (null), Is.True);
			vm.MoveUpCommand.Execute (null);
			Assert.That (vm.MoveUpCommand.CanExecute (null), Is.False);
			Assume.That (vm.SelectedTarget, Is.SameAs (target));
			Assert.That (vm.Targets[0], Is.SameAs (target));
		}

		[Test]
		public async Task MoveDownCommand ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);
			
			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First(), new[] { editor });
			await vm.AssignableTypes.Task;

			vm.SelectedType = GetTypeInfo (typeof(MockWpfButton));
			vm.AddTargetCommand.Execute (null);
			vm.AddTargetCommand.Execute (null);
			Assume.That (vm.Targets.Count, Is.EqualTo (2));

			Assume.That (vm.Targets, Is.Not.Empty, "Adding a target failed");
			var target = vm.Targets.First ();
			vm.SelectedTarget = target;

			Assume.That (vm.Targets[1], Is.Not.SameAs (target));
			Assert.That (vm.MoveDownCommand.CanExecute (null), Is.True);
			vm.MoveDownCommand.Execute (null);
			Assert.That (vm.MoveDownCommand.CanExecute (null), Is.False);
			Assume.That (vm.SelectedTarget, Is.SameAs (target));
			Assert.That (vm.Targets[1], Is.SameAs (target));
		}

		[Test]
		public async Task ReorderEnablesOnItemAdd ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);
			
			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First(), new[] { editor });
			await vm.AssignableTypes.Task;

			vm.SelectedType = GetTypeInfo (typeof(MockWpfButton));
			vm.AddTargetCommand.Execute (null);

			Assume.That (vm.Targets.Count, Is.EqualTo (1));
			Assume.That (vm.Targets, Is.Not.Empty, "Adding a target failed");
			vm.SelectedTarget = vm.Targets.First ();

			Assume.That (vm.MoveUpCommand.CanExecute (null), Is.False);
			Assume.That (vm.MoveDownCommand.CanExecute (null), Is.False);

			bool upChanged = false;
			vm.MoveUpCommand.CanExecuteChanged += (sender, args) => upChanged = true;

			vm.AddTargetCommand.Execute (null);
			Assume.That (vm.SelectedTarget, Is.SameAs (vm.Targets[vm.Targets.Count-1]), "Selected target not expected");

			Assert.That (upChanged, Is.True);
			Assert.That (vm.MoveUpCommand.CanExecute (null), Is.True);
			Assert.That (vm.MoveDownCommand.CanExecute (null), Is.False);
		}

		[Test]
		public async Task SelectOnAdd ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);
			
			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First(), new[] { editor });
			await vm.AssignableTypes.Task;

			vm.SelectedType = GetTypeInfo (typeof(MockWpfButton));
			vm.AddTargetCommand.Execute (null);

			Assume.That (vm.Targets, Is.Not.Empty, "Adding a target failed");
			Assume.That (vm.Targets.Single ().Item, Is.InstanceOf (typeof(MockWpfButton)));

			Assert.That (vm.SelectedTarget, Is.SameAs (vm.Targets[0]), "Didn't auto-select first added");
		}

		[Test]
		[Description ("The added item should be inserted after a selected item")]
		public async Task AddAfterSelected ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);
			
			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First(), new[] { editor });
			await vm.AssignableTypes.Task;

			vm.SelectedType = GetTypeInfo (typeof(MockWpfButton));
			vm.AddTargetCommand.Execute (null);
			vm.AddTargetCommand.Execute (null);
			vm.AddTargetCommand.Execute (null);
			Assume.That (vm.Targets.Count, Is.EqualTo (3), "Dummy items were not added");

			bool changed = false;
			var incc = (INotifyCollectionChanged) vm.Targets;
			incc.CollectionChanged += (sender, args) => {
				changed = true;
				Assert.That (args.Action, Is.EqualTo (NotifyCollectionChangedAction.Add));
				Assert.That (args.NewStartingIndex, Is.EqualTo (2));
			};

			vm.SelectedTarget = vm.Targets[1];
			vm.AddTargetCommand.Execute (null);

			Assert.That (changed, Is.True);
			Assert.That (vm.Targets[1].Row, Is.EqualTo (1));
			Assert.That (vm.Targets[2].Row, Is.EqualTo (2));
			Assert.That (vm.Targets[3].Row, Is.EqualTo (3));
		}

		[Test]
		public async Task SelectPreviousOnRemove ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());

			var obj = new {
				Collection = new ArrayList ()
			};

			var editor = new ReflectionObjectEditor (obj);
			
			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First(), new[] { editor });
			await vm.AssignableTypes.Task;

			vm.SelectedType = GetTypeInfo (typeof(MockWpfButton));
			vm.AddTargetCommand.Execute (null);
			vm.AddTargetCommand.Execute (null);
			vm.AddTargetCommand.Execute (null);

			Assume.That (vm.Targets, Is.Not.Empty);
			var newTarget = vm.Targets[1];
			var target = vm.Targets[2];
			vm.SelectedTarget = target;

			Assume.That (vm.RemoveTargetCommand.CanExecute (null), Is.True);
			vm.RemoveTargetCommand.Execute (null);
			Assume.That (vm.Targets, Does.Not.Contain (target));

			Assert.That (vm.SelectedTarget, Is.SameAs (newTarget));

			vm.RemoveTargetCommand.Execute (null);
			vm.RemoveTargetCommand.Execute (null);

			Assert.That (vm.SelectedTarget, Is.Null);
		}

		[Test]
		[Description ("If there's a suggested type (not other type..), it should be auto-selected to start")]
		public async Task SuggestedSelected ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());

			var collectionProperty = new Mock<IPropertyInfo> ();
			collectionProperty.SetupGet (pi => pi.Type).Returns (typeof(IList));

			ITypeInfo objType = GetTypeInfo (typeof(object));

			var editor = new Mock<IObjectEditor> ();
			editor.Setup (e => e.GetAssignableTypesAsync (collectionProperty.Object, true)).ReturnsAsync (new AssignableTypesResult (new[] { objType }, new[] { objType }));
			editor.Setup (e => e.Properties).Returns (new[] { collectionProperty.Object });

			var vm = new CollectionPropertyViewModel (platform, collectionProperty.Object, new[] { editor.Object });
			await vm.AssignableTypes.Task;

			Assert.That (vm.SelectedType, Is.EqualTo (objType));
		}

		[Test]
		[Description ("If the value updates and doesn't contain the selected target, it should clear if there are no items")]
		public async Task SelectedTargetClearsWhenNotPresent ()
		{
			var obj = new TestClass {
				Collection = new ArrayList ()
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (pi => pi.Type).Returns (typeof (IList));
			property.SetupGet (pi => pi.Name).Returns (nameof (TestClass.Collection));
			property.SetupGet (pi => pi.CanWrite).Returns (true);

			var editor = new Mock<IObjectEditor> ();
			editor.SetTarget (obj);
			editor.Setup (e => e.GetAssignableTypesAsync (property.Object, true))
				.ReturnsAsync (new AssignableTypesResult (new[] { typeof (MockSampleControl).ToTypeInfo () }));
			editor.Setup (e => e.Properties).Returns (new[] { property.Object });

			ValueInfo<IList> valueInfo = new ValueInfo<IList> {
				Value = default (IList),
				Source = ValueSource.Default
			};

			editor.Setup (oe => oe.SetValueAsync (property.Object, It.IsAny<ValueInfo<IList>> (), null))
				.Callback<IPropertyInfo, ValueInfo<IList>, PropertyVariation> ((p, vi, v) => {
					valueInfo = vi;
					editor.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (property.Object));
				})
				.Returns (Task.FromResult (true));
			editor.Setup (oe => oe.GetValueAsync<IList> (property.Object, null)).ReturnsAsync (() => valueInfo);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);
			provider.Setup (ep => ep.GetObjectEditorAsync (It.IsAny<MockSampleControl> ()))
				.ReturnsAsync ((MockSampleControl sample) => new MockObjectEditor (sample));
			provider.Setup (ep => ep.CreateObjectAsync (It.IsAny<ITypeInfo> ()))
				.ReturnsAsync ((ITypeInfo type) => new MockSampleControl ());

			var vm = new CollectionPropertyViewModel (new TargetPlatform (provider.Object), property.Object, new[] { editor.Object });
			await vm.AssignableTypes.Task;

			vm.SelectedType = GetTypeInfo (typeof (MockSampleControl));
			vm.AddTargetCommand.Execute (null);

			Assume.That (vm.Targets, Is.Not.Empty, "Adding a target failed");
			var selected = vm.Targets.Single ().Item;
			Assume.That (selected, Is.InstanceOf (typeof (MockSampleControl)));
			Assume.That (vm.SelectedTarget?.Item, Is.SameAs (selected), "Added target wasn't selected");

			editor.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (property.Object, null));

			Assert.That (vm.SelectedTarget, Is.Null, "SelectedTarget didn't clear when it was no longer present");
		}

		[Test]
		[Description ("If the value updates and doesn't contain the selected target, it should go to the first item")]
		public async Task SelectedTargetGoesToFirstItemWhenNotPresent ()
		{
			var selected = new MockSampleControl ();
			var obj = new TestClass {
				Collection = new ArrayList {
					new MockSampleControl(),
					selected
				}
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (pi => pi.Type).Returns (typeof (IList));
			property.SetupGet (pi => pi.Name).Returns (nameof (TestClass.Collection));
			property.SetupGet (pi => pi.CanWrite).Returns (true);

			var editor = new Mock<IObjectEditor> ();
			editor.SetTarget (obj);
			editor.Setup (e => e.GetAssignableTypesAsync (property.Object, true))
				.ReturnsAsync (new AssignableTypesResult (new[] { typeof (MockSampleControl).ToTypeInfo () }));
			editor.Setup (e => e.Properties).Returns (new[] { property.Object });

			ValueInfo<IList> valueInfo = new ValueInfo<IList> {
				Value = default (IList),
				Source = ValueSource.Default
			};

			editor.Setup (oe => oe.SetValueAsync (property.Object, It.IsAny<ValueInfo<IList>> (), null))
				.Callback<IPropertyInfo, ValueInfo<IList>, PropertyVariation> ((p, vi, v) => {
					valueInfo = vi;
					editor.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (property.Object));
				})
				.Returns (Task.FromResult (true));
			editor.Setup (oe => oe.GetValueAsync<IList> (property.Object, null)).ReturnsAsync (() => valueInfo);
			await editor.Object.SetValueAsync (property.Object, new ValueInfo<IList> { Value = obj.Collection, Source = ValueSource.Local });

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);
			provider.Setup (ep => ep.GetObjectEditorAsync (It.IsAny<MockSampleControl> ()))
				.ReturnsAsync ((MockSampleControl sample) => new MockObjectEditor (sample));

			var vm = new CollectionPropertyViewModel (new TargetPlatform (provider.Object), property.Object, new[] { editor.Object });
			await vm.AssignableTypes.Task;

			vm.SelectedTarget = vm.Targets.FirstOrDefault (item => item.Item == selected);

			obj.Collection.Remove (selected);
			editor.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (property.Object, null));

			Assert.That (vm.SelectedTarget, Is.Not.Null, "SelectedTarget cleared but there was another item");
			Assert.That (vm.SelectedTarget?.Item, Is.Not.SameAs (selected), "SelectedTarget didn't change to remaining item when collection changed");
		}

		[Test]
		public async Task SelectedTargetHoldsAfterCollectionChange ()
		{
			var selected = new MockSampleControl ();
			var obj = new TestClass {
				Collection = new ArrayList {
					new MockSampleControl(),
					selected
				}
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (pi => pi.Type).Returns (typeof (IList));
			property.SetupGet (pi => pi.Name).Returns (nameof(TestClass.Collection));
			property.SetupGet (pi => pi.CanWrite).Returns (true);

			var editor = new Mock<IObjectEditor> ();
			editor.SetTarget (obj);
			editor.Setup (e => e.GetAssignableTypesAsync (property.Object, true))
				.ReturnsAsync (new AssignableTypesResult (new[] { typeof (MockSampleControl).ToTypeInfo () }));
			editor.Setup (e => e.Properties).Returns (new[] { property.Object });

			ValueInfo<IList> valueInfo = new ValueInfo<IList> {
				Value = default(IList),
				Source = ValueSource.Default
			};

			editor.Setup (oe => oe.SetValueAsync (property.Object, It.IsAny<ValueInfo<IList>> (), null))
				.Callback<IPropertyInfo, ValueInfo<IList>, PropertyVariation> ((p, vi, v) => {
					valueInfo = vi;
					editor.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (property.Object));
				})
				.Returns (Task.FromResult (true));
			editor.Setup (oe => oe.GetValueAsync<IList> (property.Object, null)).ReturnsAsync (() => valueInfo);

			await editor.Object.SetValueAsync (property.Object, new ValueInfo<IList> { Value = obj.Collection, Source = ValueSource.Local });

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);
			provider.Setup (ep => ep.GetObjectEditorAsync (It.IsAny<MockSampleControl> ()))
				.ReturnsAsync ((MockSampleControl sample) => new MockObjectEditor (sample));

			var vm = new CollectionPropertyViewModel (new TargetPlatform (provider.Object), property.Object, new[] { editor.Object });
			await vm.AssignableTypes.Task;

			vm.SelectedTarget = vm.Targets.FirstOrDefault (item => item.Item == selected);

			obj.Collection.Insert (0, new MockSampleControl ());
			editor.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (property.Object, null));

			Assert.That (vm.SelectedTarget?.Item, Is.SameAs (selected), "SelectedTarget didn't hold when collection changed");
		}

		private class TestClass
		{
			public ArrayList Collection
			{
				get;
				set;
			}
		}

		private ITypeInfo GetTypeInfo (Type type)
		{
			var asm = new AssemblyInfo (type.Assembly.FullName, true);
			return new TypeInfo (asm, type.Namespace, type.Name);
		}
	}
}
