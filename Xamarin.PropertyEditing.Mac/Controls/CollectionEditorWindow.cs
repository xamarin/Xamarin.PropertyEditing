using System;

using AppKit;
using CoreGraphics;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CollectionEditorWindow
		: NSWindow
	{
		public CollectionEditorWindow (IHostResourceProvider hostResources, CollectionPropertyViewModel viewModel)
			: base (new CGRect (0, 0, 500, 400), NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable, NSBackingStore.Buffered, true)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			Delegate = new ModalWindowCloseDelegate ();
			Title = String.Format (Properties.Resources.CollectionEditorTitle, viewModel.Property.Name);

			this.collectionEditor = new CollectionEditorControl (hostResources) {
				ViewModel = viewModel,
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			ContentView.AddSubview (this.collectionEditor);

			this.ok = new FocusableButton {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityCollectionOKButton,
				BezelStyle = NSBezelStyle.Rounded,
				ControlSize = NSControlSize.Regular,
				Highlighted = true,
				//KeyEquivalent = "\r", // FIXME: The type selector popup doesn't eat this key, so it ends up closing both.Sw
				Title = Properties.Resources.OK,
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			this.ok.Activated += OnOked;
			ContentView.AddSubview (this.ok);

			this.cancel = new FocusableButton {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityCollectionCancelButton,
				BezelStyle = NSBezelStyle.Rounded,
				ControlSize = NSControlSize.Regular,
				Title = Properties.Resources.Cancel,
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			this.cancel.Activated += OnCanceled;
			ContentView.AddSubview (this.cancel);

			ContentView.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.collectionEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Width, 1, -40),
				NSLayoutConstraint.Create (this.collectionEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Top, 1, 20),
				NSLayoutConstraint.Create (this.collectionEditor, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.CenterX, 1, 0),
				NSLayoutConstraint.Create (this.collectionEditor, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.ok, NSLayoutAttribute.Top, 1, -20),

				NSLayoutConstraint.Create (this.ok, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Bottom, 1, -20),
				NSLayoutConstraint.Create (this.ok, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.collectionEditor, NSLayoutAttribute.Right, 1, 0),
				NSLayoutConstraint.Create (this.ok, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.cancel, NSLayoutAttribute.Width, 1, 0),

				NSLayoutConstraint.Create (this.cancel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.ok, NSLayoutAttribute.Left, 1, -10),
				NSLayoutConstraint.Create (this.cancel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.ok, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (this.cancel, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1, 80)
			});
		}

		public NSModalResponse ModalResponse
		{
			get;
			private set;
		} = NSModalResponse.Cancel;

		private CollectionEditorControl collectionEditor;
		private NSButton ok, cancel;

		private void OnOked (object o, EventArgs e)
		{
			ModalResponse = NSModalResponse.OK;
			CloseWindow ();
		}

		private void OnCanceled (object o, EventArgs e)
		{
			ModalResponse = NSModalResponse.Cancel;
			CloseWindow ();
		}

		private void CloseWindow ()
		{
			this.collectionEditor.ViewModel = null;
			Close ();
		}

		public static void EditCollection (NSAppearance appearance, IHostResourceProvider hostResources, CollectionPropertyViewModel collectionVm)
		{
			var w = new CollectionEditorWindow (hostResources, collectionVm) {
				Appearance = appearance
			};

			var result = (NSModalResponse)(int)NSApplication.SharedApplication.RunModalForWindow (w);
			if (result != NSModalResponse.OK) {
				collectionVm.CancelCommand.Execute (null);
				return;
			}

			collectionVm.CommitCommand.Execute (null);
		}
	}
}