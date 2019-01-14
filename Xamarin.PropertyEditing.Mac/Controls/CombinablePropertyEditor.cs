using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

using Foundation;
using AppKit;
using CoreGraphics;

using Cadenza.Collections;
using Xamarin.PropertyEditing.ViewModels;
using Xamarin.PropertyEditing.Mac.Resources;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CombinablePropertyEditor<T>
		: PropertyEditorControl<CombinablePropertyViewModel<T>>
	{
		public CombinablePropertyEditor (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;
		}

		public override NSView FirstKeyView => this.firstKeyView;
		public override NSView LastKeyView => this.lastKeyView;

		public override bool IsDynamicallySized => true;

		public override nint GetHeight (EditorViewModel vm)
		{
			var realVm = (CombinablePropertyViewModel<T>)vm;
			return checkHeight * realVm.Choices.Count;
		}

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (e.PropertyName));
		}

		protected override void SetEnabled ()
		{
			foreach (var item in this.combinableList) {
				item.Key.Enabled = ViewModel.Property.CanWrite;
			}
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				SetErrors (errors);
			} else {
				SetErrors (null);
				SetEnabled ();
			}
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (ViewModel == null)
				return;

			nint rowHeight = GetHeight (ViewModel);

			float top = checkHeight;

			while (this.combinableList.Count > ViewModel.Choices.Count) {
				var child = this.combinableList.KeyAt (ViewModel.Choices.Count);
				child.RemoveFromSuperview ();
				this.combinableList.RemoveAt (ViewModel.Choices.Count);
			}

			int i = 0;
			for (; i < ViewModel.Choices.Count; i++) {
				var choice = ViewModel.Choices[i];

				NSButton checkbox;
				if (i >= this.combinableList.Count) {
					checkbox = new NSButton {
						AllowsMixedState = true,
						ControlSize = NSControlSize.Small,
						Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
						TranslatesAutoresizingMaskIntoConstraints = false,
					};

					checkbox.SetButtonType (NSButtonType.Switch);
					checkbox.Activated += SelectionChanged;

					AddSubview (checkbox);
				} else {
					checkbox = this.combinableList.KeyAt (i);
				}

				checkbox.Title = choice.Name;
				checkbox.Frame = new CGRect (0, rowHeight - top, Frame.Width, checkHeight);

				this.combinableList[checkbox] = choice;
				top += checkHeight;
			}

			// Set our tabable order
			this.firstKeyView = this.combinableList.KeyAt (0);
			this.lastKeyView = this.combinableList.KeyAt (this.combinableList.Count - 1);
		}

		protected override void UpdateValue ()
		{
			foreach (var item in this.combinableList) {
				if (item.Value.IsFlagged.HasValue) {
					item.Key.AllowsMixedState = false;
					item.Key.State = item.Value.IsFlagged.Value ? NSCellStateValue.On : NSCellStateValue.Off;
				} else {
					item.Key.AllowsMixedState = true;
					item.Key.State = NSCellStateValue.Mixed;
				}
			}
		}

		protected override void UpdateAccessibilityValues ()
		{
			foreach (var item in combinableList) {
				item.Key.AccessibilityEnabled = item.Key.Enabled;
				item.Key.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityCombobox, ViewModel.Property.Name);
			}
		}

		private const int checkHeight = 22;
		private readonly OrderedDictionary<NSButton, FlaggableChoiceViewModel<T>> combinableList = new OrderedDictionary<NSButton, FlaggableChoiceViewModel<T>> ();
		private NSView firstKeyView;
		private NSView lastKeyView;

		private void SelectionChanged (object sender, EventArgs e)
		{
			if (sender is NSButton button) {
				var choice = this.combinableList[button];
				if (choice.Value.Equals (default (T)) && (button.State == NSCellStateValue.On)) {
					foreach (var item in this.combinableList) {
						if (!item.Value.Equals (default (T))) {
							item.Value.IsFlagged = false;
						}
					}
				}
				switch (button.State) {
					case NSCellStateValue.Off:
						choice.IsFlagged = false;
						break;
					case NSCellStateValue.On:
						choice.IsFlagged = true;
						break;
				}
			}
		}
	}
}
