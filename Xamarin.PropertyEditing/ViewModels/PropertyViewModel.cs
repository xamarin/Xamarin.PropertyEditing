﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PropertyViewModel<TValue>
		: PropertyViewModel
	{
		public PropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			SetValueResourceCommand = new RelayCommand<Resource> (OnSetValueToResource, CanSetValueToResource);
			UpdateCurrentValue ();
		}

		public ValueSource ValueSource => this.value.Source;

		public TValue Value
		{
			get { return (this.value != null) ? this.value.Value : default(TValue); }
			set
			{
				value = ValidateValue (value);
				SetValue (new ValueInfo<TValue> {
					Source = ValueSource.Local,
					Value = value
				});
			}
		}

		public IReadOnlyList<Resource> Resources => this.resources;

		public IResourceProvider ResourceProvider
		{
			get { return this.resourceProvider; }
			set
			{
				if (this.resourceProvider == value)
					return;

				this.resourceProvider = value;
				OnPropertyChanged ();
				UpdateResources ();
			}
		}

		public ICommand SetValueResourceCommand
		{
			get;
		}

		protected virtual void OnEditorPropertyChanged (object sender, EditorPropertyChangedEventArgs e)
		{
			if (e.Property != null && !Equals (e.Property, Property))
				return;

			// TODO: Smarter querying, can query the single editor and check against MultipleValues
			UpdateCurrentValue ();
		}

		protected virtual TValue ValidateValue (TValue validationValue)
		{
			return validationValue;
		}

		protected virtual void OnValueChanged ()
		{
		}

		protected override async void UpdateCurrentValue ()
		{
			if (Property == null)
				return;

			ValueInfo<TValue> currentValue = null;

			using (await AsyncWork.RequestAsyncWork (this)) {
				bool disagree = false;
				ValueInfo<TValue>[] values = await Task.WhenAll (Editors.Select (ed => ed.GetValueAsync<TValue> (Property, Variation)).ToArray ());
				foreach (ValueInfo<TValue> valueInfo in values) {
					if (currentValue == null)
						currentValue = valueInfo;
					else if (currentValue.Source != valueInfo.Source || !Equals (currentValue.Value, valueInfo.Value)) {
						// Even if the value is the same, they are not equal if the source is not the same because
						// it means the value is set differently at the source.
						disagree = true;
						currentValue = null;
						break;
					}
				}

				MultipleValues = disagree;

				// The public setter for Value is a local set for binding
				SetCurrentValue ((currentValue != null) ? currentValue : null);
			}
		}

		protected override void SetupEditor (IObjectEditor editor)
		{
			base.SetupEditor (editor);
			editor.PropertyChanged += OnEditorPropertyChanged;
		}

		protected override void TeardownEditor (IObjectEditor editor)
		{
			base.TeardownEditor (editor);
			editor.PropertyChanged -= OnEditorPropertyChanged;
		}

		private readonly ObservableCollection<Resource> resources = new ObservableCollection<Resource> ();
		private ValueInfo<TValue> value;
		private IResourceProvider resourceProvider;
		private CancellationTokenSource cancelTokenSource;
		private Task updateResourcesTask;

		private bool SetCurrentValue (ValueInfo<TValue> newValue)
		{
			if (this.value == newValue)
				return false;

			this.value = newValue;
			OnValueChanged ();
			OnPropertyChanged (nameof (Value));
			return true;
		}

		private async void SetValue (ValueInfo<TValue> newValue)
		{
			if (this.value == newValue)
				return;

			SetError (null);

			using (await AsyncWork.RequestAsyncWork (this)) {
				try {
					Task[] setValues = new Task[Editors.Count];
					int i = 0;
					foreach (IObjectEditor editor in Editors) {
						setValues[i++] = editor.SetValueAsync (Property, newValue);
					}

					await Task.WhenAll (setValues);
					UpdateCurrentValue ();
				} catch (Exception ex) {
					AggregateException aggregate = ex as AggregateException;
					if (aggregate != null) {
						aggregate = aggregate.Flatten ();
						ex = aggregate.InnerExceptions[0];
					}

					SetError (ex.ToString ());
				}
			}
		}

		private bool CanSetValueToResource (Resource resource)
		{
			if (resource == null || !Property.CanWrite)
				return false;

			// We're just going to block wait on this. There shouldn't be a scenario in which it would deadlock
			// and we simply can't work async into the ICommand system. Looks bad, but practically shouldn't be an issue.
			// Famous last words.
			this.updateResourcesTask?.Wait ();
			return this.resources.Contains (resource);
		}

		private void OnSetValueToResource (Resource resource)
		{
			if (resource == null)
				throw new ArgumentNullException (nameof (resource));
			
			SetValue (new ValueInfo<TValue> {
				Source = ValueSource.Resource,
				ValueDescriptor = resource
			});
		}

		private void UpdateResources ()
		{
			var source = new CancellationTokenSource ();
			var cancelSource = Interlocked.Exchange (ref this.cancelTokenSource, source);
			cancelSource?.Cancel ();

			this.updateResourcesTask = UpdateResourcesAsync (source.Token);
		}

		private async Task UpdateResourcesAsync (CancellationToken cancelToken)
		{
			this.resources.Clear();

			var provider = this.resourceProvider;
			if (provider != null) {
				if (cancelToken.IsCancellationRequested)
					return;

				try {
					IReadOnlyList<Resource> gottenResources = await provider.GetResourcesAsync (Property, cancelToken);
					if (cancelToken.IsCancellationRequested)
						return;

					this.resources.AddItems (gottenResources);
				} catch (OperationCanceledException) {
					return;
				}
			}

			((RelayCommand<Resource>)SetValueResourceCommand).ChangeCanExecute ();
		}
	}

	internal abstract class PropertyViewModel
		: EditorViewModel, INotifyDataErrorInfo
	{
		protected PropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (editors)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));

			Property = property;
		}

		public IPropertyInfo Property
		{
			get;
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public bool HasErrors => this.error != null;

		public IEnumerable GetErrors (string propertyName)
		{
			return (this.error != null) ? new [] { this.error } : Enumerable.Empty<string> ();
		}

		/// <summary>
		/// Gets or sets the current <see cref="PropertyVariation"/> that the value is currently looking at.
		/// </summary>
		public PropertyVariation Variation
		{
			get { return this.variation; }
			set
			{
				if (this.variation == value)
					return;

				this.variation = value;
				OnPropertyChanged ();
			}
		}

		public bool CanDelve => ValueModel != null;

		public ObjectViewModel ValueModel
		{
			get { return this.valueModel; }
			private set
			{
				this.valueModel = value;
				OnPropertyChanged ();
				OnPropertyChanged (nameof (CanDelve));
			}
		}

		/// <param name="newError">The error message or <c>null</c> to clear the error.</param>
		protected void SetError (string newError)
		{
			if (this.error == newError)
				return;

			this.error = newError;
			OnErrorsChanged (new DataErrorsChangedEventArgs (nameof (Property)));
		}

		private ObjectViewModel valueModel;
		private bool multipleValues;
		private PropertyVariation variation;
		private string error;

		private void OnErrorsChanged (DataErrorsChangedEventArgs e)
		{
			ErrorsChanged?.Invoke (this, e);
		}
	}
}
