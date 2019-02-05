using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class ObjectPropertyViewModelTests
	{
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
			syncContext.ThrowPendingExceptions ();
		}

		[Test]
		public void TypeAssumedWhenSingleAssignableType ()
		{
			object value = new object();

			var p = CreatePropertyMock ("prop");

			var childsubInfo = GetTypeInfo (typeof(SubChildClass));
			var editor = new MockObjectEditor (new[] { p.Object }, new Dictionary<IPropertyInfo, IReadOnlyList<ITypeInfo>> {
				{ p.Object, new[] { childsubInfo } }
			});

			var providerMock = CreateProviderMock (value, new MockObjectEditor { Target = value });

			var vm = new ObjectPropertyViewModel (new TargetPlatform (providerMock.Object), p.Object, new[] { editor });

			bool requested = false;
			vm.TypeRequested += (sender, args) => {
				requested = true;
			};

			Assume.That (vm.CreateInstanceCommand.CanExecute (childsubInfo), Is.True);
			vm.CreateInstanceCommand.Execute (childsubInfo);
			Assert.That (requested, Is.False, "TypeRequested was raised");

			providerMock.Verify (pr => pr.CreateObjectAsync (childsubInfo));
		}

		[Test]
		public void TypeRequestedWhenMultipleAssignableTypes ()
		{
			object value = new object();

			var p = CreatePropertyMock ("prop");

			var childsubInfo = GetTypeInfo (typeof(SubChildClass));
			var editor = new MockObjectEditor (new[] { p.Object }, new Dictionary<IPropertyInfo, IReadOnlyList<ITypeInfo>> {
				{
					p.Object,
					new[] {
						GetTypeInfo (typeof(ChildClass)),
						childsubInfo
					}
				}
			});

			var providerMock = CreateProviderMock (value, new MockObjectEditor { Target = value });

			var vm = new ObjectPropertyViewModel (new TargetPlatform (providerMock.Object), p.Object, new[] { editor });

			bool requested = false;
			vm.TypeRequested += (sender, args) => {
				requested = true;
			};

			Assume.That (vm.CreateInstanceCommand.CanExecute (childsubInfo), Is.True);
			vm.CreateInstanceCommand.Execute (childsubInfo);
			Assert.That (requested, Is.True, "TypeRequested was not raised");
		}

		[Test]
		[Description ("Values with multiple value sources should show unknown")]
		public async Task MultiValueSourcesUnknown ()
		{
			object value = new object();

			var p = CreatePropertyMock ("prop");

			var childsubInfo = GetTypeInfo (typeof(SubChildClass));
			var editor = new MockObjectEditor (new[] { p.Object }, new Dictionary<IPropertyInfo, IReadOnlyList<ITypeInfo>> {
				{ p.Object, new[] { childsubInfo } }
			});
			await editor.SetValueAsync (p.Object, new ValueInfo<object> {
				Value = value,
				Source = ValueSource.Local,
				ValueDescriptor = childsubInfo
			});

			var editor2 = new MockObjectEditor (new[] { p.Object }, new Dictionary<IPropertyInfo, IReadOnlyList<ITypeInfo>> {
				{ p.Object, new[] { childsubInfo } }
			});
			await editor2.SetValueAsync (p.Object, new ValueInfo<object> {
				Value = value,
				Source = ValueSource.Default,
				ValueDescriptor = childsubInfo
			});

			var providerMock = CreateProviderMock (value, new MockObjectEditor { Target = value });

			var vm = new ObjectPropertyViewModel (new TargetPlatform (providerMock.Object), p.Object, new[] { editor, editor2 });
			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Unknown));
		}

		[Test]
		public async Task ClearedValueSourcesDefault ()
		{
			object value = new object();

			var p = CreatePropertyMock ("prop");

			var childsubInfo = GetTypeInfo (typeof(SubChildClass));
			var editor = new MockObjectEditor (new[] { p.Object }, new Dictionary<IPropertyInfo, IReadOnlyList<ITypeInfo>> {
				{ p.Object, new[] { childsubInfo } }
			});
			await editor.SetValueAsync (p.Object, new ValueInfo<object> {
				Value = value,
				Source = ValueSource.Local,
				ValueDescriptor = childsubInfo
			});

			var providerMock = CreateProviderMock (value, new MockObjectEditor { Target = value });

			var vm = new ObjectPropertyViewModel (new TargetPlatform (providerMock.Object), p.Object, new[] { editor });
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Local));

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(ObjectPropertyViewModel.ValueSource))
					changed = true;
			};

			vm.Editors.Clear();
			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Default));
			Assert.That (changed, Is.True, "PropertyChanged did not fire for ValueSource");
		}

		[Test]
		public void NullEditors ()
		{
			object value = new object();

			var p = CreatePropertyMock ("prop");
			var providerMock = CreateProviderMock (value, new MockObjectEditor { Target = value });

			var vm = new ObjectPropertyViewModel (new TargetPlatform (providerMock.Object), p.Object, new IObjectEditor[0]);
			Assume.That (vm.ValueType, Is.Null);
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Default));

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(ObjectPropertyViewModel.ValueSource))
					changed = true;
			};

			vm.Editors.Add (null);

			Assert.That (changed, Is.False);
			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Default));
		}

		[Test]
		public async Task CanDelve ()
		{
			object value = new object();

			var p = CreatePropertyMock ("prop");

			var childsubInfo = GetTypeInfo (typeof(SubChildClass));
			var editor = new MockObjectEditor (new[] { p.Object }, new Dictionary<IPropertyInfo, IReadOnlyList<ITypeInfo>> {
				{ p.Object, new[] { childsubInfo } }
			});
			await editor.SetValueAsync (p.Object, new ValueInfo<object> { Value = value, Source = ValueSource.Local });

			var providerMock = CreateProviderMock (value, new MockObjectEditor { Target = value });

			var vm = new ObjectPropertyViewModel (new TargetPlatform (providerMock.Object), p.Object, new IObjectEditor[0]);
			Assert.That (vm.CanDelve, Is.False);

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(ObjectPropertyViewModel.CanDelve))
					changed = true;
			};

			vm.Editors.Add (editor);

			Assert.That (vm.CanDelve, Is.True);
			Assert.That (changed, Is.True, "PropertyChanged for CanDelve didn't fire adding");

			changed = false;
			vm.Editors.Clear();

			Assert.That (vm.CanDelve, Is.False);
			Assert.That (changed, Is.True, "PropertyChanged for CanDelve didn't fire clearing");

			changed = false;
			vm.Editors.Add (null);

			Assert.That (vm.CanDelve, Is.False);
			Assert.That (changed, Is.False, "PropertyChanged for CanDelve when hasn't changed");
		}

		[Test]
		[Description ("Values with multiple value types should be null")]
		public async Task MultiValueTypesNull ()
		{
			object value = new object(), value2 = new object();

			var p = CreatePropertyMock ("prop");

			var childsubInfo = GetTypeInfo (typeof(SubChildClass));
			var childInfo = GetTypeInfo (typeof(ChildClass));
			var editor = new MockObjectEditor (new[] { p.Object }, new Dictionary<IPropertyInfo, IReadOnlyList<ITypeInfo>> {
				{ p.Object, new[] { childInfo, childsubInfo } }
			});
			await editor.SetValueAsync (p.Object, new ValueInfo<object> {
				Value = value,
				Source = ValueSource.Local,
				ValueDescriptor = childInfo
			});

			var editor2 = new MockObjectEditor (new[] { p.Object }, new Dictionary<IPropertyInfo, IReadOnlyList<ITypeInfo>> {
				{ p.Object, new[] { childInfo, childsubInfo } }
			});
			await editor2.SetValueAsync (p.Object, new ValueInfo<object> {
				Value = value2,
				Source = ValueSource.Local,
				ValueDescriptor = childsubInfo
			});

			var providerMock = CreateProviderMock (value, new MockObjectEditor { Target = value });
			providerMock.Setup (a => a.GetObjectEditorAsync (value2)).ReturnsAsync (new MockObjectEditor { Target = value2 });

			var vm = new ObjectPropertyViewModel (new TargetPlatform (providerMock.Object), p.Object, new[] { editor, editor2 });
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Local));
			Assert.That (vm.ValueType, Is.Null);
		}

		private TestContext syncContext;

		private Mock<IEditorProvider> CreateProviderMock (object value, IObjectEditor editor)
		{
			var m = new Mock<IEditorProvider> ();
			m.Setup (e => e.GetObjectEditorAsync (value)).ReturnsAsync (editor);

			return m;
		}

		private Mock<IPropertyInfo> CreatePropertyMock (string name)
		{
			var m = new Mock<IPropertyInfo> ();
			m.SetupGet (p => p.Type).Returns (typeof(object));
			m.SetupGet (p => p.Name).Returns (name);

			return m;
		}

		private ITypeInfo GetTypeInfo (Type type)
		{
			var asm = new AssemblyInfo (type.Assembly.FullName, true);
			return new TypeInfo (asm, type.Namespace, type.Name);
		}

		private class SubChildClass
			: ChildClass
		{
			
		}

		private class ChildClass
		{
			
		}
	}
}
