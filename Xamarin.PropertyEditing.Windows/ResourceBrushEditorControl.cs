using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	class ResourceBrushEditorControl : PropertyEditorControl
	{
		static ResourceBrushEditorControl ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof (ResourceBrushEditorControl), new FrameworkPropertyMetadata (typeof (ResourceBrushEditorControl)));
		}

		ListBox resourceList;

		BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;
	}
}
