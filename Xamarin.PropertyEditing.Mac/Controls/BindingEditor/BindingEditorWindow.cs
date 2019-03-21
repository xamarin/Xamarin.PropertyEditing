using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Common;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BindingEditorWindow : NSPanel
	{
		private NSButton buttonDone;
		private NSButton buttonCancel;

		private NSPopUpButton bindingTypePopup;

		private NSPopUpButton valueConverterPopup;

		private NSButton addConverterButton;

		private NSView ancestorTypeBox;
		private NSView pathBox;

		private NSView mainContainer;
		private NSView bindingPropertiesView;
		private NSView flagsPropertiesView;
		private NSButton buttonMoreSettings;

		private nfloat MoreSettingsViewHeight { get; set; }

		private const float AddConverterButtonSize = 20;

		private new ModalWindowCloseDelegate Delegate
		{
			get => (ModalWindowCloseDelegate)base.Delegate;
			set => base.Delegate = value;
		}

		private readonly PropertyEditorSelector editorSelector = new PropertyEditorSelector ();

		private const string BindingPropertiesIdentifier = "BindingProperties";
		private const string FlagPropertiesIdentifier = "FlagProperties";
		private const float HeaderHeight = 28f;
		private const float TypeSelectorBorder = 25;

		internal CreateBindingViewModel ViewModel { get; }

		private HeaderView typeHeader;
		private HeaderView pathHeader;

		private UnfocusableTextField longDescription;
		private UnfocusableTextField labelOtherSettings;
		private TypeSelectorControl typeSelectorControl;

		private nfloat heightConstant, titleBarHeight;

		private NSLayoutConstraint ancestorTypeBoxHeightConstraint;
		private NSLayoutConstraint bindingPropertiesViewHeightConstraint;

		private BindingPathSelectorControl pathSelectorControl;

		internal BindingEditorWindow (IHostResourceProvider hostResources, PropertyViewModel propertyViewModel)
			: base (new CGRect (0, 0, 728, 445), NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable, NSBackingStore.Buffered, true)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));
			if (propertyViewModel == null)
				throw new ArgumentNullException (nameof (propertyViewModel));

			ViewModel = new CreateBindingViewModel (propertyViewModel.TargetPlatform, propertyViewModel.Editors.Single (), propertyViewModel.Property);

			Delegate = new ModalWindowCloseDelegate ();

			FloatingPanel = true;

			MaxSize = new CGSize (960, 720); // TODO discuss what the Max/Min Size should be and if we should have one.
			MinSize = new CGSize (320, 240);

			this.mainContainer = new NSView {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			this.buttonDone = new NSButton {
				BezelStyle = NSBezelStyle.Rounded,
				KeyEquivalent = "\r", // Fire when enter pressed
				Highlighted = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.mainContainer.AddSubview (this.buttonDone);

			this.buttonCancel = new NSButton {
				BezelStyle = NSBezelStyle.Rounded,
				Title = Properties.Resources.Cancel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.buttonCancel.Activated += (sender, e) => {
				Delegate.Response = NSModalResponse.Cancel;
				Close ();
			};

			this.mainContainer.AddSubview (this.buttonCancel);

			var bindingTypeLabel = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
				Alignment = NSTextAlignment.Right,
			};

			bindingTypeLabel.StringValue = Properties.Resources.BindingType;
			this.mainContainer.AddSubview (bindingTypeLabel);

			this.bindingTypePopup = new FocusablePopUpButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
			};

			var bindingTypeMenuList = new NSMenu ();
			this.bindingTypePopup.Menu = bindingTypeMenuList;
			this.mainContainer.AddSubview (this.bindingTypePopup);

			var valueConverterLabel = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
				Alignment = NSTextAlignment.Right,
			};

			this.ancestorTypeBox = new NSView {
				TranslatesAutoresizingMaskIntoConstraints = false,
				WantsLayer = true,

				// Layer out of alphabetical order so that WantsLayer creates the layer first
				Layer = {
					CornerRadius = 1.0f,
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					BorderWidth = 1,
				},
			};

			this.mainContainer.AddSubview (this.ancestorTypeBox);

			this.pathBox = new NSView {
				TranslatesAutoresizingMaskIntoConstraints = false,
				WantsLayer = true,

				// Layer out of alphabetical order so that WantsLayer creates the layer first
				Layer = {
					CornerRadius = 1.0f,
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					BorderWidth = 1,
				},
			};

			this.mainContainer.AddSubview (this.pathBox);

			valueConverterLabel.StringValue = Properties.Resources.Converter;
			this.mainContainer.AddSubview (valueConverterLabel);

			this.valueConverterPopup = new FocusablePopUpButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
			};

			var valueConverterMenuList = new NSMenu ();
			this.valueConverterPopup.Menu = valueConverterMenuList;
			this.mainContainer.AddSubview (this.valueConverterPopup);

			this.addConverterButton = new NSButton {
				BezelStyle = NSBezelStyle.Rounded,
				Image = NSImage.ImageNamed (NSImageName.AddTemplate),
				Title = string.Empty,
				ToolTip = Properties.Resources.AddValueConverterEllipsis,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.mainContainer.AddSubview (this.addConverterButton);

			this.buttonMoreSettings = new NSButton {
				BezelStyle = NSBezelStyle.Disclosure,
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			this.buttonMoreSettings.SetButtonType (NSButtonType.PushOnPushOff);

			this.mainContainer.AddSubview (this.buttonMoreSettings);

			this.labelOtherSettings = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.mainContainer.AddSubview (this.labelOtherSettings);

			this.bindingPropertiesView = new NSView {
				Hidden = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
				WantsLayer = true,

				// Layer out of alphabetical order so that WantsLayer creates the layer first
				Layer = {
					CornerRadius = 1.0f,
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					BorderWidth = 1,
				},
			};

			this.mainContainer.AddSubview (this.bindingPropertiesView);

			this.flagsPropertiesView = new NSView {
				Hidden = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
				WantsLayer = true,

				// Layer out of alphabetical order so that WantsLayer creates the layer first
				Layer = {
					CornerRadius = 1.0f,
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					BorderWidth = 1,
				},
			};

			this.mainContainer.AddSubview (this.flagsPropertiesView);

			//Work out the titlebar height
			this.titleBarHeight = Frame.Size.Height - ContentRectFor (Frame).Size.Height;

			this.ancestorTypeBoxHeightConstraint = NSLayoutConstraint.Create (this.ancestorTypeBox, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.mainContainer, NSLayoutAttribute.Bottom, 1f, -145f);
			this.heightConstant = this.ancestorTypeBoxHeightConstraint.Constant;

			bindingPropertiesViewHeightConstraint = NSLayoutConstraint.Create (this.bindingPropertiesView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 124);

			this.buttonMoreSettings.Activated += OnButtonMoreSettingsToggled;

			ToggleSettingsLabel (this.buttonMoreSettings.State == NSCellStateValue.Off);

			this.mainContainer.AddConstraints (new[] {
				NSLayoutConstraint.Create (bindingTypeLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.mainContainer, NSLayoutAttribute.Top, 1f, 20f),
				NSLayoutConstraint.Create (bindingTypeLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.mainContainer, NSLayoutAttribute.Left, 1f, 30f),
				NSLayoutConstraint.Create (bindingTypeLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),

				NSLayoutConstraint.Create (this.bindingTypePopup, NSLayoutAttribute.Top, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.bindingTypePopup, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Right, 1f, 10f),
				NSLayoutConstraint.Create (this.bindingTypePopup, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),

				NSLayoutConstraint.Create (this.ancestorTypeBox, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.bindingTypePopup, NSLayoutAttribute.Bottom, 1f, 24f),
				NSLayoutConstraint.Create (this.ancestorTypeBox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.ancestorTypeBox, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.pathBox,  NSLayoutAttribute.Left, 1f, -8f),
				this.ancestorTypeBoxHeightConstraint,

				NSLayoutConstraint.Create (this.pathBox, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.pathBox, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.mainContainer,  NSLayoutAttribute.Right, 1f, -30f),
				NSLayoutConstraint.Create (this.pathBox, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox,  NSLayoutAttribute.Width, 1f, -40f),
				NSLayoutConstraint.Create (this.pathBox, NSLayoutAttribute.Height, NSLayoutRelation.Equal,this.ancestorTypeBox, NSLayoutAttribute.Height, 1f, 0f),

				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Bottom, 1f, 24f),
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),

				NSLayoutConstraint.Create (this.valueConverterPopup, NSLayoutAttribute.Top, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.valueConverterPopup, NSLayoutAttribute.Left, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Right, 1f, 10f),
				NSLayoutConstraint.Create (this.valueConverterPopup, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),

				NSLayoutConstraint.Create (this.addConverterButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.valueConverterPopup, NSLayoutAttribute.Top, 1f, 2f),
				NSLayoutConstraint.Create (this.addConverterButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal,this.ancestorTypeBox, NSLayoutAttribute.Right, 1f, 0f),
				NSLayoutConstraint.Create (this.addConverterButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, AddConverterButtonSize),
				NSLayoutConstraint.Create (this.addConverterButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, AddConverterButtonSize),

				NSLayoutConstraint.Create (this.buttonMoreSettings, NSLayoutAttribute.Top, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Bottom, 1f, 16f),
				NSLayoutConstraint.Create (this.buttonMoreSettings, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Left, 1f, -5f),
				NSLayoutConstraint.Create (this.buttonMoreSettings, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
				NSLayoutConstraint.Create (this.buttonMoreSettings, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 24),

				NSLayoutConstraint.Create (this.labelOtherSettings, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.buttonMoreSettings, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.labelOtherSettings, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.buttonMoreSettings, NSLayoutAttribute.Right, 1f, 0f),
				NSLayoutConstraint.Create (this.labelOtherSettings, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),

				NSLayoutConstraint.Create (this.bindingPropertiesView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.buttonMoreSettings, NSLayoutAttribute.Bottom, 1f, 10f),
				NSLayoutConstraint.Create (this.bindingPropertiesView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.bindingPropertiesView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.flagsPropertiesView,  NSLayoutAttribute.Left, 1f, -8f),
				bindingPropertiesViewHeightConstraint,
				NSLayoutConstraint.Create (this.bindingPropertiesView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox,  NSLayoutAttribute.Width, 1f, 0f),

				NSLayoutConstraint.Create (this.flagsPropertiesView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.bindingPropertiesView,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.flagsPropertiesView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.mainContainer,  NSLayoutAttribute.Right, 1f, -30f),
				NSLayoutConstraint.Create (this.flagsPropertiesView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.pathBox,  NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (this.flagsPropertiesView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.bindingPropertiesView, NSLayoutAttribute.Height, 1f, 0f),

				NSLayoutConstraint.Create (this.buttonDone, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.mainContainer, NSLayoutAttribute.Bottom, 1f, -50f),
				NSLayoutConstraint.Create (this.buttonDone, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.pathBox, NSLayoutAttribute.Right, 1f, 0f),
				NSLayoutConstraint.Create (this.buttonDone, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),

				NSLayoutConstraint.Create (this.buttonCancel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.buttonDone, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.buttonCancel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.buttonDone, NSLayoutAttribute.Left, 1f, -10f),
				NSLayoutConstraint.Create (this.buttonCancel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
			});

			// put the MainContainer inside this panel's ContentView
			ContentView.AddSubview (this.mainContainer);

			ContentView.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.mainContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.mainContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.mainContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Height, 1f, 0f),
				NSLayoutConstraint.Create (this.mainContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Width, 1f, 0f),

			});

			Title = ViewModel.PropertyDisplay;
			this.buttonDone.Title = Properties.Resources.CreatBindingTitle;
			this.buttonDone.Enabled = ViewModel.CanCreateBinding;

			ViewModel.BindingSources.Task.ContinueWith (t => {
				foreach (BindingSource item in ViewModel.BindingSources.Value) {
					this.bindingTypePopup.Menu.AddItem (new NSMenuItem (item.Name) {
						RepresentedObject = new NSObjectFacade (item)
					});
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ());

			this.bindingTypePopup.Activated += (o, e) => {
				if (this.bindingTypePopup.Menu.HighlightedItem.RepresentedObject is NSObjectFacade facade) {
					ViewModel.SelectedBindingSource = (BindingSource)facade.Target;
				}
			};

			this.valueConverterPopup.Activated += (o, e) => {
				if (this.valueConverterPopup.Menu.HighlightedItem.RepresentedObject is NSObjectFacade facade) {
					ViewModel.SelectedValueConverter = (Resource)facade.Target;
				}
			};

			RepopulateValueConverterPopup ();

			this.addConverterButton.Activated += (sender, e) => {
				ViewModel.RequestAddValueConverter ();
			};

			this.typeHeader = new HeaderView {
				Title = Properties.Resources.Type,
			};

			this.ancestorTypeBox.AddSubview (this.typeHeader);

			this.ancestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.typeHeader, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.typeHeader, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.typeHeader, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (this.typeHeader, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, HeaderHeight),
			});

			this.typeSelectorControl = new TypeSelectorControl {
				Hidden = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.ancestorTypeBox.AddSubview (this.typeSelectorControl);

			this.ancestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.typeSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Top, 1f, TypeSelectorBorder),
				NSLayoutConstraint.Create (this.typeSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.typeSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (this.typeSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Height, 1f, -TypeSelectorBorder)
			});

			var resourceSelectorControl = new BindingResourceSelectorControl (ViewModel) {
				Hidden = true,
			};

			this.ancestorTypeBox.AddSubview (resourceSelectorControl);

			this.ancestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Top, 1f, 5f),
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Height, 1f, -10f)
			});

			var objectSelectorControl = new BindingObjectSelectorControl (ViewModel) {
				Hidden = true,
			};

			this.ancestorTypeBox.AddSubview (objectSelectorControl);

			this.ancestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Top, 1f, 5f),
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Height, 1f, -10f)
			});

			this.longDescription = new UnfocusableTextField {
				Alignment = NSTextAlignment.Left,
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = string.Empty,
			};

			this.ancestorTypeBox.AddSubview (this.longDescription);

			this.ancestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.longDescription, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Top, 1f, 10f),
				NSLayoutConstraint.Create (this.longDescription, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Left, 1f, 10f),
				NSLayoutConstraint.Create (this.longDescription, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (this.longDescription, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
			});

			this.pathHeader = new HeaderView {
				Title = Properties.Resources.Path,
			};

			this.pathBox.AddSubview (this.pathHeader);

			this.pathBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.pathHeader, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.pathBox, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.pathHeader, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.pathBox, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.pathHeader, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.pathBox, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (this.pathHeader, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, HeaderHeight),
			});

			this.pathSelectorControl = new BindingPathSelectorControl (ViewModel);

			this.pathBox.AddSubview (this.pathSelectorControl);

			this.pathBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.pathSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.pathBox, NSLayoutAttribute.Top, 1f, 5f),
				NSLayoutConstraint.Create (this.pathSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.pathBox, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (this.pathSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.pathBox, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (this.pathSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.pathBox, NSLayoutAttribute.Height, 1f, -10f)
			});

			ViewModel.PropertyChanged += OnPropertyChanged;

			ViewModel.CreateValueConverterRequested += OnCreateValueConverterRequested;

			this.buttonDone.Activated += OnButtonDoneActivated;

			CreateMorePropertiesEditors (hostResources);
		}

		private void OnButtonDoneActivated (object sender, EventArgs e)
		{
			if (this.pathSelectorControl.CustomPath.Enabled && !string.IsNullOrEmpty (this.pathSelectorControl.CustomPath.StringValue)) {
				ViewModel.Path = this.pathSelectorControl.CustomPath.StringValue;
			}
			Delegate.Response = NSModalResponse.OK;
			Close ();
		}


		private void OnButtonMoreSettingsToggled (object sender, EventArgs e)
		{
			if (sender is NSButton moreButton) {
				ToggleSettingsLabel (moreButton.State == NSCellStateValue.Off);

				this.bindingPropertiesView.Hidden = moreButton.State == NSCellStateValue.Off;
				this.flagsPropertiesView.Hidden = this.bindingPropertiesView.Hidden;

				this.bindingPropertiesViewHeightConstraint.Constant = this.MoreSettingsViewHeight;
				this.ancestorTypeBoxHeightConstraint.Constant = this.bindingPropertiesView.Hidden ? this.heightConstant : this.heightConstant - (MoreSettingsViewHeight + 20);
				this.mainContainer.SetFrameSize (new CGSize (this.mainContainer.Frame.Width, this.bindingPropertiesView.Hidden ? this.mainContainer.Frame.Height - MoreSettingsViewHeight : this.mainContainer.Frame.Height + MoreSettingsViewHeight));
				SetFrame (new CGRect (new CGPoint (Frame.X, Frame.Y), new CGSize (Frame.Width, this.mainContainer.Frame.Height + this.titleBarHeight)), false, true);
			}
		}


		private void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (CreateBindingViewModel.ShowLongDescription)) {
				this.longDescription.Hidden = !ViewModel.ShowLongDescription;
				this.longDescription.StringValue = ViewModel.ShowLongDescription ? ViewModel.SelectedBindingSource.Description : string.Empty;
				this.typeHeader.Hidden = ViewModel.ShowLongDescription;
			}

			if (e.PropertyName == nameof (CreateBindingViewModel.ShowObjectSelector)) {
				if (ViewModel.ShowObjectSelector) {
					this.typeHeader.Title = Properties.Resources.SelectObjectTitle;
				}
			}

			if (e.PropertyName == nameof (CreateBindingViewModel.ShowTypeSelector)) {
				this.typeSelectorControl.Hidden = !ViewModel.ShowTypeSelector;

				if (ViewModel.ShowTypeSelector) {
					this.typeHeader.Title = Properties.Resources.SelectTypeTitle;

					if (ViewModel.ShowTypeSelector && ViewModel.TypeSelector != null) {
						this.typeSelectorControl.ViewModel = ViewModel.TypeSelector;
					}
				}
			}

			if (e.PropertyName == nameof (CreateBindingViewModel.ShowResourceSelector)) {
				if (ViewModel.ShowResourceSelector) {
					this.typeHeader.Title = Properties.Resources.SelectResourceTitle;
				}
			}

			if (e.PropertyName == nameof (CreateBindingViewModel.SelectedValueConverter)) {
				RepopulateValueConverterPopup ();
			}

			if (e.PropertyName == nameof (CreateBindingViewModel.PropertyDisplay)) {
				Title = ViewModel.PropertyDisplay;
			}

			if (e.PropertyName == nameof (CreateBindingViewModel.CanCreateBinding)) {
				this.buttonDone.Enabled = ViewModel.CanCreateBinding;
			}
		}


		private void CreateMorePropertiesEditors (IHostResourceProvider hostResources)
		{
			// More Settings
			var controlTop = 6;

			foreach (PropertyViewModel vm in ViewModel.BindingProperties) {
				IEditorView editor = this.editorSelector.GetEditor (hostResources, vm);

				NSView nSView = new EditorContainer (hostResources, editor) {
					Identifier = BindingPropertiesIdentifier,
					Label = vm.Property.Name,
					TranslatesAutoresizingMaskIntoConstraints = false,
					ViewModel = vm,
				};

				this.bindingPropertiesView.AddSubview (nSView);

				this.bindingPropertiesView.AddConstraints (new[] {
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.bindingPropertiesView, NSLayoutAttribute.Top, 1f, controlTop),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.bindingPropertiesView, NSLayoutAttribute.Left, 1f, 16f),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.bindingPropertiesView, NSLayoutAttribute.Width, 1f, -20f),
				});

				controlTop += (int)editor.GetHeight (vm);
			}

			var boundsHeight = controlTop;

			controlTop = 9;
			foreach (PropertyViewModel vm in ViewModel.FlagsProperties) {

				IEditorView editor = this.editorSelector.GetEditor (hostResources, vm);

				NSView nSView = new EditorContainer (hostResources, editor) {
					Identifier = FlagPropertiesIdentifier,
					Label = vm.Property.Name,
					TranslatesAutoresizingMaskIntoConstraints = false,
					ViewModel = vm,
				};

				this.flagsPropertiesView.AddSubview (nSView);

				this.flagsPropertiesView.AddConstraints (new[] {
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.flagsPropertiesView, NSLayoutAttribute.Top, 1f, controlTop),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.flagsPropertiesView, NSLayoutAttribute.Left, 1f, 16f),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.flagsPropertiesView, NSLayoutAttribute.Width, 1f, -20f),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18f),
				});

				controlTop += (int)editor.GetHeight (vm);
			}

			if (boundsHeight < controlTop)
				boundsHeight = controlTop;

			MoreSettingsViewHeight = boundsHeight + 8;
		}

		private void ToggleSettingsLabel (bool show)
		{
			this.labelOtherSettings.StringValue = show ? Properties.Resources.ShowSettings : Properties.Resources.HideSettings;
		}

		private void RepopulateValueConverterPopup ()
		{
			this.valueConverterPopup.RemoveAllItems ();
			foreach (Resource item in ViewModel.ValueConverters.Value) {
				this.valueConverterPopup.Menu.AddItem (new NSMenuItem (item.Name) {
					RepresentedObject = new NSObjectFacade (item)
				});
			}
		}

		private void OnCreateValueConverterRequested (object sender, CreateValueConverterEventArgs e)
		{
			ITypeInfo valueConverter = ViewModel.TargetPlatform.EditorProvider.KnownTypes[typeof (CommonValueConverter)];

			var typesTask = ViewModel.TargetPlatform.EditorProvider.GetAssignableTypesAsync (valueConverter, childTypes: false)
				.ContinueWith (t => t.Result.GetTypeTree (), TaskScheduler.Default);

			var createValueConverterWindow = new CreateValueConverterWindow (ViewModel, new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (typesTask));
			var result = (NSModalResponse)(int)NSApplication.SharedApplication.RunModalForWindow (createValueConverterWindow);
			if (result == NSModalResponse.OK) {
				if (createValueConverterWindow.ViewModel.SelectedType != null) {
					e.Name = createValueConverterWindow.ValueConverterName;
					e.ConverterType = createValueConverterWindow.ViewModel.SelectedType;
					e.Source = createValueConverterWindow.ViewModel.Source;
				}
			}
		}
	}

	internal class ModalWindowCloseDelegate : NSWindowDelegate
	{
		public NSModalResponse Response { get; set; } = NSModalResponse.Cancel;

		public override void WillClose (NSNotification notification)
		{
			NSApplication.SharedApplication.StopModalWithCode ((int)Response);
		}
	}
}
