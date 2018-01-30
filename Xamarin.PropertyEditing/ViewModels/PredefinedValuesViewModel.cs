using System;
using System.Collections.Generic;
using System.Linq;
using Cadenza.Collections;
using Xamarin.PropertyEditing.Resources;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PredefinedValuesViewModel<TValue>
		: PropertyViewModel<TValue>
	{
		public PredefinedValuesViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
			this.predefinedValues = property as IHavePredefinedValues<TValue>;
			if (this.predefinedValues == null)
				throw new ArgumentException (nameof(property) + " did not have predefined values", nameof(property));

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
				if (!this.predefinedValues.PredefinedValues.TryGetValue (value, out realValue)) {
					if (this.predefinedValues.IsConstrainedToPredefined) {
						SetError (string.Format (LocalizationResources.InvalidValue, value)); 
						return;
					}

					// TODO: Figure out where the conversion needs to happen
				} else
					Value = realValue;

				this.valueName = value;
				OnPropertyChanged ();
			}
		}

		public bool IsConstrainedToPredefined => this.predefinedValues.IsConstrainedToPredefined;

		// TODO: Combination (flags) values

		protected override TValue ValidateValue (TValue validationValue)
		{
			if (!this.predefinedValues.IsConstrainedToPredefined || IsValueDefined (validationValue))
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
		private readonly IHavePredefinedValues<TValue> predefinedValues;

		private bool IsValueDefined (TValue value)
		{
			return this.predefinedValues.PredefinedValues.Values.Contains (value);
		}

		private bool TryGetValueName (TValue value, out string name)
		{
			name = null;

			foreach (var kvp in this.predefinedValues.PredefinedValues) {
				if (Equals (kvp.Value, value)) {
					name = kvp.Key;
					return true;
				}
			}

			return false;
		}

		private void UpdateValueName ()
		{
			string newValueName;
			if (TryGetValueName (Value, out newValueName)) {
				this.valueName = newValueName;
				OnPropertyChanged (nameof(ValueName));
			}
		}
	}
}
