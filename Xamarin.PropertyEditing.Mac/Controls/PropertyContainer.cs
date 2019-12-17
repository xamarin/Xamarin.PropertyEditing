using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyContainer
		: NSView
	{
		public PropertyContainer (IHostResourceProvider hostResources, INativeContainer nativeView, bool includePropertyButton, float vertInset = 0)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			HostResources = hostResources;

			NativeContainer = nativeView;

			AddSubview (this.label);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Left, NSLayoutRelation.GreaterThanOrEqual, this, NSLayoutAttribute.Left, 1, 0f),
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, Mac.Layout.GoldenRatioLeft, 0f),
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),
			});

			if (NativeContainer != null) {
				AddSubview (NativeContainer.NativeView);
				NativeContainer.NativeView.TranslatesAutoresizingMaskIntoConstraints = false;

				AddConstraints (new[] {
					NSLayoutConstraint.Create (NativeContainer.NativeView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
					NSLayoutConstraint.Create (NativeContainer.NativeView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.label, NSLayoutAttribute.Right, 1f, LabelToControlSpacing),
					NSLayoutConstraint.Create (NativeContainer.NativeView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1f, vertInset)
				});

				if (includePropertyButton) {
					this.propertyButton = new PropertyButton (hostResources) {
						TranslatesAutoresizingMaskIntoConstraints = false
					};

					AddSubview (this.propertyButton);
					AddConstraints (new[] {
						NSLayoutConstraint.Create (this.propertyButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this.label, NSLayoutAttribute.CenterY, 1, 0),
						NSLayoutConstraint.Create (NativeContainer.NativeView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.propertyButton, NSLayoutAttribute.Left, 1f, -EditorToButtonSpacing),
						NSLayoutConstraint.Create (this.propertyButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, -ButtonToWallSpacing),
						NSLayoutConstraint.Create (this.propertyButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, PropertyButton.DefaultSize),
					});
				} else {
					AddConstraint (NSLayoutConstraint.Create (NativeContainer.NativeView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0f));
				}
			}
		}

		protected void UpdateLabelVerticalConstraint ()
		{
			if (NativeContainer != null) {
				if (NativeContainer.NativeView is PropertyEditorControl pec) {
					if (pec.FirstKeyView != null) {
						LabelVerticalContraint = NSLayoutConstraint.Create (LabelControl, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, pec.FirstKeyView, NSLayoutAttribute.CenterY, 1, 0);
					} else {
						LabelVerticalContraint = NSLayoutConstraint.Create (LabelControl, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, pec, NSLayoutAttribute.CenterY, 1, 0);
					}
				}
			} else {
				LabelVerticalContraint = NSLayoutConstraint.Create (LabelControl, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0);
			}

			if (LabelVerticalContraint != null)
				AddConstraint (LabelVerticalContraint);
		}

		public string Label
		{
			get { return this.label.StringValue; }
			set { this.label.StringValue = value + ":"; }
		}

		protected INativeContainer NativeContainer
		{
			get;
		}

		protected UnfocusableTextField LabelControl => this.label;

		protected PropertyButton PropertyButton => this.propertyButton;

		internal const float LabelToControlSpacing = 8f;
		internal static float PropertyTotalWidth => PropertyButton.DefaultSize + ButtonToWallSpacing + EditorToButtonSpacing;

		private const float EditorToButtonSpacing = 4f;
		private const float ButtonToWallSpacing = 17f;
		protected NSLayoutConstraint LabelVerticalContraint { get; set; }

		private readonly PropertyButton propertyButton;
		private readonly UnfocusableTextField label = new UnfocusableTextField {
			Alignment = NSTextAlignment.Right,
			Cell = {
				LineBreakMode = NSLineBreakMode.TruncatingMiddle,
			},
			TranslatesAutoresizingMaskIntoConstraints = false,
		};

		protected IHostResourceProvider HostResources { get; private set; }
	}
}
