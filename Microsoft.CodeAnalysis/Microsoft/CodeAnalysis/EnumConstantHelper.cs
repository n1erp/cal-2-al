using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class EnumConstantHelper
{
	internal static EnumOverflowKind OffsetValue(ConstantValue constantValue, uint offset, out ConstantValue offsetValue)
	{
		offsetValue = ConstantValue.Bad;
		EnumOverflowKind enumOverflowKind;
		switch (constantValue.Discriminator)
		{
		case ConstantValueTypeDiscriminator.SByte:
		{
			long num6 = constantValue.SByteValue;
			enumOverflowKind = CheckOverflow(127L, num6, offset);
			if (enumOverflowKind == EnumOverflowKind.NoOverflow)
			{
				offsetValue = ConstantValue.Create((sbyte)(num6 + offset));
			}
			break;
		}
		case ConstantValueTypeDiscriminator.Byte:
		{
			ulong num2 = constantValue.ByteValue;
			enumOverflowKind = CheckOverflow(255uL, num2, offset);
			if (enumOverflowKind == EnumOverflowKind.NoOverflow)
			{
				offsetValue = ConstantValue.Create((byte)(num2 + offset));
			}
			break;
		}
		case ConstantValueTypeDiscriminator.Int16:
		{
			long num4 = constantValue.Int16Value;
			enumOverflowKind = CheckOverflow(32767L, num4, offset);
			if (enumOverflowKind == EnumOverflowKind.NoOverflow)
			{
				offsetValue = ConstantValue.Create((short)(num4 + offset));
			}
			break;
		}
		case ConstantValueTypeDiscriminator.UInt16:
		{
			ulong num = constantValue.UInt16Value;
			enumOverflowKind = CheckOverflow(65535uL, num, offset);
			if (enumOverflowKind == EnumOverflowKind.NoOverflow)
			{
				offsetValue = ConstantValue.Create((ushort)(num + offset));
			}
			break;
		}
		case ConstantValueTypeDiscriminator.Int32:
		{
			long num5 = constantValue.Int32Value;
			enumOverflowKind = CheckOverflow(2147483647L, num5, offset);
			if (enumOverflowKind == EnumOverflowKind.NoOverflow)
			{
				offsetValue = ConstantValue.Create((int)(num5 + offset));
			}
			break;
		}
		case ConstantValueTypeDiscriminator.UInt32:
		{
			ulong num3 = constantValue.UInt32Value;
			enumOverflowKind = CheckOverflow(4294967295uL, num3, offset);
			if (enumOverflowKind == EnumOverflowKind.NoOverflow)
			{
				offsetValue = ConstantValue.Create((uint)(num3 + offset));
			}
			break;
		}
		case ConstantValueTypeDiscriminator.Int64:
		{
			long int64Value = constantValue.Int64Value;
			enumOverflowKind = CheckOverflow(long.MaxValue, int64Value, offset);
			if (enumOverflowKind == EnumOverflowKind.NoOverflow)
			{
				offsetValue = ConstantValue.Create(int64Value + offset);
			}
			break;
		}
		case ConstantValueTypeDiscriminator.UInt64:
		{
			ulong uInt64Value = constantValue.UInt64Value;
			enumOverflowKind = CheckOverflow(ulong.MaxValue, uInt64Value, offset);
			if (enumOverflowKind == EnumOverflowKind.NoOverflow)
			{
				offsetValue = ConstantValue.Create(uInt64Value + offset);
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(constantValue.Discriminator);
		}
		return enumOverflowKind;
	}

	private static EnumOverflowKind CheckOverflow(long maxOffset, long previous, uint offset)
	{
		return CheckOverflow((ulong)(maxOffset - previous), offset);
	}

	private static EnumOverflowKind CheckOverflow(ulong maxOffset, ulong previous, uint offset)
	{
		return CheckOverflow(maxOffset - previous, offset);
	}

	private static EnumOverflowKind CheckOverflow(ulong maxOffset, uint offset)
	{
		if (offset > maxOffset)
		{
			if (offset - 1 != maxOffset)
			{
				return EnumOverflowKind.OverflowIgnore;
			}
			return EnumOverflowKind.OverflowReport;
		}
		return EnumOverflowKind.NoOverflow;
	}
}
