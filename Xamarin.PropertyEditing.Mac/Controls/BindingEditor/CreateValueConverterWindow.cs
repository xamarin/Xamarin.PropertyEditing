using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CreateValueConverterWindow : NSWindow
	{
		public AddValueConverterViewModel ViewModel { get; }

		private NSTextField valueConverterName;
		public string ValueConverterName {
			get { return this.valueConverterName.Cell.Title; }
		}

		public CreateValueConverterWindow (CreateBindingViewModel viewModel, AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> typetasks)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			ViewModel = new AddValueConverterViewModel (viewModel.TargetPlatform, viewModel.Target, typetasks);

			StyleMask |= NSWindowStyle.Resizable;

			Title = Properties.Resources.AddValueConverterTitle;

			MaxSize = new CGSize (500, 560); // TODO discuss what the Max/Min Size should be and if we should have one.
			MinSize = new CGSize (200, 320);

			var container = new NSView (new CGRect (CGPoint.Empty, new CGSize (400, 400))) {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			var valueConverterLabel = new UnfocusableTextField {
				StringValue = Properties.Resources.ValueConverterName,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			container.AddSubview (valueConverterLabel);

			container.AddConstraints (new[] {
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, container, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
			});

			this.valueConverterName = new NSTextField {
				ControlSize = NSControlSize.Small,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			container.AddSubview (this.valueConverterName);

			container.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.valueConverterName, NSLayoutAttribute.Top, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Bottom, 1f, 1f),
				NSLayoutConstraint.Create (this.valueConverterName, NSLayoutAttribute.Left, NSLayoutRelation.Equal, container, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (this.valueConverterName, NSLayoutAttribute.Width, NSLayoutRelation.Equal, container, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (this.valueConverterName, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
			});

			var typeSelectorControl = new TypeSelectorControl {
				ViewModel = ViewModel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			container.AddSubview (typeSelectorControl);

			container.AddConstraints (new[] {
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container, NSLayoutAttribute.Top, 1f, 45f),
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, container, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, container, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, container, NSLayoutAttribute.Height, 1f, -50f)
			});

			var buttonDone = new NSButton {
				BezelStyle = NSBezelStyle.Rounded,
				Highlighted = true,
				KeyEquivalent = "\r", // Fire when enter pressed
				Title = Properties.Resources.DoneTitle,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			buttonDone.Activated += (sender, e) => {
				NSApplication.SharedApplication.StopModalWithCode ((int)NSModalResponse.OK);
				Close ();
			};

			container.AddSubview (buttonDone);

			container.AddConstraints (new[] {
				NSLayoutConstraint.Create (buttonDone, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container, NSLayoutAttribute.Bottom, 1f, -32f),
				NSLayoutConstraint.Create (buttonDone, NSLayoutAttribute.Right, NSLayoutRelation.Equal, container, NSLayoutAttribute.Right, 1f, -16f),
				NSLayoutConstraint.Create (buttonDone, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
			});

			ContentViewController = new NSViewController (null, null) {
				View = container,
			};

			ViewModel.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof (ViewModel.SelectedType)) {
					this.valueConverterName.StringValue = ViewModel.SelectedType.Name;
				}
			};
		}

		public override void KeyUp (NSEvent theEvent)
		{
			if (theEvent.KeyCode == 53) {
				NSApplication.SharedApplication.StopModalWithCode ((int)NSModalResponse.Cancel);
				Close ();
			} else {
				base.KeyUp (theEvent);
			}
		}
	}
}
