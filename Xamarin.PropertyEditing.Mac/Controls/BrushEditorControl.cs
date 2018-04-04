using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BrushEditorControl : PropertyEditorControl
	{
		public BrushEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;
			RowHeight = 300f;
			this.comboBox = new NSComboBox {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BackgroundColor = NSColor.Clear,
				StringValue = "taco",

				ControlSize = NSControlSize.Small,
				Editable = false,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
			};

			this.comboBox.SelectionChanged += (sender, e) => {
				//ViewModel.ValueName = comboBox.SelectedValue.ToString ();
			};

			this.colorEditor = new SolidColorBrushEditor (new CGRect (0, 30, 239, 239));


			this.popUpButton = new NSPopUpButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
			};

			this.popover = new NSPopover ();

			popupButtonList = new NSMenu ();
			popUpButton.Menu = popupButtonList;

			popUpButton.Activated += (o, e) => {
				//ViewModel.ValueName = (o as NSPopUpButton).Title;
			};

			UpdateTheme ();
		}

		internal new BrushPropertyViewModel ViewModel
		{
			get { return (BrushPropertyViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		readonly NSComboBox comboBox;
		readonly NSPopUpButton popUpButton;
		readonly SolidColorBrushEditor colorEditor;
		readonly NSPopover popover;
		NSMenu popupButtonList;

		bool dataPopulated;

		public override NSView FirstKeyView => this.comboBox;

		public override NSView LastKeyView => this.comboBox;

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
		}

		protected override void SetEnabled ()
		{
		}

		protected override void UpdateAccessibilityValues ()
		{
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
		}

		protected override void UpdateValue ()
		{
			if (ViewModel.Solid != null) {
				this.comboBox.StringValue = ViewModel.Solid.Color.ToString ();
				this.popUpButton.StringValue = ViewModel.Solid.Color.ToString ();
				this.colorEditor.ViewModel = ViewModel.Solid;
			}
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			if (!dataPopulated) {
				this.DoConstraints (new[] {
						comboBox.ConstraintTo (this, (cb, c) => cb.Width == c.Width - 35),
						comboBox.ConstraintTo (this, (cb, c) => cb.Height == DefaultControlHeight),
						comboBox.ConstraintTo (this, (cb, c) => cb.Left == cb.Left + 4),
						comboBox.ConstraintTo (this, (cb, c) => cb.Top == cb.Top + 0),
					});
				AddSubview (this.comboBox);
				AddSubview (this.colorEditor);
			}
			UpdateValue ();
			dataPopulated = true;
		}
	}
}
