using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CreateVariantWindow : NSPanel
	{
		public CreateVariantViewModel ViewModel { get; }

		private new ModalWindowCloseDelegate Delegate
		{
			get => (ModalWindowCloseDelegate)base.Delegate;
			set => base.Delegate = value;
		}

		private const int minWindowWidth = 250;
		private const int minWindowHeight = 150;

		internal CreateVariantWindow (IHostResourceProvider hostResources, PropertyViewModel propertyViewModel)
			: base (new CGRect (0, 0, minWindowWidth, minWindowHeight), NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable, NSBackingStore.Buffered, true)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));
			if (propertyViewModel == null)
				throw new ArgumentNullException (nameof (propertyViewModel));

			Delegate = new ModalWindowCloseDelegate ();

			FloatingPanel = true;

			Title = Properties.Resources.AddVariationTitle;

			MinSize = new CGSize (minWindowWidth, minWindowHeight);

			// put the MainContainer inside this panel's ContentView
			ViewModel = new CreateVariantViewModel (propertyViewModel.Property);
			var createVariantView = new CreateVariantView (hostResources, new CGSize(minWindowWidth, minWindowHeight), ViewModel) {
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			ContentView.AddSubview (createVariantView);

			ContentView.AddConstraints (new[] {
				NSLayoutConstraint.Create (createVariantView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (createVariantView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (createVariantView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Height, 1f, 0f),
				NSLayoutConstraint.Create (createVariantView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Width, 1f, 0f),

			});

			var buttonOK = new FocusableCommandButton {
				BezelStyle = NSBezelStyle.Rounded,
				KeyEquivalent = "\r",
				Command = ViewModel.CreateVariantCommand,
				Enabled = false,
				Highlighted = true,
				Title = Properties.Resources.AddVariation,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			buttonOK.Activated += (sender, e) => {
				Delegate.Response = NSModalResponse.OK;
				Close ();
			};

			ContentView.AddSubview (buttonOK);

			ContentView.AddConstraints (new[] {
				NSLayoutConstraint.Create (buttonOK, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Bottom, 1, -CreateVariantView.RightEdgeMargin),
				NSLayoutConstraint.Create (buttonOK, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Right, 1, -CreateVariantView.RightEdgeMargin),
				NSLayoutConstraint.Create (buttonOK, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1, 80)
			});

			var buttonCancel = new FocusableCommandButton {
				BezelStyle = NSBezelStyle.Rounded,
				Title = Properties.Resources.Cancel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			buttonCancel.Activated += (sender, e) => {
				Close ();
			};

			ContentView.AddSubview (buttonCancel);

			ContentView.AddConstraints (new[] {
				NSLayoutConstraint.Create (buttonCancel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, buttonOK, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (buttonCancel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, buttonOK, NSLayoutAttribute.Left, 1f, -10f),
				NSLayoutConstraint.Create (buttonCancel, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1f, 80f),
			});
		}
	}
}
