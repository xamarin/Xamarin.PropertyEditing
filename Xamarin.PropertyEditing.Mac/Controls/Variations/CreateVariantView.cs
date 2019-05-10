using System;
using System.Reflection;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CreateVariantView
		: NSView
	{
		private const int ControlSpacing = 7;
		private const int HorizontalControlSpacing = 10;
		internal const int RightEdgeMargin = 12;

		private readonly IHostResourceProvider hostResources;

		public CreateVariantView (IHostResourceProvider hostResources, CGSize windowWidthHeight, CreateVariantViewModel createVariantViewModel)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			if (createVariantViewModel == null)
				throw new ArgumentNullException (nameof (createVariantViewModel));

			Initialize (createVariantViewModel, windowWidthHeight);
		}

		private void Initialize (CreateVariantViewModel createVariantViewModel, CGSize windowWidthHeight)
		{
			Frame = new CGRect (CGPoint.Empty, windowWidthHeight);

			int editorHeight = 18;

			var FrameWidthThird = Frame.Width / 3;

			var introduceVariation = new UnfocusableTextField {
				StringValue = Properties.Resources.AddVariationHelpText,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (introduceVariation);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (introduceVariation, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 8f),
				NSLayoutConstraint.Create (introduceVariation, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 18f),
				NSLayoutConstraint.Create (introduceVariation, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (introduceVariation, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, editorHeight),
			});

			var controlTop = 33;

			foreach (VariationViewModel viewModel in createVariantViewModel.VariationCategories) {

				var name = new UnfocusableTextField {
					Alignment = NSTextAlignment.Right,
					StringValue = viewModel.Name + ":",
					TranslatesAutoresizingMaskIntoConstraints = false,
				};

				AddSubview (name);

				AddConstraints (new[] {
					NSLayoutConstraint.Create (name, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, controlTop),
					NSLayoutConstraint.Create (name, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, FrameWidthThird),
					NSLayoutConstraint.Create (name, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, editorHeight)
				});

				var popUpButton = new FocusablePopUpButton {
					ControlSize = NSControlSize.Small,
					Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
					TranslatesAutoresizingMaskIntoConstraints = false,
				};

				popUpButton.Activated += (o, e) => {
					if (o is FocusablePopUpButton fpb) {
						if (fpb.SelectedItem.RepresentedObject is NSObjectFacade menuObjectFacade) {
							if (menuObjectFacade.Target is VariationFacade vf) {
								vf.ViewModel.SelectedOption = vf.Option;
							}
						}
					}
				};

				var popUpButtonList = new NSMenu ();
				popUpButton.Menu = popUpButtonList;

				AddSubview (popUpButton);

				AddConstraints (new[] {
					NSLayoutConstraint.Create (popUpButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, controlTop),
					NSLayoutConstraint.Create (popUpButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, FrameWidthThird + ControlSpacing),
					NSLayoutConstraint.Create (popUpButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -FrameWidthThird - ControlSpacing - RightEdgeMargin),
					NSLayoutConstraint.Create (popUpButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, editorHeight)
				});

				foreach (var variation in viewModel.Variations) {
					popUpButtonList.AddItem (new NSMenuItem (variation.Name) {
						RepresentedObject = new NSObjectFacade (
							new VariationFacade { 
								Option = variation,
								ViewModel = viewModel 
							}
						)
					});
				}

				controlTop += editorHeight + HorizontalControlSpacing;
			}

			Frame = new CGRect (CGPoint.Empty, new CGSize (Frame.Width, controlTop));
		}

		private class VariationFacade
		{
			public PropertyVariationOption Option { get; set; }
			public VariationViewModel ViewModel { get; set; }
		}
	}
}
