using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Common;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests
{
	public class MockEditorProvider
		: IEditorProvider
	{
		public static readonly TargetPlatform MockPlatform = new TargetPlatform (new MockEditorProvider ());

		public MockEditorProvider ()
		{
		}

		public MockEditorProvider (IObjectEditor editor)
		{
			this.editorCache.Add (editor.Target, editor);
		}

		public IReadOnlyDictionary<Type, ITypeInfo> KnownTypes
		{
			get;
		} = new Dictionary<Type, ITypeInfo> {
				{ typeof(PropertyBinding), typeof(MockBinding).ToTypeInfo() },
				{ typeof(CommonValueConverter), typeof(MockValueConverter).ToTypeInfo() },
				{ typeof(CommonBrush), typeof(CommonBrush).ToTypeInfo() },
				{ typeof(CommonSolidBrush), typeof(CommonSolidBrush).ToTypeInfo() },
				{ typeof(CommonColor), typeof(CommonColor).ToTypeInfo() }
			};

		public Task<IObjectEditor> GetObjectEditorAsync (object item)
		{
			if (this.editorCache.TryGetValue (item, out IObjectEditor cachedEditor)) {
				return Task.FromResult (cachedEditor);
			}
			IObjectEditor editor = ChooseEditor (item);
			this.editorCache.Add (item, editor);
			return Task.FromResult (editor);
		}

		public async Task<IReadOnlyCollection<IPropertyInfo>> GetPropertiesForTypeAsync (ITypeInfo type)
		{
			Type realType = ReflectionEditorProvider.GetRealType (type);
			if (realType == null)
				return Array.Empty<IPropertyInfo> ();

			if (typeof(MockControl).IsAssignableFrom (realType)) {
				object item = await CreateObjectAsync (type);
				IObjectEditor editor = ChooseEditor (item);
				return editor.Properties;
			}

			return ReflectionEditorProvider.GetPropertiesForType (realType);
		}

		public Task<AssignableTypesResult> GetAssignableTypesAsync (ITypeInfo type, bool childTypes)
		{
			if (type == KnownTypes[typeof(CommonValueConverter)])
				return Task.FromResult (new AssignableTypesResult (new[] { type }));

			return ReflectionObjectEditor.GetAssignableTypes (type, childTypes);
		}

		IObjectEditor ChooseEditor (object item)
		{
			switch (item) {
			case MockWpfControl msc:
				return new MockObjectEditor (msc);
			case MockControl mc:
				return new MockNameableEditor (mc);
			case MockBinding mb:
				return new MockBindingEditor (mb);
			default:
				return new ReflectionObjectEditor (item);
			}
		}

		public Task<object> CreateObjectAsync (ITypeInfo type)
		{
			Type realType = Type.GetType ($"{type.NameSpace}.{type.Name}, {type.Assembly.Name}");
			if (realType == null)
				return Task.FromResult<object> (null);

			return Task.FromResult (Activator.CreateInstance (realType));
		}

		public Task<IReadOnlyList<object>> GetChildrenAsync (object item)
		{
			return Task.FromResult<IReadOnlyList<object>> (Array.Empty<object> ());
		}

		public Task<IReadOnlyDictionary<Type, ITypeInfo>> GetKnownTypesAsync (IReadOnlyCollection<Type> knownTypes)
		{
			return Task.FromResult<IReadOnlyDictionary<Type, ITypeInfo>> (new Dictionary<Type, ITypeInfo> ());
		}

		private readonly Dictionary<object, IObjectEditor> editorCache = new Dictionary<object, IObjectEditor> ();
	}
}