using System;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class AutoResizingMaskEditorControl
		: PropertyEditorControl<AutoResizingPropertyViewModel>
	{
		private readonly AutoResizingView sizeInspectorView;

		public AutoResizingMaskEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{

			this.sizeInspectorView = new AutoResizingView (hostResources) {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.sizeInspectorView.MaskView.MaskChanged += (o, e) => {
				ViewModel.Value = this.sizeInspectorView.MaskView.Mask;
			};

			AddSubview (this.sizeInspectorView);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.sizeInspectorView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create (this.sizeInspectorView, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1, 70),
				NSLayoutConstraint.Create (this.sizeInspectorView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, -6)
			});

			AppearanceChanged ();
		}

		#region Overridden Methods and Properties

		private NSView firstKeyView;
		public override NSView FirstKeyView => this.firstKeyView;
		private NSView lastKeyView;
		public override NSView LastKeyView => this.lastKeyView;

		public override nint GetHeight (EditorViewModel vm)
		{
			return 200;
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (ViewModel == null)
				return;

			if (this.firstKeyView == null) {
				this.firstKeyView = this.sizeInspectorView.MaskView;
				this.lastKeyView = this.sizeInspectorView.MaskView;
			}
		}

		protected override void SetEnabled ()
		{
			this.sizeInspectorView.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.sizeInspectorView.AccessibilityEnabled = this.sizeInspectorView.Enabled;
			this.sizeInspectorView.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityBoolean, ViewModel.Property.Name);
			this.sizeInspectorView.UpdateAccessibilityValues ();
		}

		protected override void UpdateValue ()
		{
			this.sizeInspectorView.MaskView.Mask = ViewModel.Value;
		}
		#endregion
	}
}
