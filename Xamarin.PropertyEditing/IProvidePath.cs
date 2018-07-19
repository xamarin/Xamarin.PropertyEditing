using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	interface IProvidePath
	{
		IReadOnlyList<object> GetItemParents (object item);
	}
}
