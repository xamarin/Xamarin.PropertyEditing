using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal abstract class EditorViewModel
		: NotifyingObject, IComparable<EditorViewModel>
	{
		public EditorViewModel (TargetPlatform platform, IEnumerable<IObjectEditor> editors)
		{
			if (platform == null)
				throw new ArgumentNullException (nameof (platform));
			if (editors == null)
				throw new ArgumentNullException (nameof (editors));

			TargetPlatform = platform;

			var observableEditors = new ObservableCollectionEx<IObjectEditor> ();
			Editors = observableEditors;
			observableEditors.CollectionChanged += OnEditorsChanged;
			observableEditors.AddItems (editors); // Purposefully after the event hookup
		}

		public abstract string Category
		{
			get;
		}

		public abstract string Name
		{
			get;
		}

		public virtual bool CanDelve => false;

		public virtual bool DelveByDefault => false;

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
		}

		protected static AsyncWorkQueue AsyncWork => PropertiesViewModel.AsyncWork;

		public TargetPlatform TargetPlatform
		{
			get;
		}

		public PropertiesViewModel Parent
		{
			get { return this.parent; }
			internal set
			{
				if (this.parent == value)
					return;

				this.parent = value;
				OnPropertyChanged();
			}
		}

		public virtual int CompareTo (EditorViewModel other)
		{
			if (other == null)
				return -1;
			if (Name == other.Name)
				return 0;
			if (Name == null)
				return 1;

			return Name.CompareTo (other.Name);
		}

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

			RequestCurrentValueUpdate();
		}

		/// <summary>
		/// Requests that the current value be updated, not immediate.
		/// </summary>
		protected async void RequestCurrentValueUpdate ()
		{
			await UpdateCurrentValueAsync ();
		}

		/// <remarks>
		/// If your sub-class needs extra setup in the ctor before the value can be executed,
		/// safe guard your override of this method as it will be run when the editors are
		/// added in the base ctor before your sub-class' ctor has run. After you've completed
		/// setup, call <see cref="RequestCurrentValueUpdate"/>.
		/// </remarks>
		protected virtual Task UpdateCurrentValueAsync()
		{
			return Task.FromResult (true);
		}

		protected virtual void SetupEditor (IObjectEditor editor)
		{
		}

		protected virtual void TeardownEditor (IObjectEditor editor)
		{
		}

		private bool multipleValues;
		private readonly List<IObjectEditor> subscribedEditors = new List<IObjectEditor> ();
		private PropertiesViewModel parent;

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
