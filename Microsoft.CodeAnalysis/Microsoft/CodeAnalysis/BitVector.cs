using System;
using System.Collections.Generic;
using System.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal struct BitVector : IEquatable<BitVector>
{
	private const uint ZeroWord = 0u;

	private const int Log2BitsPerWord = 5;

	public const int BitsPerWord = 32;

	private static readonly uint[] s_emptyArray = Array.Empty<uint>();

	private static readonly BitVector s_nullValue = default;

	private static readonly BitVector s_emptyValue = new BitVector(0u, s_emptyArray, 0);

	private uint _bits0;

	private uint[] _bits;

	private int _capacity;

	public int Capacity => _capacity;

	public bool IsNull => _bits == null;

	public static BitVector Null => s_nullValue;

	public static BitVector Empty => s_emptyValue;

	public bool this[int index]
	{
		get
		{
			if (index >= _capacity)
			{
				return false;
			}
			int num = (index >> 5) - 1;
			return IsTrue((num < 0) ? _bits0 : _bits[num], index);
		}
		set
		{
			if (index >= _capacity)
			{
				EnsureCapacity(index + 1);
			}
			int num = (index >> 5) - 1;
			int num2 = index & 0x1F;
			uint num3 = (uint)(1 << num2);
			if (num < 0)
			{
				if (value)
				{
					_bits0 |= num3;
				}
				else
				{
					_bits0 &= ~num3;
				}
			}
			else if (value)
			{
				_bits[num] |= num3;
			}
			else
			{
				_bits[num] &= ~num3;
			}
		}
	}

	private BitVector(uint bits0, uint[] bits, int capacity)
	{
		WordsForCapacity(capacity);
		_bits0 = bits0;
		_bits = bits;
		_capacity = capacity;
	}

	public bool Equals(BitVector other)
	{
		if (_capacity == other._capacity && _bits0 == other._bits0)
		{
			return _bits.ValueEquals(other._bits);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BitVector)
		{
			return Equals((BitVector)obj);
		}
		return false;
	}

	public static bool operator ==(BitVector left, BitVector right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BitVector left, BitVector right)
	{
		return !left.Equals(right);
	}

	public override int GetHashCode()
	{
		int currentKey = _bits0.GetHashCode();
		if (_bits != null)
		{
			for (int i = 0; i < _bits.Length; i++)
			{
				currentKey = Hash.Combine(_bits[i].GetHashCode(), currentKey);
			}
		}
		return Hash.Combine(_capacity, currentKey);
	}

	private static int WordsForCapacity(int capacity)
	{
		if (capacity <= 0)
		{
			return 0;
		}
		return capacity - 1 >> 5;
	}

	[Conditional("DEBUG_BITARRAY")]
	private void Check()
	{
	}

	public void EnsureCapacity(int newCapacity)
	{
		if (newCapacity > _capacity)
		{
			int num = WordsForCapacity(newCapacity);
			if (num > _bits.Length)
			{
				Array.Resize(ref _bits, num);
			}
			_capacity = newCapacity;
		}
	}

	public IEnumerable<uint> Words()
	{
		if (_capacity > 0)
		{
			yield return _bits0;
		}
		for (int i = 0; i < _bits?.Length; i++)
		{
			yield return _bits[i];
		}
	}

	public IEnumerable<int> TrueBits()
	{
		if (_bits0 != 0)
		{
			for (int bit = 0; bit < 32; bit++)
			{
				uint num = (uint)(1 << bit);
				if ((_bits0 & num) != 0)
				{
					if (bit >= _capacity)
					{
						yield break;
					}
					yield return bit;
				}
			}
		}
		for (int bit = 0; bit < _bits.Length; bit++)
		{
			uint w = _bits[bit];
			if (w == 0)
			{
				continue;
			}
			for (int b = 0; b < 32; b++)
			{
				uint num2 = (uint)(1 << b);
				if ((w & num2) != 0)
				{
					int num3 = (bit + 1 << 5) | b;
					if (num3 >= _capacity)
					{
						yield break;
					}
					yield return num3;
				}
			}
		}
	}

	public static BitVector Create(int capacity)
	{
		int num = WordsForCapacity(capacity);
		uint[] bits = ((num == 0) ? s_emptyArray : new uint[num]);
		return new BitVector(0u, bits, capacity);
	}

	public static BitVector AllSet(int capacity)
	{
		if (capacity == 0)
		{
			return Empty;
		}
		int num = WordsForCapacity(capacity);
		uint[] array = ((num == 0) ? s_emptyArray : new uint[num]);
		int num2 = num - 1;
		uint bits = uint.MaxValue;
		for (int i = 0; i < num2; i++)
		{
			array[i] = uint.MaxValue;
		}
		int num3 = capacity & 0x1F;
		if (num3 > 0)
		{
			uint num4 = (uint)(~(-1 << num3));
			if (num2 < 0)
			{
				bits = num4;
			}
			else
			{
				array[num2] = num4;
			}
		}
		else if (num > 0)
		{
			array[num2] = uint.MaxValue;
		}
		return new BitVector(bits, array, capacity);
	}

	public BitVector Clone()
	{
		uint bits = _bits0;
		uint[] bits2;
		if (_bits == null)
		{
			bits2 = null;
		}
		else
		{
			bits2 = ((_bits.Length == 0) ? s_emptyArray : ((uint[])_bits.Clone()));
		}
		return new BitVector(bits, bits2, _capacity);
	}

	public bool IntersectWith(in BitVector other)
	{
		bool result = false;
		int num = other._bits.Length;
		uint[] bits = _bits;
		int num2 = bits.Length;
		if (num > num2)
		{
			num = num2;
		}
		uint bits2 = _bits0;
		uint num3 = bits2 & other._bits0;
		if (num3 != bits2)
		{
			_bits0 = num3;
			result = true;
		}
		for (int i = 0; i < num; i++)
		{
			uint num4 = bits[i];
			uint num5 = num4 & other._bits[i];
			if (num5 != num4)
			{
				bits[i] = num5;
				result = true;
			}
		}
		for (int j = num; j < num2; j++)
		{
			if (bits[j] != 0)
			{
				bits[j] = 0u;
				result = true;
			}
		}
		return result;
	}

	public bool UnionWith(in BitVector other)
	{
		bool result = false;
		if (other._capacity > _capacity)
		{
			EnsureCapacity(other._capacity);
		}
		uint bits = _bits0;
		_bits0 |= other._bits0;
		if (bits != _bits0)
		{
			result = true;
		}
		for (int i = 0; i < other._bits.Length; i++)
		{
			bits = _bits[i];
			_bits[i] |= other._bits[i];
			if (_bits[i] != bits)
			{
				result = true;
			}
		}
		return result;
	}

	public void Clear()
	{
		_bits0 = 0u;
		if (_bits != null)
		{
			Array.Clear(_bits, 0, _bits.Length);
		}
	}

	public static bool IsTrue(uint word, int index)
	{
		int num = index & 0x1F;
		uint num2 = (uint)(1 << num);
		return (word & num2) != 0;
	}

	public static int WordsRequired(int capacity)
	{
		if (capacity <= 0)
		{
			return 0;
		}
		return WordsForCapacity(capacity) + 1;
	}
}
