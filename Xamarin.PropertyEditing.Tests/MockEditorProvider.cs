using System;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests
{
	public class MockEditorProvider
		: IEditorProvider
	{
		public Task<IObjectEditor> GetObjectEditorAsync (object item)
		{
			var mockControl = item as MockControl;
			if (mockControl != null)
				return Task.FromResult<IObjectEditor> (new MockObjectEditor (mockControl) { SupportsDefault = true });
			return Task.FromResult<IObjectEditor> (new ReflectionObjectEditor (item));
		}
	}
}