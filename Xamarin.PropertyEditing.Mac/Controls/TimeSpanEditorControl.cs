using System;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class TimeSpanEditorControl
		: EntryPropertyEditor<TimeSpan>
	{
		public TimeSpanEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
		}

		protected override EntryPropertyEditorDelegate<TimeSpan> CreateDelegate (PropertyViewModel<TimeSpan> viewModel)
		{
			return new TimeSpanDelegate (viewModel) {
				ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView)
			};
		}

		protected override void UpdateAccessibilityValues ()
		{
			base.UpdateAccessibilityValues ();
			Entry.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityTimeSpan, ViewModel.Property.Name);
		}

		private class TimeSpanDelegate
			: EntryPropertyEditorDelegate<TimeSpan>
		{
			public TimeSpanDelegate (PropertyViewModel<TimeSpan> viewModel)
				: base (viewModel)
			{
			}

			protected override TimeSpan GetValue (string value)
			{
				return TimeSpan.Parse (value);
			}

			protected override bool CanGetValue (string value)
			{
				return TimeSpan.TryParse (value, out _);
			}
		}
	}
}
