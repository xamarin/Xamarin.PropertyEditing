using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

			var list = new List<string> (this.predefinedValues.PredefinedValues.Keys);
			// If we're constrained but can't use the default, we need a blank to represent default
			if (this.predefinedValues.IsConstrainedToPredefined) {
				if (!property.ValueSources.HasFlag (ValueSources.Default) || !TryGetValueName (default(TValue), out string defaultName)) {
					if (!list.Contains (String.Empty)) {
						this.supportUnset = true;
						list.Insert (0, String.Empty);
					}
				}
			}

			PossibleValues = list;
			UpdateValueName();
		}

		public IReadOnlyList<string> PossibleValues
		{
			get;
		}

		public string ValueName
		{
			get { return this.valueName; }
			set { SetValueName (value); }
		}

		public bool IsConstrainedToPredefined => this.predefinedValues.IsConstrainedToPredefined;

		protected override TValue CoerceValue (TValue validationValue)
		{
			if (!IsConstrainedToPredefined || IsValueDefined (validationValue))
				return base.CoerceValue (validationValue);

			return Value;
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			if (this.predefinedValues == null)
				return;

			UpdateValueName();
		}

		private readonly bool supportUnset;
		private string valueName;
		private readonly IHavePredefinedValues<TValue> predefinedValues;

		private async void SetValueName (string value)
		{
			if (value == this.valueName)
				return;

			value = value ?? String.Empty;

			TValue realValue;
			if (!this.predefinedValues.PredefinedValues.TryGetValue (value, out realValue)) {
				if (IsConstrainedToPredefined && (!this.supportUnset || value != String.Empty)) {
					SetError (String.Format (Properties.Resources.InvalidValue, value)); 
					return;
				}

				await SetValueAsync (new ValueInfo<TValue> {
					Source = ValueSource.Local,
					ValueDescriptor = value
				});
			} else
				Value = realValue;
		}

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
			if (ValueSource == ValueSource.Unset) {
				this.valueName = String.Empty;
				OnPropertyChanged (nameof(ValueName));
			// Order relevant: Value may default() to a valid value, so ValueDescriptor takes precedence
			} else if (!IsConstrainedToPredefined && CurrentValue != null && CurrentValue.ValueDescriptor is string custom) {
				this.valueName = custom;
				OnPropertyChanged (nameof (ValueName));
			} else if (TryGetValueName (Value, out string newValueName)) {
				this.valueName = newValueName;
				OnPropertyChanged (nameof(ValueName));
			}
		}
	}
}
