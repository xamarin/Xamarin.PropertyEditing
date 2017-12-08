using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionObjectEditor
		: IObjectEditor, INameableObject, IObjectEventEditor
	{
		public ReflectionObjectEditor (object target)
		{
			if (target == null)
				throw new ArgumentNullException (nameof (target));

			this.target = target;
			Type targetType = target.GetType ();

			foreach (PropertyInfo property in targetType.GetProperties ()) {
				DebuggerBrowsableAttribute browsable = property.GetCustomAttribute<DebuggerBrowsableAttribute> ();
				if (browsable != null && browsable.State == DebuggerBrowsableState.Never) {
					continue;
				}

				if (CheckAvailability (property)) {
					if (property.PropertyType.IsEnum) {
						this.properties.Add ((ReflectionPropertyInfo)Activator.CreateInstance (typeof (ReflectionEnumPropertyInfo<>).MakeGenericType (Enum.GetUnderlyingType (property.PropertyType)), property));
					}
					else {
						this.properties.Add (new ReflectionPropertyInfo (property));
					}
				}
			}

			foreach (EventInfo ev in targetType.GetEvents ()) {
				this.events.Add (new ReflectionEventInfo (ev));
			}
		}

		public event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		public object Target => this.target;

		public string TypeName => this.target.GetType ().Name;

		public IReadOnlyCollection<IPropertyInfo> Properties => this.properties;

		public IReadOnlyCollection<IEventInfo> Events => this.events;

		public IObjectEditor Parent => null;

		public IReadOnlyList<IObjectEditor> DirectChildren => EmptyDirectChildren;

		public string Name
		{
			get;
			private set;
		}

		public Task SetNameAsync (string name)
		{
			Name = name;
			return Task.FromResult (true);
		}

		public Task<string> GetNameAsync()
		{
			return Task.FromResult (Name);
		}

		public Task AttachHandlerAsync (IEventInfo ev, string handlerName)
		{
			throw new NotSupportedException ();
		}

		public Task DetachHandlerAsync (IEventInfo ev, string handlerName)
		{
			throw new NotSupportedException ();
		}

		public Task<IReadOnlyList<string>> GetHandlersAsync (IEventInfo ev)
		{
			if (ev == null)
				throw new ArgumentNullException (nameof (ev));

			ReflectionEventInfo info = ev as ReflectionEventInfo;
			if (info == null)
				throw new ArgumentException ();

			return Task.FromResult (info.GetHandlers (this.target));
		}

		public Task<IReadOnlyList<ITypeInfo>> GetAssignableTypesAsync (IPropertyInfo property)
		{
			return Task.Run (() => {
				ReflectionPropertyInfo realInfo = (ReflectionPropertyInfo) property;
				var assemblies = AppDomain.CurrentDomain.GetAssemblies ();

				return (IReadOnlyList<ITypeInfo>)assemblies.Select (a => new { Assembly = a, Info = new AssemblyInfo (a.FullName)})
					.SelectMany (a =>
						a.Assembly.DefinedTypes.Select (t => new { Type = t, Assembly = a }))
					.AsParallel ()
					.Where (t => realInfo.Type.IsAssignableFrom (t.Type))
					.Select (t => new TypeInfo (t.Assembly.Info, t.Type.Namespace, t.Type.Name)).ToList();
			});
		}

		public async Task SetValueAsync<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variation = null)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));
			
			ReflectionPropertyInfo info = property as ReflectionPropertyInfo;
			if (info == null)
				throw new ArgumentException();

			await info.SetValueAsync (this.target, value.Value);
			OnPropertyChanged (info);
		}

		public async Task<ValueInfo<T>> GetValueAsync<T> (IPropertyInfo property, PropertyVariation variation = null)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));
			
			ReflectionPropertyInfo info = property as ReflectionPropertyInfo;
			if (info == null)
				throw new ArgumentException();

			T value = await info.GetValueAsync<T> (this.target);

			return new ValueInfo<T> {
				Source = ValueSource.Local,
				Value = value
			};
		}

		private readonly object target;
		private readonly List<ReflectionPropertyInfo> properties = new List<ReflectionPropertyInfo> ();
		private readonly List<ReflectionEventInfo> events = new List<ReflectionEventInfo> ();

		private static readonly IObjectEditor[] EmptyDirectChildren = new IObjectEditor[0];
		private static Version OSVersion;

		protected virtual void OnPropertyChanged (IPropertyInfo property)
		{
			PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (property));
		}

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