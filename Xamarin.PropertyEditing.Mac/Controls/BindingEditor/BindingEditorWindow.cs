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
	internal class BindingEditorWindow : BasePanelWindow
	{
		private readonly CreateBindingViewModel viewModel;
		private readonly PropertyEditorSelector editorSelector = new PropertyEditorSelector ();

		internal BindingEditorWindow (IHostResourceProvider hostResources, PropertyViewModel propertyViewModel) 
		{
			this.viewModel = new CreateBindingViewModel (propertyViewModel.TargetPlatform, propertyViewModel.Editors.Single (), propertyViewModel.Property);

			Title = this.viewModel.PropertyDisplay;
			this.ButtonDone.Title = Properties.Resources.CreateBindingTitle;

			foreach (BindingSource item in this.viewModel.BindingSources.Value) {
				this.BindingTypePopup.Menu.AddItem (new NSMenuItem (item.Name) {
					RepresentedObject = new NSObjectFacade (item)
				});
			}

			this.BindingTypePopup.Activated += (o, e) => {
				if (this.BindingTypePopup.Menu.HighlightedItem.RepresentedObject is NSObjectFacade facade) {
					this.viewModel.SelectedBindingSource = (BindingSource)facade.Target;
				}
			};

			this.ValueConverterPopup.Activated += (o, e) => {
				if (this.ValueConverterPopup.Menu.HighlightedItem.RepresentedObject is NSObjectFacade facade) {
					this.viewModel.SelectedValueConverter = (Resource)facade.Target;
				}
			};

			RepopulateValueConverterPopup ();

			this.AddConverterButton.Activated += (sender, e) => {
				this.viewModel.SelectedValueConverter = CreateBindingViewModel.AddValueConverter;
			};

			var typeHeader = new HeaderView {
				Title = Properties.Resources.Type,
			};

			this.AncestorTypeBox.AddSubview (typeHeader);

			this.AncestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (typeHeader, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (typeHeader, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (typeHeader, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (typeHeader, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 40),
			});

			var typeSelectorControl = new TypeSelectorControl {
				Hidden = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.AncestorTypeBox.AddSubview (typeSelectorControl);

			this.AncestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Top, 1f, 36f),
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (typeSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Height, 1f, -35f)
			});

			var resourceSelectorControl = new BindingResourceSelectorControl (this.viewModel) {
				Hidden = true,
			};

			this.AncestorTypeBox.AddSubview (resourceSelectorControl);

			this.AncestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Top, 1f, 5f),
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (resourceSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Height, 1f, -10f)
			});

			var objectSelectorControl = new BindingObjectSelectorControl (this.viewModel) {
				Hidden = true,
			};

			this.AncestorTypeBox.AddSubview (objectSelectorControl);

			this.AncestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Top, 1f, 5f),
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (objectSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Height, 1f, -10f)
			});

			var longDescription = new UnfocusableTextField {
				Alignment = NSTextAlignment.Left,
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = string.Empty,
			};

			this.AncestorTypeBox.AddSubview (longDescription);

			this.AncestorTypeBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (longDescription, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Top, 1f, 10f),
				NSLayoutConstraint.Create (longDescription, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Left, 1f, 10f),
				NSLayoutConstraint.Create (longDescription, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (longDescription, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
			});

			var pathHeader = new HeaderView {
				Title = Properties.Resources.Path,
			};

			this.PathBox.AddSubview (pathHeader);

			this.PathBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (pathHeader, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.PathBox, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (pathHeader, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.PathBox, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (pathHeader, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.PathBox, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (pathHeader, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 40),
			});

			var pathSelectorControl = new BindingPathSelectorControl (this.viewModel);

			this.PathBox.AddSubview (pathSelectorControl);

			this.PathBox.AddConstraints (new[] {
				NSLayoutConstraint.Create (pathSelectorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.PathBox, NSLayoutAttribute.Top, 1f, 5f),
				NSLayoutConstraint.Create (pathSelectorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.PathBox, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (pathSelectorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.PathBox, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (pathSelectorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.PathBox, NSLayoutAttribute.Height, 1f, -10f)
			});

			this.viewModel.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof (CreateBindingViewModel.ShowLongDescription)) {
					longDescription.Hidden = !this.viewModel.ShowLongDescription;
					longDescription.StringValue = this.viewModel.ShowLongDescription ? this.viewModel.SelectedBindingSource.Description : string.Empty;
					typeHeader.Hidden = this.viewModel.ShowLongDescription;
				}

				if (e.PropertyName == nameof (CreateBindingViewModel.ShowObjectSelector)) {
					if (this.viewModel.ShowObjectSelector) {
						typeHeader.Title = Properties.Resources.SelectObjectTitle;
					}
				}

				if (e.PropertyName == nameof (CreateBindingViewModel.ShowTypeSelector)) {
					typeSelectorControl.Hidden = !this.viewModel.ShowTypeSelector;

					if (this.viewModel.ShowTypeSelector) {
						typeHeader.Title = Properties.Resources.SelectTypeTitle;

						if (this.viewModel.ShowTypeSelector && this.viewModel.TypeSelector != null) {
							typeSelectorControl.ViewModel = this.viewModel.TypeSelector;
						}
					}
				}

				if (e.PropertyName == nameof (CreateBindingViewModel.ShowResourceSelector)) {
					if (this.viewModel.ShowResourceSelector) {
						typeHeader.Title = Properties.Resources.SelectResourceTitle;
					}
				}

				if (e.PropertyName == nameof (CreateBindingViewModel.SelectedValueConverter)) {
					RepopulateValueConverterPopup ();
				}
			};

			this.viewModel.CreateValueConverterRequested += OnCreateValueConverterRequested;

			this.ButtonDone.Activated += (sender, e) => {
				if (pathSelectorControl.CustomPath.Enabled && !string.IsNullOrEmpty (pathSelectorControl.CustomPath.Cell.Title)) {
					this.viewModel.Path = pathSelectorControl.CustomPath.Cell.Title;
				}

				Close ();
			};

			// More Settings
			var controlTop = 6;
			var identifier = "BindingProperties";

			foreach (PropertyViewModel vm in this.viewModel.BindingProperties) {
				IEditorView editor = this.editorSelector.GetEditor (hostResources, vm);

				NSView nSView = new EditorContainer (hostResources, editor) {
					Identifier = identifier,
					Label = vm.Property.Name,
					TranslatesAutoresizingMaskIntoConstraints = false,
					ViewModel = vm,
				};

				this.BindingPropertiesView.AddSubview (nSView);

				this.BindingPropertiesView.AddConstraints (new[] {
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.BindingPropertiesView, NSLayoutAttribute.Top, 1f, controlTop),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.BindingPropertiesView, NSLayoutAttribute.Left, 1f, 16f),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.BindingPropertiesView, NSLayoutAttribute.Width, 1f, -20f),
				});

				controlTop += PropertyEditorControl.DefaultControlHeight;
			}

			var boundsHeight = controlTop;

			controlTop = 9;
			identifier = "FlagsProperties";
			foreach (PropertyViewModel vm in this.viewModel.FlagsProperties) {

				IEditorView editor = this.editorSelector.GetEditor (hostResources, vm); 

				NSView nSView = new EditorContainer (hostResources, editor) {
					Identifier = identifier,
					Label = vm.Property.Name,
					TranslatesAutoresizingMaskIntoConstraints = false,
					ViewModel = vm,
				};

				this.FlagsPropertiesView.AddSubview (nSView);

				this.FlagsPropertiesView.AddConstraints (new[] {
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.FlagsPropertiesView, NSLayoutAttribute.Top, 1f, controlTop),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.FlagsPropertiesView, NSLayoutAttribute.Left, 1f, 16f),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.FlagsPropertiesView, NSLayoutAttribute.Width, 1f, -20f),
					NSLayoutConstraint.Create (nSView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18f),
				});

				controlTop += PropertyEditorControl.DefaultControlHeight;
			}

			if (boundsHeight < controlTop)
				boundsHeight = controlTop;

			this.MoreSettingsViewHeight = boundsHeight + 8;
		}

		private void RepopulateValueConverterPopup ()
		{
			this.ValueConverterPopup.RemoveAllItems ();
			foreach (Resource item in this.viewModel.ValueConverters.Value) {
				this.ValueConverterPopup.Menu.AddItem (new NSMenuItem (item.Name) {
					RepresentedObject = new NSObjectFacade (item)
				});
			}
		}

		private void OnCreateValueConverterRequested (object sender, CreateValueConverterEventArgs e)
		{
			ITypeInfo valueConverter = this.viewModel.TargetPlatform.EditorProvider.KnownTypes[typeof (CommonValueConverter)];

			var typesTask = this.viewModel.TargetPlatform.EditorProvider.GetAssignableTypesAsync (valueConverter, childTypes: false)
				.ContinueWith (t => t.Result.GetTypeTree (), TaskScheduler.Default);

			var createValueConverterWindow = new CreateValueConverterWindow (this.viewModel, new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (typesTask));
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
