﻿using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections
{
	internal interface ISet
		: ICollection
	{
		void Add(object o);
		void AddAll(IEnumerable e);
		void Clear();
		bool Contains(object o);
		bool IsEmpty { get; }
		bool IsFixedSize { get; }
		bool IsReadOnly { get; }
		void Remove(object o);
		void RemoveAll(IEnumerable e);
	}
}
