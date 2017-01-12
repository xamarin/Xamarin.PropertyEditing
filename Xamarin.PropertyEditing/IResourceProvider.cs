using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public class Resource
	{
		public Resource (string name)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			Name = name;
		}

		public string Name
		{
			get;
		}
	}

	public interface IResourceProvider
	{
		/// <summary>
		/// Gets the 
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		Task<IReadOnlyList<Resource>> GetResourcesAsync (IPropertyInfo property);
	}
}