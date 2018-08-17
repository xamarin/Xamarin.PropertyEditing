using System;
using System.Collections;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	public class PropertyVariationSet
		: IList<PropertyVariation>
	{
		public PropertyVariationSet (params PropertyVariation[] variations)
		{
			if (variations == null)
				throw new ArgumentNullException (nameof(variations));

			for (int i = 0; i < variations.Length; i++)
				Add (variations[i]);
		}

		public int Count => this.variations.Count;
		bool ICollection<PropertyVariation>.IsReadOnly => false;

		public PropertyVariation this[int index]
		{
			get => this.variations[index];
			set => this.variations[index] = value;
		}

		public IEnumerator<PropertyVariation> GetEnumerator () => this.variations.GetEnumerator ();
		IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();
		public void Add (PropertyVariation item) => this.variations.Add (item);
		public void Clear () => this.variations.Clear ();
		public bool Contains (PropertyVariation item) => this.variations.Contains (item);
		public void CopyTo (PropertyVariation[] array, int arrayIndex) => this.variations.CopyTo (array, arrayIndex);
		public bool Remove (PropertyVariation item) => this.variations.Remove (item);
		public int IndexOf (PropertyVariation item) => this.variations.IndexOf (item);
		public void Insert (int index, PropertyVariation item) => this.variations.Insert (index, item);
		public void RemoveAt (int index) => this.variations.RemoveAt (index);

		private readonly List<PropertyVariation> variations = new List<PropertyVariation> ();
	}
}