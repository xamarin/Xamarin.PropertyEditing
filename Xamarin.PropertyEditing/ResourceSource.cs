using System;

namespace Xamarin.PropertyEditing
{
	public class ResourceSource
	{
		public ResourceSource (string name, bool isLocal)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			Name = name;
			IsLocal = isLocal;
		}

		public string Name
		{
			get;
		}

		/// <summary>
		/// Gets whether the source is local/relative to a target object.
		/// </summary>
		public bool IsLocal
		{
			get;
		}
	}
}
