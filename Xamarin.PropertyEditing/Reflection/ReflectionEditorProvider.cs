using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionEditorProvider
		: IEditorProvider
	{
		public IReadOnlyDictionary<Type, ITypeInfo> KnownTypes
		{
			get;
		} = new Dictionary<Type, ITypeInfo> {

		};

		public Task<IObjectEditor> GetObjectEditorAsync (object item)
		{
			return Task.FromResult<IObjectEditor> (new ReflectionObjectEditor (item));
		}

		public Task<IReadOnlyCollection<IPropertyInfo>> GetPropertiesForTypeAsync (ITypeInfo type)
		{
			return Task.Run (() => {
				Type targetType = GetRealType (type);
				return (IReadOnlyCollection<IPropertyInfo>)GetPropertiesForType (targetType);
			});
		}

		public Task<object> CreateObjectAsync (ITypeInfo type)
		{
			var realType = GetRealType (type);
			if (realType == null)
				return Task.FromResult<object> (null);

			object instance = Activator.CreateInstance (realType);
			return Task.FromResult (instance);
		}

		public Task<AssignableTypesResult> GetAssignableTypesAsync (ITypeInfo type, bool childTypes)
		{
			return ReflectionObjectEditor.GetAssignableTypes (type, childTypes);
		}

		public Task<IReadOnlyList<object>> GetChildrenAsync (object item)
		{
			return Task.FromResult ((IReadOnlyList<object>)Array.Empty<object> ());
		}

		public Task<IReadOnlyDictionary<Type, ITypeInfo>> GetKnownTypesAsync (IReadOnlyCollection<Type> knownTypes)
		{
			return Task.FromResult<IReadOnlyDictionary<Type, ITypeInfo>> (new Dictionary<Type, ITypeInfo> ());
		}

		public ITypeInfo GetRealType<T> (T item)
		{
			return item?.GetType ().ToTypeInfo ();
		}

		public static Type GetRealType (ITypeInfo type)
		{
			return Type.GetType ($"{type.NameSpace}.{type.Name}, {type.Assembly.Name}");
		}

		public static IReadOnlyList<ReflectionPropertyInfo> GetPropertiesForType (Type targetType)
		{
			var properties = new List<ReflectionPropertyInfo> ();
			foreach (PropertyInfo property in targetType.GetProperties ()) {
				DebuggerBrowsableAttribute browsable = property.GetCustomAttribute<DebuggerBrowsableAttribute> ();
				if (browsable != null && browsable.State == DebuggerBrowsableState.Never) {
					continue;
				}

				if (CheckAvailability (property)) {
					if (property.PropertyType.IsEnum) {
						properties.Add ((ReflectionPropertyInfo)Activator.CreateInstance (typeof (ReflectionEnumPropertyInfo<>).MakeGenericType (Enum.GetUnderlyingType (property.PropertyType)), property));
					}
					else {
						properties.Add (new ReflectionPropertyInfo (property));
					}
				}
			}

			return properties;
		}

		private static Version OSVersion;
		private static bool CheckAvailability (PropertyInfo property)
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