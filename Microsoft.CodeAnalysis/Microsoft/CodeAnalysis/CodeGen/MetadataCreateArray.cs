using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class MetadataCreateArray : IMetadataExpression
{
	public IArrayTypeReference ArrayType { get; }

	public ITypeReference ElementType { get; }

	public ImmutableArray<IMetadataExpression> Elements { get; }

	ITypeReference IMetadataExpression.Type => ArrayType;

	public MetadataCreateArray(IArrayTypeReference arrayType, ITypeReference elementType, ImmutableArray<IMetadataExpression> initializers)
	{
		ArrayType = arrayType;
		ElementType = elementType;
		Elements = initializers;
	}

	void IMetadataExpression.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}
}
