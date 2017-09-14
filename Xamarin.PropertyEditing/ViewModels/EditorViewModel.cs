﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal abstract class EditorViewModel
		: NotifyingObject
	{
		public EditorViewModel (IEnumerable<IObjectEditor> editors)
		{
			if (editors == null)
				throw new ArgumentNullException (nameof (editors));

			var observableEditors = new ObservableCollectionEx<IObjectEditor> ();
			Editors = observableEditors;
			observableEditors.CollectionChanged += OnEditorsChanged;
			observableEditors.AddItems (editors); // Purposefully after the event hookup

			//TODO: Property is being set after the editors added trickle down since its in the subclass
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

		public ICollection<IObjectEditor> Editors
		{
			get;
			private set;
		}

		protected static AsyncWorkQueue AsyncWork
		{
			get;
		} = new AsyncWorkQueue ();

		protected virtual void OnEditorsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
					AddEditors (e.NewItems);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveEditors (e.OldItems);
					break;
				case NotifyCollectionChangedAction.Reset:
					RemoveEditors (this.subscribedEditors);
					AddEditors ((IList)Editors);
					break;
			}

			UpdateCurrentValue ();
		}

		protected virtual void UpdateCurrentValue()
		{
		}

		protected virtual void SetupEditor (IObjectEditor editor)
		{
		}

		protected virtual void TeardownEditor (IObjectEditor editor)
		{
		}

		private bool multipleValues;
		private readonly List<IObjectEditor> subscribedEditors = new List<IObjectEditor> ();

		private void AddEditors (IList editors)
		{
			for (int i = 0; i < editors.Count; i++) {
				IObjectEditor editor = (IObjectEditor)editors[i];
				this.subscribedEditors.Add (editor);
				SetupEditor (editor);
			}
		}

		private void RemoveEditors (IList editors)
		{
			for (int i = 0; i < editors.Count; i++) {
				IObjectEditor editor = (IObjectEditor)editors[i];
				this.subscribedEditors.Remove (editor);
				TeardownEditor (editor);
			}
		}
	}
}
