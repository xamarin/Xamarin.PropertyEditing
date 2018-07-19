using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	/// <summary>
	/// <see cref="IObjectEditor"/> light-up interface for objects which have names.
	/// </summary>
	/// <remarks>
	/// Don't also expose a name property through <see cref="IObjectEditor.Properties"/>.
	/// </remarks>
	public interface INameableObject	
	{
		Task<string> GetNameAsync (); 
		Task SetNameAsync (string name);
	}
}