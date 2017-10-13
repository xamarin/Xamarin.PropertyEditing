using System;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.PropertyEditing.Resources;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class EnumPropertyViewModel<TValue>
		: PropertyViewModel<TValue>
		where TValue : struct
	{
		public EnumPropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			PossibleValues = Enum.GetNames (property.Type);
			IsFlags = property.Type.GetCustomAttribute<FlagsAttribute> () != null;
		}

		public bool IsFlags
		{
			get;
		}

		public IReadOnlyList<string> PossibleValues
		{
			get;
		}

		public string ValueName
		{
			get { return Value.ToString (); }
			set
			{
				TValue realValue;
				if (!Enum.TryParse (value, out realValue)) {
					SetError (Strings.UnableToParseValue (value));
					return;
				}

				Value = realValue;
			}
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof (ValueName));
		}
	}
}
