using System;
using System.Collections.Generic;
using System.Linq;

using AppKit;
using CoreGraphics;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class TypeSelectorWindow
		: NSWindow
	{
		public TypeSelectorWindow (TypeSelectorViewModel viewModel)
			: base (new CGRect (0, 0, 300, 300), NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable, NSBackingStore.Buffered, true)
		{
			Title = Properties.Resources.SelectObjectTitle;

			this.selector = new TypeSelectorControl {
				ViewModel = viewModel,
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			ContentView.AddSubview (this.selector);

			this.ok = NSButton.CreateButton (Properties.Resources.OK, OnOked);
			this.ok.TranslatesAutoresizingMaskIntoConstraints = false;
			ContentView.AddSubview (this.ok);

			this.cancel = NSButton.CreateButton (Properties.Resources.Cancel, OnCanceled);
			this.cancel.TranslatesAutoresizingMaskIntoConstraints = false;
			ContentView.AddSubview (this.cancel);

			ContentView.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.selector, NSLayoutAttribute.Width, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Width, 1, -20),
				NSLayoutConstraint.Create (this.selector, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (this.selector, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.CenterX, 1, 0),
				NSLayoutConstraint.Create (this.selector, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.ok, NSLayoutAttribute.Top, 1, -10),

				NSLayoutConstraint.Create (this.ok, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Bottom, 1, -10),
				NSLayoutConstraint.Create (this.ok, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Right, 1, -10),

				NSLayoutConstraint.Create (this.cancel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.ok, NSLayoutAttribute.Left, 1, -10),
				NSLayoutConstraint.Create (this.cancel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Bottom, 1, -10),
			});
		}

		private TypeSelectorControl selector;
		private NSButton ok, cancel;

		private void OnOked ()
		{
			NSApplication.SharedApplication.StopModalWithCode ((int)NSModalResponse.OK);
			Close ();
		}

		private void OnCanceled ()
		{
			NSApplication.SharedApplication.StopModalWithCode ((int)NSModalResponse.Cancel);
			Close ();
		}

		public static ITypeInfo RequestType (AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes)
		{
			var w = new TypeSelectorWindow (new TypeSelectorViewModel (assignableTypes));

			var result = (NSModalResponse)(int)NSApplication.SharedApplication.RunModalForWindow (w);
			if (result != NSModalResponse.OK)
				return null;

			return w.selector.ViewModel.SelectedType;
		}
	}
}
