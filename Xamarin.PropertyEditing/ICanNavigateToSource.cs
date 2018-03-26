using System;

namespace Xamarin.PropertyEditing
{
	/// <summary>
	/// <see cref="IPropertyInfo"/> light-up interface for navigating to value sources.
	/// </summary>
	/// <remarks>
	/// Not all possible values and value sources must be navigatable to implement this interface. That determination can
	/// be made in <see cref="CanNavigateToSource"/>. Implementing this interface will simply light-up the UI element, but
	/// its enabled status will depend on <see cref="CanNavigateToSource"/> and other dynamic factors.
	/// </remarks>
	public interface ICanNavigateToSource
	{
		/// <summary>
		/// Gets whether the value of the property attached to in the <paramref name="editor"/> can be navigated to.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="editor"/> is <c>null</c>.</exception>
		bool CanNavigateToSource (IObjectEditor editor);

		/// <exception cref="ArgumentNullException"><paramref name="editor"/> is <c>null</c>.</exception>
		void NavigateToSource (IObjectEditor editor);
	}
}