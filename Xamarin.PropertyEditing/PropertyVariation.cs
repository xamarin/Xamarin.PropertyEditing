using System;

namespace Xamarin.PropertyEditing
{
	public class PropertyVariation
	{
		public PropertyVariation (string category, string name)
		{
			if (category == null)
				throw new ArgumentNullException (nameof (category));
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			Category = category;
			Name = name;
		}

		public string Category
		{
			get;
		}

		public string Name
		{
			get;
		}
	}
}