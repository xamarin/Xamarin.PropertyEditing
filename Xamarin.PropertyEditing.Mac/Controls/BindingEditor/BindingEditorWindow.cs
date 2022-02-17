using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
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
		internal const float HeaderHeight = 28f;

		internal CreateBindingViewModel ViewModel { get; }

		private HeaderView typeHeader;

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

			ViewModel = new CreateBindingViewModel (propertyViewModel.TargetPlatform, propertyViewModel.Editors.Single (), propertyViewModel.Property, includeAddValueConverter: false);

			Delegate = new ModalWindowCloseDelegate ();

			FloatingPanel = true;

			MaxSize = new CGSize (960, 720); // TODO discuss what the Max/Min Size should be and if we should have one.
			MinSize = new CGSize (320, 240);

			this.mainContainer = new NSView {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			this.buttonDone = new FocusableButton {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityBindingEditorOK,
				BezelStyle = NSBezelStyle.Rounded,
				ControlSize = NSControlSize.Regular,
				Highlighted = true,
				KeyEquivalent = "\r", // Fire when enter pressed
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.mainContainer.AddSubview (this.buttonDone);

			this.buttonCancel = new FocusableButton {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityBindingEditorCancel,
				BezelStyle = NSBezelStyle.Rounded,
				ControlSize = NSControlSize.Regular,
				Title = Properties.Resources.Cancel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.buttonCancel.Activated += (sender, e) => {
				Delegate.Response = NSModalResponse.Cancel;
				Close ();
			};

			this.mainContainer.AddSubview (this.buttonCancel);

			var bindingTypeLabel = new UnfocusableTextField {
				Alignment = NSTextAlignment.Right,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, 13),
				StringValue = Properties.Resources.BindingType + ":",
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.mainContainer.AddSubview (bindingTypeLabel);

			this.bindingTypePopup = new FocusablePopUpButton {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityBindingEditorBindingType,
				ControlSize = NSControlSize.Regular,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, 13),
				StringValue = String.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			var bindingTypeMenuList = new NSMenu ();
			this.bindingTypePopup.Menu = bindingTypeMenuList;
			this.mainContainer.AddSubview (this.bindingTypePopup);

			var valueConverterLabel = new UnfocusableTextField {
				Alignment = NSTextAlignment.Right,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, 13),
				StringValue = Properties.Resources.Converter + ":",
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.ancestorTypeBox = new NSView {
				TranslatesAutoresizingMaskIntoConstraints = false,
				WantsLayer = true,

				// Layer out of alphabetical order so that WantsLayer creates the layer first
				Layer = {
					CornerRadius = 1.0f,
					BorderColor = new CGColor (.5f, .5f, .5f, 1.0f),
					BorderWidth = 1,
				},
			};

			this.mainContainer.AddSubview (this.ancestorTypeBox);

			this.mainContainer.AddSubview (valueConverterLabel);

			this.valueConverterPopup = new FocusablePopUpButton {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityBindingEditorValueConverter,
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
				ControlSize = NSControlSize.Regular,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, 13),
			};

			var valueConverterMenuList = new NSMenu ();
			this.valueConverterPopup.Menu = valueConverterMenuList;
			this.mainContainer.AddSubview (this.valueConverterPopup);

			this.addConverterButton = new CommandButton {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityBindingEditorAddConverter,
				BezelStyle = NSBezelStyle.Rounded,
				Command = ViewModel.RequestAddValueConverterCommand,
				Image = NSImage.ImageNamed (NSImageName.AddTemplate),
				Title = string.Empty,
				ToolTip = Properties.Resources.AddValueConverterEllipsis,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.mainContainer.AddSubview (this.addConverterButton);

			this.buttonMoreSettings = new NSButton {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityBindingEditorMore,
				BezelStyle = NSBezelStyle.Disclosure,
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			this.buttonMoreSettings.SetButtonType (NSButtonType.PushOnPushOff);

			this.mainContainer.AddSubview (this.buttonMoreSettings);

			this.labelOtherSettings = new UnfocusableTextField {
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, 13),
				StringValue = Properties.Resources.OtherSettings,
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

			this.bindingPropertiesViewHeightConstraint = NSLayoutConstraint.Create (this.bindingPropertiesView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 124);

			this.buttonMoreSettings.Activated += OnButtonMoreSettingsToggled;

			this.pathSelectorControl = new BindingPathSelectorControl (ViewModel);
			this.mainContainer.AddSubview (this.pathSelectorControl);

			this.mainContainer.AddConstraints (new[] {
				NSLayoutConstraint.Create (bindingTypeLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.bindingTypePopup, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (bindingTypeLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.mainContainer, NSLayoutAttribute.Left, 1f, 21f),
				NSLayoutConstraint.Create (bindingTypeLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 20),

				NSLayoutConstraint.Create (this.bindingTypePopup, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.mainContainer, NSLayoutAttribute.Top, 1f, 18f),
				NSLayoutConstraint.Create (this.bindingTypePopup, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Right, 1f, 10f),

				NSLayoutConstraint.Create (this.ancestorTypeBox, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.bindingTypePopup, NSLayoutAttribute.Bottom, 1f, 18f),
				NSLayoutConstraint.Create (this.ancestorTypeBox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.ancestorTypeBox, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.pathSelectorControl,  NSLayoutAttribute.Left, 1f, -8f),
				this.ancestorTypeBoxHeightConstraint,

				NSLayoutConstraint.Create (this.pathSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.pathSelectorControl, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.mainContainer,  NSLayoutAttribute.Right, 1f, -21f),
				NSLayoutConstraint.Create (this.pathSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox,  NSLayoutAttribute.Width, 1f, -30f),
				NSLayoutConstraint.Create (this.pathSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal,this.ancestorTypeBox, NSLayoutAttribute.Height, 1f, 0f),

				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Bottom, 1f, 22f),
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 20),

				NSLayoutConstraint.Create (this.valueConverterPopup, NSLayoutAttribute.Top, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.valueConverterPopup, NSLayoutAttribute.Left, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Right, 1f, 10f),
				NSLayoutConstraint.Create (this.valueConverterPopup, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Width, 1f, -AddConverterButtonSize - 83), // TODO Need a better calculation here

				NSLayoutConstraint.Create (this.addConverterButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.valueConverterPopup, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.addConverterButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal,this.ancestorTypeBox, NSLayoutAttribute.Right, 1f, 0f),
				NSLayoutConstraint.Create (this.addConverterButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, AddConverterButtonSize),
				NSLayoutConstraint.Create (this.addConverterButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, AddConverterButtonSize),

				NSLayoutConstraint.Create (this.buttonMoreSettings, NSLayoutAttribute.Top, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Bottom, 1f, 18f),
				NSLayoutConstraint.Create (this.buttonMoreSettings, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Left, 1f, -5f),
				NSLayoutConstraint.Create (this.buttonMoreSettings, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 20),
				NSLayoutConstraint.Create (this.buttonMoreSettings, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 20),

				NSLayoutConstraint.Create (this.labelOtherSettings, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.buttonMoreSettings, NSLayoutAttribute.Top, 1f, -1f),
				NSLayoutConstraint.Create (this.labelOtherSettings, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.buttonMoreSettings, NSLayoutAttribute.Right, 1f, 0f),
				NSLayoutConstraint.Create (this.labelOtherSettings, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 20),

				NSLayoutConstraint.Create (this.bindingPropertiesView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.buttonMoreSettings, NSLayoutAttribute.Bottom, 1f, 10f),
				NSLayoutConstraint.Create (this.bindingPropertiesView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.bindingPropertiesView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.flagsPropertiesView,  NSLayoutAttribute.Left, 1f, -8f),
				bindingPropertiesViewHeightConstraint,
				NSLayoutConstraint.Create (this.bindingPropertiesView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox,  NSLayoutAttribute.Width, 1f, 0f),

				NSLayoutConstraint.Create (this.flagsPropertiesView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.bindingPropertiesView,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.flagsPropertiesView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.pathSelectorControl,  NSLayoutAttribute.Right, 1f, 0f),
				NSLayoutConstraint.Create (this.flagsPropertiesView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.pathSelectorControl,  NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (this.flagsPropertiesView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.bindingPropertiesView, NSLayoutAttribute.Height, 1f, 0f),

				NSLayoutConstraint.Create (this.buttonDone, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.mainContainer, NSLayoutAttribute.Bottom, 1f, -44f),
				NSLayoutConstraint.Create (this.buttonDone, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.pathSelectorControl, NSLayoutAttribute.Right, 1f, 0f),
				NSLayoutConstraint.Create (this.buttonDone, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1f, 80f),

				NSLayoutConstraint.Create (this.buttonCancel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.buttonDone, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.buttonCancel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.buttonDone, NSLayoutAttribute.Left, 1f, -10f),
				NSLayoutConstraint.Create (this.buttonCancel, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1f, 80f),
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
			this.buttonDone.Title = Properties.Resources.CreateBindingTitle;
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
				Flush = true,
				Hidden = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.ancestorTypeBox.AddSubview (this.typeSelectorControl);

			this.ancestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.typeSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.typeHeader, NSLayoutAttribute.Bottom, 1f, 0f),
				NSLayoutConstraint.Create (this.typeSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.typeSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (this.typeSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Height, 1f, -10f)
			});

			var resourceSelectorControl = new BindingResourceSelectorControl (ViewModel) {
				Hidden = true,
			};

			this.ancestorTypeBox.AddSubview (resourceSelectorControl);

			this.ancestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Height, 1f, 0f)
			});

			var objectSelectorControl = new BindingObjectSelectorControl (ViewModel) {
				Hidden = true,
			};

			this.ancestorTypeBox.AddSubview (objectSelectorControl);

			this.ancestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.ancestorTypeBox, NSLayoutAttribute.Height, 1f, 0f)
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

			ViewModel.PropertyChanged += OnPropertyChanged;

			ViewModel.CreateValueConverterRequested += OnCreateValueConverterRequested;

			this.buttonDone.Activated += OnButtonDoneActivated;

			CreateMorePropertiesEditors (hostResources);
		}

		private void OnButtonDoneActivated (object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty (this.pathSelectorControl.CustomPath)) {
				ViewModel.Path = this.pathSelectorControl.CustomPath;
			}
			Delegate.Response = NSModalResponse.OK;
			Close ();
		}


		private void OnButtonMoreSettingsToggled (object sender, EventArgs e)
		{
			if (sender is NSButton moreButton) {
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
			var controlTop = 8;
			int editorHeight;

			foreach (PropertyViewModel vm in ViewModel.BindingProperties) {
				IEditorView editor = this.editorSelector.GetEditor (hostResources, vm);

				NSView nSView = new EditorContainer (hostResources, editor, false) {
					Identifier = BindingPropertiesIdentifier,
					Label = vm.Property.Name,
					TranslatesAutoresizingMaskIntoConstraints = false,
					ViewModel = vm,
				};

				this.bindingPropertiesView.AddSubview (nSView);

				editorHeight = (int)editor.GetHeight (vm);
				this.bindingPropertiesView.AddConstraints (new[] {
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.bindingPropertiesView, NSLayoutAttribute.Top, 1f, controlTop),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.bindingPropertiesView, NSLayoutAttribute.Right, 1f, -9f),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.bindingPropertiesView, NSLayoutAttribute.Width, 1f, 0f),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, editorHeight),
				});

				controlTop += editorHeight;
			}

			var boundsHeight = controlTop;

			controlTop = 8;
			foreach (PropertyViewModel vm in ViewModel.FlagsProperties) {

				IEditorView editor = this.editorSelector.GetEditor (hostResources, vm);

				NSView nSView = new EditorContainer (hostResources, editor, false) {
					Identifier = FlagPropertiesIdentifier,
					Label = vm.Property.Name,
					TranslatesAutoresizingMaskIntoConstraints = false,
					ViewModel = vm,
				};

				this.flagsPropertiesView.AddSubview (nSView);

				editorHeight = (int)editor.GetHeight (vm);
				this.flagsPropertiesView.AddConstraints (new[] {
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.flagsPropertiesView, NSLayoutAttribute.Top, 1f, controlTop),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.flagsPropertiesView, NSLayoutAttribute.Left, 1f, 0f),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.flagsPropertiesView, NSLayoutAttribute.Width, 1f, 0f),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, editorHeight)
				});

				controlTop += editorHeight;
			}

			if (boundsHeight < controlTop)
				boundsHeight = controlTop;

			MoreSettingsViewHeight = boundsHeight + 8;
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

			var createValueConverterWindow = new CreateValueConverterWindow (ViewModel, new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (typesTask)) {
				Appearance = EffectiveAppearance,
			};

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
}
