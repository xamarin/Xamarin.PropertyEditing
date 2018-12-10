//
// OrderedDictionaryTest.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2009 Eric Maupin (http://www.ermau.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Cadenza.Collections.Tests
{
	[TestFixture]
	public class OrderedDictionaryTest
	{
		[Test]
		public void Ctor_DictNull ()
		{
			Assert.Throws<ArgumentNullException> (() => {
				Dictionary<string, string> foo = null;
				new OrderedDictionary<string, string> (foo);
			});
		}

		[Test]
		public void Ctor_CapacityOutOfRange ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new OrderedDictionary<string, string> (-1));
		}

		[Test]
		public void Ctor_CapacityOutOfRangeWithEquality ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new OrderedDictionary<string, string> (-1, null));
		}

		[Test]
		public void KeyIndexer ()
		{
			var dict = new OrderedDictionary<string, string> { { "foo", "bar" }, { "baz", "monkeys" } };
			Assert.AreEqual ("bar", dict["foo"]);
			Assert.AreEqual ("monkeys", dict["baz"]);
		}

		[Test]
		public void KeyIndexer_KeyNotFound ()
		{
			var dict = new OrderedDictionary<string, string>
			{ { "foo", "bar" }, { "baz", "monkeys" } };

			Assert.Throws<KeyNotFoundException> (() => dict["wee"].ToString ());
		}

		[Test]
		public void KeyIndexerSet ()
		{
			var dict = new OrderedDictionary<uint, int> ();
			dict[(uint)1] = 1;
			dict[(uint)2] = 2;
			dict[(uint)3] = 3;
			dict.Remove (2);
			dict[(uint)4] = 4;

			Assert.AreEqual (1, dict[(int)0]);
			Assert.AreEqual (3, dict[(int)1]);
			Assert.AreEqual (4, dict[(int)2]);
		}

		[Test]
		public void KeyIndexerGet_KeyNull ()
		{
			var dict = new OrderedDictionary<string, string> ();
			Assert.Throws<ArgumentNullException> (() => dict[null].ToString ());
		}

		[Test]
		public void KeyIndexerSet_KeyNull ()
		{
			var dict = new OrderedDictionary<string, string> ();

			Assert.Throws<ArgumentNullException> (() => dict[null] = "foo");
		}

		[Test]
		public void IndexIndexer ()
		{
			var dict = new OrderedDictionary<uint, int> ();
			dict.Add (1, 1);
			dict.Add (2, 2);
			dict.Add (3, 3);
			dict.Remove (2);
			dict.Add (4, 4);

			Assert.AreEqual (1, dict[(int)0]);
			Assert.AreEqual (3, dict[(int)1]);
			Assert.AreEqual (4, dict[(int)2]);
		}

		[Test]
		public void IndexIndexerSet_New ()
		{
			var dict = new OrderedDictionary<string, string> ();
			dict.Add ("A", "B");
			dict.Add ("C", "D");

			var list = (IList<KeyValuePair<string, string>>)dict;
			list[1] = new KeyValuePair<string, string> ("E", "F");

			Assert.That (list.Count, Is.EqualTo (2));
			Assert.That (dict["E"], Is.EqualTo ("F"));
			Assert.That (dict.ContainsKey ("C"), Is.False);
		}

		[Test]
		public void IndexIndexerSet_Existing ()
		{
			var dict = new OrderedDictionary<string, string> ();
			dict.Add ("A", "B");
			dict.Add ("C", "D");

			var list = (IList<KeyValuePair<string, string>>)dict;
			list[1] = new KeyValuePair<string, string> ("C", "F");

			Assert.That (list.Count, Is.EqualTo (2));
			Assert.That (dict["C"], Is.EqualTo ("F"));
		}

		[Test]
		[Description ("You can't duplicate a key by entering it via indexed set.")]
		public void IndexIndexerSet_ReplicatedKey ()
		{
			var dict = new OrderedDictionary<string, string> ();
			dict.Add ("A", "B");
			dict.Add ("C", "D");

			var list = (IList<KeyValuePair<string, string>>) dict;
			Assert.That (() => list[0] = new KeyValuePair<string, string> ("C", "E"), Throws.ArgumentException);
		}

		[Test]
		public void Indexer_IndexOutOfRangeLower ()
		{
			var dict = new OrderedDictionary<string, string> ();
			Assert.Throws<ArgumentOutOfRangeException> (() => dict[-1].ToString ());
		}

		[Test]
		public void Indexer_IndexOutOfRangeUpper ()
		{
			var dict = new OrderedDictionary<string, string>
			{ { "foo", "bar" }, { "baz", "monkeys" } };

			Assert.Throws<ArgumentOutOfRangeException> (() => dict[2].ToString ());
		}

		[Test]
		public void EnumerableOrder ()
		{
			var dict = new OrderedDictionary<uint, int> ();
			dict.Add (1, 1);
			dict.Add (2, 2);
			dict.Add (3, 3);
			dict.Remove (2);
			dict.Add (4, 4);

			using (var enumerator = dict.GetEnumerator ()) {
				Assert.IsTrue (enumerator.MoveNext ());
				Assert.AreEqual (1, enumerator.Current.Value);
				Assert.IsTrue (enumerator.MoveNext ());
				Assert.AreEqual (3, enumerator.Current.Value);
				Assert.IsTrue (enumerator.MoveNext ());
				Assert.AreEqual (4, enumerator.Current.Value);
			}
		}

		[Test]
		public void Values_EnumerableOrder ()
		{
			var dict = new OrderedDictionary<uint, int> ();
			dict.Add (1, 1);
			dict.Add (2, 2);
			dict.Add (3, 3);
			dict.Remove (2);
			dict.Add (4, 4);

			Assert.AreEqual (1, dict.Values.ElementAt (0));
			Assert.AreEqual (3, dict.Values.ElementAt (1));
			Assert.AreEqual (4, dict.Values.ElementAt (2));
		}

		[Test]
		public void CopyTo ()
		{
			var dict = new OrderedDictionary<uint, int> ();
			dict.Add (1, 1);
			dict.Add (2, 2);
			dict.Add (3, 3);
			dict.Remove (2);
			dict.Add (4, 4);

			KeyValuePair<uint, int>[] a = new KeyValuePair<uint, int>[13];

			((ICollection<KeyValuePair<uint, int>>)dict).CopyTo (a, 10);

			for (int i = 0; i < 10; ++i) {
				if (i < 10)
					Assert.AreEqual (default (KeyValuePair<uint, int>), a[i]);
			}

			Assert.AreEqual (1, a[10].Value);
			Assert.AreEqual (3, a[11].Value);
			Assert.AreEqual (4, a[12].Value);
		}

		[Test]
		public void CopyTo_NullArray ()
		{
			var dict = new OrderedDictionary<string, string> ();
			KeyValuePair<string, string>[] a = null;

			Assert.Throws<ArgumentNullException> (
				() => ((ICollection<KeyValuePair<string, string>>) dict).CopyTo (a, 0));
		}

		[Test]
		public void CopyTo_ArrayTooSmall ()
		{
			var dict = new OrderedDictionary<string, string> ();
			for (int i = 0; i < 1000; ++i)
				dict.Add (i.ToString (), (i + 1).ToString ());

			KeyValuePair<string, string>[] a = new KeyValuePair<string, string>[1];
			Assert.Throws<ArgumentException> (() => ((ICollection<KeyValuePair<string, string>>) dict).CopyTo (a, 0));
		}

		[Test]
		public void CopyTo_IndexOutOfRange ()
		{
			var dict = new OrderedDictionary<string, string> ();

			Assert.Throws<ArgumentOutOfRangeException> (() =>
				((ICollection<KeyValuePair<string, string>>) dict).CopyTo (new KeyValuePair<string, string>[10], -1));
		}

		[Test]
		public void Values_CopyTo ()
		{
			var dict = new OrderedDictionary<uint, int> ();
			dict.Add (1, 1);
			dict.Add (2, 2);
			dict.Add (3, 3);
			dict.Remove (2);
			dict.Add (4, 4);

			int[] a = new int[13];

			dict.Values.CopyTo (a, 10);

			for (int i = 0; i < 10; ++i) {
				if (i < 10)
					Assert.AreEqual (default (int), a[i]);
			}

			Assert.AreEqual (1, a[10]);
			Assert.AreEqual (3, a[11]);
			Assert.AreEqual (4, a[12]);
		}

		[Test]
		public void ValuesCopyTo_NullArray ()
		{
			var dict = new OrderedDictionary<string, string> ();
			string[] a = null;

			Assert.Throws<ArgumentNullException> (() => dict.Values.CopyTo (a, 0));
		}

		[Test]
		public void ValuesCopyTo_ArrayTooSmall ()
		{
			var dict = new OrderedDictionary<string, string> ();
			for (int i = 0; i < 1000; ++i)
				dict.Add (i.ToString (), (i + 1).ToString ());

			Assert.Throws<ArgumentException> (() => dict.Values.CopyTo (new string[1], 0));
		}

		[Test]
		public void ValuesCopyTo_IndexOutOfRange ()
		{
			var dict = new OrderedDictionary<string, string> ();

			Assert.Throws<ArgumentOutOfRangeException> (() => dict.Values.CopyTo (new string[1], -1));
		}

		[Test]
		public void IsReadOnly ()
		{
			Assert.IsFalse (((ICollection<KeyValuePair<int, int>>)new OrderedDictionary<int, int> ()).IsReadOnly);
		}

		[Test]
		public void Values_IsReadOnly ()
		{
			Assert.IsTrue (new OrderedDictionary<int, int> ().Values.IsReadOnly);
		}

		[Test]
		public void Clear ()
		{
			var dict = new OrderedDictionary<int, int> { { 1, 2 }, { 2, 3 }, { 3, 4 }, { 4, 5 } };

			dict.Clear ();

			Assert.AreEqual (0, dict.Count);
			Assert.AreEqual (0, dict.Values.Count);
			Assert.IsFalse (dict.ContainsKey (1));
			Assert.IsFalse (dict.ContainsValue (2));
		}

		[Test]
		public void Values_Clear ()
		{
			var dict = new OrderedDictionary<int, int> ();

			Assert.Throws<NotSupportedException> (dict.Values.Clear);
		}

		[Test]
		public void Add ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("1", 2);
			dict.Add ("2", 3);

			Assert.AreEqual (dict[0], 2);
			Assert.AreEqual (dict[1], 3);
		}

		[Test]
		public void Add_KeyNull ()
		{
			var dict = new OrderedDictionary<string, int> ();
			Assert.Throws<ArgumentNullException> (() => dict.Add (null, 1));
		}

		[Test]
		public void Add_KeyExists ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);

			Assert.Throws<ArgumentException> (() => dict.Add ("foo", 1));
		}

		[Test]
		public void KVP_Add ()
		{
			var dict = new OrderedDictionary<uint, int> ();
			((ICollection<KeyValuePair<uint, int>>)dict).Add (new KeyValuePair<uint, int> (1, 1));
			((ICollection<KeyValuePair<uint, int>>)dict).Add (new KeyValuePair<uint, int> (2, 2));
			((ICollection<KeyValuePair<uint, int>>)dict).Add (new KeyValuePair<uint, int> (3, 3));
			((ICollection<KeyValuePair<uint, int>>)dict).Remove (new KeyValuePair<uint, int> (2, 2));
			((ICollection<KeyValuePair<uint, int>>)dict).Add (new KeyValuePair<uint, int> (4, 4));

			Assert.AreEqual (3, dict.Count);
			Assert.AreEqual (dict[0], 1);
			Assert.AreEqual (dict[1], 3);
			Assert.AreEqual (dict[2], 4);
		}

		[Test]
		public void Values_Add ()
		{
			var dict = new OrderedDictionary<string, int> ();
			Assert.Throws<NotSupportedException> (() => dict.Values.Add (1));
		}

		[Test]
		public void Insert ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("1", 2);
			dict.Add ("3", 4);

			dict.Insert (1, "2", 3);

			Assert.AreEqual (dict[0], 2);
			Assert.AreEqual (dict[1], 3);
			Assert.AreEqual (dict[2], 4);
		}

		[Test]
		public void Insert_KeyExists ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("1", 2);
			dict.Add ("3", 4);

			Assert.Throws<ArgumentException> (() => dict.Insert (1, "3", 3));
		}

		[Test]
		public void KVP_Insert ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("1", 2);
			dict.Add ("3", 4);

			((IList<KeyValuePair<string, int>>)dict).Insert (1,
				new KeyValuePair<string, int> ("2", 3));

			Assert.AreEqual (dict[0], 2);
			Assert.AreEqual (dict[1], 3);
			Assert.AreEqual (dict[2], 4);
		}

		[Test]
		public void Values_Insert ()
		{
			var dict = new OrderedDictionary<string, int> ();
			Assert.Throws<NotSupportedException> (() => ((IList<int>) dict.Values).Insert (1, 1));
		}

		[Test]
		public void Remove ()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.IsTrue (dict.Remove ("2"));
			Assert.IsFalse (dict.ContainsKey ("2"));
			Assert.IsFalse (dict.Values.Contains (3));
			Assert.AreEqual (dict[1], 4);

			Assert.IsFalse (dict.Remove ("2"));
		}

		[Test]
		public void Remove_KeyNull ()
		{
			var dict = new OrderedDictionary<string, int> ();
			Assert.Throws<ArgumentNullException> (() => dict.Remove (null));
		}

		[Test]
		public void KVP_Remove ()
		{
			var dict = new OrderedDictionary<string, string> ();
			dict.Add ("foo", "bar");

			var kvp = new KeyValuePair<string, string> ("foo", "bar");
			Assert.IsTrue (((ICollection<KeyValuePair<string, string>>)dict).Remove (kvp));
			Assert.AreEqual (0, dict.Count);
		}

		[Test]
		public void Values_Remove ()
		{
			var dict = new OrderedDictionary<string, int> ();
			Assert.Throws<NotSupportedException> (() => dict.Values.Remove (1));
		}

		[Test]
		public void ContainsKey ()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.IsFalse (dict.ContainsKey ("0"));
			Assert.IsTrue (dict.ContainsKey ("1"));
			Assert.IsTrue (dict.ContainsKey ("2"));
			Assert.IsTrue (dict.ContainsKey ("3"));
			Assert.IsFalse (dict.ContainsKey ("4"));
		}

		[Test]
		public void ContainsKey_KeyNull ()
		{
			var dict = new OrderedDictionary<string, int> ();
			Assert.Throws<ArgumentNullException> (() => dict.ContainsKey (null));
		}

		[Test]
		public void ContainsValue ()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.IsFalse (dict.ContainsValue (1));
			Assert.IsTrue (dict.ContainsValue (2));
			Assert.IsTrue (dict.ContainsValue (3));
			Assert.IsTrue (dict.ContainsValue (4));
			Assert.IsFalse (dict.ContainsValue (5));
		}

		[Test]
		public void KVP_Contains ()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };
			var co = (ICollection<KeyValuePair<string, int>>)dict;

			Assert.IsFalse (co.Contains (new KeyValuePair<string, int> ("0", 1)));
			Assert.IsTrue (co.Contains (new KeyValuePair<string, int> ("1", 2)));
			Assert.IsTrue (co.Contains (new KeyValuePair<string, int> ("2", 3)));
			Assert.IsTrue (co.Contains (new KeyValuePair<string, int> ("3", 4)));
			Assert.IsFalse (co.Contains (new KeyValuePair<string, int> ("4", 5)));
		}

		[Test]
		public void Values_Contains ()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.IsFalse (dict.Values.Contains (1));
			Assert.IsTrue (dict.Values.Contains (2));
			Assert.IsTrue (dict.Values.Contains (3));
			Assert.IsTrue (dict.Values.Contains (4));
			Assert.IsFalse (dict.Values.Contains (5));
		}

		[Test]
		public void Count ()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.AreEqual (3, dict.Count);

			dict.Add ("4", 5);
			dict.Add ("5", 6);

			Assert.AreEqual (5, dict.Count);

			dict.Clear ();

			Assert.AreEqual (0, dict.Count);
		}

		[Test]
		public void Values_Count ()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.AreEqual (3, dict.Values.Count);

			dict.Add ("4", 5);
			dict.Add ("5", 6);

			Assert.AreEqual (5, dict.Values.Count);

			dict.Clear ();

			Assert.AreEqual (0, dict.Values.Count);
		}

		[Test]
		public void TryGetValue_NullKey ()
		{
			var dict = new OrderedDictionary<string, int> ();

			int i;
			Assert.Throws<ArgumentNullException> (() => dict.TryGetValue (null, out i));
		}

		[Test]
		public void TryGetValue ()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			int v;
			Assert.IsTrue (dict.TryGetValue ("1", out v));
			Assert.AreEqual (2, v);
		}

		[Test]
		public void TryGetValue_NotFound ()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			int v;
			Assert.IsFalse (dict.TryGetValue ("4", out v));
		}

		[Test]
		public void RemoveAt ()
		{
			var dict = new OrderedDictionary<uint, int> ();
			dict.Add (1, 1);
			dict.Add (2, 2);
			dict.Add (3, 3);
			dict.Remove (2);
			dict.Add (4, 4);

			dict.RemoveAt (1);

			Assert.AreEqual (1, dict[(int)0]);
			Assert.AreEqual (4, dict[(int)1]);
		}

		[Test]
		public void RemoveAt_IndexOutOfRangeLower ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);

			Assert.Throws<ArgumentOutOfRangeException> (() => dict.RemoveAt (-1));
		}

		[Test]
		public void Values_RemoveAt ()
		{
			var dict = new OrderedDictionary<string, int> ();

			Assert.Throws<NotSupportedException> (() => ((IList<int>) dict.Values).RemoveAt (0));
		}

		[Test]
		public void IndexOf ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);

			Assert.AreEqual (1, dict.IndexOf ("bar"));
		}

		[Test]
		public void IndexOf_NotFound ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);

			Assert.AreEqual (-1, dict.IndexOf ("baz"));
		}

		[Test]
		public void IndexOf_KeyNull ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);

			Assert.Throws<ArgumentNullException> (() => dict.IndexOf (null));
		}

		[Test]
		public void IndexOf_StartIndex ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.AreEqual (2, dict.IndexOf ("baz", 1));
			Assert.AreEqual (2, dict.IndexOf ("baz", 2));
		}

		[Test]
		public void IndexOf_StartIndex_NotFound ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.AreEqual (-1, dict.IndexOf ("asdf", 2));
			Assert.AreEqual (-1, dict.IndexOf ("bar", 2));
		}

		[Test]
		public void IndexOf_StartIndex_KeyNull ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.Throws<ArgumentNullException> (() => dict.IndexOf (null, 1));
		}

		[Test]
		public void IndexOf_StartIndex_IndexOutOfRangeLower ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.Throws<ArgumentOutOfRangeException> (() => dict.IndexOf ("monkeys", -1));
		}

		[Test]
		public void IndexOf_StartIndex_IndexOutOfRangeUpper ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.Throws<ArgumentOutOfRangeException> (() => dict.IndexOf ("monkeys", 5));
		}

		[Test]
		public void IndexOf_StartIndexAndCount ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.AreEqual (1, dict.IndexOf ("bar", 1, 1));
			Assert.AreEqual (2, dict.IndexOf ("baz", 0, 3));
		}

		[Test]
		public void IndexOf_StartIndexAndCount_NotFound ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.AreEqual (-1, dict.IndexOf ("bar", 2, 1));
			Assert.AreEqual (-1, dict.IndexOf ("baz", 0, 2));
		}

		[Test]
		public void IndexOf_StartIndexAndCount_KeyNull ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.Throws<ArgumentNullException> (() => dict.IndexOf (null, 1, 2));
		}

		[Test]
		public void IndexOf_StartIndexAndCount_IndexOutOfRangeLower ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.Throws<ArgumentOutOfRangeException> (() => dict.IndexOf ("monkeys", -1, 1));
		}

		[Test]
		public void IndexOf_StartIndexAndCount_IndexOutOfRangeUpper ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.Throws<ArgumentOutOfRangeException> (() => dict.IndexOf ("monkeys", 5, 1));
		}

		[Test]
		public void IndexOf_StartIndexAndCount_CountOutOfRange ()
		{
			var dict = new OrderedDictionary<string, int> ();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.Throws<ArgumentOutOfRangeException> (() => dict.IndexOf ("monkeys", 2, 3));
		}
	}

	[TestFixture]
	public class OrderedDictionaryListContractTests : ListContract<KeyValuePair<string, string>>
	{
		protected override ICollection<KeyValuePair<string, string>> CreateCollection (IEnumerable<KeyValuePair<string, string>> values)
		{
			var d = new OrderedDictionary<string, string> ();
			foreach (var v in values)
				d.Add (v.Key, v.Value);
			return d;
		}

		protected override KeyValuePair<string, string> CreateValueA ()
		{
			return new KeyValuePair<string, string> ("A", "1");
		}

		protected override KeyValuePair<string, string> CreateValueB ()
		{
			return new KeyValuePair<string, string> ("B", "2");
		}

		protected override KeyValuePair<string, string> CreateValueC ()
		{
			return new KeyValuePair<string, string> ("C", "3");
		}
	}

	[TestFixture]
	public class OrderedDictionaryDictionaryContractTests : DictionaryContract
	{
		protected override IDictionary<string, string> CreateDictionary (IEnumerable<KeyValuePair<string, string>> values)
		{
			var d = new OrderedDictionary<string, string> ();
			foreach (var v in values)
				d.Add (v.Key, v.Value);
			return d;
		}
	}

//
// IEnumerableContract.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

	public abstract class DictionaryContract
	{

		protected abstract IDictionary<string, string> CreateDictionary (IEnumerable<KeyValuePair<string, string>> values);

		[Test]
		public void Add ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string>[0]);

			var n = d.Count;
			Assert.AreEqual (n, d.Keys.Count);
			Assert.AreEqual (n, d.Values.Count);

			// key cannot be null
			try {
				d.Add ("key", "value");
				Assert.IsTrue (d.ContainsKey ("key"));
				Assert.IsFalse (d.ContainsKey ("value"));
				Assert.AreEqual (n + 1, d.Keys.Count);
				Assert.AreEqual (n + 1, d.Values.Count);
				Assert.IsTrue (d.Keys.Contains ("key"));
				Assert.IsTrue (d.Values.Contains ("value"));

				// Cannot use Add() w/ the same key
				Assert.Throws<ArgumentException> (() => d.Add ("key", "value2"));

				Assert.Throws<ArgumentNullException> (() => d.Add (null, null));
			} catch (NotSupportedException) {
				Assert.IsTrue (d.IsReadOnly);
			}
		}

		[Test]
		public void ContainsKey ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string>[]{
				new KeyValuePair<string, string> ("another-key", "another-value"),
			});
			Assert.Throws<ArgumentNullException> (() => d.ContainsKey (null));
			Assert.IsFalse (d.ContainsKey ("key"));
			Assert.IsTrue (d.ContainsKey ("another-key"));
			Assert.IsTrue (d.Keys.Contains ("another-key"));
		}

		[Test]
		public void Remove ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string>[]{
				new KeyValuePair<string, string> ("another-key", "another-value"),
			});
			var n = d.Count;
			try {
				Assert.IsFalse (d.Remove ("key"));
				Assert.AreEqual (n, d.Count);
				Assert.IsTrue (d.Remove ("another-key"));
				Assert.AreEqual (n - 1, d.Count);
				Assert.AreEqual (n - 1, d.Keys.Count);
				Assert.AreEqual (n - 1, d.Values.Count);
				Assert.IsFalse (d.Keys.Contains ("another-key"));
				Assert.IsFalse (d.Values.Contains ("another-value"));

				Assert.Throws<ArgumentNullException> (() => d.Remove (null));
			} catch (NotSupportedException) {
				Assert.IsTrue (d.IsReadOnly);
			}
		}

		[Test]
		public void TryGetValue ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string>[]{
				new KeyValuePair<string, string> ("key", "value"),
			});
			string v = null;
			Assert.Throws<ArgumentNullException> (() => d.TryGetValue (null, out v));
			Assert.IsFalse (d.TryGetValue ("another-key", out v));
			Assert.IsTrue (d.TryGetValue ("key", out v));
			Assert.AreEqual ("value", v);
		}

		[Test]
		public void Item ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string>[]{
				new KeyValuePair<string, string> ("key", "value"),
			});
#pragma warning disable 0168
			Assert.Throws<ArgumentNullException> (() => { var _ = d[null]; });
			Assert.Throws<KeyNotFoundException> (() => { var _ = d["another-key"]; });
#pragma warning restore
			try {
				d["key"] = "another-value";
				Assert.IsFalse (d.Values.Contains ("value"));
				Assert.IsTrue (d.Values.Contains ("another-value"));
				Assert.AreEqual ("another-value", d["key"]);
				Assert.AreEqual (1, d.Keys.Count);
				Assert.AreEqual (1, d.Values.Count);
			} catch (NotSupportedException) {
				Assert.IsTrue (d.IsReadOnly);
			}
		}

		[Test]
		public void Keys_And_Values_Order_Must_Match ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string>[]{
				new KeyValuePair<string, string> ("a", "1"),
				new KeyValuePair<string, string> ("b", "2"),
				new KeyValuePair<string, string> ("c", "3"),
			});
			Assert.AreEqual (IndexOf (d.Keys, "a"), IndexOf (d.Values, "1"));
			Assert.AreEqual (IndexOf (d.Keys, "b"), IndexOf (d.Values, "2"));
			Assert.AreEqual (IndexOf (d.Keys, "c"), IndexOf (d.Values, "3"));
		}

		private int IndexOf<T> (IEnumerable<T> items, T search)
		{
			int i = 0;
			foreach (T element in items) {
				if (Equals (element, search))
					return i;

				i++;
			}

			return -1;
		}
	}

	[TestFixture]
	public class OrderedDictionaryKeysTests
		: SubCollectionContract
	{
		protected override ICollection<string> CreateCollection (IEnumerable<string> values)
		{
			var d = new OrderedDictionary<string, string> ();
			foreach (var v in values.Select (v => new KeyValuePair<string, string> (v, v)))
				d.Add (v.Key, v.Value);

			var c = d.Keys;
			Assert.IsTrue (c.IsReadOnly);
			return c;
		}
	}

	[TestFixture]
	public class OrderedDictionaryValuesTests
		: SubCollectionContract
	{
		protected override ICollection<string> CreateCollection (IEnumerable<string> values)
		{
			var d = new OrderedDictionary<string, string> ();
			foreach (var v in values.Select (v => new KeyValuePair<string, string> (v, v)))
				d.Add (v.Key, v.Value);

			var c = d.Values;
			Assert.IsTrue (c.IsReadOnly);
			return c;
		}
	}

	public abstract class SubCollectionContract
		: CollectionContract<string>
	{
		protected override string CreateValueA ()
		{
			return "A";
		}

		protected override string CreateValueB ()
		{
			return "B";
		}

		protected override string CreateValueC ()
		{
			return "C";
		}
	}

//
// IListContract.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


	public abstract class ListContract<T> : CollectionContract<T>
	{

		private IList<T> CreateList (IEnumerable<T> values)
		{
			return (IList<T>)CreateCollection (values);
		}

		[Test]
		public void IndexOf ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();

			var list = CreateList (new T[0]);

			Assert.AreEqual (-1, list.IndexOf (a));

			try {
				list.Add (a);
				Assert.AreEqual (0, list.IndexOf (a));

				list.Add (b);
				Assert.AreEqual (1, list.IndexOf (b));

				list.Remove (a);
				Assert.AreEqual (-1, list.IndexOf (a));
				Assert.AreEqual (0, list.IndexOf (b));

				list.Remove (b);
				Assert.AreEqual (-1, list.IndexOf (b));
			} catch (NotSupportedException) {
				Assert.IsTrue (list.IsReadOnly);
			}
		}

		[Test]
		public void Insert ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();

			var list = CreateList (new T[0]);

			try {
				Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, a));
				Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (1, a));

				list.Insert (0, a);
				Assert.AreEqual (0, list.IndexOf (a));

				list.Insert (0, b);
				Assert.AreEqual (2, list.Count);
				Assert.AreEqual (0, list.IndexOf (b));
				Assert.AreEqual (1, list.IndexOf (a));
			} catch (NotSupportedException) {
				Assert.IsTrue (list.IsReadOnly);
			}
		}

		[Test]
		public void RemoveAt ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();

			var list = CreateList (new T[0]);

			try {
				Assert.Throws<ArgumentOutOfRangeException> (() => list.RemoveAt (-1));
				Assert.Throws<ArgumentOutOfRangeException> (() => list.RemoveAt (0));

				list.Add (a);
				Assert.AreEqual (1, list.Count);

				list.RemoveAt (0);
				Assert.AreEqual (0, list.Count);

				list.Add (a);
				list.Add (b);
				list.RemoveAt (0);
				Assert.AreEqual (1, list.Count);
				Assert.AreEqual (0, list.IndexOf (b));
			} catch (NotSupportedException) {
				Assert.IsTrue (list.IsReadOnly);
			}
		}

		[Test]
		public void Item ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();

			var list = CreateList (new[] { a });

			Assert.AreEqual (a, list[0]);
			Assert.Throws<ArgumentOutOfRangeException> (() => list[-1].ToString());

			try {
				Assert.Throws<ArgumentOutOfRangeException> (() => list[-1] = a);
				Assert.Throws<ArgumentOutOfRangeException> (() => list[1] = a);

				list[0] = b;
				Assert.AreEqual (-1, list.IndexOf (a));
				Assert.AreEqual (0, list.IndexOf (b));
			} catch (NotSupportedException) {
				Assert.IsTrue (list.IsReadOnly);
			}
		}
	}

//
// IEnumerableContract.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

	// NOTE:  when adding new tests to this type, add them to the
	//        RunAllTests() method as well.
	//        RunAllTests() is used by IDictionaryContract<T>.Keys()/.Values()
	//        to test the behavior of the .Keys/.Values read-only collections.
	//
	// NOTE:  No test may use [ExpectedException]; use Assert.Throws<T> instead.
	public abstract class CollectionContract<T>
	{

		protected abstract ICollection<T> CreateCollection (IEnumerable<T> values);
		protected abstract T CreateValueA ();
		protected abstract T CreateValueB ();
		protected abstract T CreateValueC ();


		[Test]
		public void Ctor_Initial_Count_Is_Zero ()
		{
			var c = CreateCollection (new T[0]);
			Assert.AreEqual (0, c.Count);
		}

		[Test]
		public void Ctor_CopySequence ()
		{
			var c = CreateCollection (new[] { CreateValueA (), CreateValueB (), CreateValueC () });
			Assert.AreEqual (3, c.Count);
		}

		[Test]
		public void Add ()
		{
			var c = CreateCollection (new T[0]);
			var n = c.Count;
			try {
				c.Add (CreateValueA ());
				Assert.AreEqual (n + 1, c.Count);
			} catch (NotSupportedException) {
				Assert.IsTrue (c.IsReadOnly);
			}
		}

		[Test]
		public void Clear ()
		{
			var c = CreateCollection (new[] { CreateValueA () });
			try {
				c.Clear ();
				Assert.AreEqual (0, c.Count);
			} catch (NotSupportedException) {
				Assert.IsTrue (c.IsReadOnly);
			}
		}

		[Test]
		public void Contains ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();

			var c = CreateCollection (new[] { a, b });
			Assert.IsTrue (c.Contains (a));
			Assert.IsTrue (c.Contains (b));
			Assert.IsFalse (c.Contains (CreateValueC ()));
		}

		[Test]
		public void CopyTo_Exceptions ()
		{
			var c = CreateCollection (new[] { CreateValueA (), CreateValueB (), CreateValueC () });
			Assert.Throws<ArgumentNullException> (() => c.CopyTo (null, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => c.CopyTo (new T[3], -1));
			var d = new T[5];
			// not enough space from d[3..d.Length-1] to hold c.Count elements.
			Assert.Throws<ArgumentException> (() => c.CopyTo (d, 3));
			Assert.Throws<ArgumentException> (() => c.CopyTo (new T[0], 0));
		}

		// can fail for IDictionary<TKey,TValue> implementations; override if appropriate.
		[Test]
		public virtual void CopyTo_SequenceComparison ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();
			var c = CreateValueC ();

			var coll = CreateCollection (new[] { a, b, c });
			var d = new T[5];
			coll.CopyTo (d, 1);
			Assert.IsTrue (new[]{
				default (T), a, b, c, default (T),
			}.SequenceEqual (d));
		}

		[Test]
		public void CopyTo ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();
			var c = CreateValueC ();

			var coll = CreateCollection (new[] { a, b, c });
			var d = new T[5];
			coll.CopyTo (d, 1);
			Assert.IsTrue (Array.IndexOf (d, a) >= 0);
			Assert.IsTrue (Array.IndexOf (d, b) >= 0);
			Assert.IsTrue (Array.IndexOf (d, c) >= 0);
		}

		[Test]
		public void Remove ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();
			var c = CreateValueC ();

			var coll = CreateCollection (new[] { a, b });
			int n = coll.Count;
			try {
				Assert.IsFalse (coll.Remove (c));
				Assert.AreEqual (n, coll.Count);
				Assert.IsTrue (coll.Remove (a));
				Assert.AreEqual (n - 1, coll.Count);
			} catch (NotSupportedException) {
				Assert.IsTrue (coll.IsReadOnly);
			}
		}
	}
}
