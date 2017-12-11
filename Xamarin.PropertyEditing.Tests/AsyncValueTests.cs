using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class AsyncValueTests
	{
		[Test]
		public void ValueOnCompletion ()
		{
			var tcs = new TaskCompletionSource<string> ();
			var asyncValue = new AsyncValue<string> (tcs.Task);

			Assume.That (asyncValue.Value, Is.Null);

			bool changed = false;
			asyncValue.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (AsyncValue<string>.Value))
					changed = true;
			};

			const string value = "value";
			tcs.SetResult (value);

			Assert.That (asyncValue.Value, Is.EqualTo (value));
			Assert.That (changed, Is.True, "PropertyChanged did not fire for Value");
		}

		[Test]
		public void DefaultValue ()
		{
			var tcs = new TaskCompletionSource<bool> ();
			var value = new AsyncValue<bool> (tcs.Task, defaultValue: true);

			Assert.That (value.Value, Is.True);
		}

		[TestCase (true)]
		[TestCase (false)]
		public void ValueReplacesDefaultValue (bool value)
		{
			var tcs = new TaskCompletionSource<bool> ();
			var asyncValue = new AsyncValue<bool> (tcs.Task, defaultValue: !value);

			Assume.That (asyncValue.Value, Is.EqualTo (!value));

			tcs.SetResult (value);

			Assert.That (asyncValue.Value, Is.EqualTo (value));
		}

		[Test]
		public void IsRunning ()
		{
			var tcs = new TaskCompletionSource<string> ();
			var asyncValue = new AsyncValue<string> (tcs.Task);

			Assume.That (asyncValue.Value, Is.Null);
			Assert.That (asyncValue.IsRunning, Is.True);

			bool changed = false;
			asyncValue.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (AsyncValue<string>.IsRunning))
					changed = true;
			};

			const string value = "value";
			tcs.SetResult (value);

			Assume.That (asyncValue.Value, Is.EqualTo (value));
			Assert.That (asyncValue.IsRunning, Is.False, "IsRunning did not flip to false");
			Assert.That (changed, Is.True, "PropertyChanged did not fire for IsRunning");
		}

		[Test]
		public void AsyncValueException ()
		{
			var tcs = new TaskCompletionSource<bool> ();
			var asyncValue = new AsyncValue<bool> (tcs.Task, defaultValue: true);

			Assume.That (asyncValue.Value, Is.True);

			bool valueChanged = false, runningChanged = false;
			asyncValue.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (AsyncValue<string>.Value))
					valueChanged = true;
				else if (args.PropertyName == nameof (AsyncValue<string>.IsRunning))
					runningChanged = true;
			};

			tcs.SetException (new Exception ());

			Assert.That (asyncValue.Value, Is.True);
			Assert.That (valueChanged, Is.False, "Value should not signal change when there's an exception");
			Assert.That (asyncValue.IsRunning, Is.False);
			Assert.That (runningChanged, Is.False);
		}
	}
}
