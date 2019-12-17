using System;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class EntryPropertyEditor<T>
		: PropertyEditorControl<PropertyViewModel<T>>
	{
		public EntryPropertyEditor (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			Entry = new PropertyTextField {
				BackgroundColor = NSColor.Clear,
				ControlSize = NSControlSize.Small,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (Entry);

			RightEdgeConstraint = NSLayoutConstraint.Create (Entry, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0);
			AddConstraints (new[] {
				NSLayoutConstraint.Create (Entry, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Bottom, 1f, BottomOffset),
				NSLayoutConstraint.Create (Entry, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0),
				RightEdgeConstraint,
			});
		}

		public override NSView FirstKeyView => Entry;
		public override NSView LastKeyView => Entry;

		protected PropertyTextField Entry
		{
			get;
		}

		protected NSLayoutConstraint RightEdgeConstraint { get; }

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);
			Entry.Delegate = (ViewModel != null) ? CreateDelegate (ViewModel) : null;
		}

		protected override void UpdateValue ()
		{
			Entry.StringValue = GetValue (ViewModel.Value) ?? String.Empty;
		}

		protected override void SetEnabled ()
		{
			Entry.Enabled = ViewModel.IsInputEnabled;
		}

		protected override void UpdateAccessibilityValues ()
		{
			Entry.AccessibilityEnabled = Entry.Enabled;
		}

		protected virtual EntryPropertyEditorDelegate<T> CreateDelegate (PropertyViewModel<T> viewModel)
		{
			return new EntryPropertyEditorDelegate<T> (viewModel);
		}

		protected virtual string GetValue (T value)
		{
			return value?.ToString ();
		}
	}

	internal class EntryPropertyEditorDelegate<T>
		: NSTextFieldDelegate
	{
		public EntryPropertyEditorDelegate (PropertyViewModel<T> viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			ViewModel = viewModel;
		}

		protected PropertyViewModel<T> ViewModel
		{
			get;
		}

		public override void EditingBegan (NSNotification notification)
		{
			NSTextField text = (NSTextField)notification.Object;
			this.lastValid = text.StringValue;
		}

		public override void EditingEnded (NSNotification notification)
		{
			var text = (NSTextField)notification.Object;
			ViewModel.Value = GetValue (text.StringValue);
		}

		public override bool TextShouldEndEditing (NSControl control, NSText fieldEditor)
		{
			if (!CanGetValue (fieldEditor.Value)) {
				NSTextField text = (NSTextField)control;
				text.StringValue = this.lastValid;
				AppKitFramework.NSBeep ();
				return false;
			}

			return true;
		}

		protected virtual T GetValue (string value)
		{
			if (String.IsNullOrEmpty (value))
				return default (T);

			return (T)Convert.ChangeType (value, typeof(T));
		}

		protected virtual bool CanGetValue (string value)
		{
			return true;
		}

		private string lastValid;
	}
}
