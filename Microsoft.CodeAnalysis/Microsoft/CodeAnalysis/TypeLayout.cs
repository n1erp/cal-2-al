using System;
using System.Runtime.InteropServices;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal struct TypeLayout(LayoutKind kind, int size, byte alignment) : IEquatable<TypeLayout>
{
	private readonly byte _kind = (byte)(kind + 1);

	private readonly short _alignment = alignment;

	private readonly int _size = size;

	public LayoutKind Kind
	{
		get
		{
			if (_kind != 0)
			{
				return (LayoutKind)(_kind - 1);
			}
			return LayoutKind.Auto;
		}
	}

	public short Alignment => _alignment;

	public int Size => _size;

	public bool Equals(TypeLayout other)
	{
		if (_size == other._size && _alignment == other._alignment)
		{
			return _kind == other._kind;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is TypeLayout)
		{
			return Equals((TypeLayout)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Hash.Combine(Size, Alignment), _kind);
	}
}
