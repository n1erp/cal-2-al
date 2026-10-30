using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class ConstantValue : IEquatable<ConstantValue>
{
	private sealed class ConstantValueBad : ConstantValue
	{
		public static readonly ConstantValueBad Instance = new ConstantValueBad();

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.Bad;

		internal override NavTypeKind NavTypeKind => NavTypeKind.None;

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

		internal override NavTypeKind NavTypeKind => NavTypeKind.None;

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
		private readonly string value;

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.String;

		internal override NavTypeKind NavTypeKind => NavTypeKind.Text;

		public override string StringValue => value;

		public ConstantValueString(string value)
		{
			DebugAssertHelper.Assert(value != null, "null strings should be represented as Null constant.");
			this.value = value;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(base.GetHashCode(), value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return value == other.StringValue;
			}
			return false;
		}

		internal override string GetValueToDisplay()
		{
			if (value != null)
			{
				return string.Format(CultureInfo.InvariantCulture, "\"{0}\"", value);
			}
			return "null";
		}
	}

	private sealed class ConstantValueDecimal : ConstantValue
	{
		private readonly decimal value;

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.Decimal;

		internal override NavTypeKind NavTypeKind => NavTypeKind.Decimal;

		public override decimal DecimalValue => value;

		public ConstantValueDecimal(decimal value)
		{
			this.value = value;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(base.GetHashCode(), value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return value == other.DecimalValue;
			}
			return false;
		}

		internal override string GetValueToDisplay()
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
	}

	private sealed class ConstantValueDateTime : ConstantValue
	{
		private readonly DateTime value;

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.DateTime;

		internal override NavTypeKind NavTypeKind => NavTypeKind.DateTime;

		public override DateTime DateTimeValue => value;

		public ConstantValueDateTime(DateTime value)
		{
			this.value = value;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(base.GetHashCode(), value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return value == other.DateTimeValue;
			}
			return false;
		}
	}

	private sealed class ConstantValueTime : ConstantValue
	{
		private readonly int value;

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.Time;

		internal override NavTypeKind NavTypeKind => NavTypeKind.Time;

		public override int TimeValue => value;

		public ConstantValueTime(int value)
		{
			this.value = value;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(base.GetHashCode(), value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return value == other.TimeValue;
			}
			return false;
		}
	}

	private sealed class ConstantValueDate : ConstantValue
	{
		private readonly int value;

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.Date;

		internal override NavTypeKind NavTypeKind => NavTypeKind.Date;

		public override int DateValue => value;

		public override DateTime DateTimeValue => DateTimeUtilities.CSideDateToDateTime(value);

		public ConstantValueDate(int value)
		{
			this.value = value;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(base.GetHashCode(), value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return value == other.DateValue;
			}
			return false;
		}
	}

	private sealed class ConstantValueDuration : ConstantValue
	{
		private readonly long value;

		public override ConstantValueTypeDiscriminator Discriminator => ConstantValueTypeDiscriminator.Duration;

		internal override NavTypeKind NavTypeKind => NavTypeKind.Duration;

		public override long DurationValue => value;

		public ConstantValueDuration(long value)
		{
			this.value = value;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(base.GetHashCode(), value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return value == other.DurationValue;
			}
			return false;
		}
	}

	private abstract class ConstantValueDiscriminated : ConstantValue
	{
		private readonly ConstantValueTypeDiscriminator discriminator;

		public override ConstantValueTypeDiscriminator Discriminator => discriminator;

		internal override NavTypeKind NavTypeKind => GetNavTypeKind(discriminator);

		public ConstantValueDiscriminated(ConstantValueTypeDiscriminator discriminator)
		{
			this.discriminator = discriminator;
		}
	}

	private class ConstantValueDefault : ConstantValueDiscriminated
	{
		public static readonly ConstantValueDefault Char = new ConstantValueDefault(ConstantValueTypeDiscriminator.Char);

		public static readonly ConstantValueDefault Int32 = new ConstantValueDefault(ConstantValueTypeDiscriminator.Int32);

		public static readonly ConstantValueDefault Int64 = new ConstantValueDefault(ConstantValueTypeDiscriminator.Int64);

		public static readonly ConstantValueDefault Decimal = new ConstantValueDecimalZero();

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

	private class ConstantValueOne : ConstantValueDiscriminated
	{
		public static readonly ConstantValueOne Int32 = new ConstantValueOne(ConstantValueTypeDiscriminator.Int32);

		public static readonly ConstantValueOne Int64 = new ConstantValueOne(ConstantValueTypeDiscriminator.Int64);

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

	private sealed class ConstantValueI16 : ConstantValueDiscriminated
	{
		private readonly short value;

		public override short Int16Value => value;

		public override ushort UInt16Value => (ushort)value;

		public override char CharValue => (char)value;

		public ConstantValueI16(char value)
			: base(ConstantValueTypeDiscriminator.Char)
		{
			this.value = (short)value;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(base.GetHashCode(), value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return value == other.Int16Value;
			}
			return false;
		}
	}

	private sealed class ConstantValueI32 : ConstantValueDiscriminated
	{
		private readonly int value;

		public override int Int32Value => value;

		public override uint UInt32Value => (uint)value;

		public ConstantValueI32(int value)
			: base(ConstantValueTypeDiscriminator.Int32)
		{
			this.value = value;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(base.GetHashCode(), value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return value == other.Int32Value;
			}
			return false;
		}
	}

	private sealed class ConstantValueI64 : ConstantValueDiscriminated
	{
		private readonly long value;

		public override long Int64Value => value;

		public override ulong UInt64Value => (ulong)value;

		public ConstantValueI64(long value)
			: base(ConstantValueTypeDiscriminator.Int64)
		{
			this.value = value;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(base.GetHashCode(), value.GetHashCode());
		}

		public override bool Equals(ConstantValue other)
		{
			if (base.Equals(other))
			{
				return value == other.Int64Value;
			}
			return false;
		}
	}

	public const ConstantValue NotAvailable = null;

	public abstract ConstantValueTypeDiscriminator Discriminator { get; }

	internal abstract NavTypeKind NavTypeKind { get; }

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

	public virtual int TimeValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual int DateValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public virtual long DurationValue
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

	public object Value => Discriminator switch
	{
		ConstantValueTypeDiscriminator.Bad => null, 
		ConstantValueTypeDiscriminator.Char => Boxes.Box(CharValue), 
		ConstantValueTypeDiscriminator.Int32 => Boxes.Box(Int32Value), 
		ConstantValueTypeDiscriminator.Int64 => Boxes.Box(Int64Value), 
		ConstantValueTypeDiscriminator.Boolean => Boxes.Box(BooleanValue), 
		ConstantValueTypeDiscriminator.Decimal => Boxes.Box(DecimalValue), 
		ConstantValueTypeDiscriminator.Time => Boxes.Box(TimeValue), 
		ConstantValueTypeDiscriminator.Date => Boxes.Box(DateValue), 
		ConstantValueTypeDiscriminator.DateTime => DateTimeValue, 
		ConstantValueTypeDiscriminator.Duration => DurationValue, 
		ConstantValueTypeDiscriminator.String => StringValue, 
		_ => throw ExceptionUtilities.UnexpectedValue(Discriminator), 
	};

	public bool IsIntegral => IsIntegralType(Discriminator);

	public bool IsNegativeNumeric => Discriminator switch
	{
		ConstantValueTypeDiscriminator.Int32 => Int32Value < 0, 
		ConstantValueTypeDiscriminator.Int64 => Int64Value < 0, 
		ConstantValueTypeDiscriminator.Decimal => DecimalValue < 0m, 
		_ => false, 
	};

	public bool IsNumeric
	{
		get
		{
			ConstantValueTypeDiscriminator discriminator = Discriminator;
			if (discriminator - 2 <= ConstantValueTypeDiscriminator.Byte || discriminator == ConstantValueTypeDiscriminator.Decimal)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsUnsigned => IsUnsignedIntegralType(Discriminator);

	public bool IsBoolean => Discriminator == ConstantValueTypeDiscriminator.Boolean;

	public bool IsChar => Discriminator == ConstantValueTypeDiscriminator.Char;

	public bool IsString => Discriminator == ConstantValueTypeDiscriminator.String;

	public bool IsDecimal => Discriminator == ConstantValueTypeDiscriminator.Decimal;

	public bool IsDateTime => Discriminator == ConstantValueTypeDiscriminator.DateTime;

	public bool IsBad => Discriminator == ConstantValueTypeDiscriminator.Bad;

	public static ConstantValue Create(string value)
	{
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

	public static ConstantValue Create(int value)
	{
		return value switch
		{
			0 => (ConstantValue)ConstantValueDefault.Int32, 
			1 => ConstantValueOne.Int32, 
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

	public static ConstantValue Create(bool value)
	{
		if (value)
		{
			return ConstantValueOne.Boolean;
		}
		return ConstantValueDefault.Boolean;
	}

	public static ConstantValue Create(DateTime value)
	{
		return new ConstantValueDateTime(value);
	}

	public static ConstantValue CreateTime(int value)
	{
		return new ConstantValueTime(value);
	}

	public static ConstantValue CreateDate(int value)
	{
		return new ConstantValueDate(value);
	}

	public static ConstantValue CreateDuration(long value)
	{
		return new ConstantValueDuration(value);
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

	public static ConstantValue Create(object value, NavTypeKind st)
	{
		ConstantValueTypeDiscriminator discriminator = GetDiscriminator(st);
		DebugAssertHelper.Assert(discriminator != ConstantValueTypeDiscriminator.Bad);
		return Create(value, discriminator);
	}

	public static ConstantValue Create(object value, ConstantValueTypeDiscriminator discriminator)
	{
		DebugAssertHelper.Assert(BitConverter.IsLittleEndian);
		return discriminator switch
		{
			ConstantValueTypeDiscriminator.Byte => Create((byte)value), 
			ConstantValueTypeDiscriminator.Char => Create((value is char c) ? c : ((string)value)[0]), 
			ConstantValueTypeDiscriminator.Int32 => Create((int)value), 
			ConstantValueTypeDiscriminator.Int64 => Create((long)value), 
			ConstantValueTypeDiscriminator.Boolean => Create((bool)value), 
			ConstantValueTypeDiscriminator.Decimal => Create((decimal)value), 
			ConstantValueTypeDiscriminator.String => Create((string)value), 
			ConstantValueTypeDiscriminator.DateTime => Create((DateTime)value), 
			ConstantValueTypeDiscriminator.Time => CreateTime((int)value), 
			ConstantValueTypeDiscriminator.Date => CreateDate((int)value), 
			ConstantValueTypeDiscriminator.Duration => CreateDuration((long)value), 
			_ => throw new InvalidOperationException(), 
		};
	}

	internal static ConstantValueTypeDiscriminator GetDiscriminator(NavTypeKind st)
	{
		switch (st)
		{
		case NavTypeKind.Byte:
			return ConstantValueTypeDiscriminator.Byte;
		case NavTypeKind.Char:
			return ConstantValueTypeDiscriminator.Char;
		case NavTypeKind.Integer:
			return ConstantValueTypeDiscriminator.Int32;
		case NavTypeKind.BigInteger:
			return ConstantValueTypeDiscriminator.Int64;
		case NavTypeKind.Boolean:
			return ConstantValueTypeDiscriminator.Boolean;
		case NavTypeKind.Decimal:
			return ConstantValueTypeDiscriminator.Decimal;
		case NavTypeKind.String:
		case NavTypeKind.Text:
			return ConstantValueTypeDiscriminator.String;
		case NavTypeKind.Time:
			return ConstantValueTypeDiscriminator.Time;
		case NavTypeKind.Date:
			return ConstantValueTypeDiscriminator.Date;
		case NavTypeKind.DateTime:
			return ConstantValueTypeDiscriminator.DateTime;
		case NavTypeKind.Duration:
			return ConstantValueTypeDiscriminator.Duration;
		default:
			return ConstantValueTypeDiscriminator.Bad;
		}
	}

	private static NavTypeKind GetNavTypeKind(ConstantValueTypeDiscriminator discriminator)
	{
		return discriminator switch
		{
			ConstantValueTypeDiscriminator.Int32 => NavTypeKind.Integer, 
			ConstantValueTypeDiscriminator.Int64 => NavTypeKind.BigInteger, 
			ConstantValueTypeDiscriminator.Boolean => NavTypeKind.Boolean, 
			ConstantValueTypeDiscriminator.Decimal => NavTypeKind.Decimal, 
			ConstantValueTypeDiscriminator.String => NavTypeKind.Text, 
			_ => NavTypeKind.None, 
		};
	}

	public static bool IsIntegralType(ConstantValueTypeDiscriminator discriminator)
	{
		if (discriminator - 2 <= ConstantValueTypeDiscriminator.Int32)
		{
			return true;
		}
		return false;
	}

	public static bool IsUnsignedIntegralType(ConstantValueTypeDiscriminator discriminator)
	{
		if (discriminator == ConstantValueTypeDiscriminator.Byte)
		{
			return true;
		}
		return false;
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

	public override string ToString()
	{
		string valueToDisplay = GetValueToDisplay();
		return string.Format(CultureInfo.InvariantCulture, "{0}({1}: {2})", GetType().Name, valueToDisplay, Discriminator);
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
