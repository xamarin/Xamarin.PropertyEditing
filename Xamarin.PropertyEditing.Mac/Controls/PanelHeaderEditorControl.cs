using System;
using System.ComponentModel;
using System.IO;

using AppKit;

using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PanelHeaderEditorControl
		: NSView
	{
		public PanelHeaderEditorControl (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			NSControlSize controlSize = NSControlSize.Small;
			TranslatesAutoresizingMaskIntoConstraints = false;

			this.propertyObjectName = new NSTextField {
				ControlSize = controlSize,
				PlaceholderString = LocalizationResources.ObjectNamePlaceholder,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.propertyObjectName.Activated += OnObjectNameEdited;

			AddSubview (this.propertyObjectName);

			this.typeDisplay = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (this.typeDisplay);

			this.objectNameLabel = new UnfocusableTextField {
				Alignment = NSTextAlignment.Right,
				StringValue = Properties.Resources.Name + ":",
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (this.objectNameLabel);

			this.typeLabel = new UnfocusableTextField {
				Alignment = NSTextAlignment.Right,
				StringValue = Properties.Resources.Type + ":",
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (this.typeLabel);

			this.propertyIcon = new NSImageView {
				ImageScaling = NSImageScale.AxesIndependently,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (this.propertyIcon);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.propertyIcon, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 32),
				NSLayoutConstraint.Create (this.propertyIcon, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 32),
				NSLayoutConstraint.Create (this.propertyIcon, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 5),
				NSLayoutConstraint.Create (this.propertyIcon, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 32),

				NSLayoutConstraint.Create (this.objectNameLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (this.objectNameLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, Mac.Layout.GoldenRatioLeft, 0f),
				NSLayoutConstraint.Create (this.objectNameLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),

				NSLayoutConstraint.Create (this.propertyObjectName, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this.objectNameLabel, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create (this.propertyObjectName, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.objectNameLabel, NSLayoutAttribute.Right, 1, 4.5f),
				NSLayoutConstraint.Create (this.propertyObjectName, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1, -(14.5f + PropertyButton.DefaultSize)),

				NSLayoutConstraint.Create (this.typeLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.propertyObjectName, NSLayoutAttribute.Bottom, 1, 5),
				NSLayoutConstraint.Create (this.typeLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, Mac.Layout.GoldenRatioLeft, 0f),
				NSLayoutConstraint.Create (this.typeLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),

				NSLayoutConstraint.Create (this.typeDisplay, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.typeLabel, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (this.typeDisplay, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.propertyObjectName, NSLayoutAttribute.Width, 1, 0),
				NSLayoutConstraint.Create (this.typeDisplay, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.typeLabel, NSLayoutAttribute.Right, 1, 4),
				NSLayoutConstraint.Create (this.typeDisplay, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),
			});
		}

		public PanelViewModel ViewModel
		{
			get { return this.viewModel; }
			set {
				if (this.viewModel == value)
					return;

				if (this.viewModel != null) {
					this.viewModel.PropertyChanged -= OnViewModelPropertyChanged;
				}

				this.viewModel = value;
				if (this.viewModel != null)
					this.viewModel.PropertyChanged += OnViewModelPropertyChanged;

				this.typeDisplay.StringValue = value?.TypeName ?? String.Empty;
				UpdateValue ();
				UpdateIcon ();
				this.propertyObjectName.Editable = !this.viewModel.IsObjectNameReadOnly;
			}
		}

		public void SetNextKeyView (NSView nextKeyView)
		{
			this.propertyObjectName.NextKeyView = nextKeyView;
		}

		private readonly IHostResourceProvider hostResources;

		private NSImageView propertyIcon;
		private NSTextField propertyObjectName;
		private UnfocusableTextField typeLabel, typeDisplay, objectNameLabel;

		private PanelViewModel viewModel;

		private void OnViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PanelViewModel.ObjectName)) {
				UpdateValue ();
			} else if (e.PropertyName == nameof (PanelViewModel.TypeName)) {
				UpdateIcon ();
			} else if (String.IsNullOrEmpty (e.PropertyName)) {
				UpdateValue ();
				UpdateIcon ();
			}
		}

		private void UpdateValue ()
		{
			this.propertyObjectName.StringValue = this.viewModel.ObjectName ?? string.Empty;
			this.propertyObjectName.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityObjectName, nameof (viewModel.ObjectName));
			this.propertyObjectName.Editable = !this.viewModel.IsObjectNameReadOnly;
		}

		private void OnObjectNameEdited (object sender, EventArgs e)
		{
			this.viewModel.ObjectName = this.propertyObjectName.StringValue;
		}

		private async void UpdateIcon ()
		{
			Stream icon = await this.viewModel.GetIconAsync ();

			if (icon != null)
				this.propertyIcon.Image = NSImage.FromStream (icon);

			this.propertyIcon.Hidden = (icon == null);
		}
	}
}
