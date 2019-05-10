using System;
using System.Collections.Generic;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EditorContainer
		: PropertyContainer
	{
		public EditorContainer (IHostResourceProvider hostResources, IEditorView editorView, bool? needsPropertyButton = null)
			: base (hostResources, editorView, needsPropertyButton ?? editorView?.NeedsPropertyButton ?? false)
		{
		}

		private const string FrameChangedObservableProperty = "frame";
		private readonly NSString observerKeyPath = new NSString (FrameChangedObservableProperty);

		public IEditorView EditorView => (IEditorView)NativeContainer;

		public EditorViewModel ViewModel {
			get => EditorView?.ViewModel;
			set {
				if (EditorView == null)
					return;

				if (this.viewModelAsPropertyViewModel != null)
					this.viewModelAsPropertyViewModel.PropertyChanged -= PropertyChanged;

				EditorView.ViewModel = value;

				this.viewModelAsPropertyViewModel = value as PropertyViewModel;

				if (PropertyButton != null)
					PropertyButton.ViewModel = this.viewModelAsPropertyViewModel;

				if (this.viewModelAsPropertyViewModel != null)
					this.viewModelAsPropertyViewModel.PropertyChanged += PropertyChanged;

				UpdateLabelVerticalConstraint ();

				UpdateVariations ();

				FrameChanged ();
			}
		}

		private void PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PropertyViewModel.HasVariantChildren)) {
				FrameChanged ();
			}
		}

		public NSView LeftEdgeView
		{
			get { return this.leftEdgeView; }
			set {
				if (this.leftEdgeView != null) {
					this.leftEdgeView.RemoveFromSuperview ();
					RemoveConstraints (new[] { this.leftEdgeLeftConstraint, this.leftEdgeVCenterConstraint });
					this.leftEdgeLeftConstraint.Dispose ();
					this.leftEdgeLeftConstraint = null;
					this.leftEdgeVCenterConstraint.Dispose ();
					this.leftEdgeVCenterConstraint = null;
				}

				this.leftEdgeView = value;

				if (value != null) {
					AddSubview (value);

					value.TranslatesAutoresizingMaskIntoConstraints = false;
					this.leftEdgeLeftConstraint = NSLayoutConstraint.Create (this.leftEdgeView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 9);
					this.leftEdgeVCenterConstraint = NSLayoutConstraint.Create (this.leftEdgeView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 8);

					AddConstraints (new[] { this.leftEdgeLeftConstraint, this.leftEdgeVCenterConstraint });
				}
			}
		}

		public override bool CanBecomeKeyView { get { return true; } }

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}

		public override void Layout ()
		{
			base.Layout ();

			FrameChanged ();
		}

		protected void AppearanceChanged ()
		{
			CGColor fgColour = HostResources.GetNamedColor (NamedResources.ForegroundColor).CGColor;

			// Repaint those lines
			foreach (CALayer variationLayer in this.variationParentLayersList) {
				if (variationLayer is CAShapeLayer lineShape) {
					lineShape.StrokeColor = fgColour;
				}
			}

			foreach (CALayer variationLayer in this.variationChildLayersList) {
				if (variationLayer is CAShapeLayer lineShape) {
					lineShape.StrokeColor = fgColour;
				}
			}
		}

		private void UpdateVariations ()
		{
			ClearSubViews ();

			if (this.viewModelAsPropertyViewModel != null && this.viewModelAsPropertyViewModel.IsInputEnabled) {
				if (this.viewModelAsPropertyViewModel.HasVariations) {

					if (this.viewModelAsPropertyViewModel.IsVariant) {
						this.deleteVariantButton = new VariationButton (HostResources, "pe-variation-delete-button-active-mac-10", "pe-variation-delete-button-mac-10") {
							AccessibilityEnabled = true,
							AccessibilityTitle = Properties.Resources.AccessibilityDeleteVariationButton,
							Command = this.viewModelAsPropertyViewModel.RemoveVariationCommand,
							ToolTip = Properties.Resources.RemoveVariant,
							TranslatesAutoresizingMaskIntoConstraints = false,
						};

						AddSubview (this.deleteVariantButton);
						AddConstraints (new[] {
							NSLayoutConstraint.Create (this.deleteVariantButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 8f),
							NSLayoutConstraint.Create (this.deleteVariantButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 20f),
						});

						this.variationControlsList.Add (this.deleteVariantButton);

						// Hook into the delete button's frame change, so we can fire off some drawing. If there's a better way, let me know.
						this.deleteVariantButton.AddObserver (this, this.observerKeyPath, NSKeyValueObservingOptions.New, IntPtr.Zero);

						NextKeyView = this.deleteVariantButton;

						// Cache these before the loop
						var variationBgColour = HostResources.GetNamedColor (NamedResources.FrameBoxButtonBackgroundColor);

						NSView previousControl = this.deleteVariantButton;
						foreach (PropertyVariationOption item in this.viewModelAsPropertyViewModel.Variation) {
							var selectedVariationTextField = new UnfocusableTextField {
								BackgroundColor = variationBgColour,
								Bordered = false,
								Font = VariationOptionFont,
								TranslatesAutoresizingMaskIntoConstraints = false,
								StringValue = string.Format (" {0}: {1} ", item.Category, item.Name),
							};

							AddSubview (selectedVariationTextField);
							AddConstraints (new[] {
								NSLayoutConstraint.Create (selectedVariationTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, VariationBorderOffset),
								NSLayoutConstraint.Create (selectedVariationTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, previousControl, NSLayoutAttribute.Right, 1f, 6f),
								NSLayoutConstraint.Create (selectedVariationTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 16f),
							});

							previousControl = selectedVariationTextField;
							this.variationControlsList.Add (selectedVariationTextField);
						}
					} else {
						this.addVariantButton = new VariationButton (HostResources, "pe-variation-add-button-active-mac-10", "pe-variation-add-button-mac-10") {
							AccessibilityEnabled = true,
							AccessibilityTitle = Properties.Resources.AccessibilityAddVariationButton,
							Command = this.viewModelAsPropertyViewModel.RequestCreateVariantCommand,
							ToolTip = Properties.Resources.AddVariant,
							TranslatesAutoresizingMaskIntoConstraints = false,
						};

						LeftEdgeView = this.addVariantButton;
						NextKeyView = this.addVariantButton;

						this.variationControlsList.Add (this.addVariantButton);
					}
				}
			}
		}

		private void ClearSubViews ()
		{
			foreach (NSView item in this.variationControlsList) {
				item.RemoveFromSuperview ();
			}

			this.variationControlsList.Clear ();

			this.addVariantButton = null;

			if (this.deleteVariantButton != null) {
				this.deleteVariantButton.RemoveObserver (this, this.observerKeyPath);
				this.deleteVariantButton = null;
			}
		}

		private void ClearChildLayers ()
		{
			WantsLayer = false;

			foreach (CALayer item in this.variationChildLayersList) {
				item.RemoveFromSuperLayer ();
			}

			this.variationChildLayersList.Clear ();
		}

		private void ClearParentLayers ()
		{
			WantsLayer = false;

			foreach (CALayer item in this.variationParentLayersList) {
				item.RemoveFromSuperLayer ();
			}

			this.variationParentLayersList.Clear ();
		}

		private CAShapeLayer GetVariantLines (CGPoint start1, CGPoint[] end1, CGPoint start2, CGPoint[] end2)
		{
			var endLineShape = new CAShapeLayer ();
			var endLinePath = new NSBezierPath ();

			endLinePath.MoveTo (start1);
			endLinePath.Append (end1);
			if (!start2.IsEmpty) {
				endLinePath.MoveTo (start2);
				endLinePath.Append (end2);
				// other dash
				start2.X += 18;
				endLinePath.MoveTo (start2);
				end2[0].X += 18;
				endLinePath.Append (end2);
			}
			endLineShape.Path = endLinePath.ToCGPath ();
			endLineShape.FillColor = null;
			endLineShape.Opacity = 1.0f;
			endLineShape.StrokeColor = HostResources.GetNamedColor (NamedResources.ForegroundColor).CGColor;
			return endLineShape;
		}

		// the deleteButton's AddObserver fires this method when the observed property changes
		public override void ObserveValue (NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			if (keyPath == FrameChangedObservableProperty) {
				FrameChanged ();
				return;
			}

			base.ObserveValue (keyPath, ofObject, change, context);
		}

		private void FrameChanged ()
		{
			ClearParentLayers ();
			ClearChildLayers ();

			if (this.viewModelAsPropertyViewModel == null)
				return;

			if (this.viewModelAsPropertyViewModel.IsVariant) {
				if (this.deleteVariantButton != null) {
					WantsLayer = true;

					CAShapeLayer childLineShape = GetVariantLines (
							new CGPoint (treeLineLeftEdge, this.viewModelAsPropertyViewModel.GetIsLastVariant () ? ViewVariationChildVerticalDrawPoint : 0),
							new CGPoint[] { new CGPoint (treeLineLeftEdge, Bounds.Height) },
							new CGPoint (treeLineLeftEdge, ViewVariationChildVerticalDrawPoint),
							new CGPoint[] { new CGPoint (treeLineLeftEdge + treeLineLeafIndent, ViewVariationChildVerticalDrawPoint) }
						);
					Layer.AddSublayer (childLineShape);

					this.variationChildLayersList.Add (childLineShape);
				}
			} else if (this.viewModelAsPropertyViewModel.HasVariantChildren) {
				if (this.addVariantButton != null) {
					WantsLayer = true;

					CAShapeLayer parentLineShape = GetVariantLines (
						new CGPoint (treeLineLeftEdge, 0),
						new CGPoint[] { new CGPoint (treeLineLeftEdge, ViewVariationParentVerticalDrawPoint) },
						new CGPoint (CGPoint.Empty),
						new CGPoint[] { new CGPoint (CGPoint.Empty) }
					);
					Layer.AddSublayer (parentLineShape);

					this.variationParentLayersList.Add (parentLineShape);
				}
			}
		}

#if DEBUG // Currently only used to highlight which controls haven't been implemented
		public NSColor LabelTextColor {
			set { LabelControl.TextColor = value; }
		}
#endif

		private NSView leftEdgeView;
		private NSLayoutConstraint leftEdgeLeftConstraint, leftEdgeVCenterConstraint;

		private List<NSView> variationControlsList = new List<NSView> ();
		private List<CALayer> variationChildLayersList = new List<CALayer> ();
		private List<CALayer> variationParentLayersList = new List<CALayer> ();
		private VariationButton deleteVariantButton;
		private VariationButton addVariantButton;
		private const float treeLineLeftEdge = 14f;
		private const float treeLineLeafIndent = 4f;
		internal const float VariationBorderOffset = 5f;
		internal static NSFont VariationOptionFont = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small) - 1f);
		private nfloat ViewVariationChildVerticalDrawPoint => this.deleteVariantButton.Frame.Top + 5;
		private nfloat ViewVariationParentVerticalDrawPoint => this.addVariantButton.Frame.Top - 2;
		private PropertyViewModel viewModelAsPropertyViewModel;
	}
}