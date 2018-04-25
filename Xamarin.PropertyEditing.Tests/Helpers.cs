using System;
using System.Drawing;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests
{
	static class Helpers
	{
		public static TEnum Next<TEnum> (this Random rand, TEnum differentFrom)
		{
			TEnum val = Next<TEnum> (rand);
			while (val.Equals (differentFrom)) val = Next<TEnum> (rand);
			return val;
		}

		public static TEnum Next<TEnum> (this Random rand)
		{
			if (!typeof (TEnum).IsEnum) throw new InvalidOperationException ("Can't use GetRandomValue with a non-enum type.");

			var values = (TEnum[])Enum.GetValues (typeof (TEnum));
			var index = rand.Next (0, values.Length);
			return values[index];
		}

		public static byte NextByte (this Random rand)
		{
			return (byte)rand.Next (0, 256);
		}

		public static CommonColor NextColor (this Random rand)
			=> new CommonColor (
				rand.NextByte (),
				rand.NextByte (),
				rand.NextByte (),
				rand.NextByte ());

		static char[] vowels = new[] { 'a', 'e', 'i', 'o', 'u' };
		static char[] consonnants = new[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'r', 's', 't', 'v', 'w', 'x', 'z' };

		static char GetVowel (this Random rand) => vowels[rand.Next (0, vowels.Length)];
		static char GetConsonnant (this Random rand) => consonnants[rand.Next (0, vowels.Length)];

		public static string NextFilename (this Random rand, string extension)
			=> rand.NextString () + extension;

		public static string NextString (this Random rand, string differentFrom)
		{
			string val = NextString (rand);
			while (val == differentFrom) val = NextString (rand);
			return val;
		}

		public static string NextString (this Random rand)
			=> string.Concat (
				rand.GetConsonnant (), rand.GetVowel (),
				rand.GetConsonnant (), rand.GetVowel (),
				rand.GetConsonnant (), rand.GetVowel ()
				);

		public static string NextFormattedString (this Random rand, string format, string differentFrom)
		{
			string val = NextFormattedString (rand, format);
			while (val == differentFrom) val = NextFormattedString (rand, format);
			return val;
		}

		public static string NextFormattedString (this Random rand, string format)
			=> string.Format (format, NextString (rand));
	}
}
