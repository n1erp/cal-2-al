using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class MetadataTypeOf : IMetadataExpression
{
	private readonly ITypeReference _typeToGet;

	private readonly ITypeReference _systemType;

	public ITypeReference TypeToGet => _typeToGet;

	ITypeReference IMetadataExpression.Type => _systemType;

	public MetadataTypeOf(ITypeReference typeToGet, ITypeReference systemType)
	{
		_typeToGet = typeToGet;
		_systemType = systemType;
	}

	void IMetadataExpression.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}
}
