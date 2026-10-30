using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class MetadataNamedArgument : IMetadataNamedArgument, IMetadataExpression
{
	private readonly ISymbol _entity;

	private readonly ITypeReference _type;

	private readonly IMetadataExpression _value;

	string IMetadataNamedArgument.ArgumentName => _entity.Name;

	IMetadataExpression IMetadataNamedArgument.ArgumentValue => _value;

	bool IMetadataNamedArgument.IsField => _entity is IFieldDefinition;

	ITypeReference IMetadataExpression.Type => _type;

	public MetadataNamedArgument(ISymbol entity, ITypeReference type, IMetadataExpression value)
	{
		_entity = entity;
		_type = type;
		_value = value;
	}

	void IMetadataExpression.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}
}
