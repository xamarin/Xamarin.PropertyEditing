using System;
using System.Linq;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class RatioViewModel
		: PropertyViewModel<CommonRatio>
	{
		public double Numerator
		{
			get { return Value.Numerator; }
			set {
				if (Value.Numerator.Equals (value))
					return;

				Value = new CommonRatio (value, Value.Denominator, Value.RatioSeparator);
			}
		}

		public double Denominator
		{
			get { return Value.Denominator; }

			set {
				if (Value.Denominator.Equals (value))
					return;

				Value = new CommonRatio (Value.Numerator, value, Value.RatioSeparator);
			}
		}

		public char RatioSeparator {
			get { return Value.RatioSeparator; }

			set {
				if (Value.RatioSeparator.Equals (value))
					return;

				Value = new CommonRatio (Value.Numerator, Value.Denominator, value);
			}
		}

		internal void ValueChanged (string stringValue, int caretPosition, int selectionLength, double incrementValue)
		{
			var separator = GetSeparator (stringValue);
			SetRatioFromString (stringValue);

			var newNumerator = Value.Numerator + incrementValue;
			if (newNumerator < 1)
				newNumerator = 1;

			var newDenominator = Value.Denominator + incrementValue;
			if (newDenominator < 1)
				newDenominator = 1;

			// Full selection, increment both
			if (selectionLength == stringValue.Length) {
				// Increment both values.
				Value = new CommonRatio (newNumerator, newDenominator, separator);
			} else {
				// Only Increment numerator 
				Value = new CommonRatio (newNumerator, Value.Denominator, separator);
			}
		}

		public string ValueString
		{
			get { return Value.StringValue; }
			set {
				if (Value.StringValue.Equals (value))
					return;

				SetRatioFromString (value);
			}
		}

		public static char[] Separators = { ':', '/' };
		public static char[] SplitSeparators = { ':', '/', ' ' };

		public RatioViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariationSet variant)
			: base (platform, property, editors, variant)
		{
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof (Numerator));
			OnPropertyChanged (nameof (Denominator));
			OnPropertyChanged (nameof (RatioSeparator));
			OnPropertyChanged (nameof (ValueString));
		}

		private void SetRatioFromString (string value)
		{
			bool parsed = true;
			var parts = value.Split (SplitSeparators, StringSplitOptions.RemoveEmptyEntries);
				double numerator = 0f;
			if (parts.Length == 2) { // We have a potential ratio, let's make sure
				if (!IsDouble (parts[0], out numerator))
					parsed = false;
				double denominator = 0f;
				if (!String.IsNullOrEmpty (parts[1]) && !IsDouble (parts[1], out denominator))
					parsed = false;

				if (parsed) {
					Value = new CommonRatio (numerator, denominator, GetSeparator (value));
				}
			} else if (parts.Length == 1) { // We have a potential whole number, let's make sure
				if (!IsDouble (parts[0], out numerator)) {
					parsed = false;
				}
				if (parsed) {
					Value = new CommonRatio (numerator, 1, GetSeparator (value));
				}
			}

			if (!parsed) {
				SetError (string.Format (Properties.Resources.InvalidRatio, value));
			}
		}

		private char GetSeparator (string finalString)
		{
			var index = finalString.IndexOfAny (Separators);
			if (index != -1)
				return finalString[index];
			else 
				return ':';
		}

		private bool IsDouble (string stringValue, out double value)
		{
			//Checks parsing to number
			if (!double.TryParse (stringValue, out value))
				return false;
					
			//Checks if needs to be positive value
			if (value < 0)
				return false;
					
			//Checks a common validation
			return IsValidInput (stringValue);
		}

		public static bool IsValidInput (string finalString)
		{
			return finalString != "-" && !string.IsNullOrEmpty (finalString);
		}
	}
}
