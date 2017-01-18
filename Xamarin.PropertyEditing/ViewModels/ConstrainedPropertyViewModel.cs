using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal abstract class ConstrainedPropertyViewModel<T>
		: PropertyViewModel<T>
		where T : IComparable<T>
	{
		protected ConstrainedPropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			this.selfConstraint = property as ISelfConstrainedPropertyInfo<T>;
			this.clampProperties = property as IClampedPropertyInfo;

			this.raiseValue = new RelayCommand (() => {
				Value = IncrementValue (Value);
			}, () => {
				T value = IncrementValue (Value);
				return value.CompareTo (ValidateValue (value)) == 0;
			});

			this.lowerValue = new RelayCommand(() => {
				Value = DecrementValue (Value);
			}, () => {
				T value = DecrementValue (Value);
				return value.CompareTo (ValidateValue (value)) == 0;
			});

		    UpdateMaxMin ();
		}

		public bool IsConstrained => (this.selfConstraint != null || this.clampProperties != null);

		public T MaximumValue
		{
			get { return this.maximumValue; }
			private set
			{
				if (Equals (this.maximumValue, value))
					return;

				this.maximumValue = value;
				OnPropertyChanged();
				this.raiseValue.ChangeCanExecute ();
			}
		}

		public T MinimumValue
		{
			get { return this.minimumValue; }
			private set
			{
				if (Equals (this.minimumValue, value))
					return;

				this.minimumValue = value;
				OnPropertyChanged();
				this.lowerValue.ChangeCanExecute();
			}
		}

		public ICommand RaiseValue => this.raiseValue;

		public ICommand LowerValue => this.lowerValue;

		protected override T ValidateValue (T validationValue)
		{
			if (!IsConstrained)
				return validationValue;

			if (validationValue.CompareTo (MaximumValue) > 0)
				return MaximumValue;
			else if (validationValue.CompareTo (MinimumValue) < 0)
				return MinimumValue;

			return validationValue;
		}

		protected abstract T IncrementValue (T value);
		protected abstract T DecrementValue (T value);

		protected override void OnEditorPropertyChanged (object sender, EditorPropertyChangedEventArgs e)
		{
		    UpdateMaxMin ();
		    base.OnEditorPropertyChanged (sender, e);
		}

	    private void UpdateMaxMin ()
	    {
	        bool isDefault = true;
	        T max = default(T), min = default(T);
	        if (this.selfConstraint != null) {
	            isDefault = false;
	            max = this.selfConstraint.MaxValue;
	            min = this.selfConstraint.MinValue;
	        }

	        if (this.clampProperties != null && Editors.Count > 0) {
	            if (this.clampProperties.MaximumProperty != null) {
	                T highest = Editors.Select (ed => ed.GetValue<T> (this.clampProperties.MaximumProperty)).Min (v => v.Value);
	                max = (isDefault) ? highest : Min (max, highest);
	            }

	            if (this.clampProperties.MinimumProperty != null) {
	                T lowest = Editors.Select (ed => ed.GetValue<T> (this.clampProperties.MinimumProperty)).Max (v => v.Value);
	                min = (isDefault) ? lowest : Max (min, lowest);
	            }
	        }

	        MaximumValue = max;
	        MinimumValue = min;
	    }

		private readonly RelayCommand raiseValue, lowerValue;
	    private readonly IClampedPropertyInfo clampProperties;
		private readonly ISelfConstrainedPropertyInfo<T> selfConstraint;
		private T maximumValue;
		private T minimumValue;

	    private T Max (T left, T right)
	    {
	        if (left == null)
	            return right;

	        return (left.CompareTo (right) < 0) ? right : left;
	    }

	    private T Min (T left, T right)
	    {
            if (left == null)
                return right;

            return (left.CompareTo (right) < 0) ? left : right;
        }
	}
}
