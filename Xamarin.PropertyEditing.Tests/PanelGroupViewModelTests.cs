using System;
using System.Collections.Generic;
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
	}
}
