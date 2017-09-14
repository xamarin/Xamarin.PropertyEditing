using System;
using System.Drawing;

namespace Xamarin.PropertyEditing.Tests
{
	static class Helpers
	{
		public static TEnum Next<TEnum>(this Random rand)
		{
			if (!typeof (TEnum).IsEnum) throw new InvalidOperationException ("Can't use GetRandomValue with a non-enum type.");

			var values = (TEnum[])Enum.GetValues (typeof (TEnum));
			var index = rand.Next (0, values.Length);
			return values[index];
		}

		public static Color NextColor(this Random rand)
			=> Color.FromArgb(
				rand.Next(0, 256),
				rand.Next(0, 256),
				rand.Next(0, 256),
				rand.Next(0, 256));

		static char[] vowels = new[] { 'a', 'e', 'i', 'o', 'u' };
		static char[] consonnants = new[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'r', 's', 't', 'v', 'w', 'x', 'z' };

		static char GetVowel (this Random rand) => vowels[rand.Next (0, vowels.Length)];
		static char GetConsonnant (this Random rand) => consonnants[rand.Next (0, vowels.Length)];

		public static string NextFilename (this Random rand, string extension)
			=> string.Concat(
				rand.GetConsonnant(), rand.GetVowel(),
				rand.GetConsonnant (), rand.GetVowel (),
				rand.GetConsonnant (), rand.GetVowel (),
				extension);
	}
}
