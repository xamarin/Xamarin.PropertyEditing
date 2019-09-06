using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
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

		public override NSView FirstKeyView => this.firstKeyView;
		public override NSView LastKeyView => HeightEditor.DecrementButton;

		public NSLayoutConstraint LeftXEditorEdgeConstraint { get; }
		private OriginControl originEditor;
		private NSLayoutConstraint originViewConstraint;
		private CommonOrigin? lastOrigin = CommonOrigin.TopLeft;
		private NSView firstKeyView;

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

			YLabel = new UnfocusableTextField {
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

			HeightLabel = new UnfocusableTextField {
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

			LeftXEditorEdgeConstraint = NSLayoutConstraint.Create (XEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (XEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 3f),
				LeftXEditorEdgeConstraint,
				NSLayoutConstraint.Create (XEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, YEditor,  NSLayoutAttribute.Left, 1f, -10f),
				NSLayoutConstraint.Create (XEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),

				NSLayoutConstraint.Create (XLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, XEditor,  NSLayoutAttribute.Bottom, 1f, -4f),
				NSLayoutConstraint.Create (XLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),

				NSLayoutConstraint.Create (YEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 3f),
				NSLayoutConstraint.Create (YEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, 0),
				NSLayoutConstraint.Create (YEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, XEditor,  NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (YEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),

				NSLayoutConstraint.Create (YLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, XLabel,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (YLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),

				NSLayoutConstraint.Create (WidthEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 33f),
				NSLayoutConstraint.Create (WidthEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, XEditor,  NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (WidthEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, HeightEditor,  NSLayoutAttribute.Left, 1f, -10f),
				NSLayoutConstraint.Create (WidthEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),

				NSLayoutConstraint.Create (WidthLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, WidthEditor,  NSLayoutAttribute.Bottom, 1f, -4f),
				NSLayoutConstraint.Create (WidthLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),

				NSLayoutConstraint.Create (HeightEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, WidthEditor,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (HeightEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, 0),
				NSLayoutConstraint.Create (HeightEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, WidthEditor,  NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (HeightEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),

				NSLayoutConstraint.Create (HeightLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, WidthLabel,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (HeightLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),


				NSLayoutConstraint.Create (XLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, XEditor.Subviews[0], NSLayoutAttribute.CenterX, 1f, 0),
				NSLayoutConstraint.Create (YLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, YEditor.Subviews[0], NSLayoutAttribute.CenterX, 1f, 0),
				NSLayoutConstraint.Create (WidthLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, WidthEditor.Subviews[0], NSLayoutAttribute.CenterX, 1f, 0),
				NSLayoutConstraint.Create (HeightLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, HeightEditor.Subviews[0], NSLayoutAttribute.CenterX, 1f, 0),
			});

			this.firstKeyView = XEditor;

			AppearanceChanged ();
		}

		protected virtual void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = (T)Activator.CreateInstance (typeof (T), XEditor.Value, YEditor.Value, WidthEditor.Value, HeightEditor.Value, this.originEditor.Value);
		}

		protected override void SetEnabled ()
		{
			XEditor.Enabled = ViewModel.Property.CanWrite;
			YEditor.Enabled = ViewModel.Property.CanWrite;
			WidthEditor.Enabled = ViewModel.Property.CanWrite;
			HeightEditor.Enabled = ViewModel.Property.CanWrite;

			if (this.originEditor != null) {
				this.originEditor.Enabled = ViewModel.Property.CanWrite;
			}
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

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (ViewModel == null)
				return;

			LeftXEditorEdgeConstraint.Active = true;

			bool hasOrigin = false;
			if (ViewModel is RectanglePropertyViewModel rpvm && rpvm.HasOrigin) {
				hasOrigin = rpvm.HasOrigin;

				LeftXEditorEdgeConstraint.Active = false;

				if (this.originEditor == null) {
					this.originEditor = new OriginControl (HostResources) {
						AccessibilityEnabled = rpvm.Property.CanWrite,
						AccessibilityTitle = string.Format (Properties.Resources.AccessibilityOriginEditor, ViewModel.Property.Name),
						TranslatesAutoresizingMaskIntoConstraints = false,
					};

					this.originEditor.OriginChanged += (s, e) => {
						this.lastOrigin = this.originEditor.Value;

						if (this.originEditor.Value.HasValue) {
							var location = this.originEditor.Bounds; // TODO Think this should be the DocumentFrame
							if (this.originEditor.Value?.Horizontal != CommonOrigin.Position.Start)
								location.X += this.originEditor.Bounds.Width / (this.originEditor.Value?.Horizontal == CommonOrigin.Position.Middle ? 2 : 1);
							if (this.originEditor.Value?.Vertical != CommonOrigin.Position.Start)
								location.Y += this.originEditor.Bounds.Height / (this.originEditor.Value?.Vertical == CommonOrigin.Position.Middle ? 2 : 1);

							if ((nfloat)XEditor.Value != location.X)
								XEditor.Value = location.X;
							if ((nfloat)YEditor.Value != location.Y)
								YEditor.Value = location.Y;

							OnInputUpdated (s, e);
						}
					};

					this.originEditor.Value = this.lastOrigin;

					AddSubview (this.originEditor);

					this.originViewConstraint = NSLayoutConstraint.Create (this.originEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, XEditor, NSLayoutAttribute.Left, 1, -4);

					AddConstraints (new[] {
						NSLayoutConstraint.Create (this.originEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, XEditor,  NSLayoutAttribute.Top, 1f, 0),
						NSLayoutConstraint.Create (this.originEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 0),
						this.originViewConstraint,
						NSLayoutConstraint.Create (this.originEditor, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1, -2f),
					});

					this.firstKeyView = this.originEditor;
				}
			}

			// If we are reusing the control we'll have to hide the originEditor if we don't have Origin.
			if (this.originEditor != null) {
				this.originEditor.Hidden = !hasOrigin;
				this.originViewConstraint.Active = hasOrigin;

				if (hasOrigin)
					this.originEditor.Value = this.lastOrigin;
			}

			SetEnabled ();
		}
	}
}
