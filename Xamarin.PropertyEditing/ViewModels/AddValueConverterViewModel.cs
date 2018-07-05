using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class AddValueConverterViewModel
		: TypeSelectorViewModel
	{
		public AddValueConverterViewModel (TargetPlatform platform, object target, AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes)
			: base (assignableTypes)
		{
			if (platform == null)
				throw new ArgumentNullException (nameof(platform));
			if (target == null)
				throw new ArgumentNullException (nameof(target));

			this.platform = platform;
			this.target = target;
		}

		public string ConverterName
		{
			get { return this.converterName; }
			set
			{
				if (this.converterName == value)
					return;

				this.converterName = value;
				OnPropertyChanged();
			}
		}

		protected override void OnPropertyChanged (string propertyName = null)
		{
			base.OnPropertyChanged (propertyName);

			if (propertyName == nameof(SelectedType)) {
				if (SelectedType == null) {
					ConverterName = null;
					return;
				}

				if (this.platform.ResourceProvider != null) {
					// TODO: Go proper async, must ignorewatch for changes
					ConverterName = this.platform.ResourceProvider.SuggestResourceNameAsync (new [] { this.target }, SelectedType).Result;
				} else
					ConverterName = SelectedType.Name;
			}
		}

		private readonly TargetPlatform platform;
		private readonly object target;
		private string converterName;
	}
}