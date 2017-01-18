using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PropertyViewModel<TValue>
		: PropertyViewModel, INotifyDataErrorInfo
	{
		public PropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public bool HasErrors => this.error != null;

		public TValue Value
		{
			get { return this.value; }
			set
			{
				if (!SetCurrentValue (value))
					return;

				SetValue (new ValueInfo<TValue> {
					Source = ValueSource.Local,
					Value = value
				});
			}
		}

		public IEnumerable GetErrors (string propertyName)
		{
			return (this.error != null) ? new[] { this.error } : Enumerable.Empty<string> ();
		}

		protected override void OnEditorsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			base.OnEditorsChanged (sender, e);
			
			// TODO: This, but not in a crap way

			foreach (IObjectEditor editor in this.subscribedEditors)
				editor.PropertyChanged -= OnEditorPropertyChanged;

			this.subscribedEditors.Clear();

			foreach (IObjectEditor editor in Editors) {
				this.subscribedEditors.Add (editor);
				OnEditorPropertyChanged (editor, new EditorPropertyChangedEventArgs (null));
				editor.PropertyChanged += OnEditorPropertyChanged;
			}

			UpdateCurrentValue();
		}

		/// <param name="newError">The error message or <c>null</c> to clear the error.</param>
		protected void SetError (string newError)
		{
			this.error = newError;
			OnErrorsChanged (new DataErrorsChangedEventArgs (nameof (Property)));
		}

		protected virtual void OnEditorPropertyChanged (object sender, EditorPropertyChangedEventArgs e)
		{
			if (e.Property != null && !Equals (e.Property, Property))
				return;

			// TODO: Smarter querying, can query the single editor and check against MultipleValues
			UpdateCurrentValue ();
		}

		private string error;
		private readonly List<IObjectEditor> subscribedEditors = new List<IObjectEditor> ();
		private TValue value;

		private void UpdateCurrentValue ()
		{
			ValueInfo<TValue> currentValue = null;

			bool disagree = false;
			foreach (ValueInfo<TValue> valueInfo in Editors.Select (ed => ed.GetValue<TValue> (Property, Variation))) {
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
			SetCurrentValue ((currentValue != null) ? currentValue.Value : default (TValue));
		}

		private bool SetCurrentValue (TValue newValue)
		{
			if (Equals (this.value, newValue))
				return false;

			this.value = newValue;
			OnPropertyChanged ();
			return true;
		}

		private void SetValue (ValueInfo<TValue> newValue)
		{
			SetError (null);

			try {
				foreach (IObjectEditor editor in Editors)
					editor.SetValue (Property, newValue);
			} catch (Exception ex) {
				AggregateException aggregate = ex as AggregateException;
				if (aggregate != null) {
					aggregate = aggregate.Flatten ();
					ex = aggregate.InnerExceptions[0];
				}

				SetError (ex.ToString());
			}
		}

		private void OnErrorsChanged (DataErrorsChangedEventArgs e)
		{
			ErrorsChanged?.Invoke (this, e);
		}
	}

	internal abstract class PropertyViewModel
		: ViewModelBase
	{
		protected PropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));
			if (editors == null)
				throw new ArgumentNullException (nameof (editors));

			Property = property;

			var observableEditors = new ObservableCollection<IObjectEditor>();
			Editors = observableEditors;
			observableEditors.CollectionChanged += OnEditorsChanged;
			observableEditors.AddRange (editors); // Purposefully after the event hookup
		}

		/// <remarks>Exists primarily to support PropertyGroupDescription</remarks>
		public string Category => (!String.IsNullOrWhiteSpace(Property.Category)) ? Property.Category : "Miscellaneous"; // TODO: Localize;

		public IPropertyInfo Property
		{
			get;
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

		/// <summary>
		/// Gets if the property's value can not be determined because multiple editors disagree.
		/// </summary>
		public bool MultipleValues
		{
			get { return this.multipleValues; }
			protected set
			{
				if (this.multipleValues == value)
					return;

				this.multipleValues = value;
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

		public ICollection<IObjectEditor> Editors
		{
			get;
			private set;
		}

		private ObjectViewModel valueModel;
		private bool multipleValues;
		private PropertyVariation variation;

		protected virtual void OnEditorsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			// properties will support multi-selection of designer items by self-handling having multiple
			// property editors.
		}
	}
}
