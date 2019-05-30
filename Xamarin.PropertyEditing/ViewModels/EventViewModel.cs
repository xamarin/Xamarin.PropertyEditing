using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.PropertyEditing.Common;
using Xamarin.PropertyEditing.Properties;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class EventViewModel
		: EditorViewModel
	{
		public EventViewModel (TargetPlatform platform, IEventInfo ev, IEnumerable<IObjectEditor> editors)
			: base (platform, editors)
		{
			if (ev == null)
				throw new ArgumentNullException (nameof (ev));

			AddHandlerCommand = new RelayCommand<string> (OnAddHandler, CanAddHandler);
			RemoveHandlerCommand = new RelayCommand<string> (OnRemoveHandler, CanRemoveHandler);

			Event = ev;
			RequestCurrentValueUpdate();
		}

		public IEventInfo Event
		{
			get;
		}

		public ICommand AddHandlerCommand
		{
			get;
		}

		public ICommand RemoveHandlerCommand
		{
			get;
		}

		public bool CanAddMoreHandlers
		{
			get { return this.canAddMoreHandlers; }
			private set
			{
				if (this.canAddMoreHandlers == value)
					return;

				this.canAddMoreHandlers = value;
				OnPropertyChanged();
			}
		}

		public override string Name => Event.Name;

		public override string Category => null;

		public override bool CanWrite => this.canWrite;

		public IReadOnlyList<string> Handlers => this.handlers;

		public AsyncValue<IReadOnlyList<string>> PotentialHandlers
		{
			get { return this.potentialHandlers; }
			private set
			{
				if (this.potentialHandlers == value)
					return;

				this.potentialHandlers = value;
				OnPropertyChanged();
			}
		}

		protected override void SetupEditor (IObjectEditor editor)
		{
			base.SetupEditor (editor);

			// We can move this up to the top level VM if it becomes a perf issue
			if (editor is IObjectEventEditor eventEditor)
				eventEditor.EventHandlersChanged += OnEventHandlersChanged;
		}

		protected override void TeardownEditor (IObjectEditor editor)
		{
			base.TeardownEditor (editor);

			if (editor is IObjectEventEditor eventEditor)
				eventEditor.EventHandlersChanged -= OnEventHandlersChanged;
		}

		protected override async Task UpdateCurrentValueAsync ()
		{
			if (Event == null)
				return;
			if (Editors.Count == 0) {
				SetCurrentMethods (null);
				return;
			}

			using (await AsyncWork.RequestAsyncWork (this)) {
				// Right now we only show events if one item is selected, but there's no technical reason
				// we can't attach the same event to the same handler across multiple objects, so the ground
				// work is done.
				bool disagree = false;

				bool canAddMultiple = true;
				Task<IReadOnlyList<string>>[] methodListTasks = new Task<IReadOnlyList<string>>[Editors.Count];
				int i = 0;
				foreach (IObjectEditor editor in Editors) {
					if (!(editor is IObjectEventEditor eventEditor)) {
						CanAddMoreHandlers = false;
						SetCanWrite (false);
						return;
					}

					methodListTasks[i++] = eventEditor.GetHandlersAsync (Event);
					canAddMultiple &= eventEditor.SupportsMultipleHandlers;
				}

				SetCanWrite (true);

				IReadOnlyList<string>[] methodLists = await Task.WhenAll (methodListTasks);
				IReadOnlyList<string> methods = methodLists[0];
				for (i = 1; i < methodLists.Length; i++) {
					IReadOnlyList<string> methodList = methodLists[i];
					if (methodList.Count != methods.Count) {
						disagree = true;
						break;
					}

					for (int x = 0; x < methodList.Count; i++) {
						if (methodList[x] != methods[x]) {
							disagree = true;
							break;
						}
					}

					if (disagree)
						break;
				}

				CanAddMoreHandlers = (canAddMultiple || (methods?.Count ?? 0) == 0);
				MultipleValues = disagree;
				SetCurrentMethods ((!disagree) ? methods : null);
				OnPotentialHandlersChanged();
			}
		}

		private readonly RelayCommand<string> addHandlerCommand, removeHandlerCommand;
		private readonly ObservableCollectionEx<string> handlers = new ObservableCollectionEx<string> ();
		private AsyncValue<IReadOnlyList<string>> potentialHandlers;
		private bool canWrite;
		private bool canAddMoreHandlers;

		private void SetCurrentMethods (IReadOnlyList<string> names)
		{
			this.handlers.Reset (names ?? Array.Empty<string>());
		}

		private void SetCanWrite (bool newCanWrite)
		{
			if (this.canWrite == newCanWrite)
				return;

			this.canWrite = newCanWrite;
			OnPropertyChanged (nameof(CanWrite));
		}

		private void OnPotentialHandlersChanged()
		{
			PotentialHandlers = new AsyncValue<IReadOnlyList<string>> (GetPotentialHandlers ());
		}

		private void OnEventHandlersChanged (object sender, EventHandlersChangedEventArgs e)
		{
			if (e.EventInfo != Event)
				return;

			RequestCurrentValueUpdate();
		}

		private async Task<IReadOnlyList<string>> GetPotentialHandlers ()
		{
			var commandsTask = new Task<IReadOnlyList<string>>[Editors.Count];
			int i = 0;
			foreach (IObjectEditor editor in Editors) {
				if (!(editor is IObjectEventEditor eventEditor))
					return Array.Empty<string> ();

				commandsTask[i++] = eventEditor.GetPotentialHandlersAsync (Event);
			}

			var potentialResults = await Task.WhenAll (commandsTask);
			var lcd = new HashSet<string> (potentialResults[0]);
			for (i = 1; i < potentialResults.Length; i++) {
				lcd.IntersectWith (potentialResults[i]);
			}

			lcd.ExceptWith (Handlers);

			return new ObservableCollection<string> (lcd);
		}

		private bool CanAddHandler (string name)
		{
			return CanWrite && !String.IsNullOrWhiteSpace (name) && !Handlers.Contains (name.Trim());
		}

		private async void OnAddHandler (string name)
		{
			name = name.Trim();

			try {
				foreach (IObjectEventEditor editor in Editors) {
					await editor.AttachHandlerAsync (Event, name);
				}
			} catch (Exception ex) {
				TargetPlatform.ReportError (ex.Message, ex);
			}

			if (PotentialHandlers.Value is IList<string> list)
				list.Remove (name);
		}

		private bool CanRemoveHandler (string name)
		{
			return CanWrite && Handlers.Contains (name);
		}

		private async void OnRemoveHandler (string name)
		{
			foreach (IObjectEventEditor editor in Editors) {
				await editor.DetachHandlerAsync (Event, name);
			}

			OnPotentialHandlersChanged();
		}
	}
}
