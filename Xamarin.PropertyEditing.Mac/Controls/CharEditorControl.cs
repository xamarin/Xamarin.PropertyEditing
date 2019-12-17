using System;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CharEditorControl
		: EntryPropertyEditor<char>
	{
		public CharEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			Entry.Tag = 1;
		}

		protected override EntryPropertyEditorDelegate<char> CreateDelegate (PropertyViewModel<char> viewModel)
		{
			return new CharDelegate (viewModel);
		}

		protected override void UpdateAccessibilityValues ()
		{
			base.UpdateAccessibilityValues ();
			Entry.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityChar, ViewModel.Property.Name);
		}

		protected override string GetValue (char value)
		{
			return (value == default (char)) ? null : value.ToString();
		}

		private class CharDelegate
			: EntryPropertyEditorDelegate<char>
		{
			public CharDelegate (PropertyViewModel<char> viewModel)
				: base (viewModel)
			{
			}

			protected override char GetValue (string value)
			{
				return (String.IsNullOrEmpty (value) ? default (char) : value[0]);
			}

			protected override bool CanGetValue (string value)
			{
				return Char.TryParse (value, out _);
			}
		}
	}
}
