using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public class EditorPropertyChangedEventArgs
		: EventArgs
	{
		/// <param name="property">The property that was updated, or <c>null</c> if a full refresh is required.</param>
		public EditorPropertyChangedEventArgs (IPropertyInfo property)
		{
			Property = property;
		}

		/// <summary>
		/// Gets the property that was changed, or <c>null</c> for a full refresh.
		/// </summary>
		public IPropertyInfo Property
		{
			get;
		}
	}

	public interface IObjectEditor
	{
		/// <summary>
		/// Gets the target object being edited.
		/// </summary>
		object Target { get; }

		/// <summary>
		/// Gets the real type name for the object.
		/// </summary>
		/// <remarks>Also used for iconography.</remarks>
		string TypeName { get; }

		/// <remarks>
		/// These properties should be minimally equatable to the same property for another object of the same type. This includes
		/// for base class properties for two different derived types.
		/// </remarks>
		IReadOnlyCollection<IPropertyInfo> Properties { get; }

		/// <summary>
		/// Gets the parent object editor for the object this editor represents or <c>null</c> if none.
		/// </summary>
		IObjectEditor Parent { get; }

		/// <remarks>
		/// These are children that do not participate in the standard element hierarchy, such as segments in a segmented control.
		/// Note that objects defined by editors do not need to match real objects in the real hierarchy, they can be faked. An existing
		/// iOS property chooser itself can be maped to having an object editor for its actual object.
		/// </remarks>
		IReadOnlyList<IObjectEditor> DirectChildren { get; }

		/// <summary>
		/// Raised when a property's value changes. <see cref="EditorPropertyChangedEventArgs.Property"/> can be <c>null</c> for a full refresh.
		/// </summary>
		event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		/// <param name="property">The property to get retrieve assignable types for.</param>
		/// <param name="childTypes">Whether or not to return assignable types for a property's children rather than the property itself.</param>
		/// <returns>An <see cref="AssignableTypesResult"/> instance containing the assignable types. No assignable types should use an empty array and not return <c>null</c>.</returns>
		Task<AssignableTypesResult> GetAssignableTypesAsync (IPropertyInfo property, bool childTypes);

		/*
		 * Dealing with async values in the context of what's possible across all target platforms is a bit complex.
		 * While implicit safety may be able to be ensured, it would be exhaustive to reason it out and could change
		 * at any moment, so we need an explicit safety strategy. As we make value gets and sets async, we open ourselves
		 * to race conditions even on just the UI thread. Value changes can come from the user, or from the object editor
		 * at any point. So, we break these changes into two categories: Blocking UI & Waiting Values.
		 *
		 * Anything that needs to deal with the current state of the values will Wait asynchronously on prior changes to
		 * finish. These largely involve value changes from the object editor, which just acts as a INPC and does not
		 * contain the value itself, so it will be safe and the latest value when queried upon reaching active state in
		 * the wait queue (see AsyncWorkQueue). Selected objects (when >1) need to wait on values as they need to be
		 * able to calculate the intersection values.
		 *
		 * Anything that needs to deal with user input will need to block its UI. We simply can't reason about the user
		 * changing values while waiting for previous changes, because they value they're changing may change underneath
		 * them or become invalid as a result of a prior pending change.
		 */


		/// <remarks>
		/// <para>For the <see cref="ValueSource.Default"/> or <see cref="ValueSource.Unset"/> sources, implementers should
		/// ignore <paramref name="value"/>'s <see cref="ValueInfo{T}.Value"/> property and unset the value completely.
		/// For XML based backings this usually means removing the attribute altogether. Implementers should not simply set
		/// the value to its default value as this would still be a local value and override inheritance, styles, etc.</para>
		/// <para>When <paramref name="value"/>'s <see cref="ValueInfo{T}.Source"/> is <see cref="ValueSource.Resource"/>,
		/// the <see cref="ValueInfo{T}.SourceDescriptor"/> will be set to a <see cref="Resource"/> instance. Implementers
		/// need not see whether the resource contains a value itself. For a source of <see cref="ValueSource.Binding"/>,
		/// the <see cref="ValueInfo{T}.SourceDescriptor"/> will be the binding object created.</para>
		/// <para>When the <paramref name="property"/>'s <see cref="IPropertyInfo.Type"/> is an <c>object</c>
		/// <see cref="ValueInfo{T}.ValueDescriptor"/> will contain an <see cref="ITypeInfo"/> representing the real type
		/// of the value.</para>
		/// <para>When the <see cref="ValueInfo{T}.Source"/> is <see cref="ValueSource.Local"/> and <see cref="ValueInfo{T}.Value"/>
		/// is the same as the default value, implementers should consider unsetting the value such that the subsequent
		/// <see cref="GetValueAsync{T}(IPropertyInfo, PropertyVariation)"/> would return <see cref="ValueSource.Default"/>
		/// for <see cref="ValueInfo{T}.Source"/>. This allows users to clear the value for a property and have it remove
		/// the attribute for XML backed platforms without having to issue an <see cref="ValueSource.Unset"/>.</para>
		/// <para>Before the returned task completes, in order:
		/// 1. <see cref="GetValueAsync{T}(IPropertyInfo, PropertyVariation)"/> must be able to retrieve the new value.
		/// 2. <see cref="PropertyChanged"/> should fire with the appropriate property.
		/// For defensive purposes, consumers will not assume the <paramref name="value"/> they pass in will be the same
		/// as the new value and as a result will re-query the value upon the assumed <see cref="PropertyChanged"/> firing.
		/// There should not be a case where <see cref="PropertyChanged"/> is not fired when SetValueAsync is called, consumers
		/// will do basic verification before calling. Even <see cref="ValueInfo{T}.Source"/> changes with the value staying the
		/// same is a change in the property.
		/// </para>
		/// </remarks>
		Task SetValueAsync<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variation = null);

		/// <remarks>
		/// <para>Implementers should strive to include the actual value of resources or bindings for <see cref="ValueInfo{T}.Value"/>
		/// whereever possible.</para>
		/// <para>If the platform can know the value of a property when unset, it should return that value and the <see cref="ValueSource.Default"/>
		/// source. If the platform only knows that the value is unset, use <see cref="ValueSource.Unset"/> instead.</para>
		///<para>When the property's value <see cref="ValueInfo{T}.Source"/> is <see cref="ValueSource.Resource"/>,
		/// the <see cref="ValueInfo{T}.SourceDescriptor"/> should be set to a <see cref="Resource"/> instance. Implementers should
		/// strive to retrieve the resource's value and use <see cref="Resource{T}"/> isntead. For a source of
		/// <see cref="ValueSource.Binding"/>, the <see cref="ValueInfo{T}.SourceDescriptor"/> will be the binding object created.</para>
		/// <para>When the <paramref name="property"/>'s <see cref="IPropertyInfo.Type"/> is an <c>object</c>
		/// <see cref="ValueInfo{T}.ValueDescriptor"/> should contain an <see cref="ITypeInfo"/> representing the real type
		/// of the value.</para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
		Task<ValueInfo<T>> GetValueAsync<T> (IPropertyInfo property, PropertyVariation variation = null);
	}
}
