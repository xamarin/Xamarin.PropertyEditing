using System;
using System.Collections;
using System.Diagnostics;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BooleanEditorControl : PropertyEditorControl
	{
		static NSProcessInfo processInfo;
		static bool BezelColourAvailable;
		public BooleanEditorControl ()
		{
			BooleanEditor = new NSButton () { TranslatesAutoresizingMaskIntoConstraints = false };
			BooleanEditor.SetButtonType (NSButtonType.Switch);

			// Only do this check once and store the value for future use.
			if (processInfo == null) {
				processInfo = new NSProcessInfo ();
				BezelColourAvailable = processInfo.IsOperatingSystemAtLeastVersion (new NSOperatingSystemVersion (10, 12, 1));
			}

			if (BezelColourAvailable) {
				BooleanEditor.BezelColor = NSColor.Clear;
			}

			BooleanEditor.Title = string.Empty;

			// update the value on 'enter'
			BooleanEditor.Activated += (sender, e) => {
				ViewModel.Value = BooleanEditor.State == NSCellStateValue.On ? true : false;
			};
			AddSubview (BooleanEditor);
		}

		internal NSButton BooleanEditor { get; set; }

		public string Title { 
			get { return BooleanEditor.Title; } 
			set { BooleanEditor.Title = value; } 
		}

		internal new PropertyViewModel<bool> ViewModel {
			get { return (PropertyViewModel<bool>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		protected override void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PropertyViewModel<bool>.Value)) {
				UpdateModelValue ();
			}
		}

		protected override void UpdateModelValue ()
		{
			BooleanEditor.State = ViewModel.Value ? NSCellStateValue.On : NSCellStateValue.Off;
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				if (BezelColourAvailable) {
					BooleanEditor.BezelColor = NSColor.Red;
				}
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			}
			else {
				if (BezelColourAvailable) {
					BooleanEditor.BezelColor = NSColor.Clear;
				}
			}
		}

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}
	}
}
