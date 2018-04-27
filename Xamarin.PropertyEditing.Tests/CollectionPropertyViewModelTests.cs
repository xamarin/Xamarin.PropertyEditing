using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.Tests.MockControls;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class CollectionPropertyViewModelTests
	{
		[Test]
		public async Task AddType ()
		{
			TargetPlatform platform = new TargetPlatform (new MockEditorProvider());
			var obj = new {
				Collection = new ArrayList ()
			};
			var editor = new ReflectionObjectEditor (obj);
			
			var vm = new CollectionPropertyViewModel (platform, editor.Properties.First(), new[] { editor });

			var buttonType = GetTypeInfo (typeof(MockWpfButton));
			vm.TypeRequested += (o, e) => {
				e.SelectedType = buttonType;
			};

			await vm.AssignableTypes.Task;
			vm.SelectedType = vm.SuggestedTypes.First ();

			Assert.That (vm.SuggestedTypes, Contains.Item (buttonType));
		}

		[Test]
		public async Task AddTarget ()
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

			Assert.That (vm.Targets, Is.Not.Empty, "Adding a target failed");
			Assume.That (vm.Targets.Single (), Is.InstanceOf (typeof(MockWpfButton)));
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
			object target = vm.Targets.Skip (1).First ();
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
			object target = vm.Targets.First ();
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

			Assume.That (vm.Targets, Is.Not.Empty, "Adding a target failed");
			vm.SelectedTarget = vm.Targets.First ();
			object target = vm.SelectedTarget;

			Assume.That (vm.MoveUpCommand.CanExecute (null), Is.False);
			Assume.That (vm.MoveDownCommand.CanExecute (null), Is.False);

			bool downChanged = false;
			vm.MoveDownCommand.CanExecuteChanged += (sender, args) => downChanged = true;

			vm.AddTargetCommand.Execute (null);
			Assume.That (vm.SelectedTarget, Is.SameAs (target), "Selected target changed");

			Assert.That (vm.MoveUpCommand.CanExecute (null), Is.False);
			Assert.That (downChanged, Is.True);
			Assert.That (vm.MoveDownCommand.CanExecute (null), Is.True);
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
			Assume.That (vm.Targets.Single (), Is.InstanceOf (typeof(MockWpfButton)));

			Assert.That (vm.SelectedTarget, Is.SameAs (vm.Targets[0]), "Didn't auto-select first added");
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
			object newTarget = vm.Targets[1];
			object target = vm.Targets[2];
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

		private ITypeInfo GetTypeInfo (Type type)
		{
			var asm = new AssemblyInfo (type.Assembly.FullName, true);
			return new TypeInfo (asm, type.Namespace, type.Name);
		}
	}
}
