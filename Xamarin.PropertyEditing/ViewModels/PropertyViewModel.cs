using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal abstract class PropertyViewModel<TValue>
		: PropertyViewModel
	{
		private TValue value;

		protected PropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
		}

		public TValue Value
		{
			get { return this.value; }
			set
			{
				this.value = value;
				OnPropertyChanged ();
			}
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
			observableEditors.CollectionChanged += OnEditorsChanged;
			observableEditors.AddRange (editors); // Purposefully after the event hookup
			Editors = observableEditors;
		}

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
			private set
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

		private void OnEditorsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			// properties will support multi-selection of designer items by self-handling having multiple
			// property editors.
		}
	}
}
