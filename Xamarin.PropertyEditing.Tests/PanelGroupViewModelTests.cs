using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class PanelGroupViewModelTests
	{
		[Test]
		public void HasChildElementsUpdates ()
		{
			var property = new Mock<IPropertyInfo> ();
			property.Setup (p => p.Name).Returns ("Name");
			property.Setup (p => p.Category).Returns ("Category");
			property.Setup (p => p.Type).Returns (typeof(string));
			property.Setup (p => p.IsUncommon).Returns (false);

			var editor = new MockObjectEditor (property.Object);
			var propertyVm = new StringPropertyViewModel (MockEditorProvider.MockPlatform, property.Object, new[] { editor });
			var group = new PanelGroupViewModel (MockEditorProvider.MockPlatform, "Category", new[] { propertyVm });

			Assert.That (group.HasChildElements, Is.True);

			bool changed = false;
			group.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(PanelGroupViewModel.HasChildElements))
					changed = true;
			};

			group.Remove (propertyVm);
			Assert.That (group.HasChildElements, Is.False);
			Assert.That (changed, Is.True, "INPC did not fire for HasChildElements during remove");
			changed = false;

			group.Add (propertyVm);
			Assert.That (group.HasChildElements, Is.True);
			Assert.That (changed, Is.True, "INPC did not fire for HasChildElements during add");
		}

		[Test]
		public void HasUncommonElementsUpdates ()
		{
			var property = new Mock<IPropertyInfo> ();
			property.Setup (p => p.Name).Returns ("Name");
			property.Setup (p => p.Category).Returns ("Category");
			property.Setup (p => p.Type).Returns (typeof (string));
			property.Setup (p => p.IsUncommon).Returns (true);

			var editor = new MockObjectEditor (property.Object);
			var propertyVm = new StringPropertyViewModel (MockEditorProvider.MockPlatform, property.Object, new[] { editor });
			var group = new PanelGroupViewModel (MockEditorProvider.MockPlatform, "Category", new[] { propertyVm });

			Assert.That (group.HasChildElements, Is.True);
			Assert.That (group.HasUncommonElements, Is.True);

			bool childChanged = false, uncommonChanged = false;
			group.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (PanelGroupViewModel.HasChildElements))
					childChanged = true;
				else if (args.PropertyName == nameof(PanelGroupViewModel.HasUncommonElements))
					uncommonChanged = true;
			};

			group.Remove (propertyVm);
			Assert.That (group.HasChildElements, Is.False);
			Assert.That (group.HasUncommonElements, Is.False);
			Assert.That (childChanged, Is.True, "INPC did not fire for HasChildElements during remove");
			Assert.That (uncommonChanged, Is.True, "INPC did not fire for HasUncommonElements during remove");
			childChanged = false;

			group.Add (propertyVm);
			Assert.That (group.HasChildElements, Is.True);
			Assert.That (group.HasUncommonElements, Is.True);
			Assert.That (childChanged, Is.True, "INPC did not fire for HasChildElements during add");
			Assert.That (uncommonChanged, Is.True, "INPC did not fire for HasUncommonElements during add");
		}

		[Test]
		public void Replace ()
		{
			var property = new Mock<IPropertyInfo> ();
			property.Setup (p => p.Name).Returns ("Name");
			property.Setup (p => p.Category).Returns ("Category");
			property.Setup (p => p.Type).Returns (typeof (string));
			property.Setup (p => p.IsUncommon).Returns (false);

			var editor = new MockObjectEditor (property.Object);
			var propertyVm = new StringPropertyViewModel (MockEditorProvider.MockPlatform, property.Object, new[] { editor });
			var group = new PanelGroupViewModel (MockEditorProvider.MockPlatform, "Category", new[] { propertyVm });
			Assume.That (group.Editors[0], Is.SameAs (propertyVm));

			bool changed = false;
			((INotifyCollectionChanged)group.Editors).CollectionChanged += (sender, args) => {
				changed = true;
				Assert.That (args.Action, Is.EqualTo (NotifyCollectionChangedAction.Replace));
			};

			var propertyVm2 = new StringPropertyViewModel (MockEditorProvider.MockPlatform, property.Object, new[] { editor });
			group.Replace (propertyVm, propertyVm2);
			Assert.That (group.Editors[0], Is.SameAs (propertyVm2));
			Assert.That (changed, Is.True, "INCC wasn't fired'");
		}

		[Test]
		public void TryGetEditor ()
		{
			var property = new Mock<IPropertyInfo> ();
			property.Setup (p => p.Name).Returns ("Name");
			property.Setup (p => p.Category).Returns ("Category");
			property.Setup (p => p.Type).Returns (typeof (string));
			property.Setup (p => p.IsUncommon).Returns (false);

			var editor = new MockObjectEditor (property.Object);
			var propertyVm = new StringPropertyViewModel (MockEditorProvider.MockPlatform, property.Object, new[] { editor });
			var group = new PanelGroupViewModel (MockEditorProvider.MockPlatform, "Category", new[] { propertyVm });

			Assert.That (group.TryGetEditor (property.Object, out EditorViewModel evm), Is.True, "Couldn't find editor");
			Assert.That (evm, Is.SameAs (propertyVm));
		}

		private class HostedViewModel
			: EditorViewModel, IPropertyHost
		{
			public HostedViewModel (TargetPlatform platform, IEnumerable<IObjectEditor> editors, PropertyViewModel hosted)
				: base (platform, editors)
			{
				HostedProperty = hosted;
			}

			public override string Category => HostedProperty.Category;

			public override string Name => HostedProperty.Name;

			public PropertyViewModel HostedProperty
			{
				get;
			}
		}

		[Test]
		public void TryGetEditorHosted ()
		{
			var property = new Mock<IPropertyInfo> ();
			property.Setup (p => p.Name).Returns ("Name");
			property.Setup (p => p.Category).Returns ("Category");
			property.Setup (p => p.Type).Returns (typeof (string));
			property.Setup (p => p.IsUncommon).Returns (false);

			var editor = new MockObjectEditor (property.Object);
			var propertyVm = new StringPropertyViewModel (MockEditorProvider.MockPlatform, property.Object, new[] { editor });
			var host = new HostedViewModel (MockEditorProvider.MockPlatform, new[] { editor }, propertyVm);
			var group = new PanelGroupViewModel (MockEditorProvider.MockPlatform, "Category", new[] { host });

			Assert.That (group.TryGetEditor (property.Object, out EditorViewModel evm), Is.True, "Couldn't find editor");
			Assert.That (evm, Is.SameAs (host));
		}
	}
}
