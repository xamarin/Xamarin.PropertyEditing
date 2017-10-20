using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	/// <summary>
	/// Represents groups of availability constraints that are ORed together.
	/// </summary>
	/// <remarks>
	/// The individual availability constraints are ANDed together within the group to produce the groups' result.
	/// The groups are ORed together, so only one needs to have its constituents all true.
	/// </remarks>
	public class MultiAvailabilityConstraint
		: IAvailabilityConstraint
	{
		public MultiAvailabilityConstraint (IEnumerable<IEnumerable<IAvailabilityConstraint>> constraintGroups)
		{
			if (constraintGroups == null)
				throw new ArgumentNullException (nameof(constraintGroups));

			List<IPropertyInfo> properties = new List<IPropertyInfo> ();
			this.constraints = new List<List<IAvailabilityConstraint>> ();
			foreach (var g in constraintGroups) {
				var group = new List<IAvailabilityConstraint> ();
				foreach (var c in g) {
					if (c.ConstrainingProperties != null) {
						foreach (var p in c.ConstrainingProperties) {
							if (!properties.Contains (p))
								properties.Add (p);
						}
					}

					group.Add (c);
				}

				this.constraints.Add (group);
			}

			ConstrainingProperties = properties;
		}

		public IReadOnlyList<IPropertyInfo> ConstrainingProperties {
			get;
		}

		public async Task<bool> GetIsAvailableAsync (IObjectEditor editor)
		{			
			HashSet<Task<bool>> tasks = new HashSet<Task<bool>> ();
			for (int i = 0; i < this.constraints.Count; i++) {
				tasks.Add (Task.WhenAll (this.constraints[i].Select (c => c.GetIsAvailableAsync (editor)).ToArray ())
					.ContinueWith (t => {
						if (t.IsFaulted)
							return false;

						return Array.TrueForAll (t.Result, b => b);
					}, TaskScheduler.Default));
			}

			// The groups of constraints are ||ed, each group's constraints are &&ed

			while (tasks.Count > 0) {
				var task = await Task.WhenAny (tasks).ConfigureAwait (false);
				if (task.IsFaulted)
					return false;
				if (task.Result)
					return true;

				tasks.Remove (task);
			}

			return false;
		}

		private readonly List<List<IAvailabilityConstraint>> constraints;
	}
}
