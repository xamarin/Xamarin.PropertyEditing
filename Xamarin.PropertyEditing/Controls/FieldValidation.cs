using System;
using System.Linq;

namespace Xamarin.PropertyEditing
{
	public class FieldValidation
	{
		public const string DefaultInitialValue = "1";
		public const double DefaultIntengerIncrementValue = 1;
		public const double DefaultDecimalIncrementValue = 0.5d;

		public const int DefaultXcodeMaxRoundDigits = 6;
		public static char[] Separators = { '/', ':' };

		public static void SetSpinButtonValue (string newValue, int roundDigits, out string finalStringValue, out double finalDecimalValue)
		{
			if (CheckIfRatio (newValue)) {
				finalStringValue = FieldValidation.RoundRatioValue (newValue, roundDigits);
				finalDecimalValue = newValue.Split (FieldValidation.GetSeparator (newValue))[0].ToEditorDouble ();
			}
			else {
				finalStringValue = FieldValidation.RoundDoubleValue (newValue, roundDigits);
				finalDecimalValue = newValue.ToEditorDouble ();
			}
		}

		public static double CoerceValue (double actualValue, double minimumValue, double maximumValue, bool isWrap)
		{
			return (isWrap && actualValue == minimumValue) ? maximumValue : CoerceValue (actualValue, minimumValue, maximumValue);
		}
		public static double CoerceValue (double actualValue, double minimumValue, double maximumValue)
		{
			return Math.Min (maximumValue, Math.Max (actualValue, minimumValue));
		}

		public static string FixInitialValue (string value, string defaultValue = DefaultInitialValue)
		{
			double parsedValue;
			if (!value.ToEditorDoubleIfPossible (out parsedValue))
				value = defaultValue;
			return string.IsNullOrEmpty (value) ? defaultValue : value;
		}

		public static string GetInitialOrDefaultValue (string value, double minimumValue, double maximumValue)
		{
			return string.IsNullOrEmpty (value) ?
				FixZeroDoubleValue (Math.Min (Math.Max (minimumValue, 0), maximumValue).ToEditorString ()) :
				value;
		}

		public static void SetSpinButtonValue (double newValue, string actualStringValue, int roundDigits, out string finalStringValue, out double finalDecimalValue)
		{
			//We round the new value
			newValue = Math.Round (newValue, roundDigits);
			//Get the current data to increment
			double originalValue;
			//values 0 are writed like 0.0 we need to fix it
			if (CheckIfRatio (actualStringValue)) {
				originalValue = actualStringValue.Split (FieldValidation.GetSeparator (actualStringValue))[0].ToEditorDouble ();
				finalStringValue = string.Concat (FieldValidation.FixZeroDoubleValue (newValue.ToEditorString ()), actualStringValue.Substring (originalValue.ToEditorString ().Length));
			}
			else {
				finalStringValue = FieldValidation.FixZeroDoubleValue (newValue.ToEditorString ());
			}
			finalDecimalValue = newValue;
		}

		public static bool IsNumberLast (string value)
		{
			return value.Length > 0 && Char.IsNumber (value.LastOrDefault ());
		}

		public static string FixZeroDoubleValue (string value)
		{
			return value == "0.0" ? "0" : value;
		}

		public static string RoundDoubleValue (string value, int decimals)
		{
			return FixZeroDoubleValue (Math.Round (value.ToEditorDouble (), decimals).ToEditorString ());
		}

		public static string RoundRatioValue (string value, int decimals)
		{
			var separator = FieldValidation.GetSeparator (value);
			var values = value.Split (separator);
			return string.Concat (RoundDoubleValue (values[0], decimals), separator, RoundDoubleValue (values[1], decimals));
		}

		public static bool IsRatio (string value)
		{
			if (IsNumberLast (value) && CheckIfRatio (value, ValidationType.Decimal, false)) {
				var values = value.Split (FieldValidation.GetSeparator (value));
				return !values[0].ToEditorDouble ().Equals (0) && !values[1].ToEditorDouble ().Equals (0);
			}
			return CheckIfNumber (value, ValidationType.Decimal, false);
		}

		public static bool ValidateDecimal (string finalString, bool allowNegativeValues)
		{
			double value;
			//Checks parsing to number
			if (!double.TryParse (finalString, out value))
				return false;
			//Checks if needs to be possitive value
			if (!allowNegativeValues && value < 0)
				return false;
			//Checks a common validation
			return CommonValidate (finalString);
		}

		public static bool ValidateInteger (string finalString, bool allowNegativeValues)
		{
			int value;
			//Checks parsing to number
			if (!int.TryParse (finalString, out value))
				return false;
			//Checks if needs to be possitive value
			if (!allowNegativeValues && value < 0)
				return false;
			//Checks a common validation
			return CommonValidate (finalString);
		}

		static bool CommonValidate (string finalString)
		{
			return finalString != "-" && !string.IsNullOrEmpty (finalString);
		}

		public static bool ValidateRatio (string finalString, ValidationType mode, bool allowNegativeValues)
		{
			if (CheckIfRatio (finalString, mode, allowNegativeValues) || CheckIfNumber (finalString, mode, allowNegativeValues))
				return true;
			return false;
		}

		public static char GetSeparator (string finalString)
		{
			foreach (var separator in Separators) {
				if (finalString.Contains (separator))
					return separator;
			}
			return default (char);
		}

		public static bool CheckIfRatio (string value)
		{
			return CheckIfRatio (value, ValidationType.Decimal, false);
		}

		public static bool CheckIfRatio (string finalString, ValidationType mode, bool allowNegativeValues)
		{
			foreach (var separator in Separators) {
				var parts = finalString.Split (new[] { separator });
				if (parts.Length == 2) {
					bool parsed = true;

					if (!CheckIfNumber (parts[0], mode, allowNegativeValues))
						parsed = false;

					if (!String.IsNullOrEmpty (parts[1]) && !CheckIfNumber (parts[1], mode, allowNegativeValues))
						parsed = false;

					if (parsed)
						return true;
				}
			}
			return false;
		}

		public static bool CheckIfNumber (string finalString, ValidationType mode, bool allowNegativeValues)
		{
			return mode == ValidationType.Decimal ?
				ValidateDecimal (finalString, allowNegativeValues) :
				ValidateInteger (finalString, allowNegativeValues);
		}
	}
}
