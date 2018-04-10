using System;
using System.Collections.Generic;
using System.ComponentModel;
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

			this.coerce = property as ICoerce<IReadOnlyList<TValue>>;
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
				var newValues = new Dictionary<string, bool?> (this.predefinedValues.PredefinedValues.Count);

				ValueInfo<IReadOnlyList<TValue>>[] values = await Task.WhenAll (Editors.Select (ed => ed.GetValueAsync<IReadOnlyList<TValue>> (Property, Variation)).ToArray ());
				foreach (ValueInfo<IReadOnlyList<TValue>> valueInfo in values) {
					if (valueInfo == null || valueInfo.Value == null || valueInfo.Source == ValueSource.Unset) {
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

				await base.UpdateCurrentValueAsync ();
			}
		}

		private bool fromUpdate;
		private readonly IValidator<IReadOnlyList<TValue>> validator;
		private readonly ICoerce<IReadOnlyList<TValue>> coerce;
		private readonly IHavePredefinedValues<TValue> predefinedValues;

		private async void OnChoiceVmPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (this.fromUpdate)
				return;

			await PushValuesAsync (sender as FlaggableChoiceViewModel<TValue>);
		}

		private async Task PushValuesAsync (FlaggableChoiceViewModel<TValue> changedChoice)
		{
			SetError (null);

			using (await AsyncWork.RequestAsyncWork (this)) {
				try {
					// Snapshot current choices so we don't catch updates mid-push for multi-editors
					var currentChoices = Choices.ToDictionary (c => c, c => c.IsFlagged);

					foreach (IObjectEditor editor in Editors) {
						ValueInfo<IReadOnlyList<TValue>> value = await editor.GetValueAsync<IReadOnlyList<TValue>> (Property, Variation);
						HashSet<TValue> current;
						if (value.Value == null || value.Source == ValueSource.Unset)
							current = new HashSet<TValue> ();
						else
							current = new HashSet<TValue> (value.Value);

						foreach (var choice in currentChoices) {
							if (!choice.Value.HasValue)
								continue;

							if (choice.Value.Value)
								current.Add (choice.Key.Value);
							else
								current.Remove (choice.Key.Value);
						}

						IReadOnlyList<TValue> values = current.ToArray ();
						if (this.validator != null) {
							if (!this.validator.IsValid (values)) {
								// Some combinables simply don't have a valid "none", but if we're going from indeterminate we still need to
								// update the value, so we'll flip the changed value to true in that case so we don't go right back to indeterminate
								if (values.Count == 0) {
									changedChoice.IsFlagged = true;
									// We're explicitly triggering a change and need the update here so we need to update our snapshot.
									currentChoices = Choices.ToDictionary (c => c, c => c.IsFlagged);
								}

								continue;
							}
						}

						if (this.coerce != null)
							values = this.coerce.CoerceValue (values);

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
