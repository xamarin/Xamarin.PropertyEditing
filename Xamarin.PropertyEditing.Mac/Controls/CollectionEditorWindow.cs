using System;
using System.Collections.Generic;
using System.Linq;

using AppKit;
using CoreGraphics;
using Foundation;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CollectionEditorWindow
		: NSWindow
	{
		public CollectionEditorWindow (IHostResourceProvider hostResources, CollectionPropertyViewModel viewModel)
			: base (new CGRect (0, 0, 500, 400), NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable, NSBackingStore.Buffered, true)
		{
			Delegate = new ModalDialogDelegate ();
			Title = String.Format (Resources.LocalizationResources.CollectionEditorTitle, viewModel.Property.Name);

			this.collectionEditor = new CollectionEditorControl (hostResources) {
				ViewModel = viewModel,
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			ContentView.AddSubview (this.collectionEditor);

			this.ok = NSButton.CreateButton (Properties.Resources.OK, OnOked);
			this.ok.TranslatesAutoresizingMaskIntoConstraints = false;
			//this.ok.KeyEquivalent = "\r"; // FIXME: The type selector popup doesn't eat this key, so it ends up closing both.
			ContentView.AddSubview (this.ok);

			this.cancel = NSButton.CreateButton (Properties.Resources.Cancel, OnCanceled);
			this.cancel.TranslatesAutoresizingMaskIntoConstraints = false;
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

		private void OnOked ()
		{
			ModalResponse = NSModalResponse.OK;
			this.collectionEditor.ViewModel = null;
			Close ();
		}

		private void OnCanceled ()
		{
			ModalResponse = NSModalResponse.Cancel;
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

		private class ModalDialogDelegate
			: NSWindowDelegate
		{
			public override void WillClose (NSNotification notification)
			{
				NSModalResponse response = ((CollectionEditorWindow)notification.Object).ModalResponse;
				NSApplication.SharedApplication.StopModalWithCode ((int)response);
			}
		}
	}
}