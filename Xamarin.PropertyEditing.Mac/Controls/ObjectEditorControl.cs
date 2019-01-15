using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ObjectEditorControl
		: PropertyEditorControl<ObjectPropertyViewModel>
	{
		public ObjectEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			this.typeLabel = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			AddSubview (this.typeLabel);

			this.createObject = new NSButton {
				Title = Properties.Resources.New,
				TranslatesAutoresizingMaskIntoConstraints = false,
				BezelStyle = NSBezelStyle.RoundRect
			};
			this.createObject.Activated += OnNewPressed;
			AddSubview (this.createObject);

			this.buttonConstraint = NSLayoutConstraint.Create (this.createObject, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this.typeLabel, NSLayoutAttribute.Trailing, 1f, 12);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.typeLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1f, 0f),
				NSLayoutConstraint.Create (this.typeLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.typeLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, 0),
				this.buttonConstraint,
				NSLayoutConstraint.Create (this.createObject, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 0).WithPriority (NSLayoutPriority.DefaultLow),
				NSLayoutConstraint.Create (this.createObject, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
			});
		}

		public override NSView FirstKeyView => this.createObject;

		public override NSView LastKeyView => this.createObject;

		protected override void UpdateValue ()
		{
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
		}

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
		}

		protected override void SetEnabled ()
		{
			this.createObject.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (oldModel is ObjectPropertyViewModel ovm) {
				ovm.TypeRequested -= OnTypeRequested;
				ovm.CreateInstanceCommand.CanExecuteChanged -= OnCreateInstanceExecutableChanged;
			}

			if (ViewModel != null) {
				ViewModel.TypeRequested += OnTypeRequested;
				ViewModel.CreateInstanceCommand.CanExecuteChanged += OnCreateInstanceExecutableChanged;

				OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (null));
			}
		}

		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
			case nameof (ObjectPropertyViewModel.ValueType):
				UpdateTypeLabel ();
				break;
			case null:
			case "":
				UpdateTypeLabel ();
				UpdateCreateInstanceCommand ();
				break;
			}

			base.OnPropertyChanged (sender, e);
		}

		private readonly UnfocusableTextField typeLabel;
		private readonly NSButton createObject;
		private readonly NSLayoutConstraint buttonConstraint;

		private void OnCreateInstanceExecutableChanged (object sender, EventArgs e)
		{
			UpdateCreateInstanceCommand ();
		}

		private void OnTypeRequested (object sender, TypeRequestedEventArgs e)
		{
			var tcs = new TaskCompletionSource<ITypeInfo> ();
			e.SelectedType = tcs.Task;

			var vm = new TypeSelectorViewModel (ViewModel.AssignableTypes);
			var selector = new TypeSelectorControl {
				ViewModel = vm,
				Appearance = EffectiveAppearance
			};

			vm.PropertyChanged += (vms, ve) => {
				if (ve.PropertyName == nameof (TypeSelectorViewModel.SelectedType)) {
					tcs.TrySetResult (vm.SelectedType);
				}
			};

			var popover = new NSPopover {
				Behavior = NSPopoverBehavior.Transient,
				Delegate = new PopoverDelegate<ITypeInfo> (tcs),
				ContentViewController = new NSViewController {
					View = selector,
					PreferredContentSize = new CoreGraphics.CGSize (360, 335)
				},
			};

			tcs.Task.ContinueWith (t => {
				popover.PerformClose (popover);
				popover.Dispose ();
			}, TaskScheduler.FromCurrentSynchronizationContext());

			popover.Show (new CoreGraphics.CGRect (this.createObject.Frame.Width / 2, 0, 2, 2), this.createObject, NSRectEdge.MinYEdge);
		}

		private void UpdateTypeLabel ()
		{
			if (ViewModel.ValueType == null) {
				this.typeLabel.StringValue = String.Empty;
				this.buttonConstraint.Active = false;
			} else {
				this.typeLabel.StringValue = $"({ViewModel.ValueType.Name})";
				this.buttonConstraint.Active = true;
			}
		}

		private void UpdateCreateInstanceCommand()
		{
			this.createObject.Enabled = ViewModel.CreateInstanceCommand.CanExecute (null);
		}

		private void OnNewPressed (object sender, EventArgs e)
		{
			ViewModel.CreateInstanceCommand.Execute (null);
		}

		private class PopoverDelegate<T>
			: NSPopoverDelegate
		{
			public PopoverDelegate (TaskCompletionSource<T> tcs)
			{
				this.tcs = tcs;
			}

			public override void WillClose (NSNotification notification)
			{
				this.tcs.TrySetCanceled ();
			}

			private readonly TaskCompletionSource<T> tcs;
		}
	}
}
