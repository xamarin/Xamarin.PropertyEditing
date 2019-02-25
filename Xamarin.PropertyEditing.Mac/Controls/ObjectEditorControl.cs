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

			this.createObject = new FocusableButton {
				Title = Properties.Resources.New,
				BezelStyle = NSBezelStyle.Rounded
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
				NSLayoutConstraint.Create (this.createObject, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1f, 70f),
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
			this.createObject.AccessibilityTitle = String.Format (Properties.Resources.NewInstanceForProperty, ViewModel.Property.Name);
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
			e.SelectedType = e.RequestAt (HostResources, this.createObject, ViewModel.AssignableTypes);
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
	}
}
