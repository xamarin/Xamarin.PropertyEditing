using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	internal static class Numeric<T>
	{
		public static Func<T, T> Increment
		{
			get
			{ 
				SetupMath();
				return IncrementValue;
			}
		}

		public static Func<T, T> Decrement
		{
			get
			{
				SetupMath();
				return DecrementValue;
			}
		}

		private static Func<T, T> IncrementValue, DecrementValue;
		private static NullableConverter NullConverter;

		private static Func<T, T> GetNullableVersion (Expression shiftExpression, Type realType)
		{
			var v = Expression.Parameter (typeof(T));
			var convert = Expression.Call (Expression.Constant (NullConverter), nameof(NullableConverter.ConvertFrom), null, Expression.Convert (v, typeof(object)));
			var shift = Expression.Invoke (shiftExpression, Expression.Convert (convert, realType));
			var conditional = Expression.Condition (Expression.Equal (v, Expression.Constant (null)), Expression.Default (realType), shift);

			// (T v) => (T)((v == null) ? default(realType) : shiftExpression ((realType)NullableConverter.ConvertFrom ((object)v));
			return Expression.Lambda<Func<T, T>> (Expression.Convert (conditional, typeof(T)), v).Compile();
		}

		private static void SetupMath ()
		{
			if (IncrementValue != null)
				return;

			Type t = typeof(T);
			bool isNullable = (t.Name == "Nullable`1");
			if (isNullable) {
				NullConverter = new NullableConverter (typeof(T));
				t = Nullable.GetUnderlyingType (t);
			}

			Task<Func<T,T>> createIncrement = Task.Run (() => {
				var value = Expression.Parameter (t);

				Expression add;
				if (t == typeof(byte) || t == typeof(sbyte)) {
					// (t value) => (t)((int)value + 1);
					add = Expression.Convert (Expression.Add (Expression.Convert (value, typeof(int)), Expression.Constant (1)), t);
				} else {
					var shiftby = Expression.Convert (Expression.Constant (1), t);
					add = Expression.Add (value, shiftby);
				}

				var increment = Expression.Lambda (add, value);
				if (isNullable)
					return GetNullableVersion (increment, t);

				return (Func<T, T>)increment.Compile();
			});

			Task<Func<T,T>> createDecrement = Task.Run (() => {
				var shiftby = Expression.Convert (Expression.Constant (1), t);
				var value = Expression.Parameter (t);

				Expression sub;
				if (t == typeof(byte) || t == typeof(sbyte)) {
					sub = Expression.Convert (Expression.Subtract (Expression.Convert (value, typeof(int)), Expression.Constant (1)), t);
				} else {
					sub = Expression.Subtract (value, shiftby);
				}

				var decrement = Expression.Lambda (sub, value);
				if (isNullable)
					return GetNullableVersion (decrement, t);

				return (Func<T, T>)decrement.Compile();
			});

			IncrementValue = createIncrement.Result;
			DecrementValue = createDecrement.Result;
		}
	}
}