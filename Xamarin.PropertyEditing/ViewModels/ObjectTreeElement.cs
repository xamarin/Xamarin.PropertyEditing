using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class ObjectTreeElement
	{
		public ObjectTreeElement (IEditorProvider provider, IObjectEditor editor)
		{
			if (provider == null)
				throw new ArgumentNullException (nameof(provider));
			if (editor == null)
				throw new ArgumentNullException (nameof(editor));

			Editor = editor;
			Children = new AsyncValue<IReadOnlyList<ObjectTreeElement>> (QueryChildrenAsync (provider));

			string typeName = $"[{Editor.TargetType.Name}]";

			Task<string> nameTask;
			INameableObject nameable = Editor as INameableObject;
			if (nameable != null) {
				nameTask = nameable.GetNameAsync ().ContinueWith (t =>
					(!String.IsNullOrWhiteSpace (t.Result)) ? $"{typeName} \"{t.Result}\"" : typeName, TaskScheduler.Default);
			} else
				nameTask = Task.FromResult (typeName);

			Name = new AsyncValue<string> (nameTask, typeName);
		}

		public AsyncValue<string> Name
		{
			get;
		}

		public IObjectEditor Editor
		{
			get;
		}

		public AsyncValue<IReadOnlyList<ObjectTreeElement>> Children
		{
			get;
		}

		private async Task<IReadOnlyList<ObjectTreeElement>> QueryChildrenAsync (IEditorProvider provider)
		{
			var targets = await provider.GetChildrenAsync (Editor.Target);

			List<Task<IObjectEditor>> editorTasks = new List<Task<IObjectEditor>> ();
			foreach (object target in targets) {
				editorTasks.Add (provider.GetObjectEditorAsync (target));
			}

			IObjectEditor[] editors = await Task.WhenAll (editorTasks);
			return editors.Select (e => new ObjectTreeElement (provider, e)).ToArray ();
		}
	}
}