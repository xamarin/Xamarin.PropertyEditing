using System;
using System.Collections;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BooleanEditorControl
		: PropertyEditorControl<PropertyViewModel<bool?>>
	{
		public BooleanEditorControl (IHostResourceProvider hostResource)
			: base (hostResource)
		{
			BooleanEditor = new FocusableBooleanButton ()
			{
				ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView)
			};
			BooleanEditor.Title = string.Empty;

			// update the value on 'enter'
			BooleanEditor.Activated += (sender, e) => {
				switch (BooleanEditor.State) {
					case NSCellStateValue.Off:
						ViewModel.Value = false;
						break;
					case NSCellStateValue.On:
						ViewModel.Value = true;
						break;
				}
			};

			AddSubview (BooleanEditor);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (BooleanEditor, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (BooleanEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0f),
			});
		}

		internal NSButton BooleanEditor { get; set; }

		public override NSView FirstKeyView => BooleanEditor;
		public override NSView LastKeyView => BooleanEditor;

		public string Title { 
			get { return BooleanEditor.Title; } 
			set { BooleanEditor.Title = value; } 
		}

		protected override void UpdateValue ()
		{
			if (ViewModel.Value.HasValue) {
				BooleanEditor.State = ViewModel.Value.Value ? NSCellStateValue.On : NSCellStateValue.Off;
				BooleanEditor.AllowsMixedState = false;
			} else {
				BooleanEditor.AllowsMixedState = true;
				BooleanEditor.State = NSCellStateValue.Mixed;
			}
		}

		protected override void SetEnabled ()
		{
			BooleanEditor.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			BooleanEditor.AccessibilityEnabled = BooleanEditor.Enabled;
			BooleanEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityBoolean, ViewModel.Property.Name);
		}
	}
}
