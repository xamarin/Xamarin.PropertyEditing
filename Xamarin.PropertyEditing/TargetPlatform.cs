using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	/// <summary>
	/// Represents a target platform by defining top level feature support and presentation preferences 
	/// </summary>
	public sealed class TargetPlatform
	{
		public TargetPlatform (IEditorProvider editorProvider)
		{
			if (editorProvider == null)
				throw new ArgumentNullException (nameof(editorProvider));

			EditorProvider = editorProvider;
		}

		public TargetPlatform (IEditorProvider editorProvider, IResourceProvider resourceProvider)
			: this (editorProvider)
		{
			if (resourceProvider == null)
				throw new ArgumentNullException (nameof(resourceProvider));

			ResourceProvider = resourceProvider;
		}

		public TargetPlatform (IEditorProvider editorProvider, IBindingProvider bindingProvider)
			: this (editorProvider)
		{
			if (bindingProvider == null)
				throw new ArgumentNullException (nameof (bindingProvider));

			BindingProvider = bindingProvider;
		}

		public TargetPlatform (IEditorProvider editorProvider, IResourceProvider resourceProvider, IBindingProvider bindingProvider)
			: this (editorProvider, resourceProvider)
		{
			if (bindingProvider == null)
				throw new ArgumentNullException (nameof(bindingProvider));

			BindingProvider = bindingProvider;
		}

		/// <summary>
		/// Gets the <see cref="IEditorProvider"/> associated with this platform.
		/// </summary>
		public IEditorProvider EditorProvider
		{
			get;
		}

		public IResourceProvider ResourceProvider
		{
			get;
			private set;
		}

		public IBindingProvider BindingProvider
		{
			get;
			private set;
		}

		public IIconProvider IconProvider
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets whether the platform supports custom expressions (default false).
		/// </summary>
		public bool SupportsCustomExpressions
		{
			get;
			set;
		}

		/// <summary>
		/// Specifies whether Material Design is relevant to theplatform.
		/// </summary>
		public bool SupportsMaterialDesign
		{
			get;
			set;
		}

		public bool SupportsBrushOpacity
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a dictionary defining the property types will be grouped into a single editor and their groups key.
		/// </summary>
		public IReadOnlyDictionary<Type, string> GroupedTypes
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a collection of group keys that should be automatically expanded in grouped mode at first load.
		/// </summary>
		public IReadOnlyCollection<string> AutoExpandGroups
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a callback for errors that should be edge cases and/or don't have a defined way of displaying in the UI.
		/// </summary>
		/// <remarks>
		/// The string parameter contains a message that typically prefixes an exception message with context (ex. "Error creating variant: [exception message]")
		/// </remarks>
		public Action<string, Exception> ErrorHandler
		{
			get;
			set;
		}

		internal void ReportError (string message, Exception exception)
		{
			if (ErrorHandler != null)
				ErrorHandler (message, exception);
			else
				throw new Exception (message, exception);
		}

		internal TargetPlatform WithProvider (IEditorProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException (nameof(provider));

			return new TargetPlatform (provider) {
				ResourceProvider = ResourceProvider,
				BindingProvider = BindingProvider,
				IconProvider = IconProvider,
				SupportsMaterialDesign = SupportsMaterialDesign,
				SupportsCustomExpressions = SupportsCustomExpressions,
				SupportsBrushOpacity = SupportsBrushOpacity,
				GroupedTypes = GroupedTypes,
				AutoExpandGroups = AutoExpandGroups,
				ErrorHandler = ErrorHandler
			};
		}
	}
}
