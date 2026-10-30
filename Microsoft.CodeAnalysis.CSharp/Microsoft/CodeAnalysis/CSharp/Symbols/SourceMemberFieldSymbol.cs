using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceMemberFieldSymbol : SourceFieldSymbolWithSyntaxReference
{
	private readonly DeclarationModifiers _modifiers;

	protected sealed override DeclarationModifiers Modifiers => _modifiers;

	protected abstract TypeSyntax TypeSyntax { get; }

	protected abstract SyntaxTokenList ModifiersTokenList { get; }

	public abstract bool HasInitializer { get; }

	public override Symbol AssociatedSymbol => null;

	public override int FixedSize
	{
		get
		{
			state.NotePartComplete(CompletionPart.Members);
			return 0;
		}
	}

	internal SourceMemberFieldSymbol(SourceMemberContainerTypeSymbol containingType, DeclarationModifiers modifiers, string name, SyntaxReference syntax, Location location)
		: base(containingType, name, syntax, location)
	{
		_modifiers = modifiers;
	}

	protected void TypeChecks(TypeSymbol type, DiagnosticBag diagnostics)
	{
		if (type.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_VarDeclIsStaticClass, ErrorLocation, type);
		}
		else if (type.SpecialType == SpecialType.System_Void)
		{
			diagnostics.Add(ErrorCode.ERR_FieldCantHaveVoidType, TypeSyntax.Location);
		}
		else if (type.IsRestrictedType(ignoreSpanLikeTypes: true))
		{
			diagnostics.Add(ErrorCode.ERR_FieldCantBeRefAny, TypeSyntax.Location, type);
		}
		else if (type.IsByRefLikeType && (IsStatic || !containingType.IsByRefLikeType))
		{
			diagnostics.Add(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, TypeSyntax.Location, type);
		}
		else if (IsConst && !type.CanBeConst())
		{
			SyntaxToken syntaxToken = default;
			foreach (SyntaxToken modifiersToken in ModifiersTokenList)
			{
				if (modifiersToken.Kind() == SyntaxKind.ConstKeyword)
				{
					syntaxToken = modifiersToken;
					break;
				}
			}
			diagnostics.Add(ErrorCode.ERR_BadConstType, syntaxToken.GetLocation(), type);
		}
		else if (IsVolatile && !type.IsValidVolatileFieldType())
		{
			diagnostics.Add(ErrorCode.ERR_VolatileStruct, ErrorLocation, this, type);
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (!this.IsNoMoreVisibleThan(type, ref useSiteDiagnostics))
		{
			diagnostics.Add(ErrorCode.ERR_BadVisFieldType, ErrorLocation, this, type);
		}
		diagnostics.Add(ErrorLocation, useSiteDiagnostics);
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		ConstantValue constantValue = GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
		if (IsConst && constantValue != null && Type.SpecialType == SpecialType.System_Decimal)
		{
			CommonFieldWellKnownAttributeData decodedWellKnownAttributeData = GetDecodedWellKnownAttributeData();
			if (decodedWellKnownAttributeData == null || decodedWellKnownAttributeData.ConstValue == Microsoft.CodeAnalysis.ConstantValue.Unset)
			{
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDecimalConstantAttribute(constantValue.DecimalValue));
			}
		}
	}

	internal static DeclarationModifiers MakeModifiers(NamedTypeSymbol containingType, SyntaxToken firstIdentifier, SyntaxTokenList modifiers, DiagnosticBag diagnostics, out bool modifierErrors)
	{
		DeclarationModifiers defaultAccess = (containingType.IsInterface ? DeclarationModifiers.Public : DeclarationModifiers.Private);
		DeclarationModifiers allowedModifiers = DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Abstract | DeclarationModifiers.Static | DeclarationModifiers.New | DeclarationModifiers.ReadOnly | DeclarationModifiers.Const | DeclarationModifiers.Volatile | DeclarationModifiers.Unsafe | DeclarationModifiers.Fixed;
		SourceLocation sourceLocation = new SourceLocation(in firstIdentifier);
		DeclarationModifiers declarationModifiers = ModifierUtils.MakeAndCheckNontypeMemberModifiers(modifiers, defaultAccess, allowedModifiers, sourceLocation, diagnostics, out modifierErrors);
		if ((declarationModifiers & DeclarationModifiers.Abstract) != 0)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractField, sourceLocation);
			declarationModifiers &= ~DeclarationModifiers.Abstract;
		}
		if ((declarationModifiers & DeclarationModifiers.Fixed) != 0)
		{
			if ((declarationModifiers & DeclarationModifiers.Static) != 0)
			{
				diagnostics.Add(ErrorCode.ERR_BadMemberFlag, sourceLocation, SyntaxFacts.GetText(SyntaxKind.StaticKeyword));
			}
			if ((declarationModifiers & DeclarationModifiers.ReadOnly) != 0)
			{
				diagnostics.Add(ErrorCode.ERR_BadMemberFlag, sourceLocation, SyntaxFacts.GetText(SyntaxKind.ReadOnlyKeyword));
			}
			if ((declarationModifiers & DeclarationModifiers.Const) != 0)
			{
				diagnostics.Add(ErrorCode.ERR_BadMemberFlag, sourceLocation, SyntaxFacts.GetText(SyntaxKind.ConstKeyword));
			}
			if ((declarationModifiers & DeclarationModifiers.Volatile) != 0)
			{
				diagnostics.Add(ErrorCode.ERR_BadMemberFlag, sourceLocation, SyntaxFacts.GetText(SyntaxKind.VolatileKeyword));
			}
			declarationModifiers &= ~(DeclarationModifiers.Static | DeclarationModifiers.ReadOnly | DeclarationModifiers.Const | DeclarationModifiers.Volatile);
		}
		if ((declarationModifiers & DeclarationModifiers.Const) != 0)
		{
			if ((declarationModifiers & DeclarationModifiers.Static) != 0)
			{
				diagnostics.Add(ErrorCode.ERR_StaticConstant, sourceLocation, firstIdentifier.ValueText);
			}
			if ((declarationModifiers & DeclarationModifiers.ReadOnly) != 0)
			{
				diagnostics.Add(ErrorCode.ERR_BadMemberFlag, sourceLocation, SyntaxFacts.GetText(SyntaxKind.ReadOnlyKeyword));
			}
			if ((declarationModifiers & DeclarationModifiers.Volatile) != 0)
			{
				diagnostics.Add(ErrorCode.ERR_BadMemberFlag, sourceLocation, SyntaxFacts.GetText(SyntaxKind.VolatileKeyword));
			}
			if ((declarationModifiers & DeclarationModifiers.Unsafe) != 0)
			{
				diagnostics.Add(ErrorCode.ERR_BadMemberFlag, sourceLocation, SyntaxFacts.GetText(SyntaxKind.UnsafeKeyword));
			}
			declarationModifiers |= DeclarationModifiers.Static;
		}
		else
		{
			containingType.CheckUnsafeModifier(declarationModifiers, sourceLocation, diagnostics);
		}
		return declarationModifiers;
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
				GetFieldType(ConsList<FieldSymbol>.Empty);
				break;
			case CompletionPart.Members:
				_ = FixedSize;
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

	internal override NamedTypeSymbol FixedImplementationType(PEModuleBuilder emitModule)
	{
		return null;
	}
}
