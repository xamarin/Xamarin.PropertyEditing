using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class PanelViewModelTests
		: PropertiesViewModelTests<PanelViewModel>
	{
		[Test]
		[Description ("We must be sure that if the selected objects list changes while the provider is still retrieving that we end up with the right result")]
		public async Task InteruptedEditorRetrievalResolvesCorrectlyItemAdded ()
		{
			var obj1 = new object ();
			var obj2 = new object ();

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (pi => pi.Type).Returns (typeof (string));

			var editor1 = new Mock<IObjectEditor> ();
			editor1.SetupGet (oe => oe.Target).Returns (obj1);
			editor1.SetupGet (oe => oe.TargetType).Returns (obj1.GetType ().ToTypeInfo ());
			editor1.SetupGet (oe => oe.Properties).Returns (new[] { property.Object });

			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (oe => oe.Target).Returns (obj2);
			editor2.SetupGet (oe => oe.TargetType).Returns (obj2.GetType ().ToTypeInfo ());
			editor2.SetupGet (oe => oe.Properties).Returns (new[] { property.Object });

			Task<IObjectEditor> returnObject = null;

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj1)).Returns (() => {
				returnObject = Task.Delay (2000).ContinueWith (t => editor1.Object);
				return returnObject;
			});
			provider.Setup (ep => ep.GetObjectEditorAsync (obj2)).ReturnsAsync (editor2.Object);

			var vm = new PanelViewModel (new TargetPlatform (provider.Object));
			vm.SelectedObjects.Add (obj1);

			Assume.That (returnObject, Is.Not.Null);
			Assume.That (returnObject.IsCompleted, Is.False);

			vm.SelectedObjects.Remove (obj1);

			await returnObject;
			await Task.Yield ();

			Assert.That (vm.Properties, Is.Empty);
		}

		[Test]
		[Description ("When filtered Text exists then list is reduced.")]
		public async Task PropertyListFilteredTextReducesList ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (new TargetPlatform (provider));
			Assume.That (vm.ArrangeMode, Is.EqualTo (PropertyArrangeMode.Name));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assume.That (vm.ArrangedEditors[0].Editors.Count, Is.EqualTo (2));

			vm.FilterText = "sub";
			Assert.That (vm.ArrangedEditors[0].Editors.Count, Is.EqualTo (1));
		}

		[Test]
		public async Task PropertyListIsFiltered ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (new TargetPlatform (provider));
			Assume.That (vm.ArrangeMode, Is.EqualTo (PropertyArrangeMode.Name));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assume.That (vm.ArrangedEditors[0].Editors.Count, Is.EqualTo (2));

			Assume.That (vm.IsFiltering, Is.False);
			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(PanelViewModel.IsFiltering)) {
					changed = true;
				}
			};

			vm.FilterText = "sub";
			Assume.That (vm.ArrangedEditors[0].Editors.Count, Is.EqualTo (1));
			Assert.That (vm.IsFiltering, Is.True);
			Assert.That (changed, Is.True);
			changed = false;

			vm.FilterText = null;
			Assert.That (vm.IsFiltering, Is.False);
			Assert.That (changed, Is.True);
		}

		[Test]
		public void UncommonPropertiesFiltered ()
		{
			var obj = new TestClassSub ();

			var property = new Mock<IPropertyInfo> ();
			property.Setup (pi => pi.Name).Returns (nameof(TestClass.Property));
			property.Setup (pi => pi.Type).Returns (typeof(string));
			property.Setup (pi => pi.RealType).Returns (typeof (string).ToTypeInfo ());
			property.Setup (pi => pi.IsUncommon).Returns (true);

			var subProperty = new Mock<IPropertyInfo> ();
			subProperty.Setup (pi => pi.Name).Returns (nameof (TestClassSub.SubProperty));
			subProperty.Setup (pi => pi.Type).Returns (typeof (int));
			subProperty.Setup (pi => pi.RealType).Returns (typeof (int).ToTypeInfo ());
			subProperty.Setup (pi => pi.IsUncommon).Returns (false);

			var editor = new Mock<IObjectEditor> ();
			editor.SetTarget (obj);
			editor.Setup (e => e.Properties).Returns (new[] { property.Object, subProperty.Object });

			var provider = new MockEditorProvider (editor.Object);

			var vm = new PanelViewModel (new TargetPlatform (provider));
			Assume.That (vm.ArrangeMode, Is.EqualTo (PropertyArrangeMode.Name));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assume.That (vm.ArrangedEditors[0].Editors.Count, Is.EqualTo (2));
			Assume.That (vm.IsFiltering, Is.False);
			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (PanelViewModel.IsFiltering)) {
					changed = true;
				}
			};

			vm.FilterText = "sub";
			Assert.That (vm.ArrangedEditors[0].Editors.Count, Is.EqualTo (1), "Uncommon property wasn't filtered out");
			Assert.That (vm.IsFiltering, Is.True);
			Assert.That (changed, Is.True);
			changed = false;

			vm.FilterText = null;
			Assert.That (vm.IsFiltering, Is.False);
			Assert.That (changed, Is.True);
		}

		[Test]
		[Description ("When filtered Text is cleared then list is restored back to its original.")]
		public async Task PropertyListFilteredTextClearedRestoresList ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));
			
			var vm = new PanelViewModel (new TargetPlatform (provider));
			Assume.That (vm.ArrangeMode, Is.EqualTo (PropertyArrangeMode.Name));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assume.That (vm.ArrangedEditors[0].Editors.Count, Is.EqualTo (2));

			vm.FilterText = "sub";
			Assume.That (vm.ArrangedEditors[0].Editors.Count, Is.EqualTo (1));

			vm.FilterText = String.Empty;
			Assert.That (vm.ArrangedEditors[0].Editors.Count, Is.EqualTo (2));
		}

		[Test]
		public async Task PropertyCategoryArrange ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (new TargetPlatform (provider)) { ArrangeMode = PropertyArrangeMode.Category };
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assert.That (vm.ArrangedEditors.FirstOrDefault (g => g.Category == "Sub"), Is.Not.Null);
		}

		[Test]
		public void GroupedPropertiesArrange ()
		{
			var intProvider = new IntegerPropertyViewModelTests ();
			var stringProvider = new StringViewModelTests ();
			var brushProvider = new SolidBrushPropertyViewModelTests();

			var intProperty = intProvider.GetPropertyMock ("int", "A");
			var stringProperty1 = stringProvider.GetPropertyMock ("string1");
			var stringProperty2 = stringProvider.GetPropertyMock ("string2");
			var brushProperty = brushProvider.GetPropertyMock ("brush", "C");

			var editor = new MockObjectEditor (intProperty.Object, stringProperty1.Object, stringProperty2.Object, brushProperty.Object);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (editor.Target)).ReturnsAsync (editor);

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(string), "B" }
				}
			};

			var vm = new PanelViewModel (platform);
			Assume.That (vm.ArrangeMode, Is.EqualTo (PropertyArrangeMode.Name));

			vm.ArrangeMode = PropertyArrangeMode.Category;
			vm.SelectedObjects.Add (editor.Target);
			Assert.That (vm.ArrangedEditors[0].Category, Is.EqualTo ("A"));
			Assert.That (vm.ArrangedEditors[1].Category, Is.EqualTo ("B"));
			Assert.That (vm.ArrangedEditors[2].Category, Is.EqualTo ("C"));
		}

		[Test]
		public async Task PropertyListCategoryFiltered ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (new TargetPlatform (provider)) { ArrangeMode = PropertyArrangeMode.Category };
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);

			vm.FilterText = "sub";
			Assert.That (vm.ArrangedEditors.Count, Is.EqualTo (1));

			var group = vm.ArrangedEditors.FirstOrDefault (g => g.Category == "Sub");
			Assert.That (group, Is.Not.Null);
			Assert.That (group.Editors.Count, Is.EqualTo (1));
		}

		[Test]
		public async Task PropertyListCategoryGroupedWithUnnamedCategory ()
		{
			// Purposefully a null category, so we get an Unnamed Category
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof(string));
			normalProp.SetupGet (p => p.Name).Returns ("name");

			var groupProp = new Mock<IPropertyInfo> ();
			groupProp.SetupGet (p => p.Type).Returns (typeof(int));

			var target = new object ();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object, groupProp.Object));

			var mockPlatform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(int), "ints" }
				}
			};

			var editor = await provider.Object.GetObjectEditorAsync (target);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (mockPlatform) { ArrangeMode = PropertyArrangeMode.Category };
			vm.SelectedObjects.Add (target);

			Assert.That (vm.ArrangedEditors.Count, Is.EqualTo (2));
			Assert.That (vm.ArrangedEditors[0].Category, Is.EqualTo (Properties.Resources.UnnamedCategory), "Grouped group not found or out of order");
		}

		[Test]
		public async Task PropertyListCategoryWithoutNameFiltered ()
		{
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof(string));
			normalProp.SetupGet (p => p.Name).Returns ("name");

			var groupProp = new Mock<IPropertyInfo> ();
			groupProp.SetupGet (p => p.Type).Returns (typeof(int));

			var target = new object ();
			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object, groupProp.Object));

			var mockPlatform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(int), "ints" }
				}
			};			

			var editor = await provider.Object.GetObjectEditorAsync (target);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (mockPlatform) { ArrangeMode = PropertyArrangeMode.Category };
			vm.SelectedObjects.Add (target);

			Assume.That (vm.ArrangedEditors.Count, Is.EqualTo (2));

			vm.FilterText = "name";
			Assert.That (vm.ArrangedEditors.Count, Is.EqualTo (1));

			var group = vm.ArrangedEditors.FirstOrDefault (g => g.Category == "ints");
			Assert.That (group, Is.Null);
		}

		[Test]
		public void GroupedWithNullGroupedTypes ()
		{
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof(string));
			normalProp.SetupGet (p => p.Name).Returns ("name");

			var groupProp = new Mock<IPropertyInfo> ();
			groupProp.SetupGet (p => p.Type).Returns (typeof(int));

			var target = new object();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object, groupProp.Object));

			var platform = new TargetPlatform (provider.Object);
			Assume.That (platform.GroupedTypes, Is.Null);

			var vm = new PanelViewModel (platform) {
				ArrangeMode = PropertyArrangeMode.Category
			};

			Assert.That (() => vm.SelectedObjects.Add (target), Throws.Nothing);
		}

		[Test]
		[Description ("Bug coverage for grouped property failing to add if last")]
		public void AddGroupedAtEnd ()
		{
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof(string));
			normalProp.SetupGet (p => p.Category).Returns ("Category");
			normalProp.SetupGet (p => p.Name).Returns ("name");

			var groupProp = new Mock<IPropertyInfo> ();
			groupProp.SetupGet (p => p.Type).Returns (typeof(int));

			var target = new object();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object, groupProp.Object));

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(int), "ints" }
				}
			};

			var vm = new PanelViewModel (platform) {
				ArrangeMode = PropertyArrangeMode.Category,
				AutoExpand = true
			};
			vm.SelectedObjects.Add (target);

			Assert.That (vm.ArrangedEditors.Any (g => g.Category == "ints"), Is.True, "Does not have grouped editors category");
		}

		[Test]
		[Description ("#544 Bug coverage for removing groupable items when not grouped")]
		public void FilterGroupableWhenNotGrouped ()
		{
			var stringProvider = new StringViewModelTests ();

			var stringProperty1 = stringProvider.GetPropertyMock ("string1");
			var stringProperty2 = stringProvider.GetPropertyMock ("string2");

			var editor = new MockObjectEditor (stringProperty1.Object, stringProperty2.Object);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (editor.Target)).ReturnsAsync (editor);

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(string), "B" }
				}
			};

			var vm = new PanelViewModel (platform);
			Assume.That (vm.ArrangeMode, Is.EqualTo (PropertyArrangeMode.Name));
			vm.SelectedObjects.Add (editor.Target);
			Assume.That (vm.ArrangedEditors[0].Editors.Count, Is.EqualTo (2));

			vm.FilterText = "A";
			Assert.That (vm.ArrangedEditors.Count, Is.EqualTo (0));
		}

		[Test]
		public async Task AutoExpand ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (new TargetPlatform (provider)) {
				ArrangeMode = PropertyArrangeMode.Category,
				AutoExpand = true
			};
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assert.That (vm.GetIsExpanded (vm.ArrangedEditors[0].Category), Is.True);
		}

		[Test]
		public void AutoExpandGroupedProperties ()
		{
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof(string));
			normalProp.SetupGet (p => p.Category).Returns ("Category");
			normalProp.SetupGet (p => p.Name).Returns ("name");

			var groupProp = new Mock<IPropertyInfo> ();
			groupProp.SetupGet (p => p.Type).Returns (typeof(int));

			var target = new object();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object, groupProp.Object));

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(int), "ints" }
				}
			};

			var vm = new PanelViewModel (platform) {
				ArrangeMode = PropertyArrangeMode.Category,
				AutoExpand = true
			};
			vm.SelectedObjects.Add (target);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assume.That (vm.ArrangedEditors.Any (g => g.Category == "ints"), Is.True, "Does not have grouped editors category");
			Assert.That (vm.GetIsExpanded ("ints"), Is.True);
		}

		[Test]
		public void AutoExpandChosenGroups ()
		{
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof (string));
			normalProp.SetupGet (p => p.Category).Returns ("Category");
			normalProp.SetupGet (p => p.Name).Returns ("name");
			
			var target = new object ();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object));

			var platform = new TargetPlatform (provider.Object) {
				AutoExpandGroups = new[] { normalProp.Object.Category }
			};

			var vm = new PanelViewModel (platform) {
				ArrangeMode = PropertyArrangeMode.Category
			};
			vm.SelectedObjects.Add (target);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assert.That (vm.GetIsExpanded (normalProp.Object.Category), Is.True);
		}

		[Test]
		public void GroupedTypesDoNotLeavePhantomCategory ()
		{
			var target = new object ();

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.Category).Returns ((string)null);
			property.SetupGet (p => p.Name).Returns ("name");

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (property.Object) { Target = target });

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(string), "strings" }
				}
			};

			var vm = CreateVm (platform);
			vm.ArrangeMode = PropertyArrangeMode.Category;
			vm.AutoExpand = true;
			vm.SelectedObjects.Add (target);

			Assert.That (vm.ArrangedEditors.Count, Is.EqualTo (1));
		}

		[Test]
		[Description ("https://github.com/xamarin/Xamarin.PropertyEditing/issues/525")]
		public void SwitchingFromObjectWithGroupedType ()
		{
			var targetWithProperties = new object ();
			var targetWithoutProperties = new object ();

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.Category).Returns ((string)null);
			property.SetupGet (p => p.Name).Returns ("name");

			var property2 = new Mock<IPropertyInfo> ();
			property2.SetupGet (p => p.Type).Returns (typeof (string));
			property2.SetupGet (p => p.Category).Returns ((string)null);
			property2.SetupGet (p => p.Name).Returns ("name2");

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (targetWithProperties))
				.ReturnsAsync (new MockObjectEditor (property.Object, property2.Object) { Target = targetWithProperties });
			provider.Setup (ep => ep.GetObjectEditorAsync (targetWithoutProperties))
				.ReturnsAsync (new MockObjectEditor (new IPropertyInfo[0]) { Target = targetWithoutProperties });

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(string), "strings" }
				}
			};

			var vm = CreateVm (platform);
			vm.ArrangeMode = PropertyArrangeMode.Category;
			vm.AutoExpand = true;
			vm.SelectedObjects.Add (targetWithProperties);

			Assume.That (vm.ArrangedEditors.Count, Is.EqualTo (1));

			Assert.That (() => vm.SelectedObjects.ReplaceOrAdd (targetWithProperties, targetWithoutProperties),
				Throws.Nothing);
			Assert.That (vm.ArrangedEditors.Count, Is.EqualTo (0));
		}

		[Test]
		public void SwitchingToObjectWithGroupedType ()
		{
			var targetWithProperties = new object ();
			var targetWithoutProperties = new object ();

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.Category).Returns ((string)null);
			property.SetupGet (p => p.Name).Returns ("name");

			var property2 = new Mock<IPropertyInfo> ();
			property2.SetupGet (p => p.Type).Returns (typeof (string));
			property2.SetupGet (p => p.Category).Returns ((string)null);
			property2.SetupGet (p => p.Name).Returns ("name2");

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (targetWithProperties))
				.ReturnsAsync (new MockObjectEditor (property.Object, property2.Object) { Target = targetWithProperties });
			provider.Setup (ep => ep.GetObjectEditorAsync (targetWithoutProperties))
				.ReturnsAsync (new MockObjectEditor (new IPropertyInfo[0]) { Target = targetWithoutProperties });

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(string), "strings" }
				}
			};

			var vm = CreateVm (platform);
			vm.ArrangeMode = PropertyArrangeMode.Category;
			vm.AutoExpand = true;
			vm.SelectedObjects.Add (targetWithoutProperties);

			Assume.That (vm.ArrangedEditors.Count, Is.EqualTo (0));

			Assert.That (() => vm.SelectedObjects.ReplaceOrAdd (targetWithoutProperties, targetWithProperties),
				Throws.Nothing);
			Assert.That (vm.ArrangedEditors.Count, Is.EqualTo (1));

			var group = vm.ArrangedEditors[0].Editors[0] as PropertyGroupViewModel;
			Assert.That (group, Is.Not.Null);
			Assert.That (group.Properties.Count, Is.EqualTo (2));
		}

		[Test]
		public void GroupedTypeMultiselect ()
		{
			var outer = new object ();
			var inner = new object ();

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.Category).Returns ((string)null);
			property.SetupGet (p => p.Name).Returns ("name");

			var property2 = new Mock<IPropertyInfo> ();
			property2.SetupGet (p => p.Type).Returns (typeof (string));
			property2.SetupGet (p => p.Category).Returns ((string)null);
			property2.SetupGet (p => p.Name).Returns ("name2");

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (outer))
				.ReturnsAsync (new MockObjectEditor (property.Object, property2.Object) { Target = outer });
			provider.Setup (ep => ep.GetObjectEditorAsync (inner))
				.ReturnsAsync (new MockObjectEditor (property.Object) { Target = inner });

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(string), "strings" }
				}
			};

			var vm = CreateVm (platform);
			vm.ArrangeMode = PropertyArrangeMode.Category;
			vm.AutoExpand = true;
			vm.SelectedObjects.Add (outer);

			Assume.That (vm.ArrangedEditors.Count, Is.EqualTo (1));

			var group = vm.ArrangedEditors[0].Editors[0] as PropertyGroupViewModel;
			Assume.That (group, Is.Not.Null);
			Assume.That (group.Properties.Count, Is.EqualTo (2));

			bool shouldChange = false, changed = false;
			if (group.Properties is INotifyCollectionChanged incc) {
				shouldChange = true;
				incc.CollectionChanged += (o, e) => changed = true;
			}

			vm.SelectedObjects.Add (inner);
			Assert.That (vm.ArrangedEditors[0].Editors[0] as PropertyGroupViewModel, Is.SameAs (group));
			Assert.That (group.Properties.Count, Is.EqualTo (1), "Number of remaining properties isn't correct");
			Assert.That (group.Properties[0].Property, Is.EqualTo (property.Object), "Wrong property found in the group");
			Assert.That (changed, Is.EqualTo (shouldChange), "Changed status didn't match expected");
			
			vm.SelectedObjects.Remove (inner);
			group = vm.ArrangedEditors[0].Editors[0] as PropertyGroupViewModel;
			Assert.That (group, Is.Not.Null);
			Assert.That (group.Properties.Count, Is.EqualTo (2), "Outer properties didn't restore");
		}

		[Test]
		public void GroupedTypeWhileNamed ()
		{
			var outer = new object ();

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.Category).Returns ((string)null);
			property.SetupGet (p => p.Name).Returns ("name");

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (outer))
				.ReturnsAsync (new MockObjectEditor (property.Object) { Target = outer });

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(string), "strings" }
				}
			};

			var vm = CreateVm (platform);
			vm.ArrangeMode = PropertyArrangeMode.Name;
			vm.SelectedObjects.Add (outer);

			Assume.That (vm.ArrangedEditors.Count, Is.EqualTo (1));

			var group = vm.ArrangedEditors[0].Editors[0] as StringPropertyViewModel;
			Assert.That (group, Is.Not.Null);
		}


		[Test]
		public void UncommonVariantsAddedWhenPropertyIs ()
		{
			var variations = new[] {
				new PropertyVariationOption ("Width", "Compact"),
				new PropertyVariationOption ("Width", "Regular"),
				new PropertyVariationOption ("Gamut", "P3"),
				new PropertyVariationOption ("Gamut", "sRGB"),
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Name).Returns ("Variation");
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.RealType).Returns (typeof (string).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Default | ValueSources.Local);
			property.SetupGet (p => p.Variations).Returns (variations);
			property.SetupGet (p => p.IsUncommon).Returns (true);

			var properties = new ObservableCollection<IPropertyInfo> ();

			var variants = new[] {
				new PropertyVariation (variations[0]),
				new PropertyVariation (variations[0], variations[2])
			};

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.TargetType).Returns (typeof (object).ToTypeInfo ());
			editor.SetupGet (oe => oe.Target).Returns (target);
			editor.SetupGet (oe => oe.Properties).Returns (properties);
			editor.Setup (oe => oe.GetPropertyVariantsAsync (property.Object)).ReturnsAsync (variants);
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, null)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Any",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[0])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[1])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact+P3",
					Source = ValueSource.Local
				});

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			vm.ArrangeMode = PropertyArrangeMode.Category;
			vm.SelectedObjects.Add (target);
			Assume.That (vm.Properties, Is.Empty);

			properties.Add (property.Object);

			var stringVms = vm.ArrangedEditors[0].UncommonEditors.OfType<StringPropertyViewModel> ().ToList ();
			Assert.That (stringVms.Count, Is.EqualTo (3), "Not including correct number of properties with variants");
			Assert.That (stringVms.Count (svm => svm.Variation == null), Is.EqualTo (1), "Did not include neutral property");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[0]), Is.EqualTo (1), "Missing variant property");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[1]), Is.EqualTo (1), "Missing variant property");
		}

		[Test]
		public void GetIsLastVariant ([Values (true, false)] bool isUncommon, [Values (true, false)] bool isLast)
		{
			var variations = new[] {
				new PropertyVariationOption ("Width", "Compact"),
				new PropertyVariationOption ("Width", "Regular"),
				new PropertyVariationOption ("Gamut", "P3"),
				new PropertyVariationOption ("Gamut", "sRGB"),
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Name).Returns ("Variation");
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.RealType).Returns (typeof (string).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Default | ValueSources.Local);
			property.SetupGet (p => p.Variations).Returns (variations);
			property.SetupGet (p => p.IsUncommon).Returns (isUncommon);

			var properties = new ObservableCollection<IPropertyInfo> ();

			var variants = new[] {
				new PropertyVariation (variations[0]),
				new PropertyVariation (variations[0], variations[2])
			};

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.TargetType).Returns (typeof (object).ToTypeInfo ());
			editor.SetupGet (oe => oe.Target).Returns (target);
			editor.SetupGet (oe => oe.Properties).Returns (properties);
			editor.Setup (oe => oe.GetPropertyVariantsAsync (property.Object)).ReturnsAsync (variants);
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, null)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Any",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[0])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[1])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact+P3",
					Source = ValueSource.Local
				});

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			vm.ArrangeMode = PropertyArrangeMode.Category;
			vm.SelectedObjects.Add (target);
			Assume.That (vm.Properties, Is.Empty);

			properties.Add (property.Object);

			var stringVms = ((isUncommon) ? vm.ArrangedEditors[0].UncommonEditors : vm.ArrangedEditors[0].Editors)
				.OfType<StringPropertyViewModel> ();
			var prvm = (isLast)
				? stringVms.Last (pvm => pvm.Property == property.Object)
				: stringVms.Skip(1).First (pvm => pvm.Property == property.Object);

			Assert.That (vm.GetIsLastVariant (prvm), Is.EqualTo (isLast));
		}

		internal override PanelViewModel CreateVm (TargetPlatform platform)
		{
			return new PanelViewModel (platform);
		}
	}
}
