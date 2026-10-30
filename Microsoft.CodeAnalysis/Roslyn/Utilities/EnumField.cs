using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal readonly struct EnumField(string name, ulong value, object identityOpt = null)
{
	private class EnumFieldComparer : IComparer<EnumField>
	{
		int IComparer<EnumField>.Compare(EnumField field1, EnumField field2)
		{
			long value = (long)field2.Value;
			int num = value.CompareTo((long)field1.Value);
			if (num != 0)
			{
				return num;
			}
			return string.CompareOrdinal(field1.Name, field2.Name);
		}
	}

	public static readonly IComparer<EnumField> Comparer = new EnumFieldComparer();

	public readonly string Name = name;

	public readonly ulong Value = value;

	public readonly object IdentityOpt = identityOpt;

	public bool IsDefault => Name == null;

	private string GetDebuggerDisplay()
	{
		return $"{{{Name} = {Value}}}";
	}

	internal static EnumField FindValue(ArrayBuilder<EnumField> sortedFields, ulong value)
	{
		int num = 0;
		int num2 = sortedFields.Count;
		while (num < num2)
		{
			int num3 = num + (num2 - num) / 2;
			long num4 = (long)(value - sortedFields[num3].Value);
			if (num4 == 0L)
			{
				while (num3 >= num && sortedFields[num3].Value == value)
				{
					num3--;
				}
				return sortedFields[num3 + 1];
			}
			if (num4 > 0)
			{
				num2 = num3;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return default;
	}
}
