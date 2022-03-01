using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class TypeEditorControl
		: PropertyEditorControl<TypePropertyViewModel>
	{
		public TypeEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			this.typeLabel = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			AddSubview (this.typeLabel);

			this.selectType = new FocusableButton {
				BezelStyle = NSBezelStyle.Rounded,
				Title = Properties.Resources.Select,
				ProxyResponder = new ProxyResponder(this, ProxyRowType.SingleView)
			};

			this.selectType.Activated += OnSelectPressed;
			AddSubview (this.selectType);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.typeLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.typeLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.typeLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, 0),
				NSLayoutConstraint.Create (this.typeLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.selectType, NSLayoutAttribute.Left, 1, -4),

				NSLayoutConstraint.Create (this.selectType, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0),
				NSLayoutConstraint.Create (this.selectType, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.selectType, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, DefaultButtonWidth),
			});
		}

		public override NSView FirstKeyView => this.selectType;

		public override NSView LastKeyView => this.selectType;

		protected override void UpdateValue ()
		{
		}

		protected override void SetEnabled ()
		{
			this.selectType.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.selectType.AccessibilityEnabled = this.selectType.Enabled;
			this.selectType.AccessibilityTitle = string.Format (Properties.Resources.SelectTypeForProperty, ViewModel.Property.Name);
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (oldModel is TypePropertyViewModel tvm) {
				tvm.TypeRequested -= OnTypeRequested;
			}

			if (ViewModel != null) {
				ViewModel.TypeRequested += OnTypeRequested;

				OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (null));
			}
		}

		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
			case nameof (TypePropertyViewModel.Value):
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
		private readonly FocusableButton selectType;

		private void OnTypeRequested (object sender, TypeRequestedEventArgs e)
		{
			e.SelectedType = e.RequestAt (HostResources, this.selectType, ViewModel.AssignableTypes);
		}

		private void UpdateTypeLabel ()
		{
			if (ViewModel.Value == null) {
				this.typeLabel.StringValue = $"({Properties.Resources.ObjectTypeLabelNone})";
			} else {
				this.typeLabel.StringValue = $"({ViewModel.Value.Name})";
			}
		}

		private void UpdateCreateInstanceCommand ()
		{
			this.selectType.Enabled = ViewModel.SelectTypeCommand.CanExecute (null);
		}

		private void OnSelectPressed (object sender, EventArgs e)
		{
			ViewModel.SelectTypeCommand.Execute (null);
		}
	}
}
