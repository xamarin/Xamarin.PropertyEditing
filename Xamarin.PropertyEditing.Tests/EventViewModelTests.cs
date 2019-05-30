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
	internal class EventViewModelTests
	{
		[Test]
		public void LoadsHandlers()
		{
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var editor = new MockObjectEditor {
				Events = new[] { info.Object }
			};
			const string handler = "handler";
			editor.AttachHandlerAsync (info.Object, handler);

			var ev = new EventViewModel (MockEditorProvider.MockPlatform, info.Object, new[] { editor });

			Assert.That (ev.Handlers, Contains.Item (handler));
		}

		[Test]
		public void AddHandler()
		{
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var handlers = new List<string> ();
			var editor = new Mock<IObjectEditor> ();
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.Setup (ee => ee.GetHandlersAsync (info.Object)).ReturnsAsync (handlers);
			eeditor.Setup (ee => ee.GetPotentialHandlersAsync (It.IsAny<IEventInfo> ())).ReturnsAsync (Array.Empty<string> ());
			eeditor.Setup (ee => ee.AttachHandlerAsync (It.IsAny<IEventInfo> (), It.IsAny<string> ()))
				.Callback<IEventInfo,string> ((ev, n) => {
					handlers.Add (n);
					eeditor.Raise (ee => ee.EventHandlersChanged += null, new EventHandlersChangedEventArgs (ev));
				})
				.Returns (Task.CompletedTask);
			eeditor.SetupGet (e => e.Events).Returns (new[] { info.Object });

			var vm = new EventViewModel (MockEditorProvider.MockPlatform, info.Object, new[] { editor.Object });
			Assume.That (vm.Handlers, Is.Empty);

			bool changed = false;
			var incc = (INotifyCollectionChanged) vm.Handlers;
			incc.CollectionChanged += (o, e) => { changed = true; };

			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof (EventViewModel.Handlers))
					changed = true;
			};

			const string newMethodName = "foo";
			Assert.That (vm.AddHandlerCommand.CanExecute (newMethodName), Is.True);
			vm.AddHandlerCommand.Execute (newMethodName);
			Assert.That (changed, Is.True);
			eeditor.Verify (i => i.AttachHandlerAsync (info.Object, newMethodName));
			Assert.That (vm.Handlers, Contains.Item (newMethodName));
		}
		
		[Test]
		public void RemoveHandler()
		{
			const string newMethodName = "foo";
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var handlers = new List<string> { newMethodName };
			var editor = new Mock<IObjectEditor> ();
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.Setup (ee => ee.GetHandlersAsync (info.Object)).ReturnsAsync (handlers);
			eeditor.Setup (ee => ee.GetPotentialHandlersAsync (It.IsAny<IEventInfo> ())).ReturnsAsync (Array.Empty<string> ());
			eeditor.Setup (ee => ee.DetachHandlerAsync (It.IsAny<IEventInfo> (), It.IsAny<string> ()))
				.Callback<IEventInfo,string> ((ev, n) => {
					handlers.Remove (n);
					eeditor.Raise (ee => ee.EventHandlersChanged += null, new EventHandlersChangedEventArgs (ev));
				})
				.Returns (Task.CompletedTask);
			eeditor.SetupGet (e => e.Events).Returns (new[] { info.Object });

			var vm = new EventViewModel (MockEditorProvider.MockPlatform, info.Object, new[] { editor.Object });
			Assume.That (vm.Handlers, Is.Not.Empty);
			Assume.That (vm.Handlers, Contains.Item (newMethodName));

			bool changed = false;
			var incc = (INotifyCollectionChanged) vm.Handlers;
			incc.CollectionChanged += (o, e) => { changed = true; };

			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof (EventViewModel.Handlers))
					changed = true;
			};
			
			Assert.That (vm.RemoveHandlerCommand.CanExecute (newMethodName), Is.True);
			vm.RemoveHandlerCommand.Execute (newMethodName);
			Assert.That (changed, Is.True);
			eeditor.Verify (i => i.DetachHandlerAsync (info.Object, newMethodName));
			Assert.That (vm.Handlers, Does.Not.Contain (newMethodName));
		}

		[Test]
		public void CantRemoveNotPresentHandler()
		{
			const string newMethodName = "foo";
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var editor = new Mock<IObjectEditor> ();
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.Setup (ee => ee.GetHandlersAsync (info.Object)).ReturnsAsync (new[] { newMethodName });
			eeditor.Setup (ee => ee.GetPotentialHandlersAsync (It.IsAny<IEventInfo> ())).ReturnsAsync (Array.Empty<string> ());
			eeditor.Setup (ee => ee.AttachHandlerAsync (It.IsAny<IEventInfo> (), It.IsAny<string> ()))
				.Callback<IEventInfo,string> ((ev, n) => eeditor.Raise (ee => ee.EventHandlersChanged += null, new EventHandlersChangedEventArgs (ev)))
				.Returns (Task.CompletedTask);
			eeditor.SetupGet (e => e.Events).Returns (new[] { info.Object });

			var vm = new EventViewModel (MockEditorProvider.MockPlatform, info.Object, new[] { editor.Object });
			Assume.That (vm.Handlers, Is.Not.Empty);

			bool changed = false;
			var incc = (INotifyCollectionChanged) vm.Handlers;
			incc.CollectionChanged += (o, e) => { changed = true; };

			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof (EventViewModel.Handlers))
					changed = true;
			};

			const string notThere = "notThere";
			Assert.That (vm.RemoveHandlerCommand.CanExecute (notThere), Is.False);
		}

		[Test]
		public async Task DisagreeingValues()
		{
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var editor = new MockObjectEditor {
				Events = new[] { info.Object }
			};
			const string handler = "handler";
			await editor.AttachHandlerAsync (info.Object, handler);

			var editor2 = new MockObjectEditor {
				Events = new[] { info.Object }
			};
			await editor.AttachHandlerAsync (info.Object, "handler2");

			var ev = new EventViewModel (MockEditorProvider.MockPlatform, info.Object, new[] { editor, editor2 });

			Assert.That (ev.Handlers, Is.Empty);
			Assert.That (ev.MultipleValues, Is.True);
		}

		[Test]
		public async Task PotentialHandlers ()
		{
			const string handlerName = "foo";
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var potentialHandlers = new[] { handlerName };
			var handlers = new List<string> { "bar" };
			var editor = new Mock<IObjectEditor> ();
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.Setup (ee => ee.GetHandlersAsync (info.Object)).ReturnsAsync (handlers);
			eeditor.Setup (ee => ee.GetPotentialHandlersAsync (It.IsAny<IEventInfo> ())).ReturnsAsync (potentialHandlers);
			eeditor.Setup (ee => ee.DetachHandlerAsync (It.IsAny<IEventInfo> (), It.IsAny<string> ()))
				.Callback<IEventInfo,string> ((ev, n) => {
					handlers.Remove (n);
					eeditor.Raise (ee => ee.EventHandlersChanged += null, new EventHandlersChangedEventArgs (ev));
				})
				.Returns (Task.CompletedTask);
			eeditor.SetupGet (e => e.Events).Returns (new[] { info.Object });

			var vm = new EventViewModel (MockEditorProvider.MockPlatform, info.Object, new[] { editor.Object });
			var loadedPotentialHandlers = await vm.PotentialHandlers.Task;
			CollectionAssert.AreEqual (potentialHandlers, loadedPotentialHandlers);
		}

		[Test]
		[Description ("Suggested handlers that are already attached should be filtered out")]
		public void SuggestedHandlersFilteredByAttached ()
		{
			const string handlerName = "foo";
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var handlers = new List<string> { handlerName, "bar" };
			var editor = new Mock<IObjectEditor> ();
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.Setup (ee => ee.GetHandlersAsync (info.Object)).ReturnsAsync (handlers);
			eeditor.Setup (ee => ee.GetPotentialHandlersAsync (It.IsAny<IEventInfo> ())).ReturnsAsync (new[] { handlerName });
			eeditor.Setup (ee => ee.DetachHandlerAsync (It.IsAny<IEventInfo> (), It.IsAny<string> ()))
				.Callback<IEventInfo,string> ((ev, n) => {
					handlers.Remove (n);
					eeditor.Raise (ee => ee.EventHandlersChanged += null, new EventHandlersChangedEventArgs (ev));
				})
				.Returns (Task.CompletedTask);
			eeditor.SetupGet (e => e.Events).Returns (new[] { info.Object });

			var vm = new EventViewModel (MockEditorProvider.MockPlatform, info.Object, new[] { editor.Object });
			Assume.That (vm.Handlers, Is.Not.Empty);
			Assume.That (vm.Handlers, Contains.Item (handlerName));

			Assert.That (vm.PotentialHandlers.Value, Does.Not.Contain (handlerName));
		}

		[Test]
		public void SuggestedHandlerRemovedWhenAttached ()
		{
			const string handlerName = "foo";
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var handlers = new List<string> { "bar" };
			var editor = new Mock<IObjectEditor> ();
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.Setup (ee => ee.GetHandlersAsync (info.Object)).ReturnsAsync (handlers);
			eeditor.Setup (ee => ee.GetPotentialHandlersAsync (It.IsAny<IEventInfo> ())).ReturnsAsync (new[] { handlerName });
			eeditor.Setup (ee => ee.AttachHandlerAsync (It.IsAny<IEventInfo> (), It.IsAny<string> ()))
				.Callback<IEventInfo,string> ((ev, n) => {
					handlers.Add (n);
					eeditor.Raise (ee => ee.EventHandlersChanged += null, new EventHandlersChangedEventArgs (ev));
				})
				.Returns (Task.CompletedTask);
			eeditor.Setup (ee => ee.DetachHandlerAsync (It.IsAny<IEventInfo> (), It.IsAny<string> ()))
				.Callback<IEventInfo,string> ((ev, n) => {
					handlers.Remove (n);
					eeditor.Raise (ee => ee.EventHandlersChanged += null, new EventHandlersChangedEventArgs (ev));
				})
				.Returns (Task.CompletedTask);
			eeditor.SetupGet (e => e.Events).Returns (new[] { info.Object });

			var vm = new EventViewModel (MockEditorProvider.MockPlatform, info.Object, new[] { editor.Object });
			Assume.That (vm.Handlers, Does.Not.Contain (handlerName));
			Assume.That (vm.PotentialHandlers.Value, Contains.Item (handlerName));

			vm.AddHandlerCommand.Execute (handlerName);
			Assume.That (vm.Handlers, Contains.Item (handlerName));
			Assert.That (vm.PotentialHandlers.Value, Does.Not.Contain (handlerName));
		}

		[Test]
		public void SuggestedHandlerAddedWhenDetached ()
		{
			const string handlerName = "foo";
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var handlers = new List<string> { handlerName, "bar" };
			var editor = new Mock<IObjectEditor> ();
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.Setup (ee => ee.GetHandlersAsync (info.Object)).ReturnsAsync (handlers);
			eeditor.Setup (ee => ee.GetPotentialHandlersAsync (It.IsAny<IEventInfo> ())).ReturnsAsync (new[] { handlerName });
			eeditor.Setup (ee => ee.DetachHandlerAsync (It.IsAny<IEventInfo> (), It.IsAny<string> ()))
				.Callback<IEventInfo,string> ((ev, n) => {
					handlers.Remove (n);
					eeditor.Raise (ee => ee.EventHandlersChanged += null, new EventHandlersChangedEventArgs (ev));
				})
				.Returns (Task.CompletedTask);
			eeditor.SetupGet (e => e.Events).Returns (new[] { info.Object });

			var vm = new EventViewModel (MockEditorProvider.MockPlatform, info.Object, new[] { editor.Object });
			Assume.That (vm.Handlers, Contains.Item (handlerName));
			Assume.That (vm.PotentialHandlers.Value, Does.Not.Contains (handlerName));

			vm.RemoveHandlerCommand.Execute (handlerName);
			Assume.That (vm.Handlers, Does.Not.Contain (handlerName));
			Assert.That (vm.PotentialHandlers.Value, Contains.Item (handlerName));
		}

		[Test]
		public void CantAddIfAlreadyAdded ()
		{
			const string handlerName = "foo";
			const string name = "name";
			var info = new Mock<IEventInfo> ();
			info.SetupGet (i => i.Name).Returns (name);

			var handlers = new List<string> { handlerName };
			var editor = new Mock<IObjectEditor> ();
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.Setup (ee => ee.GetHandlersAsync (info.Object)).ReturnsAsync (handlers);
			eeditor.SetupGet (e => e.Events).Returns (new[] { info.Object });

			var vm = new EventViewModel (MockEditorProvider.MockPlatform, info.Object, new[] { editor.Object });
			Assume.That (vm.Handlers, Contains.Item (handlerName));

			Assert.That (vm.AddHandlerCommand.CanExecute (handlerName), Is.False);
		}
	}
}
