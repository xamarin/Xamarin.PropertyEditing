using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface INameableObject
	{
		Task<string> GetNameAsync (); 
		Task SetNameAsync (string name);
	}
}