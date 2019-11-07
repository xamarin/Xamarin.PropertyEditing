using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using Xamarin.PropertyEditing.ViewModels;

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

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			// Windows has the surprising behavior that when an automation group (like a group of combinable checkboxes)
			// has a colon in the name, the group label is not read by the narrator. This causes a problem for the Android
			// designer, which has property names like "app:layout_anchorGravity". We work around this by replacing the
			// colon with a space in the group's automation name.
			var propertyPresenter = this.FindParent<PropertyPresenter> ();
			if (propertyPresenter != null) {
				var name = (propertyPresenter.DataContext as PropertyViewModel)?.Name;

				if (name != null && name.Contains (":", StringComparison.Ordinal)) {
					var automationName = name.Replace (':', ' ');
					AutomationProperties.SetName (propertyPresenter, automationName);
				}
			}
		}
	}
}
