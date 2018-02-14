using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class NumericPropertyViewModel<T>
		: ConstrainedPropertyViewModel<T>
		where T : struct, IComparable<T>
	{
		public NumericPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
			this.raiseValue = new RelayCommand (() => {
				Value = Numeric<T>.Increment (Value);
			}, () => {
				T value = Numeric<T>.Increment (Value);
				return value.CompareTo (ValidateValue (value)) == 0;
			});

			this.lowerValue = new RelayCommand(() => {
				Value = Numeric<T>.Decrement (Value);
			}, () => {
				T value = Numeric<T>.Decrement (Value);
				return value.CompareTo (ValidateValue (value)) == 0;
			});
		}

		public ICommand RaiseValue => this.raiseValue;

		public ICommand LowerValue => this.lowerValue;

		protected override void OnPropertyChanged (string propertyName = null)
		{
			base.OnPropertyChanged (propertyName);

			switch (propertyName) {
			case nameof(MinimumValue):
				this.lowerValue?.ChangeCanExecute();
				break;
			case nameof(MaximumValue):
				this.raiseValue?.ChangeCanExecute();
				break;
			}
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();

			if (this.lowerValue != null) {
				this.lowerValue.ChangeCanExecute ();
				this.raiseValue.ChangeCanExecute ();
			}
		}

		private readonly RelayCommand raiseValue, lowerValue;
	}
}