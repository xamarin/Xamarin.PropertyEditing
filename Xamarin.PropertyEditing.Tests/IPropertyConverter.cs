using System;

namespace Xamarin.PropertyEditing.Tests
{
	interface IPropertyConverter
	{
		bool TryConvert<TFrom> (TFrom fromValue, Type toType, out object toValue);
	}
}
