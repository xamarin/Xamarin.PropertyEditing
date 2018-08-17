using System;
using System.Collections;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	public class PropertyVariation
		: IList<PropertyVariationOption>
	{
		public PropertyVariation (params PropertyVariationOption[] options)
		{
			if (options == null)
				throw new ArgumentNullException (nameof(options));

			for (int i = 0; i < options.Length; i++)
				Add (options[i]);
		}

		public int Count => this.variations.Count;
		bool ICollection<PropertyVariationOption>.IsReadOnly => false;

		public PropertyVariationOption this[int index]
		{
			get => this.variations[index];
			set => this.variations[index] = value;
		}

		public IEnumerator<PropertyVariationOption> GetEnumerator () => this.variations.GetEnumerator ();
		IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();
		public void Add (PropertyVariationOption item) => this.variations.Add (item);
		public void Clear () => this.variations.Clear ();
		public bool Contains (PropertyVariationOption item) => this.variations.Contains (item);
		public void CopyTo (PropertyVariationOption[] array, int arrayIndex) => this.variations.CopyTo (array, arrayIndex);
		public bool Remove (PropertyVariationOption item) => this.variations.Remove (item);
		public int IndexOf (PropertyVariationOption item) => this.variations.IndexOf (item);
		public void Insert (int index, PropertyVariationOption item) => this.variations.Insert (index, item);
		public void RemoveAt (int index) => this.variations.RemoveAt (index);

		private readonly List<PropertyVariationOption> variations = new List<PropertyVariationOption> ();
	}
}