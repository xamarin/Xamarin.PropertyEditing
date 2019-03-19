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
		public VariationViewModel (string category, IEnumerable<PropertyVariationOption> variations)
		{
			if (category == null)
				throw new ArgumentNullException (nameof(category));
			if (variations == null)
				throw new ArgumentNullException (nameof (variations));

			Name = category;
			var vs = variations.ToList ();
			vs.Insert (0, new PropertyVariationOption (category, Resources.Any));
			Variations = vs;

			this.selectedOption = vs[0];
		}

		public string Name
		{
			get;
		}

		public IReadOnlyList<PropertyVariationOption> Variations
		{
			get;
		}

		public bool IsAnySelected => SelectedOption == Variations[0];

		public PropertyVariationOption SelectedOption
		{
			get { return this.selectedOption; }
			set
			{
				if (this.selectedOption == value)
					return;

				this.selectedOption = value;
				OnPropertyChanged();
				OnPropertyChanged (nameof(IsAnySelected));
			}
		}

		private PropertyVariationOption selectedOption;
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

		public PropertyVariation Variation
		{
			get { return this.variation; }
			private set
			{
				if (this.variation == value)
					return;

				this.variation = value;
				OnPropertyChanged();
			}
		}

		private readonly IPropertyInfo property;
		private PropertyVariation variation;

		private void OnCategoryPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (VariationViewModel.SelectedOption))
				((RelayCommand)CreateVariantCommand).ChangeCanExecute ();
		}

		private void OnCreateVariant ()
		{
			Variation = new PropertyVariation (VariationCategories
				.Where (vm => !vm.IsAnySelected)
				.Select (vm => vm.SelectedOption)
				.ToArray ());
		}

		private bool CanCreateVariant ()
		{
			return !VariationCategories.All (vm => vm.IsAnySelected);
		}
	}
}
