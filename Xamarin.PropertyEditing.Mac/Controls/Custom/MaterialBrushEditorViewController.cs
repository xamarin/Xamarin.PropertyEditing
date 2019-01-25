using System;
using System.ComponentModel;
using System.Linq;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class MaterialBrushEditorViewController
		: NotifyingViewController<BrushPropertyViewModel>
	{
		public MaterialBrushEditorViewController (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;
			PreferredContentSize = new CGSize (430, 230);
		}

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.Value):
				case nameof (BrushPropertyViewModel.MaterialDesign):
					if (this.materialEditor != null)
						this.materialEditor.ViewModel = ViewModel?.MaterialDesign;
					if (this.alphaSpinEditor != null)
						alphaSpinEditor.Value = ViewModel.MaterialDesign.Alpha;
					break;
			}
		}

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			if (ViewLoaded && materialEditor != null)
				this.materialEditor.ViewModel = ViewModel?.MaterialDesign;
		}

		public override void LoadView ()
		{
			var stack = new NSStackView () {
				Orientation = NSUserInterfaceLayoutOrientation.Vertical
			};

			this.materialEditor = new MaterialView {
				ViewModel = ViewModel?.MaterialDesign
			};

			this.alphaChannelEditor = new AlphaChannelEditor ();
			this.alphaSpinEditor = new ComponentSpinEditor (this.hostResources, this.alphaChannelEditor) {
				BackgroundColor = NSColor.Clear
			};
			this.alphaSpinEditor.ValueChanged += UpdateComponent;

			var alphaStack = new NSStackView () {
				Orientation = NSUserInterfaceLayoutOrientation.Horizontal
			};

			var alphaLabel = new UnfocusableTextField {
				StringValue = $"{Properties.Resources.Alpha}:",
				Alignment = NSTextAlignment.Left,
			};
			alphaLabel.Cell.LineBreakMode = NSLineBreakMode.Clipping;

			alphaStack.AddView (alphaLabel, NSStackViewGravity.Trailing);
			alphaStack.AddView (alphaSpinEditor, NSStackViewGravity.Trailing);

			stack.AddView (this.materialEditor, NSStackViewGravity.Leading);
			stack.AddView (alphaStack, NSStackViewGravity.Trailing);

			View = stack;
		}

		private readonly IHostResourceProvider hostResources;

		private MaterialView materialEditor;
		private AlphaChannelEditor alphaChannelEditor;
		private ComponentSpinEditor alphaSpinEditor;

		private void UpdateComponent (object sender, EventArgs args)
		{
			if (ViewModel == null)
				return;

			var editor = sender as NumericSpinEditor;
			ViewModel.MaterialDesign.Alpha = (byte)editor.Value;
		}
	}
}
