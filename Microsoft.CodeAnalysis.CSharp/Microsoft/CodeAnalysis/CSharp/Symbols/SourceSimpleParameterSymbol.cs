using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceSimpleParameterSymbol : SourceParameterSymbol
{
	internal override ConstantValue ExplicitDefaultConstantValue => null;

	internal override bool IsMetadataOptional => false;

	public override bool IsParams => false;

	internal override bool HasDefaultArgumentSyntax => false;

	public override ImmutableArray<CustomModifier> CustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override SyntaxReference SyntaxReference => null;

	internal override bool IsExtensionMethodThis => false;

	internal override bool IsIDispatchConstant => false;

	internal override bool IsIUnknownConstant => false;

	internal override bool IsCallerFilePath => false;

	internal override bool IsCallerLineNumber => false;

	internal override bool IsCallerMemberName => false;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => null;

	internal override bool HasOptionalAttribute => false;

	internal override SyntaxList<AttributeListSyntax> AttributeDeclarationList => default;

	internal override ConstantValue DefaultValueFromAttributes => null;

	public SourceSimpleParameterSymbol(Symbol owner, TypeSymbol parameterType, int ordinal, RefKind refKind, string name, ImmutableArray<Location> locations)
		: base(owner, parameterType, ordinal, refKind, name, locations)
	{
	}

	internal override CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		state.NotePartComplete(CompletionPart.Attributes);
		return CustomAttributesBag<CSharpAttributeData>.Empty;
	}
}
