using System;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;

using Foundation;
using AppKit;
using CoreGraphics;

using Xamarin.PropertyEditing.ViewModels;
using System.Collections.Generic;
using ObjCRuntime;
using System.Linq;
using System.Collections.ObjectModel;
using Xamarin.PropertyEditing.Reflection;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PredefinedValuesEditor<T>
		: PropertyEditorControl
	{
		const string setBezelColorSelector = "setBezelColor:";

		private readonly NSComboBox comboBox;
		List<NSButton> combinableList = new List<NSButton> ();
		bool dataPopulated;

		public PredefinedValuesEditor ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			this.comboBox = new NSComboBox () {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BackgroundColor = NSColor.Clear,
				StringValue = String.Empty,
				Cell = {
					ControlSize = NSControlSize.Regular	
				}
			};

			this.comboBox.SelectionChanged += (sender, e) => {
				EditorViewModel.ValueName = comboBox.SelectedValue.ToString ();
				dataPopulated = false;
			};
		}

		public override NSView FirstKeyView => this.comboBox;
		public override NSView LastKeyView => this.comboBox;

		protected PredefinedValuesViewModel<T> EditorViewModel => (PredefinedValuesViewModel<T>)ViewModel;

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (e.PropertyName));
		}

		protected override void SetEnabled ()
		{
			if (EditorViewModel.IsCombinable) {
				foreach (var item in combinableList) {
					this.comboBox.Enabled = ViewModel.Property.CanWrite;
				}
			} 
			else {
				this.comboBox.Editable = ViewModel.Property.CanWrite;
			}
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				if (EditorViewModel.IsCombinable) {
					foreach (var item in combinableList) {
						if (item.RespondsToSelector (new Selector (setBezelColorSelector))) {
							item.BezelColor = NSColor.Red;
						}
					}
				} else {
					this.comboBox.BackgroundColor = NSColor.Red;
				}
						    
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			} else {
				if (EditorViewModel.IsCombinable) {
					foreach (var item in combinableList) {
						if (item.RespondsToSelector (new Selector (setBezelColorSelector))) {
							item.BezelColor = NSColor.Clear;
						}
					}
				} else {
					comboBox.BackgroundColor = NSColor.Clear;
				}
				SetEnabled ();
			}
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			if (!dataPopulated) {
				if (EditorViewModel.IsCombinable) {
					combinableList.Clear ();

					var top = 0;
					foreach (var item in EditorViewModel.PossibleValues) {
						var BooleanEditor = new NSButton (new CGRect (0, top, 200, 24)) { TranslatesAutoresizingMaskIntoConstraints = false };
						BooleanEditor.SetButtonType (NSButtonType.Switch);
						BooleanEditor.Title = item.Key;
						BooleanEditor.State = item.Value.Checked ? NSCellStateValue.On : NSCellStateValue.Off;
						BooleanEditor.Activated += BooleanEditor_Activated;

						AddSubview (BooleanEditor);
						combinableList.Add (BooleanEditor);
						top += 24;
					}

					// Set our new RowHeight
					RowHeight = top;
				} else {
					this.comboBox.RemoveAll ();

					// Once the VM is loaded we need a one time population
					foreach (var item in EditorViewModel.PossibleValues) {
						this.comboBox.Add (new NSString (item.Key));
					}

					AddSubview (this.comboBox);

					this.DoConstraints (new[] {
						comboBox.ConstraintTo (this, (cb, c) => cb.Width == c.Width),
						comboBox.ConstraintTo (this, (cb, c) => cb.Left == c.Left),
					});
				}

				dataPopulated = true;
			}

			base.OnViewModelChanged (oldModel);
		}

		void BooleanEditor_Activated (object sender, EventArgs e)
		{
			var buttonTitles = combinableList.Where (y => y.State == NSCellStateValue.On).Select (x => x.Title);
			T realValue = default (T);
			Func<T, T, T> or = DynamicBuilder.GetOrOperator<T> ();
			foreach (var pvalue in EditorViewModel.PossibleValues) {
				foreach (var title in buttonTitles) {
					if (pvalue.Key == title) {
						realValue = or (realValue, pvalue.Value.Value);
					}
				}
			}

			EditorViewModel.Value = realValue;
			dataPopulated = false;
		}

		protected override void UpdateValue ()
		{
			if (EditorViewModel.IsCombinable) {
				foreach (var item in combinableList) {
					item.State = EditorViewModel.PossibleValues[item.Title].Checked ? NSCellStateValue.On : NSCellStateValue.Off;
				}
			} else {
				this.comboBox.StringValue = EditorViewModel.ValueName ?? String.Empty;
			}
		}

		protected override void UpdateAccessibilityValues ()
		{
			if (EditorViewModel.IsCombinable) {
				foreach (var item in combinableList) {
					item.AccessibilityEnabled = item.Enabled;
					item.AccessibilityTitle = ViewModel.Property.Name + " Enumeration Flag"; // TODO Localizatiion
				}
			} else {
				comboBox.AccessibilityEnabled = comboBox.Enabled;
				comboBox.AccessibilityTitle = ViewModel.Property.Name + " Enumeration Combo Box"; // TODO Localizatiion
			}
		}
	}
}
