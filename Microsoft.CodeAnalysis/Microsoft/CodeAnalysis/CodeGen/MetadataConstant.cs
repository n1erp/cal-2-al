using System.Diagnostics;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class MetadataConstant : IMetadataExpression
{
	public ITypeReference Type { get; }

	public object Value { get; }

	public MetadataConstant(ITypeReference type, object value)
	{
		Type = type;
		Value = value;
	}

	void IMetadataExpression.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	[Conditional("DEBUG")]
	internal static void AssertValidConstant(object value)
	{
	}
}
