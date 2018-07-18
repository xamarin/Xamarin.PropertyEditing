using System;
using System.ComponentModel;
using System.Linq;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class MaterialBrushEditorViewController : NotifyingViewController<BrushPropertyViewModel>
	{
		public MaterialBrushEditorViewController ()
		{
			PreferredContentSize = new CGSize (430, 230);
		}

		private MaterialView materialEditor;
		private AlphaChannelEditor alphaChannelEditor;
		private ComponentSpinEditor alphaSpinEditor;

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.Value):
				case nameof (BrushPropertyViewModel.MaterialDesign):
					if (this.materialEditor != null)
						this.materialEditor.ViewModel = ViewModel;
					if (this.alphaSpinEditor != null)
						alphaSpinEditor.Value = ViewModel.MaterialDesign.Alpha;
					break;
			}
		}

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			if (ViewLoaded && materialEditor != null)
				this.materialEditor.ViewModel = ViewModel;
		}

		void UpdateComponent (object sender, EventArgs args)
		{
			if (ViewModel == null)
				return;
			
			var editor = sender as NumericSpinEditor;
			ViewModel.MaterialDesign.Alpha = (byte)editor.Value;
		}

		public override void LoadView ()
		{
			var stack = new NSStackView () {
				Orientation = NSUserInterfaceLayoutOrientation.Vertical
			};

			this.materialEditor = new MaterialView {
				ViewModel = ViewModel
			};

			this.alphaChannelEditor = new AlphaChannelEditor ();
			this.alphaSpinEditor = new ComponentSpinEditor (this.alphaChannelEditor) {
				BackgroundColor = NSColor.Clear
			};
			this.alphaSpinEditor.ValueChanged += UpdateComponent;

			var alphaStack = new NSStackView () {
				Orientation = NSUserInterfaceLayoutOrientation.Horizontal
			};

			var alphaLabel = new NSTextField {
				Bordered = false,
				Editable = false,
				Selectable = false,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultPropertyLabelFontSize),
				AccessibilityElement = false,
				StringValue = $"{Properties.Resources.Alpha}:",
				Alignment = NSTextAlignment.Left,
				BackgroundColor = NSColor.Clear,
			};
			alphaLabel.Cell.UsesSingleLineMode = true;
			alphaLabel.Cell.LineBreakMode = NSLineBreakMode.Clipping;
			alphaStack.AddView (alphaLabel, NSStackViewGravity.Trailing);
			alphaStack.AddView (alphaSpinEditor, NSStackViewGravity.Trailing);

			stack.AddView (this.materialEditor, NSStackViewGravity.Leading);
			stack.AddView (alphaStack, NSStackViewGravity.Trailing);

			View = stack;
		}
	}
}
