using System;
using System.Collections;
using System.Diagnostics;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BasePointEditorControl<T> : PropertyEditorControl<PropertyViewModel<T>>
	{
		internal UnfocusableTextField XLabel { get; set; }
		internal NumericSpinEditor<T> XEditor { get; set; }
		internal UnfocusableTextField YLabel { get; set; }
		internal NumericSpinEditor<T> YEditor { get; set; }

		public override NSView FirstKeyView => XEditor;
		public override NSView LastKeyView => YEditor.DecrementButton;

		protected override nint BaseHeight => 36;

		protected BasePointEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			XLabel = new UnfocusableTextField {
				Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			XEditor = new NumericSpinEditor<T> (hostResources) {
				BackgroundColor = NSColor.Clear,
				Tag = 1,
				Value = 0.0f
			};
			XEditor.ValueChanged += OnInputUpdated;

			YLabel = new UnfocusableTextField {
				Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			YEditor = new NumericSpinEditor<T> (hostResources) {
				BackgroundColor = NSColor.Clear,
				Value = 0.0f
			};
			YEditor.ValueChanged += OnInputUpdated;

			AddSubview (XLabel);
			AddSubview (XEditor);
			AddSubview (YLabel);
			AddSubview (YEditor);

			nfloat labelHeight = NSFont.SystemFontSizeForControlSize (NSControlSize.Small);
			AddConstraints (new[] {
				NSLayoutConstraint.Create (XLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Bottom, 1f, BottomOffset + 1),
				NSLayoutConstraint.Create (XLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, labelHeight),

				NSLayoutConstraint.Create (XEditor, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, XLabel,  NSLayoutAttribute.Top, 1f, -1f),
				NSLayoutConstraint.Create (XEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (XEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, YEditor,  NSLayoutAttribute.Left, 1f, -10f),

				NSLayoutConstraint.Create (YEditor, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, YLabel,  NSLayoutAttribute.Top, 1f, -1f),
				NSLayoutConstraint.Create (YEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, 0),
				NSLayoutConstraint.Create (YEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, XEditor,  NSLayoutAttribute.Width, 1f, 0f),

				NSLayoutConstraint.Create (YLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, XLabel,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (YLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, labelHeight),

				NSLayoutConstraint.Create (XLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, XEditor.Subviews[0], NSLayoutAttribute.CenterX, 1f, 0),
				NSLayoutConstraint.Create (YLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, YEditor.Subviews[0], NSLayoutAttribute.CenterX, 1f, 0),
			});

			AppearanceChanged ();
		}

		protected override void SetEnabled ()
		{
			XEditor.Enabled =
			YEditor.Enabled = ViewModel.IsInputEnabled;
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityEnabled = XEditor.Enabled;
			XEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityXEditor, ViewModel.Property.Name);

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityYEditor, ViewModel.Property.Name);
		}

		protected virtual void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = (T)Activator.CreateInstance (typeof (T), XEditor.Value, YEditor.Value);
		}

		protected override void AppearanceChanged ()
		{
			base.AppearanceChanged ();

			XLabel.TextColor = HostResources.GetNamedColor (NamedResources.DescriptionLabelColor);
			YLabel.TextColor = HostResources.GetNamedColor (NamedResources.DescriptionLabelColor);
		}
	}
}
