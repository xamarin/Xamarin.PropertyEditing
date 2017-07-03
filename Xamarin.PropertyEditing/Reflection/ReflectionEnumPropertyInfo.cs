using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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

		public override void SetValue<TValue> (object target, TValue value)
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

		public override TValue GetValue<TValue> (object target)
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
					DynamicMethod method = new DynamicMethod ("Caster", typeof(T), new[] { typeof(object) });
					ILGenerator generator = method.GetILGenerator ();
					generator.Emit (OpCodes.Ldarg_0);
					generator.Emit (OpCodes.Castclass, typeof(T));
					generator.Emit (OpCodes.Ret);
					return method.CreateDelegate (typeof(Func<object, T>));
				});
			});
		}

		private static Task<Delegate> GetOrAddHasFlagMethod<T> ()
		{
			return HasFlagsMethods.GetOrAdd (typeof(T), t => {
				return Task.Run (() => {
					DynamicMethod method = new DynamicMethod ("HasFlags", typeof(bool), new[] { typeof(T), typeof(T) });
					ILGenerator generator = method.GetILGenerator ();
					generator.Emit (OpCodes.Ldarg_0);
					generator.Emit (OpCodes.Ldarg_1);
					generator.Emit (OpCodes.And); // arg0 & arg1
					generator.Emit (OpCodes.Ldc_I4_0);
					if (typeof (T) == typeof (uint))
						generator.Emit (OpCodes.Conv_U4);
					else if (typeof (T) == typeof (long))
						generator.Emit (OpCodes.Conv_I8);
					else if (typeof (T) == typeof (ulong))
						generator.Emit (OpCodes.Conv_U8);

					generator.Emit (OpCodes.Ceq); // pushes 1 if not equal
					generator.Emit (OpCodes.Ldc_I4_0);
					generator.Emit (OpCodes.Ceq); // reverses
					generator.Emit (OpCodes.Ret);
					return method.CreateDelegate (typeof(Func<T, T, bool>));
				});
			});
		}

		private static Task<Delegate> GetOrAddOrOperator<T> ()
		{
			return OrOperators.GetOrAdd (typeof(T), t => {
				return Task.Run (() => {
					DynamicMethod method = new DynamicMethod ("Or", typeof(T), new[] { typeof(T), typeof(T) });
					ILGenerator generator = method.GetILGenerator ();
					generator.Emit (OpCodes.Ldarg_0);
					generator.Emit (OpCodes.Ldarg_1);
					generator.Emit (OpCodes.Or);
					generator.Emit (OpCodes.Ret);
					return method.CreateDelegate (typeof(Func<T, T, T>));
				});
			});
		}
	}
}
