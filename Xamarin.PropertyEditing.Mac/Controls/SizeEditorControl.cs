using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class SizeEditorControl<T> : BasePointEditorControl<T>
		where T : struct
	{
		public SizeEditorControl (IHostResourceProvider hostResource)
			: base (hostResource)
		{
			XLabel.StringValue = "WIDTH"; // TODO Localise

			XEditor.Frame = new CGRect (0, 13, 90, 20);
			
			YLabel.StringValue = "HEIGHT"; // TODO Localise

			YEditor.Frame = new CGRect (132, 13, 90, 20);
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityWidthEditor, ViewModel.Property.Name);
			YEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityHeightEditor, ViewModel.Property.Name);
		}
	}

	internal class SystemSizeEditorControl
		: SizeEditorControl<Size>
	{
		public SystemSizeEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
		}

		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.Width;
			YEditor.Value = ViewModel.Value.Height;
		}
	}

	internal class CommonSizeEditorControl
		: SizeEditorControl<CommonSize>
	{
		public CommonSizeEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
		}

		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.Width;
			YEditor.Value = ViewModel.Value.Height;
		}
	}
}
