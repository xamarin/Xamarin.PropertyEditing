using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xamarin.PropertyEditing
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	public interface IPerformanceProvider
	{
		void Stop (string reference, string tag, string path, string member);

		void Start (string reference, string tag, string path, string member);
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public class Performance
	{
		public static IPerformanceProvider Provider { get; private set; } = new PerformanceProvider ();

		public static void SetProvider (IPerformanceProvider instance)
		{
			Provider = instance;
		}

		public static void Start (string reference, string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			Provider?.Start (reference, tag, path, member);
		}

		public static void Stop (string reference, string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			Provider?.Stop (reference, tag, path, member);
		}

		public static IDisposable StartNew (string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			return new DisposablePerformanceReference (tag, path, member);
		}

		class DisposablePerformanceReference : IDisposable
		{
			string _reference;
			string _tag;
			string _path;
			string _member;

			public DisposablePerformanceReference (string tag, string path, string member)
			{
				_reference = Guid.NewGuid ().ToString ();
				_tag = tag;
				_path = path;
				_member = member;
				Start (_reference, _tag, _path, _member);
			}

			public void Dispose ()
			{
				Stop (_reference, _tag, _path, _member);
			}
		}
	}

	public class PerformanceProvider : IPerformanceProvider
	{
		internal class Statistic
		{
			public readonly List<Tuple<string, Stopwatch>> Times = new List<Tuple<string, Stopwatch>> ();
			public int CallCount;
			public TimeSpan TotalTime;
			public bool IsDetail;
		}

		readonly Dictionary<string, Statistic> _Statistics = new Dictionary<string, Statistic> ();

		internal Dictionary<string, Statistic> Statistics
		{
			get { return _Statistics; }
		}

		public void Clear ()
		{
			Statistics.Clear ();
		}

		public void Start (string reference, string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			Stopwatch watch = Stopwatch.StartNew ();
			string id = GetId (tag, path, member);

			Statistic stats = GetStat (id);

			if (tag != null)
				stats.IsDetail = true;

			stats.CallCount++;
			stats.Times.Add (new Tuple<string, Stopwatch> (reference, watch));
		}

		public void Stop (string reference, string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			string id = GetId (tag, path, member);
			Statistic stats = GetStat (id);

			if (!stats.Times.Any ())
				return;

			Stopwatch watch = stats.Times.Single (s => s.Item1 == reference).Item2;
			stats.TotalTime += watch.Elapsed;
		}

		public IEnumerable<string> GetStats ()
		{
			yield return "ID                                                                               | Call Count | Total Time | Avg Time";
			foreach (KeyValuePair<string, Statistic> kvp in Statistics.OrderBy (kvp => kvp.Key)) {
				string key = ShortenPath (kvp.Key);
				double total = kvp.Value.TotalTime.TotalMilliseconds;// TimeSpan.FromTicks (kvp.Value.TotalTime).TotalMilliseconds;
				double avg = total / kvp.Value.CallCount;
				yield return string.Format ("{0,-80} | {1,-10} | {2,-8}ms | {3,-8}ms", key, kvp.Value.CallCount, total, avg);
			}
		}

		public void DumpStats()
		{
			foreach (string s in GetStats ())
				Debug.WriteLine (s);

			Clear ();
		}

		static string ShortenPath (string path)
		{
			int index = path.LastIndexOf ("Xamarin.PropertyEditing");
			if (index > -1)
				path = path.Substring (index + "Xamarin.PropertyEditing".Length);

			return path;
		}

		static string GetId (string tag, string path, string member)
		{
			return string.Format ("{0}:{1}{2}", path, member, (tag != null ? "-" + tag : string.Empty));
		}

		Statistic GetStat (string id)
		{
			Statistic stats;
			if (!Statistics.TryGetValue (id, out stats)) {
				Statistics[id] = stats = new Statistic ();
			}
			return stats;
		}
	}
}