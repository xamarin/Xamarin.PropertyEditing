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
	}
}