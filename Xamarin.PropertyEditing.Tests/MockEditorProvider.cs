using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests
{
	public class MockEditorProvider
		: IEditorProvider
	{
		public static readonly TargetPlatform MockPlatform = new TargetPlatform (new MockEditorProvider ());

		public Task<IObjectEditor> GetObjectEditorAsync (object item)
		{
			if (this.editorCache.TryGetValue (item, out IObjectEditor cachedEditor)) {
				return Task.FromResult (cachedEditor);
			}
			IObjectEditor editor = ChooseEditor (item);
			this.editorCache.Add (item, editor);
			return Task.FromResult (editor);
		}

		IObjectEditor ChooseEditor (object item)
		{
			switch (item) {
			case MockWpfControl msc:
				return new MockObjectEditor (msc);
			case MockControl mc:
				return new MockNameableEditor (mc);
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

		public Task<IReadOnlyDictionary<Type, ITypeInfo>> GetKnownTypesAsync (IReadOnlyCollection<Type> knownTypes)
		{
			return Task.FromResult<IReadOnlyDictionary<Type, ITypeInfo>> (new Dictionary<Type, ITypeInfo> ());
		}


		private readonly Dictionary<object, IObjectEditor> editorCache = new Dictionary<object, IObjectEditor> ();
	}
}