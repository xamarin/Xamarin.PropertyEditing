using System;
using System.ComponentModel;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class StateGroupEditorControl : PropertyEditorControl<StatePropertyGroupViewModel>
	{
		private FocusableComboBox stateGroupComboBox;
		private StateGroupEditor stateGroupEditor;

		private NSView firstkeyView;
		private NSView lastkeyView;
		public override NSView FirstKeyView => firstkeyView;
		public override NSView LastKeyView => lastkeyView;

		public StateGroupEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			AppearanceChanged ();
		}

		#region Overridden properties and Methods

		protected override void AppearanceChanged ()
		{
			base.AppearanceChanged ();
		}

		protected override void UpdateValue ()
		{

		}

		protected override void SetEnabled ()
		{
			if (ViewModel != null) {
				if (ViewModel.CanDelve) {
					this.stateGroupComboBox.Enabled = ViewModel.HostedProperty.Property.CanWrite;
				} else {
					this.stateGroupEditor.Enabled = ViewModel.HostedProperty.Property.CanWrite;
				}
			}
		}

		protected override void OnViewModelChanged (EditorViewModel oldModel)
		{
			if (ViewModel != null) {
				if (ViewModel.CanDelve) {
					RequireStateGroupComboBox (ViewModel.HostedProperty);
				} else {
					RequireStateGroupEditor (ViewModel.HostedProperty);
				}

				OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (null));
			}

			base.OnViewModelChanged (oldModel);
		}

		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			base.OnPropertyChanged (sender, e);
		}
		#endregion

		private void RequireStateGroupEditor (PropertyViewModel hostedProperty)
		{
			if (this.stateGroupEditor != null)
				return;

			RemoveStateGroupComboBox ();

			this.stateGroupEditor = new StateGroupEditor(HostResources, ViewModel) {
				AllowsExpansionToolTips = true,
				ControlSize = NSControlSize.Small,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.stateGroupEditor.UpdateAccessibilityValues ();
			this.stateGroupEditor.Enabled = ViewModel.HostedProperty.Property.CanWrite;

			AddSubview (this.stateGroupEditor);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.stateGroupEditor, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.stateGroupEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0f),
			});

			this.firstkeyView = this.stateGroupEditor.StateGroupCombobox;
			this.lastkeyView = this.stateGroupEditor.StateGroupEditButton;
		}

		private void RemoveStateGroupEditor ()
		{
			if (this.stateGroupEditor == null)
				return;

			this.stateGroupEditor.RemoveFromSuperview ();
			this.stateGroupEditor.Dispose ();
			this.stateGroupEditor = null;
		}

		private void RequireStateGroupComboBox (PropertyViewModel hostedProperty)
		{
			if (this.stateGroupComboBox != null)
				return;

			RemoveStateGroupEditor ();

			this.stateGroupComboBox = new FocusableComboBox {
				AllowsExpansionToolTips = true,
				BackgroundColor = NSColor.Clear,
				Cell = {
					LineBreakMode = NSLineBreakMode.TruncatingTail,
					UsesSingleLineMode = true,
				},
				ControlSize = NSControlSize.Small,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
			};

			this.stateGroupComboBox.SelectionChanged += (sender, e) => {
				//hostedProperty.Value = this.stateGroupComboBox.SelectedValue.ToString ();
			};

			// Once the VM is loaded we need a one time population
			foreach (var item in ViewModel.Properties) {
				this.stateGroupComboBox.Add (new NSString (item.Name));
			}

			this.stateGroupComboBox.Enabled = ViewModel.HostedProperty.Property.CanWrite;

			AddSubview (this.stateGroupComboBox);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.stateGroupComboBox, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.stateGroupComboBox, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0f),
			});

			this.firstkeyView = this.stateGroupComboBox;
			this.lastkeyView = this.stateGroupComboBox;
		}

		private void RemoveStateGroupComboBox ()
		{
			if (this.stateGroupComboBox == null)
				return;

			this.stateGroupComboBox.RemoveFromSuperview ();
			this.stateGroupComboBox.Dispose ();
			this.stateGroupComboBox = null;
		}
	}
}
