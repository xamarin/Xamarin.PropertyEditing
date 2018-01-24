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
	internal class EventViewModelTests
	{
		[Test]
		public void MethodName()
		{
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var editor = new MockObjectEditor {
				Events = new[] { info.Object }
			};
			const string handler = "handler";
			editor.AttachHandlerAsync (info.Object, handler);

			var ev = new EventViewModel (TargetPlatform.Default, info.Object, new[] { editor });

			Assert.That (ev.MethodName, Is.EqualTo (handler));
		}

		[Test]
		public void SetMethodName()
		{
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var editor = new Mock<IObjectEditor> ();
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.SetupGet (e => e.Events).Returns (new[] { info.Object });

			var vm = new EventViewModel (TargetPlatform.Default, info.Object, new[] { editor.Object });
			Assume.That (vm.MethodName, Is.Null);

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof (EventViewModel.MethodName))
					changed = true;
			};

			const string newMethodName = "foo";
			vm.MethodName = newMethodName;

			Assert.That (changed, Is.True);
			eeditor.Verify (i => i.AttachHandlerAsync (info.Object, newMethodName));
		}

		[Test]
		public void ChangeMethodName ()
		{
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			const string oldHandler = "oldHandler";
			var editor = new Mock<IObjectEditor> ();
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.SetupGet (e => e.Events).Returns (new[] { info.Object });
			eeditor.Setup (e => e.GetHandlersAsync (info.Object)).ReturnsAsync (new[] { oldHandler });
			var vm = new EventViewModel (TargetPlatform.Default, info.Object, new[] { editor.Object });
			Assume.That (vm.MethodName, Is.EqualTo (oldHandler));

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof (EventViewModel.MethodName))
					changed = true;
			};

			const string newMethodName = "foo";
			vm.MethodName = newMethodName;

			Assert.That (changed, Is.True);
			eeditor.Verify (i => i.DetachHandlerAsync (info.Object, oldHandler));
			eeditor.Verify (i => i.AttachHandlerAsync (info.Object, newMethodName));
		}

		[Test]
		public void DisagreeingValues()
		{
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var editor = new MockObjectEditor {
				Events = new[] { info.Object }
			};
			const string handler = "handler";
			editor.AttachHandlerAsync (info.Object, handler);

			var editor2 = new MockObjectEditor {
				Events = new[] { info.Object }
			};
			editor.AttachHandlerAsync (info.Object, "handler2");

			var ev = new EventViewModel (TargetPlatform.Default, info.Object, new[] { editor, editor2 });

			Assert.That (ev.MethodName, Is.Null);
			Assert.That (ev.MultipleValues, Is.True);
		}
	}
}
