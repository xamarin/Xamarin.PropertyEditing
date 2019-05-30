using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IObjectEventEditor
	{
		/// <summary>
		/// Fired whenever the result from <see cref="GetHandlersAsync"/> or <see cref="GetPotentialHandlers"/> have changed.
		/// </summary>
		/// <remarks>
		/// You can pass a null <see cref="IEventInfo"/> into the <see cref="EventHandlersChangedEventArgs"/> to signify that all events
		/// should be updated.
		/// </remarks>
		event EventHandler<EventHandlersChangedEventArgs> EventHandlersChanged;

		IReadOnlyCollection<IEventInfo> Events { get; }

		/// <summary>
		/// Gets whether or not to allow for attaching multiple handlers to an event.
		/// </summary>
		bool SupportsMultipleHandlers { get; }

		/// <exception cref="ArgumentNullException"><paramref name="ev"/> or <paramref name="handlerName"/> is <c>null</c>.</exception>
		Task AttachHandlerAsync (IEventInfo ev, string handlerName);

		/// <exception cref="ArgumentNullException"><paramref name="ev"/> or <paramref name="handlerName"/> is <c>null</c>.</exception>
		/// <remarks>Implementers should safely ignore handlers that are not in their handler list.</remarks>
		Task DetachHandlerAsync (IEventInfo ev, string handlerName);

		/// <summary>
		/// Gets the method names for the handlers attached to <paramref name="ev"/>
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="ev"/> is <c>null</c>.</exception>
		Task<IReadOnlyList<string>> GetHandlersAsync (IEventInfo ev);

		/// <summary>
		/// Gets existing handlers that would be assignable to <paramref name="ev"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="ev"/> is <c>null</c>.</exception>
		Task<IReadOnlyList<string>> GetPotentialHandlersAsync (IEventInfo ev);
	}

	public class EventHandlersChangedEventArgs
		: EventArgs
	{
		/// <param name="eventInfo">The event that changed. Can be <c>null</c> to signify all events should be updated.</param>
		public EventHandlersChangedEventArgs (IEventInfo eventInfo)
		{
			EventInfo = eventInfo;
		}

		public IEventInfo EventInfo
		{
			get;
		}
	}
}
