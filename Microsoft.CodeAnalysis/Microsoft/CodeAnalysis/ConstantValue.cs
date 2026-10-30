using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class ConstantValue : IEquatable<ConstantValue>
{
	private sealed class ConstantValueBad : ConstantValue
	{
		public static readonly ConstantValueBad Instance = new ConstantValueBad();

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.Bad;

		internal override SpecialType SpecialType => SpecialType.None;

		private ConstantValueBad()
		{
		}

		public override bool Equals(ConstantValue other)
		{
			return (object)this == other;
		}

		public override int GetHashCode()
		{
			return RuntimeHelpers.GetHashCode(this);
		}

		internal override string GetValueToDisplay()
		{
			return "bad";
		}
	}

	private sealed class ConstantValueNull : ConstantValue
	{
		public static readonly ConstantValueNull Instance = new ConstantValueNull();

		public static readonly ConstantValueNull Uninitialized = new ConstantValueNull();

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.Nothing;

		internal override SpecialType SpecialType => SpecialType.None;

		public override string StringValue => null;

		public override bool IsDefaultValue => true;

		private ConstantValueNull()
		{
		}

		public override bool Equals(ConstantValue other)
		{
			return (object)this == other;
		}

		public override int GetHashCode()
		{
			return RuntimeHelpers.GetHashCode(this);
		}

		internal override string GetValueToDisplay()
		{
			if ((object)this != Uninitialized)
			{
				return "null";
			}
			return "unset";
		}
	}

	private sealed class ConstantValueString : ConstantValue
	{
		private readonly string _value;

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.String;

		internal override SpecialType SpecialType => SpecialType.System_String;

		public override string StringValue => _value;

		public ConstantValueString(string value)
		{
			_value = value;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(base.GetHashCode(), _value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return _value == other.StringValue;
			}
			return false;
		}

		internal override string GetValueToDisplay()
		{
			if (_value != null)
			{
				return $"\"{_value}\"";
			}
			return "null";
		}
	}

	private sealed class ConstantValueDecimal : ConstantValue
	{
		private readonly decimal _value;

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.Decimal;

		internal override SpecialType SpecialType => SpecialType.System_Decimal;

		public override decimal DecimalValue => _value;

		public ConstantValueDecimal(decimal value)
		{
			_value = value;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			decimal value = _value;
			return Hash.Combine(hashCode, value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return _value == other.DecimalValue;
			}
			return false;
		}
	}

	private sealed class ConstantValueDateTime : ConstantValue
	{
		private readonly DateTime _value;

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.DateTime;

		internal override SpecialType SpecialType => SpecialType.System_DateTime;

		public override DateTime DateTimeValue => _value;

		public ConstantValueDateTime(DateTime value)
		{
			_value = value;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			DateTime value = _value;
			return Hash.Combine(hashCode, value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return _value == other.DateTimeValue;
			}
			return false;
		}
	}

	private abstract class ConstantValueDiscriminated : ConstantValue
	{
		private readonly ConstantValueTypeDiscriminator _discriminator;

		public override ConstantValueTypeDiscriminator Discriminator => _discriminator;

		internal override SpecialType SpecialType => GetSpecialType(_discriminator);

		public ConstantValueDiscriminated(ConstantValueTypeDiscriminator discriminator)
		{
			_discriminator = discriminator;
		}
	}

	private class ConstantValueDefault : ConstantValueDiscriminated
	{
		public static readonly ConstantValueDefault SByte = new ConstantValueDefault(ConstantValueTypeDiscriminator.SByte);

		public static readonly ConstantValueDefault Byte = new ConstantValueDefault(ConstantValueTypeDiscriminator.Byte);

		public static readonly ConstantValueDefault Int16 = new ConstantValueDefault(ConstantValueTypeDiscriminator.Int16);

		public static readonly ConstantValueDefault UInt16 = new ConstantValueDefault(ConstantValueTypeDiscriminator.UInt16);

		public static readonly ConstantValueDefault Int32 = new ConstantValueDefault(ConstantValueTypeDiscriminator.Int32);

		public static readonly ConstantValueDefault UInt32 = new ConstantValueDefault(ConstantValueTypeDiscriminator.UInt32);

		public static readonly ConstantValueDefault Int64 = new ConstantValueDefault(ConstantValueTypeDiscriminator.Int64);

		public static readonly ConstantValueDefault UInt64 = new ConstantValueDefault(ConstantValueTypeDiscriminator.UInt64);

		public static readonly ConstantValueDefault Char = new ConstantValueDefault(ConstantValueTypeDiscriminator.Char);

		public static readonly ConstantValueDefault Single = new ConstantValueSingleZero();

		public static readonly ConstantValueDefault Double = new ConstantValueDoubleZero();

		public static readonly ConstantValueDefault Decimal = new ConstantValueDecimalZero();

		public static readonly ConstantValueDefault DateTime = new ConstantValueDefault(ConstantValueTypeDiscriminator.DateTime);

		public static readonly ConstantValueDefault Boolean = new ConstantValueDefault(ConstantValueTypeDiscriminator.Boolean);

		public override byte ByteValue => 0;

		public override sbyte SByteValue => 0;

		public override bool BooleanValue => false;

		public override double DoubleValue => 0.0;

		public override float SingleValue => 0f;

		public override decimal DecimalValue => 0m;

		public override char CharValue => '\0';

		public override DateTime DateTimeValue => default;

		public override bool IsDefaultValue => true;

		protected ConstantValueDefault(ConstantValueTypeDiscriminator discriminator)
			: base(discriminator)
		{
		}

		public override bool Equals(ConstantValue other)
		{
			return (object)this == other;
		}

		public override int GetHashCode()
		{
			return RuntimeHelpers.GetHashCode(this);
		}
	}

	private sealed class ConstantValueDecimalZero : ConstantValueDefault
	{
		internal ConstantValueDecimalZero()
			: base(ConstantValueTypeDiscriminator.Decimal)
		{
		}

		public override bool Equals(ConstantValue other)
		{
			if ((object)other == this)
			{
				return true;
			}
			if ((object)other == null)
			{
				return false;
			}
			if (Discriminator == other.Discriminator)
			{
				return other.DecimalValue == 0m;
			}
			return false;
		}
	}

	private sealed class ConstantValueDoubleZero : ConstantValueDefault
	{
		internal ConstantValueDoubleZero()
			: base(ConstantValueTypeDiscriminator.Double)
		{
		}

		public override bool Equals(ConstantValue other)
		{
			if ((object)other == this)
			{
				return true;
			}
			if ((object)other == null)
			{
				return false;
			}
			if (Discriminator == other.Discriminator)
			{
				return other.DoubleValue == 0.0;
			}
			return false;
		}
	}

	private sealed class ConstantValueSingleZero : ConstantValueDefault
	{
		internal ConstantValueSingleZero()
			: base(ConstantValueTypeDiscriminator.Single)
		{
		}

		public override bool Equals(ConstantValue other)
		{
			if ((object)other == this)
			{
				return true;
			}
			if ((object)other == null)
			{
				return false;
			}
			if (Discriminator == other.Discriminator)
			{
				return other.SingleValue == 0f;
			}
			return false;
		}
	}

	private class ConstantValueOne : ConstantValueDiscriminated
	{
		public static readonly ConstantValueOne SByte = new ConstantValueOne(ConstantValueTypeDiscriminator.SByte);

		public static readonly ConstantValueOne Byte = new ConstantValueOne(ConstantValueTypeDiscriminator.Byte);

		public static readonly ConstantValueOne Int16 = new ConstantValueOne(ConstantValueTypeDiscriminator.Int16);

		public static readonly ConstantValueOne UInt16 = new ConstantValueOne(ConstantValueTypeDiscriminator.UInt16);

		public static readonly ConstantValueOne Int32 = new ConstantValueOne(ConstantValueTypeDiscriminator.Int32);

		public static readonly ConstantValueOne UInt32 = new ConstantValueOne(ConstantValueTypeDiscriminator.UInt32);

		public static readonly ConstantValueOne Int64 = new ConstantValueOne(ConstantValueTypeDiscriminator.Int64);

		public static readonly ConstantValueOne UInt64 = new ConstantValueOne(ConstantValueTypeDiscriminator.UInt64);

		public static readonly ConstantValueOne Single = new ConstantValueOne(ConstantValueTypeDiscriminator.Single);

		public static readonly ConstantValueOne Double = new ConstantValueOne(ConstantValueTypeDiscriminator.Double);

		public static readonly ConstantValueOne Decimal = new ConstantValueDecimalOne();

		public static readonly ConstantValueOne Boolean = new ConstantValueOne(ConstantValueTypeDiscriminator.Boolean);

		public override byte ByteValue => 1;

		public override sbyte SByteValue => 1;

		public override bool BooleanValue => true;

		public override double DoubleValue => 1.0;

		public override float SingleValue => 1f;

		public override decimal DecimalValue => 1m;

		protected ConstantValueOne(ConstantValueTypeDiscriminator discriminator)
			: base(discriminator)
		{
		}

		public override bool Equals(ConstantValue other)
		{
			return (object)this == other;
		}

		public override int GetHashCode()
		{
			return RuntimeHelpers.GetHashCode(this);
		}
	}

	private sealed class ConstantValueDecimalOne : ConstantValueOne
	{
		internal ConstantValueDecimalOne()
			: base(ConstantValueTypeDiscriminator.Decimal)
		{
		}

		public override bool Equals(ConstantValue other)
		{
			if ((object)other == this)
			{
				return true;
			}
			if ((object)other == null)
			{
				return false;
			}
			if (Discriminator == other.Discriminator)
			{
				return other.DecimalValue == 1m;
			}
			return false;
		}
	}

	private sealed class ConstantValueI8 : ConstantValueDiscriminated
	{
		private readonly byte _value;

		public override byte ByteValue => _value;

		public override sbyte SByteValue => (sbyte)_value;

		public ConstantValueI8(sbyte value)
			: base(ConstantValueTypeDiscriminator.SByte)
		{
			_value = (byte)value;
		}

		public ConstantValueI8(byte value)
			: base(ConstantValueTypeDiscriminator.Byte)
		{
			_value = value;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			byte value = _value;
			return Hash.Combine(hashCode, value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return _value == other.ByteValue;
			}
			return false;
		}
	}

	private sealed class ConstantValueI16 : ConstantValueDiscriminated
	{
		private readonly short _value;

		public override short Int16Value => _value;

		public override ushort UInt16Value => (ushort)_value;

		public override char CharValue => (char)_value;

		public ConstantValueI16(short value)
			: base(ConstantValueTypeDiscriminator.Int16)
		{
			_value = value;
		}

		public ConstantValueI16(ushort value)
			: base(ConstantValueTypeDiscriminator.UInt16)
		{
			_value = (short)value;
		}

		public ConstantValueI16(char value)
			: base(ConstantValueTypeDiscriminator.Char)
		{
			_value = (short)value;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			short value = _value;
			return Hash.Combine(hashCode, value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return _value == other.Int16Value;
			}
			return false;
		}
	}

	private sealed class ConstantValueI32 : ConstantValueDiscriminated
	{
		private readonly int _value;

		public override int Int32Value => _value;

		public override uint UInt32Value => (uint)_value;

		public ConstantValueI32(int value)
			: base(ConstantValueTypeDiscriminator.Int32)
		{
			_value = value;
		}

		public ConstantValueI32(uint value)
			: base(ConstantValueTypeDiscriminator.UInt32)
		{
			_value = (int)value;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			int value = _value;
			return Hash.Combine(hashCode, value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return _value == other.Int32Value;
			}
			return false;
		}
	}

	private sealed class ConstantValueI64 : ConstantValueDiscriminated
	{
		private readonly long _value;

		public override long Int64Value => _value;

		public override ulong UInt64Value => (ulong)_value;

		public ConstantValueI64(long value)
			: base(ConstantValueTypeDiscriminator.Int64)
		{
			_value = value;
		}

		public ConstantValueI64(ulong value)
			: base(ConstantValueTypeDiscriminator.UInt64)
		{
			_value = (long)value;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			long value = _value;
			return Hash.Combine(hashCode, value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return _value == other.Int64Value;
			}
			return false;
		}
	}

	private sealed class ConstantValueDouble : ConstantValueDiscriminated
	{
		private readonly double _value;

		public override double DoubleValue => _value;

		public ConstantValueDouble(double value)
			: base(ConstantValueTypeDiscriminator.Double)
		{
			_value = value;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			double value = _value;
			return Hash.Combine(hashCode, value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				double value = _value;
				return value.Equals(other.DoubleValue);
			}
			return false;
		}
	}

	private sealed class ConstantValueSingle : ConstantValueDiscriminated
	{
		private readonly double _value;

		public override double DoubleValue => _value;

		public override float SingleValue => (float)_value;

		public ConstantValueSingle(double value)
			: base(ConstantValueTypeDiscriminator.Single)
		{
			_value = value;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			double value = _value;
			return Hash.Combine(hashCode, value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				double value = _value;
				return value.Equals(other.DoubleValue);
			}
			return false;
		}
	}

	public const ConstantValue NotAvailable = null;

	public abstract ConstantValueTypeDiscriminator Discriminator { get; }

	internal abstract SpecialType SpecialType { get; }

	public virtual string StringValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual bool BooleanValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual sbyte SByteValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual byte ByteValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual short Int16Value => SByteValue;

	public virtual ushort UInt16Value => ByteValue;

	public virtual int Int32Value => Int16Value;

	public virtual uint UInt32Value => UInt16Value;

	public virtual long Int64Value => Int32Value;

	public virtual ulong UInt64Value => UInt32Value;

	public virtual char CharValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual decimal DecimalValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual DateTime DateTimeValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual double DoubleValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual float SingleValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual bool IsDefaultValue => false;

	public static ConstantValue Bad => ConstantValueBad.Instance;

	public static ConstantValue Null => ConstantValueNull.Instance;

	public static ConstantValue Nothing => Null;

	public static ConstantValue Unset => ConstantValueNull.Uninitialized;

	public static ConstantValue True => ConstantValueOne.Boolean;

	public static ConstantValue False => ConstantValueDefault.Boolean;

	public object Value => Discriminator switch
	{
		ConstantValueTypeDiscriminator.Bad => null, 
		ConstantValueTypeDiscriminator.Nothing => null, 
		ConstantValueTypeDiscriminator.SByte => Boxes.Box(SByteValue), 
		ConstantValueTypeDiscriminator.Byte => Boxes.Box(ByteValue), 
		ConstantValueTypeDiscriminator.Int16 => Boxes.Box(Int16Value), 
		ConstantValueTypeDiscriminator.UInt16 => Boxes.Box(UInt16Value), 
		ConstantValueTypeDiscriminator.Int32 => Boxes.Box(Int32Value), 
		ConstantValueTypeDiscriminator.UInt32 => Boxes.Box(UInt32Value), 
		ConstantValueTypeDiscriminator.Int64 => Boxes.Box(Int64Value), 
		ConstantValueTypeDiscriminator.UInt64 => Boxes.Box(UInt64Value), 
		ConstantValueTypeDiscriminator.Char => Boxes.Box(CharValue), 
		ConstantValueTypeDiscriminator.Boolean => Boxes.Box(BooleanValue), 
		ConstantValueTypeDiscriminator.Single => Boxes.Box(SingleValue), 
		ConstantValueTypeDiscriminator.Double => Boxes.Box(DoubleValue), 
		ConstantValueTypeDiscriminator.Decimal => Boxes.Box(DecimalValue), 
		ConstantValueTypeDiscriminator.DateTime => DateTimeValue, 
		ConstantValueTypeDiscriminator.String => StringValue, 
		_ => throw ExceptionUtilities.UnexpectedValue(Discriminator), 
	};

	public bool IsIntegral => IsIntegralType(Discriminator);

	public bool IsNegativeNumeric => Discriminator switch
	{
		ConstantValueTypeDiscriminator.SByte => SByteValue < 0, 
		ConstantValueTypeDiscriminator.Int16 => Int16Value < 0, 
		ConstantValueTypeDiscriminator.Int32 => Int32Value < 0, 
		ConstantValueTypeDiscriminator.Int64 => Int64Value < 0, 
		ConstantValueTypeDiscriminator.Single => SingleValue < 0f, 
		ConstantValueTypeDiscriminator.Double => DoubleValue < 0.0, 
		ConstantValueTypeDiscriminator.Decimal => DecimalValue < 0m, 
		_ => false, 
	};

	public bool IsNumeric
	{
		get
		{
			switch (Discriminator)
			{
			case ConstantValueTypeDiscriminator.SByte:
			case ConstantValueTypeDiscriminator.Byte:
			case ConstantValueTypeDiscriminator.Int16:
			case ConstantValueTypeDiscriminator.UInt16:
			case ConstantValueTypeDiscriminator.Int32:
			case ConstantValueTypeDiscriminator.UInt32:
			case ConstantValueTypeDiscriminator.Int64:
			case ConstantValueTypeDiscriminator.UInt64:
			case ConstantValueTypeDiscriminator.Single:
			case ConstantValueTypeDiscriminator.Double:
			case ConstantValueTypeDiscriminator.Decimal:
				return true;
			default:
				return false;
			}
		}
	}

	public bool IsUnsigned => IsUnsignedIntegralType(Discriminator);

	public bool IsBoolean => Discriminator == ConstantValueTypeDiscriminator.Boolean;

	public bool IsChar => Discriminator == ConstantValueTypeDiscriminator.Char;

	public bool IsString => Discriminator == ConstantValueTypeDiscriminator.String;

	public bool IsDecimal => Discriminator == ConstantValueTypeDiscriminator.Decimal;

	public bool IsDateTime => Discriminator == ConstantValueTypeDiscriminator.DateTime;

	public bool IsFloating
	{
		get
		{
			if (Discriminator != ConstantValueTypeDiscriminator.Double)
			{
				return Discriminator == ConstantValueTypeDiscriminator.Single;
			}
			return true;
		}
	}

	public bool IsBad => Discriminator == ConstantValueTypeDiscriminator.Bad;

	public bool IsNull => (object)this == Null;

	public bool IsNothing => (object)this == Nothing;

	public static ConstantValue Create(string value)
	{
		if (value == null)
		{
			return Null;
		}
		return new ConstantValueString(value);
	}

	public static ConstantValue Create(char value)
	{
		if (value == '\0')
		{
			return ConstantValueDefault.Char;
		}
		return new ConstantValueI16(value);
	}

	public static ConstantValue Create(sbyte value)
	{
		return value switch
		{
			0 => (ConstantValue)ConstantValueDefault.SByte, 
			1 => ConstantValueOne.SByte, 
			_ => new ConstantValueI8(value), 
		};
	}

	public static ConstantValue Create(byte value)
	{
		return value switch
		{
			0 => (ConstantValue)ConstantValueDefault.Byte, 
			1 => ConstantValueOne.Byte, 
			_ => new ConstantValueI8(value), 
		};
	}

	public static ConstantValue Create(short value)
	{
		return value switch
		{
			0 => (ConstantValue)ConstantValueDefault.Int16, 
			1 => ConstantValueOne.Int16, 
			_ => new ConstantValueI16(value), 
		};
	}

	public static ConstantValue Create(ushort value)
	{
		return value switch
		{
			0 => (ConstantValue)ConstantValueDefault.UInt16, 
			1 => ConstantValueOne.UInt16, 
			_ => new ConstantValueI16(value), 
		};
	}

	public static ConstantValue Create(int value)
	{
		return value switch
		{
			0 => (ConstantValue)ConstantValueDefault.Int32, 
			1 => ConstantValueOne.Int32, 
			_ => new ConstantValueI32(value), 
		};
	}

	public static ConstantValue Create(uint value)
	{
		return value switch
		{
			0u => (ConstantValue)ConstantValueDefault.UInt32, 
			1u => ConstantValueOne.UInt32, 
			_ => new ConstantValueI32(value), 
		};
	}

	public static ConstantValue Create(long value)
	{
		return value switch
		{
			0L => (ConstantValue)ConstantValueDefault.Int64, 
			1L => ConstantValueOne.Int64, 
			_ => new ConstantValueI64(value), 
		};
	}

	public static ConstantValue Create(ulong value)
	{
		return value switch
		{
			0uL => (ConstantValue)ConstantValueDefault.UInt64, 
			1uL => ConstantValueOne.UInt64, 
			_ => new ConstantValueI64(value), 
		};
	}

	public static ConstantValue Create(bool value)
	{
		if (value)
		{
			return ConstantValueOne.Boolean;
		}
		return ConstantValueDefault.Boolean;
	}

	public static ConstantValue Create(float value)
	{
		if (BitConverter.DoubleToInt64Bits(value) == 0L)
		{
			return ConstantValueDefault.Single;
		}
		if (value == 1f)
		{
			return ConstantValueOne.Single;
		}
		return new ConstantValueSingle(value);
	}

	public static ConstantValue CreateSingle(double value)
	{
		if (BitConverter.DoubleToInt64Bits(value) == 0L)
		{
			return ConstantValueDefault.Single;
		}
		if (value == 1.0)
		{
			return ConstantValueOne.Single;
		}
		return new ConstantValueSingle(value);
	}

	public static ConstantValue Create(double value)
	{
		if (BitConverter.DoubleToInt64Bits(value) == 0L)
		{
			return ConstantValueDefault.Double;
		}
		if (value == 1.0)
		{
			return ConstantValueOne.Double;
		}
		return new ConstantValueDouble(value);
	}

	public static ConstantValue Create(decimal value)
	{
		if (decimal.GetBits(value)[3] == 0)
		{
			if (value == 0m)
			{
				return ConstantValueDefault.Decimal;
			}
			if (value == 1m)
			{
				return ConstantValueOne.Decimal;
			}
		}
		return new ConstantValueDecimal(value);
	}

	public static ConstantValue Create(DateTime value)
	{
		if (value == default(DateTime))
		{
			return ConstantValueDefault.DateTime;
		}
		return new ConstantValueDateTime(value);
	}

	public static ConstantValue Create(object value, SpecialType st)
	{
		ConstantValueTypeDiscriminator discriminator = GetDiscriminator(st);
		return Create(value, discriminator);
	}

	public static ConstantValue CreateSizeOf(SpecialType st)
	{
		int num = st.SizeInBytes();
		if (num != 0)
		{
			return Create(num);
		}
		return null;
	}

	public static ConstantValue Create(object value, ConstantValueTypeDiscriminator discriminator)
	{
		switch (discriminator)
		{
		case ConstantValueTypeDiscriminator.Nothing:
			return Null;
		case ConstantValueTypeDiscriminator.SByte:
			return Create((sbyte)value);
		case ConstantValueTypeDiscriminator.Byte:
			return Create((byte)value);
		case ConstantValueTypeDiscriminator.Int16:
			return Create((short)value);
		case ConstantValueTypeDiscriminator.UInt16:
			return Create((ushort)value);
		case ConstantValueTypeDiscriminator.Int32:
			return Create((int)value);
		case ConstantValueTypeDiscriminator.UInt32:
			return Create((uint)value);
		case ConstantValueTypeDiscriminator.Int64:
			return Create((long)value);
		case ConstantValueTypeDiscriminator.UInt64:
			return Create((ulong)value);
		case ConstantValueTypeDiscriminator.Char:
			return Create((char)value);
		case ConstantValueTypeDiscriminator.Boolean:
			return Create((bool)value);
		case ConstantValueTypeDiscriminator.Single:
			if (!(value is double))
			{
				return Create((float)value);
			}
			return CreateSingle((double)value);
		case ConstantValueTypeDiscriminator.Double:
			return Create((double)value);
		case ConstantValueTypeDiscriminator.Decimal:
			return Create((decimal)value);
		case ConstantValueTypeDiscriminator.DateTime:
			return Create((DateTime)value);
		case ConstantValueTypeDiscriminator.String:
			return Create((string)value);
		default:
			throw new InvalidOperationException();
		}
	}

	public static ConstantValue Default(SpecialType st)
	{
		return Default(GetDiscriminator(st));
	}

	public static ConstantValue Default(ConstantValueTypeDiscriminator discriminator)
	{
		switch (discriminator)
		{
		case ConstantValueTypeDiscriminator.Bad:
			return Bad;
		case ConstantValueTypeDiscriminator.SByte:
			return ConstantValueDefault.SByte;
		case ConstantValueTypeDiscriminator.Byte:
			return ConstantValueDefault.Byte;
		case ConstantValueTypeDiscriminator.Int16:
			return ConstantValueDefault.Int16;
		case ConstantValueTypeDiscriminator.UInt16:
			return ConstantValueDefault.UInt16;
		case ConstantValueTypeDiscriminator.Int32:
			return ConstantValueDefault.Int32;
		case ConstantValueTypeDiscriminator.UInt32:
			return ConstantValueDefault.UInt32;
		case ConstantValueTypeDiscriminator.Int64:
			return ConstantValueDefault.Int64;
		case ConstantValueTypeDiscriminator.UInt64:
			return ConstantValueDefault.UInt64;
		case ConstantValueTypeDiscriminator.Char:
			return ConstantValueDefault.Char;
		case ConstantValueTypeDiscriminator.Boolean:
			return ConstantValueDefault.Boolean;
		case ConstantValueTypeDiscriminator.Single:
			return ConstantValueDefault.Single;
		case ConstantValueTypeDiscriminator.Double:
			return ConstantValueDefault.Double;
		case ConstantValueTypeDiscriminator.Decimal:
			return ConstantValueDefault.Decimal;
		case ConstantValueTypeDiscriminator.DateTime:
			return ConstantValueDefault.DateTime;
		case ConstantValueTypeDiscriminator.Nothing:
		case ConstantValueTypeDiscriminator.String:
			return Null;
		default:
			throw ExceptionUtilities.UnexpectedValue(discriminator);
		}
	}

	internal static ConstantValueTypeDiscriminator GetDiscriminator(SpecialType st)
	{
		return st switch
		{
			SpecialType.System_SByte => ConstantValueTypeDiscriminator.SByte, 
			SpecialType.System_Byte => ConstantValueTypeDiscriminator.Byte, 
			SpecialType.System_Int16 => ConstantValueTypeDiscriminator.Int16, 
			SpecialType.System_UInt16 => ConstantValueTypeDiscriminator.UInt16, 
			SpecialType.System_Int32 => ConstantValueTypeDiscriminator.Int32, 
			SpecialType.System_UInt32 => ConstantValueTypeDiscriminator.UInt32, 
			SpecialType.System_Int64 => ConstantValueTypeDiscriminator.Int64, 
			SpecialType.System_UInt64 => ConstantValueTypeDiscriminator.UInt64, 
			SpecialType.System_Char => ConstantValueTypeDiscriminator.Char, 
			SpecialType.System_Boolean => ConstantValueTypeDiscriminator.Boolean, 
			SpecialType.System_Single => ConstantValueTypeDiscriminator.Single, 
			SpecialType.System_Double => ConstantValueTypeDiscriminator.Double, 
			SpecialType.System_Decimal => ConstantValueTypeDiscriminator.Decimal, 
			SpecialType.System_DateTime => ConstantValueTypeDiscriminator.DateTime, 
			SpecialType.System_String => ConstantValueTypeDiscriminator.String, 
			_ => ConstantValueTypeDiscriminator.Bad, 
		};
	}

	private static SpecialType GetSpecialType(ConstantValueTypeDiscriminator discriminator)
	{
		return discriminator switch
		{
			ConstantValueTypeDiscriminator.SByte => SpecialType.System_SByte, 
			ConstantValueTypeDiscriminator.Byte => SpecialType.System_Byte, 
			ConstantValueTypeDiscriminator.Int16 => SpecialType.System_Int16, 
			ConstantValueTypeDiscriminator.UInt16 => SpecialType.System_UInt16, 
			ConstantValueTypeDiscriminator.Int32 => SpecialType.System_Int32, 
			ConstantValueTypeDiscriminator.UInt32 => SpecialType.System_UInt32, 
			ConstantValueTypeDiscriminator.Int64 => SpecialType.System_Int64, 
			ConstantValueTypeDiscriminator.UInt64 => SpecialType.System_UInt64, 
			ConstantValueTypeDiscriminator.Char => SpecialType.System_Char, 
			ConstantValueTypeDiscriminator.Boolean => SpecialType.System_Boolean, 
			ConstantValueTypeDiscriminator.Single => SpecialType.System_Single, 
			ConstantValueTypeDiscriminator.Double => SpecialType.System_Double, 
			ConstantValueTypeDiscriminator.Decimal => SpecialType.System_Decimal, 
			ConstantValueTypeDiscriminator.DateTime => SpecialType.System_DateTime, 
			ConstantValueTypeDiscriminator.String => SpecialType.System_String, 
			_ => SpecialType.None, 
		};
	}

	public static bool IsIntegralType(ConstantValueTypeDiscriminator discriminator)
	{
		if (discriminator - 2 <= ConstantValueTypeDiscriminator.UInt32)
		{
			return true;
		}
		return false;
	}

	public static bool IsUnsignedIntegralType(ConstantValueTypeDiscriminator discriminator)
	{
		switch (discriminator)
		{
		case ConstantValueTypeDiscriminator.Byte:
		case ConstantValueTypeDiscriminator.UInt16:
		case ConstantValueTypeDiscriminator.UInt32:
		case ConstantValueTypeDiscriminator.UInt64:
			return true;
		default:
			return false;
		}
	}

	public static bool IsBooleanType(ConstantValueTypeDiscriminator discriminator)
	{
		return discriminator == ConstantValueTypeDiscriminator.Boolean;
	}

	public static bool IsCharType(ConstantValueTypeDiscriminator discriminator)
	{
		return discriminator == ConstantValueTypeDiscriminator.Char;
	}

	public static bool IsStringType(ConstantValueTypeDiscriminator discriminator)
	{
		return discriminator == ConstantValueTypeDiscriminator.String;
	}

	public static bool IsDecimalType(ConstantValueTypeDiscriminator discriminator)
	{
		return discriminator == ConstantValueTypeDiscriminator.Decimal;
	}

	public static bool IsDateTimeType(ConstantValueTypeDiscriminator discriminator)
	{
		return discriminator == ConstantValueTypeDiscriminator.DateTime;
	}

	public static bool IsFloatingType(ConstantValueTypeDiscriminator discriminator)
	{
		if (discriminator != ConstantValueTypeDiscriminator.Double)
		{
			return discriminator == ConstantValueTypeDiscriminator.Single;
		}
		return true;
	}

	public void Serialize(BlobBuilder writer)
	{
		switch (Discriminator)
		{
		case ConstantValueTypeDiscriminator.Boolean:
			writer.WriteBoolean(BooleanValue);
			break;
		case ConstantValueTypeDiscriminator.SByte:
			writer.WriteSByte(SByteValue);
			break;
		case ConstantValueTypeDiscriminator.Byte:
			writer.WriteByte(ByteValue);
			break;
		case ConstantValueTypeDiscriminator.Int16:
		case ConstantValueTypeDiscriminator.Char:
			writer.WriteInt16(Int16Value);
			break;
		case ConstantValueTypeDiscriminator.UInt16:
			writer.WriteUInt16(UInt16Value);
			break;
		case ConstantValueTypeDiscriminator.Single:
			writer.WriteSingle(SingleValue);
			break;
		case ConstantValueTypeDiscriminator.Int32:
			writer.WriteInt32(Int32Value);
			break;
		case ConstantValueTypeDiscriminator.UInt32:
			writer.WriteUInt32(UInt32Value);
			break;
		case ConstantValueTypeDiscriminator.Double:
			writer.WriteDouble(DoubleValue);
			break;
		case ConstantValueTypeDiscriminator.Int64:
			writer.WriteInt64(Int64Value);
			break;
		case ConstantValueTypeDiscriminator.UInt64:
			writer.WriteUInt64(UInt64Value);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(Discriminator);
		}
	}

	public override string ToString()
	{
		string valueToDisplay = GetValueToDisplay();
		return $"{GetType().Name}({valueToDisplay}: {Discriminator})";
	}

	internal virtual string GetValueToDisplay()
	{
		return Value.ToString();
	}

	public virtual bool Equals(ConstantValue other)
	{
		if ((object)other == this)
		{
			return true;
		}
		if ((object)other == null)
		{
			return false;
		}
		return Discriminator == other.Discriminator;
	}

	public static bool operator ==(ConstantValue left, ConstantValue right)
	{
		if ((object)right == left)
		{
			return true;
		}
		return left?.Equals(right) ?? false;
	}

	public static bool operator !=(ConstantValue left, ConstantValue right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Discriminator.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ConstantValue);
	}
}
