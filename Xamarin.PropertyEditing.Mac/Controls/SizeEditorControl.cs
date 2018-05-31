using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class SizeEditorControl<T> : BasePointEditorControl<T>
		where T : struct
	{
		public SizeEditorControl ()
		{
			XLabel.Frame = new CGRect (24, -6, 50, 22);
			XLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			XLabel.StringValue = "WIDTH"; // TODO Localise

			XEditor.Frame = new CGRect (4, 13, 90, 20);

			YLabel.Frame = new CGRect (150, -6, 50, 22);
			YLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			YLabel.StringValue = "HEIGHT"; // TODO Localise

			YEditor.Frame = new CGRect (132, 13, 90, 20);
		}

		public override nint GetHeight (PropertyViewModel vm)
		{
			return 33;
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityWidthEditor, ViewModel.Property.Name);
			YEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityHeightEditor, ViewModel.Property.Name);
		}
	}

	internal class SystemSizeEditorControl : SizeEditorControl<Size>
	{
		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.Width;
			YEditor.Value = ViewModel.Value.Height;
		}
	}

	internal class CommonSizeEditorControl : SizeEditorControl<CommonSize>
	{
		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.Width;
			YEditor.Value = ViewModel.Value.Height;
		}
	}
}
