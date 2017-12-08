using System.Collections.Generic;
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
			if (this.editorCache.TryGetValue (item, out IObjectEditor cachedEditor)) {
				return Task.FromResult (cachedEditor);
			}
			IObjectEditor editor = (item is MockControl mockControl)
				? (IObjectEditor)(new MockObjectEditor (mockControl) { SupportsDefault = true })
				: new ReflectionObjectEditor (item);
			this.editorCache.Add (item, editor);
			return Task.FromResult (editor);
		}

		public Task<object> CreateObjectAsync (ITypeInfo type)
		{
			throw new System.NotImplementedException ();
		}

		private Dictionary<object, IObjectEditor> editorCache = new Dictionary<object, IObjectEditor> ();
	}
}