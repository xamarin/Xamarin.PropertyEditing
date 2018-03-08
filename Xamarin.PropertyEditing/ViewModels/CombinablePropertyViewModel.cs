using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class FlaggableChoiceViewModel<TValue>
		: NotifyingObject
	{
		public FlaggableChoiceViewModel (string name, TValue value)
		{
			Name = name;
			Value = value;
		}

		public string Name
		{
			get;
		}

		public TValue Value
		{
			get;
		}

		public bool? IsFlagged
		{
			get { return this.isFlagged; }
			set
			{
				if (this.isFlagged == value)
					return;

				this.isFlagged = value;
				OnPropertyChanged ();
			}
		}

		private bool? isFlagged;
	}

	internal class CombinablePropertyViewModel<TValue>
		: PropertyViewModel<TValue>
	{
		public CombinablePropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
			this.predefinedValues = property as IHavePredefinedValues<TValue>;
			if (this.predefinedValues == null)
				throw new ArgumentException ("Property does not have predefined values", nameof(property));
			if (this.predefinedValues.IsValueCombinable && !this.predefinedValues.IsConstrainedToPredefined)
				throw new NotSupportedException ("Properties with combinable values can not be unconstrained currently");

			this.validator = property as IValidator<IReadOnlyList<TValue>>;

			var choices = new List<FlaggableChoiceViewModel<TValue>> (this.predefinedValues.PredefinedValues.Count);
			foreach (var kvp in this.predefinedValues.PredefinedValues) {
				var choiceVm = new FlaggableChoiceViewModel<TValue> (kvp.Key, kvp.Value);
				choiceVm.PropertyChanged += OnChoiceVmPropertyChanged;
				choices.Add (choiceVm);
			}

			Choices = choices;
			RequestCurrentValueUpdate ();
		}

		public IReadOnlyList<FlaggableChoiceViewModel<TValue>> Choices
		{
			get;
		}

		protected override async Task UpdateCurrentValueAsync ()
		{
			if (this.predefinedValues == null)
				return;

			using (await AsyncWork.RequestAsyncWork (this)) {
				await base.UpdateCurrentValueAsync ();

				var newValues = new Dictionary<string, bool?> (this.predefinedValues.PredefinedValues.Count);

				ValueInfo<IReadOnlyList<TValue>>[] values = await Task.WhenAll (Editors.Select (ed => ed.GetValueAsync<IReadOnlyList<TValue>> (Property, Variation)).ToArray ());
				foreach (ValueInfo<IReadOnlyList<TValue>> valueInfo in values) {
					if (valueInfo.Value == null || valueInfo.Source == ValueSource.Unset) {
						foreach (var kvp in this.predefinedValues.PredefinedValues) {
							newValues[kvp.Key] = null;
						}

						continue;
					}

					foreach (var kvp in this.predefinedValues.PredefinedValues) {
						bool currentValue = valueInfo.Value.Contains (kvp.Value);
						if (newValues.TryGetValue (kvp.Key, out bool? presentValue)) {
							if (presentValue.HasValue && presentValue.Value != currentValue) {
								newValues[kvp.Key] = null;
							}
						} else
							newValues[kvp.Key] = currentValue;
					}
				}

				this.fromUpdate = true;
				foreach (var vm in Choices) {
					if (newValues.TryGetValue (vm.Name, out bool? value)) {
						vm.IsFlagged = value;
					} else {
						vm.IsFlagged = false;
					}
				}
				this.fromUpdate = false;
			}
		}

		private bool fromUpdate;
		private readonly IValidator<IReadOnlyList<TValue>> validator;
		private readonly IHavePredefinedValues<TValue> predefinedValues;

		private async void OnChoiceVmPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (this.fromUpdate)
				return;

			await PushValuesAsync ();
		}

		private async Task PushValuesAsync ()
		{
			SetError (null);

			using (await AsyncWork.RequestAsyncWork (this)) {
				try {
					foreach (IObjectEditor editor in Editors) {
						ValueInfo<IReadOnlyList<TValue>> value = await editor.GetValueAsync<IReadOnlyList<TValue>> (Property, Variation);
						HashSet<TValue> current;
						if (value.Value == null || value.Source == ValueSource.Unset)
							current = new HashSet<TValue> ();
						else
							current = new HashSet<TValue> (value.Value);

						foreach (var choice in Choices) {
							if (!choice.IsFlagged.HasValue)
								continue;

							if (choice.IsFlagged.Value)
								current.Add (choice.Value);
							else
								current.Remove (choice.Value);
						}

						IReadOnlyList<TValue> values = current.ToArray ();
						if (this.validator != null)
							values = this.validator.ValidateValue (values);

						await editor.SetValueAsync (Property, new ValueInfo<IReadOnlyList<TValue>> {
							Source = ValueSource.Local,
							Value = values
						});
					}
				} catch (Exception ex) {
					if (ex is AggregateException aggregate) {
						aggregate = aggregate.Flatten ();
						ex = aggregate.InnerExceptions[0];
					}

					SetError (ex.ToString ());
				}
			}
		}
	}
}
