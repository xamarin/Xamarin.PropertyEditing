using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionEnumPropertyInfo<T>
		: ReflectionPropertyInfo, IHavePredefinedValues<T>
	{
		public ReflectionEnumPropertyInfo (PropertyInfo propertyInfo)
			: base (propertyInfo)
		{
			string[] names = Enum.GetNames (propertyInfo.PropertyType);
			Array values = Enum.GetValues (propertyInfo.PropertyType);

			var predefinedValues = new Dictionary<string, T> (names.Length);
			for (int i = 0; i < names.Length; i++) {
				predefinedValues.Add (names[i], (T)values.GetValue (i));
			}

			PredefinedValues = predefinedValues;

			FlagsAttribute flags = PropertyInfo.PropertyType.GetCustomAttribute<FlagsAttribute> ();
			if (IsValueCombinable = flags != null) {
				DynamicBuilder.RequestOrOperator<T> ();
				DynamicBuilder.RequestHasFlagMethod<T> ();
				DynamicBuilder.RequestCaster<IReadOnlyList<T>> ();
			}
		}

		public bool IsConstrainedToPredefined => true;

		public bool IsValueCombinable
		{
			get;
		}

		public IReadOnlyDictionary<string, T> PredefinedValues
		{
			get;
		}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public override async Task SetValueAsync<TValue> (object target, TValue value)
		{
			IReadOnlyList<T> values = value as IReadOnlyList<T>;
			if (values != null) {
				if (!IsValueCombinable)
					throw new ArgumentException ("Can not set a combined value on a non-combinable type", nameof(value));

				Func<T, T, T> or = DynamicBuilder.GetOrOperator<T> ();

				T realValue = values.Count > 0 ? values[0] : default(T);
				for (int i = 1; i < values.Count; i++) {
					realValue = or (realValue, values[i]);
				}

				PropertyInfo.SetValue (target, realValue);
			} else {
				object convertedValue = Enum.ToObject (PropertyInfo.PropertyType, value);
				PropertyInfo.SetValue (target, convertedValue);
			}
		}

		public override async Task<TValue> GetValueAsync<TValue> (object target)
		{
			if (typeof(TValue) == typeof(IReadOnlyList<T>)) {
				T realValue = (T)PropertyInfo.GetValue (target);

				Func<T, T, bool> hasFlag = DynamicBuilder.GetHasFlagMethod<T> ();

				List<T> values = new List<T> ();
				foreach (T value in PredefinedValues.Values) {
					if (hasFlag (realValue, value))
						values.Add (value);
				}

				Func<object, TValue> caster = DynamicBuilder.GetCaster<TValue> ();
				return caster (values);
			}

			return (TValue) PropertyInfo.GetValue (target);
		}
	}
#pragma warning restore CS1998

	internal static class DynamicBuilder
	{
		public static void RequestCaster<T> ()
		{
			GetOrAddCaster<T> ();
		}

		public static void RequestOrOperator<T> ()
		{
			GetOrAddOrOperator<T> ();
		}
		
		public static void RequestHasFlagMethod<T> ()
		{
			GetOrAddHasFlagMethod<T> ();
		}

		public static Func<T, T, T> GetOrOperator<T> ()
		{
			Task<Delegate> result = GetOrAddOrOperator<T> ();
			return (Func<T, T, T>) result.Result;
		}

		public static Func<T, T, bool> GetHasFlagMethod<T> ()
		{
			Task<Delegate> result = GetOrAddHasFlagMethod<T> ();
			return (Func<T, T, bool>) result.Result;
		}

		public static Func<object, T> GetCaster<T> ()
		{
			Task<Delegate> result = GetOrAddCaster<T> ();
			return (Func<object, T>) result.Result;
		}

		private static readonly ConcurrentDictionary<Type, Task<Delegate>> Casters = new ConcurrentDictionary<Type, Task<Delegate>> ();
		private static readonly ConcurrentDictionary<Type, Task<Delegate>> OrOperators = new ConcurrentDictionary<Type, Task<Delegate>> ();
		private static readonly ConcurrentDictionary<Type, Task<Delegate>> HasFlagsMethods = new ConcurrentDictionary<Type, Task<Delegate>> ();

		private static Task<Delegate> GetOrAddCaster<T> ()
		{
			return Casters.GetOrAdd (typeof(T), t => {
				return Task.Run (() => {
					var arg = Expression.Parameter (typeof(object));
					return Expression.Lambda (Expression.Convert (arg, typeof(T)), arg).Compile ();
				});
			});
		}

		private static Task<Delegate> GetOrAddHasFlagMethod<T> ()
		{
			return HasFlagsMethods.GetOrAdd (typeof(T), t => {
				return Task.Run (() => {
					var left = Expression.Parameter (typeof(T));
					var right = Expression.Parameter (typeof(T));
					var hasFlag = Expression.Equal (Expression.And (left, right), right);
					return Expression.Lambda (hasFlag, left, right).Compile();
				});
			});
		}

		private static Task<Delegate> GetOrAddOrOperator<T> ()
		{
			return OrOperators.GetOrAdd (typeof(T), t => {
				return Task.Run (() => {
					var left = Expression.Parameter (typeof(T));
					var right = Expression.Parameter (typeof(T));
					var or = Expression.Or (left, right);
					return Expression.Lambda (or, left, right).Compile ();
				});
			});
		}
	}
}
