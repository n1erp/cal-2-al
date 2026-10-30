using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class AbstractLookupSymbolsInfo<TSymbol> where TSymbol : class, ISymbol
{
	public struct ArityEnumerator : IEnumerator<int>, IEnumerator, IDisposable
	{
		private int _current;

		private readonly int _low32bits;

		private int[] _arities;

		private const int resetValue = -1;

		private const int reachedEndValue = int.MaxValue;

		public int Current => _current;

		object IEnumerator.Current => _current;

		internal ArityEnumerator(int bitVector, HashSet<int> arities)
		{
			_current = -1;
			_low32bits = bitVector;
			if (arities == null)
			{
				_arities = null;
				return;
			}
			_arities = arities.ToArray();
			Array.Sort(_arities);
		}

		public void Dispose()
		{
			_arities = null;
		}

		public bool MoveNext()
		{
			if (_current == int.MaxValue)
			{
				return false;
			}
			int i;
			for (i = ++_current; i < 32; i++)
			{
				if (((_low32bits >> i) & 1) != 0)
				{
					_current = i;
					return true;
				}
			}
			if (_arities != null)
			{
				int num = _arities.BinarySearch(i);
				if (num < 0)
				{
					num = ~num;
				}
				if (num < _arities.Length)
				{
					_current = _arities[num];
					return true;
				}
			}
			_current = int.MaxValue;
			return false;
		}

		public void Reset()
		{
			_current = -1;
		}
	}

	public interface IArityEnumerable
	{
		int Count { get; }

		ArityEnumerator GetEnumerator();
	}

	private struct UniqueSymbolOrArities(int arity, TSymbol uniqueSymbol) : IArityEnumerable
	{
		private object _uniqueSymbolOrArities = uniqueSymbol;

		private int _arityBitVectorOrUniqueArity = arity;

		private bool HasUniqueSymbol
		{
			get
			{
				if (_uniqueSymbolOrArities != null)
				{
					return !(_uniqueSymbolOrArities is HashSet<int>);
				}
				return false;
			}
		}

		public int Count
		{
			get
			{
				int num = BitArithmeticUtilities.CountBits(_arityBitVectorOrUniqueArity);
				HashSet<int> hashSet = (HashSet<int>)_uniqueSymbolOrArities;
				if (hashSet != null)
				{
					num += hashSet.Count;
				}
				return num;
			}
		}

		public void AddSymbol(TSymbol symbol, int arity)
		{
			if (symbol == null || symbol != _uniqueSymbolOrArities)
			{
				if (HasUniqueSymbol)
				{
					_uniqueSymbolOrArities = null;
					int arityBitVectorOrUniqueArity = _arityBitVectorOrUniqueArity;
					_arityBitVectorOrUniqueArity = 0;
					AddArity(arityBitVectorOrUniqueArity);
				}
				AddArity(arity);
			}
		}

		private void AddArity(int arity)
		{
			if (arity < 32)
			{
				int num = 1 << arity;
				_arityBitVectorOrUniqueArity |= num;
				return;
			}
			HashSet<int> hashSet = _uniqueSymbolOrArities as HashSet<int>;
			if (hashSet == null)
			{
				hashSet = (HashSet<int>)(_uniqueSymbolOrArities = new HashSet<int>());
			}
			hashSet.Add(arity);
		}

		public void GetUniqueSymbolOrArities(out IArityEnumerable arities, out TSymbol uniqueSymbol)
		{
			if (HasUniqueSymbol)
			{
				arities = null;
				uniqueSymbol = (TSymbol)_uniqueSymbolOrArities;
				return;
			}
			object obj;
			if (_uniqueSymbolOrArities != null || _arityBitVectorOrUniqueArity != 0)
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
			return new ArityEnumerator(_arityBitVectorOrUniqueArity, (HashSet<int>)_uniqueSymbolOrArities);
		}
	}

	private readonly IEqualityComparer<string> _comparer;

	private readonly Dictionary<string, UniqueSymbolOrArities> _nameMap;

	internal string FilterName { get; set; }

	public ICollection<string> Names => _nameMap.Keys;

	public int Count => _nameMap.Count;

	protected AbstractLookupSymbolsInfo(IEqualityComparer<string> comparer)
	{
		_comparer = comparer;
		_nameMap = new Dictionary<string, UniqueSymbolOrArities>(comparer);
	}

	public bool CanBeAdded(string name)
	{
		if (FilterName != null)
		{
			return _comparer.Equals(name, FilterName);
		}
		return true;
	}

	public void AddSymbol(TSymbol symbol, string name, int arity)
	{
		if (!_nameMap.TryGetValue(name, out var value))
		{
			value = new UniqueSymbolOrArities(arity, symbol);
			_nameMap.Add(name, value);
		}
		else
		{
			value.AddSymbol(symbol, arity);
			_nameMap[name] = value;
		}
	}

	public bool TryGetAritiesAndUniqueSymbol(string name, out IArityEnumerable arities, out TSymbol uniqueSymbol)
	{
		if (!_nameMap.TryGetValue(name, out var value))
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
		_nameMap.Clear();
		FilterName = null;
	}
}
