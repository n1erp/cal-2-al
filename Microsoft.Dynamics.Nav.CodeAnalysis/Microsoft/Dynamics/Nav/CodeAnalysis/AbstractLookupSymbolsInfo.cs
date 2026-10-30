using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class AbstractLookupSymbolsInfo<TSymbol> where TSymbol : class, ISymbol
{
	public struct ArityEnumerator : IEnumerator<int>, IDisposable, IEnumerator
	{
		private int current;

		private readonly int low32bits;

		private int[] arities;

		private const int ResetValue = -1;

		private const int ReachedEndValue = int.MaxValue;

		public int Current => current;

		object IEnumerator.Current => current;

		internal ArityEnumerator(int bitVector, HashSet<int> arities)
		{
			current = -1;
			low32bits = bitVector;
			if (arities == null)
			{
				this.arities = null;
				return;
			}
			this.arities = arities.ToArray();
			Array.Sort(this.arities);
		}

		public void Dispose()
		{
			arities = null;
		}

		public bool MoveNext()
		{
			if (current == int.MaxValue)
			{
				return false;
			}
			int i;
			for (i = ++current; i < 32; i++)
			{
				if (((low32bits >> i) & 1) != 0)
				{
					current = i;
					return true;
				}
			}
			if (arities != null)
			{
				int num = Array.BinarySearch(arities, i);
				if (num < 0)
				{
					num = ~num;
				}
				if (num < arities.Length)
				{
					current = arities[num];
					return true;
				}
			}
			current = int.MaxValue;
			return false;
		}

		public void Reset()
		{
			current = -1;
		}
	}

	public interface IArityEnumerable
	{
		int Count { get; }

		ArityEnumerator GetEnumerator();
	}

	private struct UniqueSymbolOrArities : IArityEnumerable
	{
		private object uniqueSymbolOrArities;

		private int arityBitVectorOrUniqueArity;

		private bool HasUniqueSymbol
		{
			get
			{
				if (uniqueSymbolOrArities != null)
				{
					return !(uniqueSymbolOrArities is HashSet<int>);
				}
				return false;
			}
		}

		public int Count
		{
			get
			{
				DebugAssertHelper.Assert(!HasUniqueSymbol);
				int num = BitArithmeticUtilities.CountBits(arityBitVectorOrUniqueArity);
				HashSet<int> hashSet = (HashSet<int>)uniqueSymbolOrArities;
				if (hashSet != null)
				{
					num += hashSet.Count;
				}
				return num;
			}
		}

		internal TSymbol UniqueSymbol => uniqueSymbolOrArities as TSymbol;

		public UniqueSymbolOrArities(int arity, TSymbol uniqueSymbol)
		{
			uniqueSymbolOrArities = uniqueSymbol;
			arityBitVectorOrUniqueArity = arity;
			DebugAssertHelper.Assert(uniqueSymbol != null || arity == 0);
		}

		public void AddSymbol(TSymbol symbol, int arity)
		{
			if (symbol == uniqueSymbolOrArities)
			{
				DebugAssertHelper.Assert(arity == arityBitVectorOrUniqueArity);
				return;
			}
			if (HasUniqueSymbol)
			{
				DebugAssertHelper.Assert(uniqueSymbolOrArities is TSymbol);
				uniqueSymbolOrArities = null;
				int arity2 = arityBitVectorOrUniqueArity;
				arityBitVectorOrUniqueArity = 0;
				AddArity(arity2);
			}
			AddArity(arity);
		}

		private void AddArity(int arity)
		{
			DebugAssertHelper.Assert(!HasUniqueSymbol);
			if (arity < 32)
			{
				int num = 1 << arity;
				arityBitVectorOrUniqueArity |= num;
				return;
			}
			HashSet<int> hashSet = uniqueSymbolOrArities as HashSet<int>;
			if (hashSet == null)
			{
				hashSet = (HashSet<int>)(uniqueSymbolOrArities = new HashSet<int>());
			}
			hashSet.Add(arity);
		}

		public void GetUniqueSymbolOrArities(out IArityEnumerable arities, out TSymbol uniqueSymbol)
		{
			if (HasUniqueSymbol)
			{
				arities = null;
				uniqueSymbol = (TSymbol)uniqueSymbolOrArities;
				return;
			}
			object obj;
			if (uniqueSymbolOrArities != null || arityBitVectorOrUniqueArity != 0)
			{
				IArityEnumerable arityEnumerable = this;
				obj = arityEnumerable;
			}
			else
			{
				obj = null;
			}
			arities = (IArityEnumerable)obj;
			uniqueSymbol = null;
		}

		public ArityEnumerator GetEnumerator()
		{
			DebugAssertHelper.Assert(!HasUniqueSymbol);
			return new ArityEnumerator(arityBitVectorOrUniqueArity, (HashSet<int>)uniqueSymbolOrArities);
		}
	}

	private readonly Dictionary<string, UniqueSymbolOrArities> nameMap;

	public ICollection<string> Names => nameMap.Keys;

	internal IEnumerable<TSymbol> Symbols => nameMap.Values.Select((UniqueSymbolOrArities v) => v.UniqueSymbol);

	public int Count => nameMap.Count;

	protected AbstractLookupSymbolsInfo(IEqualityComparer<string> comparer)
	{
		nameMap = new Dictionary<string, UniqueSymbolOrArities>(comparer);
	}

	public void AddSymbol(TSymbol symbol, string name, int arity)
	{
		if (!nameMap.TryGetValue(name, out var value))
		{
			value = new UniqueSymbolOrArities(arity, symbol);
			nameMap.Add(name, value);
		}
		else
		{
			value.AddSymbol(symbol, arity);
			nameMap[name] = value;
		}
	}

	public bool TryGetAritiesAndUniqueSymbol(string name, out IArityEnumerable arities, out TSymbol uniqueSymbol)
	{
		if (!nameMap.TryGetValue(name, out var value))
		{
			arities = null;
			uniqueSymbol = null;
			return false;
		}
		value.GetUniqueSymbolOrArities(out arities, out uniqueSymbol);
		return true;
	}

	public void Clear()
	{
		nameMap.Clear();
	}
}
