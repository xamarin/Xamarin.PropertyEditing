using System;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionEditorProvider
		: IEditorProvider
	{
		public Task<IObjectEditor> GetObjectEditorAsync (object item)
		{
			return Task.FromResult<IObjectEditor> (new ReflectionObjectEditor (item));
		}

		public Task<object> CreateObjectAsync (ITypeInfo type)
		{
			var realType = GetRealType (type);
			if (realType == null)
				return Task.FromResult<object> (null);

			object instance = Activator.CreateInstance (realType);
			return Task.FromResult (instance);
		}

		public static Type GetRealType (ITypeInfo type)
		{
			return Type.GetType ($"{type.NameSpace}.{type.Name}, {type.Assembly.Name}");
		}
	}
}