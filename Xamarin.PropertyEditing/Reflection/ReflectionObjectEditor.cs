using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionObjectEditor
		: IObjectEditor
	{
		public ReflectionObjectEditor (object target)
		{
			if (target == null)
				throw new ArgumentNullException (nameof (target));

			this.target = target;

			foreach (PropertyInfo property in target.GetType ().GetProperties ()) {
				DebuggerBrowsableAttribute browsable = property.GetCustomAttribute<DebuggerBrowsableAttribute> ();
				if (browsable != null && browsable.State == DebuggerBrowsableState.Never) {
					continue;
				}

				this.properties.Add (new ReflectionPropertyInfo (property));
			}
		}

		public event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		public IReadOnlyCollection<IPropertyInfo> Properties => this.properties;

		public IObjectEditor Parent => null;

		public IReadOnlyList<IObjectEditor> DirectChildren => EmptyDirectChildren;

		public void SetValue<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variation = null)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));
			
			ReflectionPropertyInfo info = property as ReflectionPropertyInfo;
			if (info == null)
				throw new ArgumentException();

			info.SetValue (this.target, value.Value);
			OnPropertyChanged (info);
		}

		public ValueInfo<T> GetValue<T> (IPropertyInfo property, PropertyVariation variation = null)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));
			
			ReflectionPropertyInfo info = property as ReflectionPropertyInfo;
			if (info == null)
				throw new ArgumentException();

			return new ValueInfo<T> {
				Source = ValueSource.Local,
				Value = info.GetValue<T> (this.target)
			};
		}

		private readonly object target;
		private readonly List<ReflectionPropertyInfo> properties = new List<ReflectionPropertyInfo> ();

		private static readonly IObjectEditor[] EmptyDirectChildren = new IObjectEditor[0];

		protected virtual void OnPropertyChanged (IPropertyInfo property)
		{
			PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (property));
		}
	}
}