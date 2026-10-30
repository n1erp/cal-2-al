using System;
using System.Collections.Generic;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class SwitchConstantValueHelper
{
	public class SwitchLabelsComparer : EqualityComparer<object>
	{
		public override bool Equals(object first, object second)
		{
			ConstantValue constantValue = first as ConstantValue;
			if (constantValue != null)
			{
				ConstantValue constantValue2 = second as ConstantValue;
				if (constantValue2 != null)
				{
					if (!IsValidSwitchCaseLabelConstant(constantValue) || !IsValidSwitchCaseLabelConstant(constantValue2))
					{
						return constantValue.Equals(constantValue2);
					}
					return CompareSwitchCaseLabelConstants(constantValue, constantValue2) == 0;
				}
			}
			if (first is string a)
			{
				return string.Equals(a, second as string, StringComparison.Ordinal);
			}
			return first.Equals(second);
		}

		public override int GetHashCode(object obj)
		{
			ConstantValue constantValue = obj as ConstantValue;
			if (constantValue != null)
			{
				switch (constantValue.Discriminator)
				{
				case ConstantValueTypeDiscriminator.SByte:
				case ConstantValueTypeDiscriminator.Int16:
				case ConstantValueTypeDiscriminator.Int32:
				case ConstantValueTypeDiscriminator.Int64:
					return constantValue.Int64Value.GetHashCode();
				case ConstantValueTypeDiscriminator.Byte:
				case ConstantValueTypeDiscriminator.UInt16:
				case ConstantValueTypeDiscriminator.UInt32:
				case ConstantValueTypeDiscriminator.UInt64:
				case ConstantValueTypeDiscriminator.Char:
				case ConstantValueTypeDiscriminator.Boolean:
					return constantValue.UInt64Value.GetHashCode();
				case ConstantValueTypeDiscriminator.String:
					return constantValue.StringValue.GetHashCode();
				}
			}
			return obj.GetHashCode();
		}
	}

	public static bool IsValidSwitchCaseLabelConstant(ConstantValue constant)
	{
		switch (constant.Discriminator)
		{
		case ConstantValueTypeDiscriminator.Nothing:
		case ConstantValueTypeDiscriminator.SByte:
		case ConstantValueTypeDiscriminator.Byte:
		case ConstantValueTypeDiscriminator.Int16:
		case ConstantValueTypeDiscriminator.UInt16:
		case ConstantValueTypeDiscriminator.Int32:
		case ConstantValueTypeDiscriminator.UInt32:
		case ConstantValueTypeDiscriminator.Int64:
		case ConstantValueTypeDiscriminator.UInt64:
		case ConstantValueTypeDiscriminator.Char:
		case ConstantValueTypeDiscriminator.Boolean:
		case ConstantValueTypeDiscriminator.String:
			return true;
		default:
			return false;
		}
	}

	public static int CompareSwitchCaseLabelConstants(ConstantValue first, ConstantValue second)
	{
		if (first.IsNull)
		{
			if (!second.IsNull)
			{
				return -1;
			}
			return 0;
		}
		if (second.IsNull)
		{
			return 1;
		}
		switch (first.Discriminator)
		{
		case ConstantValueTypeDiscriminator.SByte:
		case ConstantValueTypeDiscriminator.Int16:
		case ConstantValueTypeDiscriminator.Int32:
		case ConstantValueTypeDiscriminator.Int64:
			return first.Int64Value.CompareTo(second.Int64Value);
		case ConstantValueTypeDiscriminator.Byte:
		case ConstantValueTypeDiscriminator.UInt16:
		case ConstantValueTypeDiscriminator.UInt32:
		case ConstantValueTypeDiscriminator.UInt64:
		case ConstantValueTypeDiscriminator.Char:
		case ConstantValueTypeDiscriminator.Boolean:
			return first.UInt64Value.CompareTo(second.UInt64Value);
		case ConstantValueTypeDiscriminator.String:
			return string.CompareOrdinal(first.StringValue, second.StringValue);
		default:
			throw ExceptionUtilities.UnexpectedValue(first.Discriminator);
		}
	}
}
