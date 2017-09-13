using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IObjectEventEditor
	{
		IReadOnlyCollection<IEventInfo> Events { get; }

		/// <exception cref="ArgumentNullException"><paramref name="ev"/> or <paramref name="handlerName"/> is <c>null</c>.</exception>
		Task AttachHandlerAsync (IEventInfo ev, string handlerName);

		/// <exception cref="ArgumentNullException"><paramref name="ev"/> or <paramref name="handlerName"/> is <c>null</c>.</exception>
		Task DetatchHandlerAsync (IEventInfo ev, string handlerName);

		/// <summary>
		/// Gets the method names for the handlers attached to <paramref name="ev"/>
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="ev"/> is <c>null</c>.</exception>
		Task<IReadOnlyList<string>> GetHandlersAsync (IEventInfo ev);
	}
}
