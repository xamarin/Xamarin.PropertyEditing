using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name = "TextBox", Type = typeof(TextBox))]
	public class StringEditorControl
		: PropertyEditorControl
	{
	}
}
