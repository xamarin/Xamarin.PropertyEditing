using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class EventViewModel
		: EditorViewModel
	{
		// TODO: Break error handling out into reusable interface implementer
		public EventViewModel (IEventInfo ev, IEnumerable<IObjectEditor> editors)
			: base (editors)
		{
			if (ev == null)
				throw new ArgumentNullException (nameof (ev));

			Event = ev;
			RequestCurrentValueUpdate();
		}

		public IEventInfo Event
		{
			get;
		}

		public override string Name => Event.Name;

		public override string Category => null;

		public string MethodName
		{
			get { return this.methodName; }
			set
			{
				if (this.methodName == value)
					return;

				SetMethodName (value);
			}
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
				IReadOnlyList<string>[] methodLists = await Task.WhenAll (Editors.OfType<IObjectEventEditor>().Select (ed => ed.GetHandlersAsync (Event)));

				bool disagree = false;
				IReadOnlyList<string> methods = methodLists[0];
				for (int i = 1; i < methodLists.Length; i++) {
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

				MultipleValues = disagree;
				SetCurrentMethods ((!disagree) ? methods : null);
			}
		}

		private string methodName;

		private void SetCurrentMethods (IReadOnlyList<string> names)
		{
			// Currently the UI only handles single attachments, but things like inspector
			// might be able to make use of a full list, so most of the ground work is done.
			string name = names?.FirstOrDefault ();

			this.methodName = name;
			OnPropertyChanged (nameof (MethodName));
		}

		private async void SetMethodName (string name)
		{
			IObjectEventEditor editor = Editors.OfType<IObjectEventEditor>().First ();

			if (this.methodName != null)
				await editor.DetachHandlerAsync (Event, this.methodName);

			if (!String.IsNullOrWhiteSpace (name))
				await editor.AttachHandlerAsync (Event, name);

			await UpdateCurrentValueAsync ();
		}
	}
}
