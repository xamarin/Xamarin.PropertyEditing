using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class AutoResizingView : NSView
	{
		private static NSImage defaultBackground;
		private readonly UnfocusableTextField maskLabel;
		private readonly UnfocusableTextField previewLabel;
		private bool enabled;
		private IHostResourceProvider hostResources;

		internal AutoResizingMaskView MaskView {
			get;
			private set;
		}

		internal AutoResizingPreviewView PreviewView {
			get;
			private set;
		}

		public bool Enabled {
			get { return this.enabled; }

			internal set {
				if (this.enabled == value)
					return;

				this.enabled = value;

				MaskView.Enabled = this.enabled;

				PreviewView.Enabled = this.enabled;
			}
		}

		private const string DeviceFrameName = "pe-device-frame";

		public AutoResizingView (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			MaskView = new AutoResizingMaskView {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (MaskView);

			PreviewView = new AutoResizingPreviewView (GetBackgroundImage ()) {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (PreviewView);

			this.maskLabel = new UnfocusableTextField {
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultDescriptionLabelFontSize),
				StringValue = Properties.Resources.Autosizing.ToUpper (),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (this.maskLabel);

			MaskView.MaskChanged += MaskChanged;

			this.previewLabel = new UnfocusableTextField {
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultDescriptionLabelFontSize),
				StringValue = Properties.Resources.Example.ToUpper (),
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			AddSubview (this.previewLabel);

			AddConstraints (new NSLayoutConstraint[] {
				NSLayoutConstraint.Create (MaskView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0),
				NSLayoutConstraint.Create (MaskView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (MaskView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, 0),

				NSLayoutConstraint.Create (this.maskLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, MaskView, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (this.maskLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),
				NSLayoutConstraint.Create (this.maskLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, MaskView, NSLayoutAttribute.CenterX, 1, 0),

				NSLayoutConstraint.Create (PreviewView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, MaskView, NSLayoutAttribute.Left, 1, 0),
				NSLayoutConstraint.Create (PreviewView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.maskLabel, NSLayoutAttribute.Bottom, 1, 5),
				NSLayoutConstraint.Create (PreviewView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, MaskView, NSLayoutAttribute.Width, 1, 0),

				NSLayoutConstraint.Create (this.previewLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, PreviewView, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (this.previewLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),
				NSLayoutConstraint.Create (this.previewLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, PreviewView, NSLayoutAttribute.CenterX, 1, 0),
			});

			AppearanceChanged ();
		}

		#region Overrriden Methods and Properties


		public override bool IsFlipped {
			get { return true; }
		}

		public override CGSize IntrinsicContentSize {
			get { return new CGSize (-1, 90); }
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}

		#endregion

		private void MaskChanged (object sender, EventArgs e)
		{
			PreviewView.Mask = MaskView.Mask;
		}

		internal void UpdateAccessibilityValues ()
		{
			if (MaskView != null) {
				MaskView.AccessibilityEnabled = this.enabled;
				MaskView.AccessibilityTitle = Properties.Resources.AccessibilityMaskView;
			}

			if (PreviewView != null) {
				PreviewView.AccessibilityEnabled = this.enabled;
				PreviewView.AccessibilityTitle = Properties.Resources.AccessibilityPreviewMaskView;
			}
		}

		private void AppearanceChanged ()
		{
			NSColor labelColor = this.hostResources.GetNamedColor (NamedResources.DescriptionLabelColor);
			this.maskLabel.TextColor = labelColor;
			this.previewLabel.TextColor = labelColor;
		}

		public NSImage GetBackgroundImage ()
		{
			if (defaultBackground == null) {
				defaultBackground = this.hostResources.GetNamedImage (DeviceFrameName);
				// The workaround method isn't available in MonoMac
#pragma warning disable 0618
				defaultBackground.Flipped = true;
#pragma warning restore 0618
			}
			return defaultBackground;
		}
	}

	internal struct Line
	{
		public CGPoint P1;
		public CGPoint P2;
	}

	internal static class DrawUtils
	{
		public static void DrawStraightLine (CGPoint p1, CGPoint p2, int[] dashes = null)
		{
			foreach (var segment in GetSegments (p1, p2, dashes)) {
				var line = RectForLine (segment.P1, segment.P2);
				NSGraphics.RectFill (line);
			}
		}

		public static IEnumerable<Line> GetSegments (CGPoint p1, CGPoint p2, int[] dashes)
		{
			if (dashes == null) {
				yield return new Line { P1 = p1, P2 = p2 };
				yield break;
			}

			int axisMultiplier = GetAxisAdder (p1, p2);
			int initialOrder = GlobalOrder (p1, p2);
			var currentStart = p1;
			var currentEnd = p1;
			int dashIndex = 0;

			while (GlobalOrder (currentEnd, p2) == initialOrder) {
				var dash = dashes[dashIndex];
				currentEnd = new CGPoint (currentEnd.X + (dash * (short)(axisMultiplier >> 16)),
					currentEnd.Y + (dash * (short)(axisMultiplier & 0xFFFF)));

				// if index is odd, we are skipping drawing and repositioning ourselves
				if ((dashIndex % 2) == 1) {
					currentStart = currentEnd;
					dashIndex = (dashIndex + 1) % dashes.Length;
					continue;
				}

				if (GlobalOrder (currentEnd, p2) == initialOrder)
					yield return new Line { P1 = currentStart, P2 = currentEnd };
				dashIndex = (dashIndex + 1) % dashes.Length;
			}

			yield return new Line { P1 = currentStart, P2 = p2 };
		}

		private static int GlobalOrder (CGPoint p1, CGPoint p2)
		{
			return (p1.X < p2.X ? 1 : 0) << 1 | (p1.Y < p2.Y ? 1 : 0);
		}

		private static int GetAxisAdder (CGPoint p1, CGPoint p2)
		{
			if (p1.Y == p2.Y)
				return p1.X < p2.X ? ((short)1) << 16 : ((short)-1) << 16;
			else
				return p1.Y < p2.Y ? ((short)1) : ((short)-1);
		}

		private static CGRect RectForLine (CGPoint p1, CGPoint p2)
		{
			return new CGRect (p1.X < p2.X ? p1.X : p2.X,
								  p1.Y < p2.Y ? p1.Y : p2.Y,
								  Math.Max (1, Math.Abs (p2.X - p1.X)),
								  Math.Max (1, Math.Abs (p2.Y - p1.Y)));
		}
	}

	internal static class DrawingUtils
	{
		private static nfloat[] lightBezelGreys, fullBezelGreys, fullBezelGreysPixelFix;

		public static void UpdateBezelGreys (bool isLightTheme)
		{
			if (isLightTheme) {
				lightBezelGreys = new nfloat[] { .59f, .71f, .71f, .71f };
				fullBezelGreys = new nfloat[] { .71f, .96f, .71f, .96f, .61f, .89f, .96f };
				fullBezelGreysPixelFix = new nfloat[] { .61f, .73f };
			} else {
				lightBezelGreys = new nfloat[] { .41f, .29f, .29f, .29f };
				fullBezelGreys = new nfloat[] { .29f, .04f, .29f, .04f, .39f, .11f, .04f };
				fullBezelGreysPixelFix = new nfloat[] { .39f, .27f };
			}
		}

		/// <summary>
		/// Draws a shaded, 1 pixel-wide, bezel around a rect
		/// </summary>
		public static CGRect DrawLightShadedBezel (CGRect rect, bool flipped = false)
		{
			return NSGraphics.DrawTiledRects (rect, rect,
											  new NSRectEdge[] { !flipped ? NSRectEdge.MaxYEdge : NSRectEdge.MinYEdge, NSRectEdge.MinXEdge, NSRectEdge.MaxXEdge, !flipped ? NSRectEdge.MinYEdge : NSRectEdge.MaxYEdge },
											  lightBezelGreys);
		}

		/// <summary>
		/// Draws a shaded bezel of more than one pixel around a rect. Returned rectangle is the inner usable area.
		/// </summary>
		public static CGRect DrawFullShadedBezel (CGRect rect, bool flipped)
		{
			var top = !flipped ? NSRectEdge.MaxYEdge : NSRectEdge.MinYEdge;
			var bottom = !flipped ? NSRectEdge.MinYEdge : NSRectEdge.MaxYEdge;

			var inner = NSGraphics.DrawTiledRects (rect, rect,
												   new NSRectEdge[] { NSRectEdge.MinXEdge, NSRectEdge.MinXEdge, NSRectEdge.MaxXEdge, NSRectEdge.MaxXEdge, top, top, top },
												   fullBezelGreys);
			// Redo the top and bottom to clear bad pixels
			inner.Intersect (NSGraphics.DrawTiledRects (rect, rect,
														new NSRectEdge[] { top, bottom },
														fullBezelGreysPixelFix));
			return inner;
		}
	}
}
