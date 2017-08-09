﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
				throw new ArgumentException (nameof (property) + " did not have predefined values", nameof (property));

			UpdatePossibleValues ();
			UpdateValueName ();
		}

		public bool IsCombinable {
			get { return this.predefinedValues.IsValueCombinable; }
		}

		IReadOnlyDictionary<string, ValueChecked> possibleValues;
		public IReadOnlyDictionary<string, ValueChecked> PossibleValues
		{
			get { return this.possibleValues; }
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
				} else {
                    this.valueName = value;
					Value = realValue;
				}
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

			UpdatePossibleValues ();
			UpdateValueName ();
		}

		private string valueName;
		private readonly ReflectionEnumPropertyInfo<TValue> predefinedValues;

		private bool IsValueDefined (TValue value)
		{
			if (IsCombinable) {
				return Enum.ToObject (Property.Type, value) != null;
			} else {
				return this.predefinedValues.PredefinedValues.Values.Contains (value);
			}
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

		void UpdatePossibleValues ()
		{
			if (IsCombinable) {
				possibleValues = this.predefinedValues.PredefinedValues.ToDictionary (x => x.Key, y => new ValueChecked { Value = y.Value, Checked = (Convert.ToInt64 (y.Value) & Convert.ToInt64 (this.Value)) == Convert.ToInt64 (y.Value) });
			} else {
				possibleValues = this.predefinedValues.PredefinedValues.ToDictionary (x => x.Key, y => new ValueChecked ());
			}
		}

		public class ValueChecked
		{
			public TValue Value;
			public bool Checked;
		}
	}
}
