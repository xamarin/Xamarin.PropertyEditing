using System;
using System.Collections.Generic;

namespace Cadenza.Collections
{
	internal interface IReadOnlyOrderedDictionary<TKey, TValue>
		: IReadOnlyDictionary<TKey, TValue>
	{
		KeyValuePair<TKey, TValue> this[int index] { get; }

		int IndexOf (TKey key);
	}
}
