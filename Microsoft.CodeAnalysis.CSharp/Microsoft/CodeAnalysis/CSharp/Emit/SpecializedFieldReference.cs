using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class SpecializedFieldReference : TypeMemberReference, ISpecializedFieldReference, IFieldReference, ITypeMemberReference, IReference, INamedEntity
{
	private readonly FieldSymbol _underlyingField;

	protected override Symbol UnderlyingSymbol => _underlyingField;

	IFieldReference ISpecializedFieldReference.UnspecializedVersion => _underlyingField.OriginalDefinition;

	ISpecializedFieldReference IFieldReference.AsSpecializedFieldReference => this;

	bool IFieldReference.IsContextualNamedEntity => false;

	public SpecializedFieldReference(FieldSymbol underlyingField)
	{
		_underlyingField = underlyingField;
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	ITypeReference IFieldReference.GetType(EmitContext context)
	{
		ImmutableArray<CustomModifier> customModifiers = _underlyingField.CustomModifiers;
		ITypeReference typeReference = ((PEModuleBuilder)context.Module).Translate(_underlyingField.Type, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		if (customModifiers.Length == 0)
		{
			return typeReference;
		}
		return new ModifiedTypeReference(typeReference, customModifiers.As<ICustomModifier>());
	}

	IFieldDefinition IFieldReference.GetResolvedField(EmitContext context)
	{
		return null;
	}
}
