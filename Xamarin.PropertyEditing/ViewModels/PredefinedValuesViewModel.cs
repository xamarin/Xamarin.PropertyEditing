using System;
using System.Collections.Generic;
using Cadenza.Collections;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PredefinedValuesViewModel<TValue>
		: PropertyViewModel<TValue>
	{
		public PredefinedValuesViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			this.predefinedValues = property as IHavePredefinedValues<TValue>;
			if (this.predefinedValues == null)
				throw new ArgumentException (nameof(property) + " did not have predefined values", nameof(property));

			this.valueLookup = new BidirectionalDictionary<string, TValue> (this.predefinedValues.PredefinedValues);
			UpdateValueName();
		}

		public IEnumerable<string> PossibleValues
		{
			get { return this.predefinedValues.PredefinedValues.Keys; }
		}

		public string ValueName
		{
			get { return this.valueName; }
			set
			{
				if (value == this.valueName)
					return;

				TValue realValue;
				if (!this.valueLookup.TryGetValue (value, out realValue)) {
					if (this.predefinedValues.IsConstrainedToPredefined) {
						SetError ("Invalid value"); // TODO: Localize & improve
						return;
					}

					// TODO: Figure out where the conversion needs to happen
				} else
					Value = realValue;

				this.valueName = value;
				OnPropertyChanged ();
			}
		}

		// TODO: Combination (flags) values

		protected override TValue ValidateValue (TValue validationValue)
		{
			if (!this.predefinedValues.IsConstrainedToPredefined || this.valueLookup.ContainsValue (validationValue))
				return validationValue;

			return Value;
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			if (this.predefinedValues == null)
				return;

			UpdateValueName();
		}

		private string valueName;
		private BidirectionalDictionary<string, TValue> valueLookup;
		private IHavePredefinedValues<TValue> predefinedValues;

		private void UpdateValueName ()
		{
			string newValueName;
			if (this.valueLookup.TryGetKey (Value, out newValueName)) {
				this.valueName = newValueName;
				OnPropertyChanged (nameof(ValueName));
			}
		}
	}
}
