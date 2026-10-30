using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class NeverEmptyStructTypeCache : EmptyStructTypeCache
{
	public NeverEmptyStructTypeCache()
		: base(null, dev12CompilerCompatibility: false)
	{
	}

	public override bool IsEmptyStructType(TypeSymbol type)
	{
		return false;
	}
}
