using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BaseRectangleEditorControl<T> : PropertyEditorControl<PropertyViewModel<T>>
	{
		protected UnfocusableTextField XLabel { get; set; }
		protected NumericSpinEditor<T> XEditor { get; set; }
		protected UnfocusableTextField YLabel { get; set; }
		protected NumericSpinEditor<T> YEditor { get; set; }
		protected UnfocusableTextField WidthLabel { get; set; }
		protected NumericSpinEditor<T> WidthEditor { get; set; }
		protected UnfocusableTextField HeightLabel { get; set; }
		protected NumericSpinEditor<T> HeightEditor { get; set; }

		public override NSView FirstKeyView => XEditor;
		public override NSView LastKeyView => HeightEditor.DecrementButton;

		protected override nint BaseHeight => 68;

		protected BaseRectangleEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			XLabel = new UnfocusableTextField {
				Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			XEditor = new NumericSpinEditor<T> (hostResources) {
				BackgroundColor = NSColor.Clear,
				Value = 0.0f
			};
			XEditor.ValueChanged += OnInputUpdated;

			YLabel =  new UnfocusableTextField {
				Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			YEditor = new NumericSpinEditor<T> (hostResources) {
				BackgroundColor = NSColor.Clear,
				Value = 0.0f
			};
			YEditor.ValueChanged += OnInputUpdated;

			WidthLabel = new UnfocusableTextField {
				Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			WidthEditor = new NumericSpinEditor<T> (hostResources) {
				BackgroundColor = NSColor.Clear,
				Value = 0.0f
			};
			WidthEditor.ValueChanged += OnInputUpdated;

			HeightLabel =  new UnfocusableTextField {
				Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			HeightEditor = new NumericSpinEditor<T> (hostResources) {
				BackgroundColor = NSColor.Clear,
				Value = 0.0f
			};
			HeightEditor.ValueChanged += OnInputUpdated;

			AddSubview (XLabel);
			AddSubview (XEditor);
			AddSubview (YLabel);
			AddSubview (YEditor);
			AddSubview (WidthLabel);
			AddSubview (WidthEditor);
			AddSubview (HeightLabel);
			AddSubview (HeightEditor);

			nfloat labelHeight = NSFont.SystemFontSizeForControlSize (NSControlSize.Small);
			AddConstraints (new[] {
				NSLayoutConstraint.Create (WidthLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Bottom, 1f, BottomOffset + 1),
				NSLayoutConstraint.Create (WidthLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, labelHeight),

				NSLayoutConstraint.Create (WidthEditor, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, WidthLabel,  NSLayoutAttribute.Top, 1f, -1f),
				NSLayoutConstraint.Create (WidthEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (WidthEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, HeightEditor,  NSLayoutAttribute.Left, 1f, -10f),

				NSLayoutConstraint.Create (HeightEditor, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, HeightLabel,  NSLayoutAttribute.Top, 1f, -1f),
				NSLayoutConstraint.Create (HeightEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, 0),
				NSLayoutConstraint.Create (HeightEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, WidthEditor,  NSLayoutAttribute.Width, 1f, 0f),

				NSLayoutConstraint.Create (HeightLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, WidthLabel,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (HeightLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, labelHeight),

				NSLayoutConstraint.Create (WidthLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, WidthEditor.Subviews[0], NSLayoutAttribute.CenterX, 1f, 0),
				NSLayoutConstraint.Create (HeightLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, HeightEditor.Subviews[0], NSLayoutAttribute.CenterX, 1f, 0),

				NSLayoutConstraint.Create (XLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, WidthEditor,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (XLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, labelHeight),

				NSLayoutConstraint.Create (XEditor, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, XLabel,  NSLayoutAttribute.Top, 1f, -1f),
				NSLayoutConstraint.Create (XEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, WidthEditor,  NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (XEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, YEditor,  NSLayoutAttribute.Left, 1f, -10f),

				NSLayoutConstraint.Create (YLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, XLabel,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (YLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, labelHeight),

				NSLayoutConstraint.Create (YEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, XEditor,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (YEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, HeightEditor,  NSLayoutAttribute.Right, 1f, 0),
				NSLayoutConstraint.Create (YEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, XEditor,  NSLayoutAttribute.Width, 1f, 0f),

				NSLayoutConstraint.Create (XLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, XEditor.Subviews[0], NSLayoutAttribute.CenterX, 1f, 0),
				NSLayoutConstraint.Create (YLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, YEditor.Subviews[0], NSLayoutAttribute.CenterX, 1f, 0),
			});

			AppearanceChanged ();
		}

		protected virtual void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = (T)Activator.CreateInstance (typeof(T), XEditor.Value, YEditor.Value, WidthEditor.Value, HeightEditor.Value);
		}

		protected override void SetEnabled ()
		{
			XEditor.Enabled = 
			YEditor.Enabled = 
			WidthEditor.Enabled = 
			HeightEditor.Enabled = ViewModel.IsInputEnabled;
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityEnabled = XEditor.Enabled;
			XEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityXEditor, ViewModel.Property.Name);

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityYEditor, ViewModel.Property.Name);

			WidthEditor.AccessibilityEnabled = WidthEditor.Enabled;
			WidthEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityWidthEditor, ViewModel.Property.Name);

			HeightEditor.AccessibilityEnabled = HeightEditor.Enabled;
			HeightEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityHeightEditor, ViewModel.Property.Name);
		}

		protected override void AppearanceChanged ()
		{
			base.AppearanceChanged ();

			XLabel.TextColor = HostResources.GetNamedColor (NamedResources.DescriptionLabelColor);
			YLabel.TextColor = HostResources.GetNamedColor (NamedResources.DescriptionLabelColor);
			WidthLabel.TextColor = HostResources.GetNamedColor (NamedResources.DescriptionLabelColor);
			HeightLabel.TextColor = HostResources.GetNamedColor (NamedResources.DescriptionLabelColor);
		}
	}
}
