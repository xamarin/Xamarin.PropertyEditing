using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BooleanEditorControl : PropertyEditorControl
	{
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
	}
}
