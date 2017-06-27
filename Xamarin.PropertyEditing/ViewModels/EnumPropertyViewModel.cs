using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class EnumPropertyViewModel<TValue>
		: PropertyViewModel<TValue>
		where TValue : struct
	{
		public EnumPropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			var enumNames = Enum.GetNames (property.Type);
			IsFlags = property.Type.GetCustomAttribute<FlagsAttribute> () != null;
			if (IsFlags) {
				var dict = new Dictionary<string, bool> ();
				foreach (var item in enumNames) {
					dict.Add (item, (Value as Enum).HasFlag ((Enum)Enum.Parse (property.Type, item)));
				}
				PossibleValues = dict;
			} else {
				PossibleValues = enumNames.ToDictionary (x => x, y => false);
			}
		}

		public bool IsFlags
		{
			get;
		}

		public IReadOnlyDictionary<string, bool> PossibleValues
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
					SetError ("Can't parse value"); // TODO: Localize & improve
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
