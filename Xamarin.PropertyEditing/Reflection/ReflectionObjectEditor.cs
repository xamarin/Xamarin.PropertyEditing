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

			this.properties.AddRange (ReflectionEditorProvider.GetPropertiesForType (targetType));

			foreach (EventInfo ev in targetType.GetEvents ()) {
				this.events.Add (new ReflectionEventInfo (ev));
			}
		}

		public event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		public object Target => this.target;

		public ITypeInfo TargetType => Target.GetType ().ToTypeInfo ();

		public IReadOnlyCollection<IPropertyInfo> Properties => this.properties;

		public IReadOnlyDictionary<IPropertyInfo, KnownProperty> KnownProperties => null;

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

		public Task<AssignableTypesResult> GetAssignableTypesAsync (IPropertyInfo property, bool childTypes)
		{
			return GetAssignableTypes (property.RealType, childTypes);
		}

		public Task<IReadOnlyCollection<PropertyVariation>> GetPropertyVariantsAsync (IPropertyInfo property)
		{
			return Task.FromResult<IReadOnlyCollection<PropertyVariation>> (new PropertyVariation[0]);
		}

		public Task RemovePropertyVariantAsync (IPropertyInfo property, PropertyVariation variant)
		{
			return Task.CompletedTask;
		}

		public async Task SetValueAsync<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variations = null)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));
			
			ReflectionPropertyInfo info = property as ReflectionPropertyInfo;
			if (info == null)
				throw new ArgumentException();

			await info.SetValueAsync (this.target, value.Value);
			OnPropertyChanged (info);
		}

		public Task<ITypeInfo> GetValueTypeAsync (IPropertyInfo property, PropertyVariation variations = null)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));
			
			ReflectionPropertyInfo info = property as ReflectionPropertyInfo;
			if (info == null)
				throw new ArgumentException();

			return Task.FromResult (info.GetValueType (Target));
		}

		public async Task<ValueInfo<T>> GetValueAsync<T> (IPropertyInfo property, PropertyVariation variations = null)
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

		internal static Task<AssignableTypesResult> GetAssignableTypes (ITypeInfo type, bool childTypes)
		{
			return Task.Run (() => {
				var types = AppDomain.CurrentDomain.GetAssemblies ().SelectMany (a => a.GetTypes ()).AsParallel ()
					.Where (t => {
						try {
							return t.Namespace != null && !t.IsAbstract && !t.IsInterface && t.IsPublic && t.GetConstructor (Type.EmptyTypes) != null;
						} catch (TypeLoadException) {
							return false;
						}
					});

				Type realType = ReflectionEditorProvider.GetRealType (type);
				if (childTypes) {
					var generic = realType.GetInterface ("ICollection`1");
					if (generic != null) {
						realType = generic.GetGenericArguments()[0];
					} else {
						realType = typeof(object);
					}
				}

				types = types.Where (t => realType.IsAssignableFrom (t));

				return new AssignableTypesResult (types.Select (t => {
					string asmName = t.Assembly.GetName ().Name;
					return new TypeInfo (new AssemblyInfo (asmName, isRelevant: asmName.StartsWith ("Xamarin")), t.Namespace, t.Name);
				}).ToList ());
			});
		}

		private readonly object target;
		private readonly List<ReflectionPropertyInfo> properties = new List<ReflectionPropertyInfo> ();
		private readonly List<ReflectionEventInfo> events = new List<ReflectionEventInfo> ();

		private static readonly IObjectEditor[] EmptyDirectChildren = new IObjectEditor[0];

		protected virtual void OnPropertyChanged (IPropertyInfo property)
		{
			PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (property));
		}
	}
}