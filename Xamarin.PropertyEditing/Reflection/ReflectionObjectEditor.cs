using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

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

				if (CheckAvailability (property))
					this.properties.Add (new ReflectionPropertyInfo (property));
			}
		}

		public event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		public object Target => this.target;

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
		private static Version OSVersion;

		protected virtual void OnPropertyChanged (IPropertyInfo property)
		{
			PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (property));
		}

		public static bool CheckAvailability (PropertyInfo property)
		{
			Attribute availibility = property.GetCustomAttributes ().FirstOrDefault (a => a.GetType ().Name == "IntroducedAttribute");
			if (availibility == null)
				return true;

			var versionProperty = availibility.GetType ().GetProperty ("Version");
			if (versionProperty == null)
				return false;

			if (OSVersion == null) {
				Type processInfoType = Type.GetType ("Foundation.NSProcessInfo, Xamarin.Mac");
				object processInfo = Activator.CreateInstance (processInfoType);
				object version = processInfoType.GetProperty ("OperatingSystemVersion").GetValue (processInfo);

				Type nsosversionType = version.GetType ();
				int major = (int)Convert.ChangeType (nsosversionType.GetField ("Major").GetValue (version), typeof(int));
				int minor = (int)Convert.ChangeType (nsosversionType.GetField ("Minor").GetValue (version), typeof(int));
				int build = (int)Convert.ChangeType (nsosversionType.GetField ("PatchVersion").GetValue (version), typeof(int));

				OSVersion = new Version (major, minor, build);
				processInfoType.GetMethod ("Dispose").Invoke (processInfo, null);
			}

			Version available = (Version)versionProperty.GetValue (availibility);
			return (OSVersion >= available);
		}
	}
}