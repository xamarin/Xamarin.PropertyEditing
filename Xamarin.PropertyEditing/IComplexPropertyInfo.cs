using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	public interface IComplexPropertyInfo : IPropertyInfo
	{
		IReadOnlyCollection<ISubPropertyInfo> Properties { get; }
	}
}
