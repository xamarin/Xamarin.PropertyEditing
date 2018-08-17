using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.PropertyEditing.Properties;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class VariationViewModel
		: NotifyingObject
	{
		public VariationViewModel (string category, IEnumerable<PropertyVariation> variations)
		{
			if (category == null)
				throw new ArgumentNullException (nameof(category));
			if (variations == null)
				throw new ArgumentNullException (nameof (variations));

			Name = category;
			var vs = variations.ToList ();
			vs.Insert (0, new PropertyVariation (category, Resources.Any));
			Variations = vs;

			this.selectedVariation = vs[0];
		}

		public string Name
		{
			get;
		}

		public IReadOnlyList<PropertyVariation> Variations
		{
			get;
		}

		public bool IsAnySelected => SelectedVariation == Variations[0];

		public PropertyVariation SelectedVariation
		{
			get { return this.selectedVariation; }
			set
			{
				if (this.selectedVariation == value)
					return;

				this.selectedVariation = value;
				OnPropertyChanged();
				OnPropertyChanged (nameof(IsAnySelected));
			}
		}

		private PropertyVariation selectedVariation;
	}

	internal class CreateVariantViewModel
		: NotifyingObject
	{
		public CreateVariantViewModel (IPropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));

			this.property = property;
			VariationCategories = property.Variations
				.GroupBy (v => v.Category)
				.Select (g => new VariationViewModel (g.Key, g))
				.ToList();

			foreach (var vvm in VariationCategories) {
				vvm.PropertyChanged += OnCategoryPropertyChanged;
			}

			CreateVariantCommand = new RelayCommand (OnCreateVariant, CanCreateVariant);
		}

		public IReadOnlyList<VariationViewModel> VariationCategories
		{
			get;
		}

		public ICommand CreateVariantCommand
		{
			get;
		}

		public PropertyVariationSet Variant
		{
			get { return this.variant; }
			private set
			{
				if (this.variant == value)
					return;

				this.variant = value;
				OnPropertyChanged();
			}
		}

		private readonly IPropertyInfo property;
		private PropertyVariationSet variant;

		private void OnCategoryPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (VariationViewModel.SelectedVariation))
				((RelayCommand)CreateVariantCommand).ChangeCanExecute ();
		}

		private void OnCreateVariant ()
		{
			Variant = new PropertyVariationSet (VariationCategories
				.Where (vm => !vm.IsAnySelected)
				.Select (vm => vm.SelectedVariation)
				.ToArray ());
		}

		private bool CanCreateVariant ()
		{
			return !VariationCategories.All (vm => vm.IsAnySelected);
		}
	}
}
