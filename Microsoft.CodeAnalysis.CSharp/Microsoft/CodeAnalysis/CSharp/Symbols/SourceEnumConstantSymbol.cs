using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceEnumConstantSymbol : SourceFieldSymbolWithSyntaxReference
{
	private sealed class ZeroValuedEnumConstantSymbol : SourceEnumConstantSymbol
	{
		public ZeroValuedEnumConstantSymbol(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, DiagnosticBag diagnostics)
			: base(containingEnum, syntax, diagnostics)
		{
		}

		protected override ConstantValue MakeConstantValue(HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, DiagnosticBag diagnostics)
		{
			return Microsoft.CodeAnalysis.ConstantValue.Default(ContainingType.EnumUnderlyingType.SpecialType);
		}
	}

	private sealed class ExplicitValuedEnumConstantSymbol : SourceEnumConstantSymbol
	{
		private readonly SyntaxReference _equalsValueNodeRef;

		public ExplicitValuedEnumConstantSymbol(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, EqualsValueClauseSyntax initializer, DiagnosticBag diagnostics)
			: base(containingEnum, syntax, diagnostics)
		{
			_equalsValueNodeRef = initializer.GetReference();
		}

		protected override ConstantValue MakeConstantValue(HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, DiagnosticBag diagnostics)
		{
			return ConstantValueUtils.EvaluateFieldConstant(this, (EqualsValueClauseSyntax)_equalsValueNodeRef.GetSyntax(), dependencies, earlyDecodingWellKnownAttributes, diagnostics);
		}
	}

	private sealed class ImplicitValuedEnumConstantSymbol : SourceEnumConstantSymbol
	{
		private readonly SourceEnumConstantSymbol _otherConstant;

		private readonly uint _otherConstantOffset;

		public ImplicitValuedEnumConstantSymbol(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, SourceEnumConstantSymbol otherConstant, uint otherConstantOffset, DiagnosticBag diagnostics)
			: base(containingEnum, syntax, diagnostics)
		{
			_otherConstant = otherConstant;
			_otherConstantOffset = otherConstantOffset;
		}

		protected override ConstantValue MakeConstantValue(HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, DiagnosticBag diagnostics)
		{
			ConstantValue constantValue = _otherConstant.GetConstantValue(new ConstantFieldsInProgress(this, dependencies), earlyDecodingWellKnownAttributes);
			if (constantValue == Microsoft.CodeAnalysis.ConstantValue.Unset)
			{
				return Microsoft.CodeAnalysis.ConstantValue.Unset;
			}
			if (constantValue.IsBad)
			{
				return Microsoft.CodeAnalysis.ConstantValue.Bad;
			}
			if (EnumConstantHelper.OffsetValue(constantValue, _otherConstantOffset, out var offsetValue) == EnumOverflowKind.OverflowReport)
			{
				diagnostics.Add(ErrorCode.ERR_EnumeratorOverflow, Locations[0], this);
			}
			return offsetValue;
		}
	}

	public override ImmutableArray<CustomModifier> CustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override Symbol AssociatedSymbol => null;

	protected sealed override DeclarationModifiers Modifiers => DeclarationModifiers.Static | DeclarationModifiers.Public | DeclarationModifiers.Const;

	public new EnumMemberDeclarationSyntax SyntaxNode => (EnumMemberDeclarationSyntax)base.SyntaxNode;

	protected override SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList
	{
		get
		{
			if (containingType.AnyMemberHasAttributes)
			{
				return SyntaxNode.AttributeLists;
			}
			return default;
		}
	}

	public static SourceEnumConstantSymbol CreateExplicitValuedConstant(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		EqualsValueClauseSyntax equalsValue = syntax.EqualsValue;
		return new ExplicitValuedEnumConstantSymbol(containingEnum, syntax, equalsValue, diagnostics);
	}

	public static SourceEnumConstantSymbol CreateImplicitValuedConstant(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, SourceEnumConstantSymbol otherConstant, int otherConstantOffset, DiagnosticBag diagnostics)
	{
		if ((object)otherConstant == null)
		{
			return new ZeroValuedEnumConstantSymbol(containingEnum, syntax, diagnostics);
		}
		return new ImplicitValuedEnumConstantSymbol(containingEnum, syntax, otherConstant, (uint)otherConstantOffset, diagnostics);
	}

	protected SourceEnumConstantSymbol(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, DiagnosticBag diagnostics)
		: base(containingEnum, syntax.Identifier.ValueText, syntax.GetReference(), syntax.Identifier.GetLocation())
	{
		if (Name == "value__")
		{
			diagnostics.Add(ErrorCode.ERR_ReservedEnumerator, ErrorLocation, "value__");
		}
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return ContainingType;
	}

	internal sealed override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.Type:
				state.NotePartComplete(CompletionPart.Type);
				break;
			case CompletionPart.Members:
				state.NotePartComplete(CompletionPart.Members);
				break;
			case CompletionPart.TypeMembers:
				GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete(CompletionPart.ImportsAll | CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.StartInterfaces | CompletionPart.FinishInterfaces | CompletionPart.EnumUnderlyingType | CompletionPart.TypeArguments | CompletionPart.TypeParameters | CompletionPart.SynthesizedExplicitImplementations | CompletionPart.StartMemberChecks | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompleted);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}
}
