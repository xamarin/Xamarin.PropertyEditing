using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xamarin.PropertyEditing.Windows
{
	internal class CombinablePredefinedValuesEditor
		: PropertyEditorControl
	{
		static CombinablePredefinedValuesEditor ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof (CombinablePredefinedValuesEditor), new FrameworkPropertyMetadata (typeof (CombinablePredefinedValuesEditor)));
			FocusableProperty.OverrideMetadata (typeof(CombinablePredefinedValuesEditor), new FrameworkPropertyMetadata (false));
		}
	}
}
