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
			return subrowHeight * realVm.Choices.Count + 6;
		}

		protected override void SetEnabled ()
		{
			foreach (var item in this.combinableList) {
				item.Key.Enabled = ViewModel.Property.CanWrite;
			}
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (ViewModel == null)
				return;

			float top = 3;

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
					checkbox = new FocusableBooleanButton ();
					checkbox.Activated += SelectionChanged;

					AddSubview (checkbox);

					this.AddConstraints (new[] {
						NSLayoutConstraint.Create (checkbox, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, top),
						NSLayoutConstraint.Create (checkbox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
						NSLayoutConstraint.Create (checkbox, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, 0),
						NSLayoutConstraint.Create (checkbox, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, subrowHeight),
					});
				} else {
					checkbox = this.combinableList.KeyAt (i);
				}

				checkbox.Title = choice.Name;

				this.combinableList[checkbox] = choice;
				top += subrowHeight;
			}

			// Set our tabable order
			var firstButton = (FocusableBooleanButton) this.combinableList.KeyAt (0);
			this.firstKeyView = firstButton;
			
			var lastButton = (FocusableBooleanButton)this.combinableList.KeyAt (this.combinableList.Count - 1);
			this.lastKeyView = lastButton;

			if (combinableList.Count > 0)
			{
				if (firstButton == lastButton) {
					firstButton.ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView);
				} else {
					firstButton.ProxyResponder = new ProxyResponder (this, ProxyRowType.FirstView);
					lastButton.ProxyResponder = new ProxyResponder (this, ProxyRowType.LastView);
				}
			}

			SetEnabled ();

			UpdateAccessibilityValues ();
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
				item.Key.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityBoolean, ViewModel.Property.Name);
			}
		}

		private const int subrowHeight = 20;
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
