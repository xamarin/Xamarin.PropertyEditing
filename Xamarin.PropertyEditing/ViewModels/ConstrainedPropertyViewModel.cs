using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal abstract class ConstrainedPropertyViewModel<T>
		: PropertyViewModel<T>
	{
		static ConstrainedPropertyViewModel()
		{
			Type t = typeof(T);
			if (t.Name != "Nullable`1")
				return;

			// We could do a dynamicmethod here for call speed, but it's probably not needed.
			RawType = t.GetGenericArguments ()[0];
			Comparer = RawType.GetMethod ("CompareTo", new[] { RawType });
			NullableConverter = new NullableConverter (t);
		}

		protected ConstrainedPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariationSet variant = null)
			: base (platform, property, editors, variant)
		{
			this.selfConstraint = property as ISelfConstrainedPropertyInfo<T>;
			this.clampProperties = property as IClampedPropertyInfo;

		    UpdateMaxMin ();
		}

		public bool IsConstrained => (this.selfConstraint != null || this.clampProperties != null);

		public T MaximumValue
		{
			get { return this.maximumValue; }
			protected set
			{
				if (Equals (this.maximumValue, value))
					return;

				this.maximumValue = value;
				OnPropertyChanged();
			}
		}

		public T MinimumValue
		{
			get { return this.minimumValue; }
			protected set
			{
				if (Equals (this.minimumValue, value))
					return;

				this.minimumValue = value;
				OnPropertyChanged();
			}
		}

		protected override T CoerceValue (T validationValue)
		{
			if (IsConstrained) {
				if (Compare (validationValue, MaximumValue) > 0)
					validationValue = MaximumValue;
				else if (Compare (validationValue, MinimumValue) < 0)
					validationValue = MinimumValue;
			}

			return base.CoerceValue (validationValue);
		}

		protected int Compare (T left, T right)
		{
			if (left is IComparable<T> comparable)
				return comparable.CompareTo (right);
			if (Equals (left, right))
				return 0;
			if (ReferenceEquals (left, null))
				return -1;
			if (ReferenceEquals (right, null))
				return 1;

			return (int) Comparer.Invoke (NullableConverter.ConvertTo (left, RawType), new[] { NullableConverter.ConvertTo (right, RawType) });
		}

		protected override void OnEditorPropertyChanged (object sender, EditorPropertyChangedEventArgs e)
		{
			if (this.clampProperties != null) {
				if (e.Property == null || e.Property.Equals (this.clampProperties.MaximumProperty) || e.Property.Equals (this.clampProperties.MinimumProperty))
					UpdateMaxMin ();
			}

			base.OnEditorPropertyChanged (sender, e);
		}

	    private async void UpdateMaxMin ()
	    {
			bool isDefault = true;
			T max = default(T), min = default(T);
			if (this.selfConstraint != null) {
				isDefault = false;
				max = this.selfConstraint.MaxValue;
				min = this.selfConstraint.MinValue;
			}

			if (this.clampProperties != null && Editors.Count > 0) {
				bool doMax = this.clampProperties.MaximumProperty != null;
				bool doMin = this.clampProperties.MinimumProperty != null;

				using (await AsyncWork.RequestAsyncWork (this)) {
					// TODO: max/min property get error case
					foreach (IObjectEditor editor in Editors) {
						if (doMax) {
							ValueInfo<T> maxinfo = await editor.GetValueAsync<T> (this.clampProperties.MaximumProperty);
							max = (isDefault) ? maxinfo.Value : Min (max, maxinfo.Value);
						}

						if (doMin) {
							ValueInfo<T> mininfo = await editor.GetValueAsync<T> (this.clampProperties.MinimumProperty);
							min = (isDefault) ? mininfo.Value : Max (min, mininfo.Value);
						}
					}
				}
			}

	        MaximumValue = max;
	        MinimumValue = min;
	    }

	    private readonly IClampedPropertyInfo clampProperties;
		private readonly ISelfConstrainedPropertyInfo<T> selfConstraint;
		private T maximumValue;
		private T minimumValue;

		private T Max (T left, T right)
		{
			if (left == null)
				return right;

			return (Compare (left, right) < 0) ? right : left;
		}

		private T Min (T left, T right)
		{
			if (left == null)
				return right;

			return (Compare (left, right) < 0) ? left : right;
		}

		private static readonly Type RawType;
		private static readonly MethodInfo Comparer;
		private static readonly NullableConverter NullableConverter;
	}
}
