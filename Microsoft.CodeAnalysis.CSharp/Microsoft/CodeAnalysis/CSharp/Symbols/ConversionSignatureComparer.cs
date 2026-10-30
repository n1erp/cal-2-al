using System.Collections.Generic;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ConversionSignatureComparer : IEqualityComparer<SourceUserDefinedConversionSymbol>
{
	private static readonly ConversionSignatureComparer s_comparer = new ConversionSignatureComparer();

	public static ConversionSignatureComparer Comparer => s_comparer;

	private ConversionSignatureComparer()
	{
	}

	public bool Equals(SourceUserDefinedConversionSymbol member1, SourceUserDefinedConversionSymbol member2)
	{
		if ((object)member1 == member2)
		{
			return true;
		}
		if ((object)member1 == null || (object)member2 == null)
		{
			return false;
		}
		if (member1.ParameterCount != 1 || member2.ParameterCount != 1)
		{
			return false;
		}
		if (member1.ReturnType.Equals(member2.ReturnType, TypeCompareKind.IgnoreDynamicAndTupleNames))
		{
			return member1.ParameterTypes[0].Equals(member2.ParameterTypes[0], TypeCompareKind.IgnoreDynamicAndTupleNames);
		}
		return false;
	}

	public int GetHashCode(SourceUserDefinedConversionSymbol member)
	{
		if ((object)member == null)
		{
			return 0;
		}
		int currentKey = 1;
		currentKey = Hash.Combine(member.ReturnType.GetHashCode(), currentKey);
		if (member.ParameterCount != 1)
		{
			return currentKey;
		}
		return Hash.Combine(member.ParameterTypes[0].GetHashCode(), currentKey);
	}
}
