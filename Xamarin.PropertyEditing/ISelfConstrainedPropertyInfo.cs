using System;

namespace Xamarin.PropertyEditing
{
	// Might want to replace constraints with a more generic interface, similar to availability
	public interface ISelfConstrainedPropertyInfo<out T>
		where T : IComparable<T>
	{
		T MaxValue { get; }
		T MinValue { get; }
	}
}