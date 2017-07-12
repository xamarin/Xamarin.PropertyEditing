using System;
using System.Collections.Generic;
using System.Linq;
using Cadenza.Collections;
using Xamarin.PropertyEditing.Reflection;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PredefinedValuesViewModel<TValue>
		: PropertyViewModel<TValue>
	{
		public PredefinedValuesViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			this.predefinedValues = property as ReflectionEnumPropertyInfo<TValue>;
			if (this.predefinedValues == null)
				throw new ArgumentException (nameof(property) + " did not have predefined values", nameof(property));

			if (IsCombinable) {
				var dict = new Dictionary<string, bool> ();
				foreach (var item in this.predefinedValues.PredefinedValues.Keys) {
					dict.Add (item, (Enum.ToObject(property.Type , Value) as Enum).HasFlag ((Enum)Enum.Parse (property.Type, item)));
				}
				PossibleValues = dict;
			} else {
				PossibleValues =  this.predefinedValues.PredefinedValues.Keys.ToDictionary (x => x, y => false);
			}

			UpdateValueName ();
		}

		public bool IsCombinable
		{
			get
			{
				return this.predefinedValues.IsValueCombinable;
			}
		}

		public void SetValue<T> (object target, T value)
		{
			this.predefinedValues.SetValue (target, value);
		}

		public IReadOnlyDictionary<string, bool> PossibleValues
		{
			get;
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
		private readonly ReflectionEnumPropertyInfo<TValue> predefinedValues;

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
