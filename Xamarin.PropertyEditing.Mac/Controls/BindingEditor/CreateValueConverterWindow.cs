using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CreateValueConverterWindow : NSPanel
	{
		private new ModalWindowCloseDelegate Delegate {
			get => (ModalWindowCloseDelegate)base.Delegate;
			set => base.Delegate = value;
		}

		public AddValueConverterViewModel ViewModel { get; }

		private NSTextField valueConverterName;
		public string ValueConverterName {
			get { return this.valueConverterName.Cell.Title; }
		}

		public CreateValueConverterWindow (CreateBindingViewModel viewModel, AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> typetasks)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			Delegate = new ModalWindowCloseDelegate ();
			ViewModel = new AddValueConverterViewModel (viewModel.TargetPlatform, viewModel.Target, typetasks);

			StyleMask |= NSWindowStyle.Resizable;

			Title = Properties.Resources.AddValueConverterTitle;

			MaxSize = new CGSize (500, 560); // TODO discuss what the Max/Min Size should be and if we should have one.
			MinSize = new CGSize (200, 320);

			var container = new NSView (new CGRect (CGPoint.Empty, new CGSize (400, 400))) {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			var valueConverterLabel = new UnfocusableTextField {
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, 13),
				StringValue = Properties.Resources.ValueConverterName + ":",
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			container.AddSubview (valueConverterLabel);

			container.AddConstraints (new[] {
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container, NSLayoutAttribute.Top, 1f, 18f),
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, container, NSLayoutAttribute.Left, 1f, 21f),
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 20),
			});

			this.valueConverterName = new NSTextField {
				ControlSize = NSControlSize.Regular,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			container.AddSubview (this.valueConverterName);

			container.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.valueConverterName, NSLayoutAttribute.Top, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Bottom, 1f, 1f),
				NSLayoutConstraint.Create (this.valueConverterName, NSLayoutAttribute.Left, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.valueConverterName, NSLayoutAttribute.Width, NSLayoutRelation.Equal, container, NSLayoutAttribute.Width, 1f, -40f),
			});

			var typeSelectorControl = new TypeSelectorControl {
				Flush = true,
				ViewModel = ViewModel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			container.AddSubview (typeSelectorControl);

			container.AddConstraints (new[] {
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.valueConverterName, NSLayoutAttribute.Bottom, 1f, 8f),
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.valueConverterName, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, container, NSLayoutAttribute.Height, 1f, -95f),
			});

			var buttonSelect = new NSButton {
				BezelStyle = NSBezelStyle.Rounded,
				ControlSize = NSControlSize.Regular,
				Enabled = false,
				Highlighted = true,
				KeyEquivalent = "\r", // Fire when enter pressed
				Title = Properties.Resources.Select,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			buttonSelect.Activated += (sender, e) => {
				Delegate.Response = NSModalResponse.OK;
				Close ();
			};

			container.AddSubview (buttonSelect);

			container.AddConstraints (new[] {
				NSLayoutConstraint.Create (buttonSelect, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, container, NSLayoutAttribute.Bottom, 1f, -20f),
				NSLayoutConstraint.Create (buttonSelect, NSLayoutAttribute.Right, NSLayoutRelation.Equal, typeSelectorControl, NSLayoutAttribute.Right, 1f, 0f),
				NSLayoutConstraint.Create (buttonSelect, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1f, 80f),
			});

			var buttonCancel = new NSButton {
				BezelStyle = NSBezelStyle.Rounded,
				ControlSize = NSControlSize.Regular,
				Title = Properties.Resources.Cancel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			buttonCancel.Activated += (sender, e) => {
				Delegate.Response = NSModalResponse.Cancel;
				Close ();
			};

			container.AddSubview (buttonCancel);

			container.AddConstraints (new[] {
				NSLayoutConstraint.Create (buttonCancel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, buttonSelect, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (buttonCancel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, buttonSelect, NSLayoutAttribute.Left, 1f, -10f),
				NSLayoutConstraint.Create (buttonCancel, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1f, 80f),
			});

			ContentViewController = new NSViewController (null, null) {
				View = container,
			};

			ViewModel.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof (AddValueConverterViewModel.SelectedType)) {
					this.valueConverterName.StringValue = ViewModel.SelectedType != null ? ViewModel.SelectedType.Name : string.Empty;
					buttonSelect.Enabled = ViewModel.SelectedType != null;
				}
			};
		}
	}
}
