using System;
using System.Drawing;
using System.Text;

namespace Xamarin.PropertyEditing
{
	public static class StringConversionExtensions
	{
		static readonly System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;

		/* public static string ToEditorString (this bool value)
		{
			return value ? "true" : "false";
		}*/

		public static double ToEditorDouble (this string value)
		{
			if (string.IsNullOrEmpty (value))
				return 0;

			double result;
			if (ToEditorDoubleIfPossible (value, out result))
				return result;

			// Constraint multilpliers can be specified as '9:5'. we should treat this as the float '1.8'
			// They can also be specified as 9/5, which should be treated as the float 1.8 as well.
			var parts = value.Split (':');
			if (parts.Length != 2)
				parts = value.Split ('/');

			if (parts.Length == 2) {
				try {
					var first = double.Parse (parts[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					var second = double.Parse (parts[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					return first / second;
				}
				catch {
					throw new ArgumentException (string.Format ("The value '{0}' could not be parsed as a float", value));
				}
			}
			throw new NotSupportedException (string.Format ("Unsupported number format '{0}'", value));
		}

		public static bool ToEditorDoubleIfPossible (this string value, out double number)
		{
			number = 0;
			return double.TryParse (value, System.Globalization.NumberStyles.Any, invariantCulture.NumberFormat, out number);
		}

		public static string ToEditorString (this double value)
		{
			return value == 0d ? "0.0" : value.ToString (invariantCulture.NumberFormat);
		}
	}
}
