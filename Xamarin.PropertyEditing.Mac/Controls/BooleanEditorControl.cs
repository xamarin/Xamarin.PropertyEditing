using System;
using System.Collections;
using System.Diagnostics;
using AppKit;
using Foundation;
using ObjCRuntime;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BooleanEditorControl : PropertyEditorControl
	{
		const string setBezelColorSelector = "setBezelColor:";

		public BooleanEditorControl ()
		{
			BooleanEditor = new NSButton () { TranslatesAutoresizingMaskIntoConstraints = false };
			BooleanEditor.SetButtonType (NSButtonType.Switch);

			BooleanEditor.Title = string.Empty;

			// update the value on 'enter'
			BooleanEditor.Activated += (sender, e) => {
				ViewModel.Value = BooleanEditor.State == NSCellStateValue.On ? true : false;
			};

			AddSubview (BooleanEditor);
		}

		internal NSButton BooleanEditor { get; set; }

		public override NSView FirstKeyView => BooleanEditor;
		public override NSView LastKeyView => BooleanEditor;

		public string Title { 
			get { return BooleanEditor.Title; } 
			set { BooleanEditor.Title = value; } 
		}

		internal new PropertyViewModel<bool> ViewModel {
			get { return (PropertyViewModel<bool>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		protected override void UpdateValue ()
		{
			BooleanEditor.State = ViewModel.Value ? NSCellStateValue.On : NSCellStateValue.Off;
			BooleanEditor.Title = ViewModel.Value.ToString ();
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				if (this.BooleanEditor.RespondsToSelector (new Selector (setBezelColorSelector))) {
					BooleanEditor.BezelColor = NSColor.Red;
				}
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			}
			else {
				if (this.BooleanEditor.RespondsToSelector (new Selector (setBezelColorSelector)) && BooleanEditor.Enabled) {
					BooleanEditor.BezelColor = NSColor.Clear;
				}
				SetEnabled ();
			}
		}

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void SetEnabled ()
		{
			BooleanEditor.Enabled = ViewModel.Property.CanWrite;
		}
	}
}
