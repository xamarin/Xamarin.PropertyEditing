using System;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionEditorProvider
		: IEditorProvider
	{
		public Task<IObjectEditor> GetObjectEditorAsync (object item)
		{
			throw new NotImplementedException ();
		}
	}
}