using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceUserDefinedOperatorSymbolBase : SourceMemberMethodSymbol
{
	private readonly string _name;

	private readonly bool _isExpressionBodied;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeSymbol _lazyReturnType;

	protected abstract ParameterListSyntax ParameterListSyntax { get; }

	protected abstract TypeSyntax ReturnTypeSyntax { get; }

	public sealed override string Name => _name;

	public sealed override bool ReturnsVoid
	{
		get
		{
			LazyMethodChecks();
			return base.ReturnsVoid;
		}
	}

	public sealed override bool IsVararg => false;

	public sealed override bool IsExtensionMethod => false;

	public sealed override ImmutableArray<Location> Locations => locations;

	internal sealed override int ParameterCount
	{
		get
		{
			if (_lazyParameters.IsDefault)
			{
				return GetSyntax().ParameterList.ParameterCount;
			}
			return _lazyParameters.Length;
		}
	}

	public sealed override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			LazyMethodChecks();
			return _lazyParameters;
		}
	}

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public sealed override ImmutableArray<TypeParameterConstraintClause> TypeParameterConstraintClauses => ImmutableArray<TypeParameterConstraintClause>.Empty;

	public override RefKind RefKind => RefKind.None;

	public sealed override TypeSymbol ReturnType
	{
		get
		{
			LazyMethodChecks();
			return _lazyReturnType;
		}
	}

	internal override bool IsExpressionBodied => _isExpressionBodied;

	protected SourceUserDefinedOperatorSymbolBase(MethodKind methodKind, string name, SourceMemberContainerTypeSymbol containingType, Location location, BaseMethodDeclarationSyntax syntax, DiagnosticBag diagnostics)
		: base(containingType, syntax.GetReference(), location)
	{
		_name = name;
		_isExpressionBodied = syntax.Body == null && syntax.ExpressionBody != null;
		DeclarationModifiers defaultAccess = DeclarationModifiers.Private;
		DeclarationModifiers allowedModifiers = DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Static | DeclarationModifiers.Extern | DeclarationModifiers.Unsafe;
		DeclarationModifiers declarationModifiers = ModifierUtils.MakeAndCheckNontypeMemberModifiers(syntax.Modifiers, defaultAccess, allowedModifiers, location, diagnostics, out var _);
		this.CheckUnsafeModifier(declarationModifiers, diagnostics);
		MakeFlags(methodKind, declarationModifiers, returnsVoid: false, isExtensionMethod: false);
		if (ContainingType.IsInterface)
		{
			return;
		}
		if (ContainingType.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_OperatorInStaticClass, location, this);
			return;
		}
		if (DeclaredAccessibility != Accessibility.Public || !IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_OperatorsMustBeStatic, Locations[0], this);
		}
		bool flag = syntax.HasAnyBody();
		if (flag && IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_ExternHasBody, location, this);
		}
		else if (!flag && !IsExtern && !IsAbstract && !IsPartial)
		{
			diagnostics.Add(ErrorCode.ERR_ConcreteMissingBody, location, this);
		}
		CSDiagnosticInfo cSDiagnosticInfo = ModifierUtils.CheckAccessibility(DeclarationModifiers);
		if (cSDiagnosticInfo != null)
		{
			diagnostics.Add(cSDiagnosticInfo, location);
		}
	}

	internal BaseMethodDeclarationSyntax GetSyntax()
	{
		return (BaseMethodDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}

	protected override void MethodChecks(DiagnosticBag diagnostics)
	{
		Binder binder = DeclaringCompilation.GetBinderFactory(syntaxReferenceOpt.SyntaxTree).GetBinder(ReturnTypeSyntax, GetSyntax(), this).WithAdditionalFlags(BinderFlags.SuppressConstraintChecks);
		_lazyParameters = ParameterHelpers.MakeParameters(binder, this, ParameterListSyntax, out var arglistToken, diagnostics, allowRefOrOut: true, allowThis: false, addRefReadOnlyModifier: false);
		if (arglistToken.Kind() == SyntaxKind.ArgListKeyword)
		{
			diagnostics.Add(ErrorCode.ERR_IllegalVarArgs, new SourceLocation(in arglistToken));
		}
		_lazyReturnType = binder.BindType(ReturnTypeSyntax, diagnostics);
		if (_lazyReturnType.IsRestrictedType(ignoreSpanLikeTypes: true))
		{
			diagnostics.Add(ErrorCode.ERR_MethodReturnCantBeRefAny, ReturnTypeSyntax.Location, _lazyReturnType);
		}
		if (_lazyReturnType.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_ReturnTypeIsStaticClass, ReturnTypeSyntax.Location, _lazyReturnType);
		}
		SetReturnsVoid(_lazyReturnType.SpecialType == SpecialType.System_Void);
		if (!ContainingType.IsInterfaceType() && !ContainingType.IsStatic)
		{
			CheckEffectiveAccessibility(_lazyReturnType, _lazyParameters, diagnostics);
			CheckValueParameters(diagnostics);
			CheckOperatorSignatures(diagnostics);
		}
	}

	private void CheckValueParameters(DiagnosticBag diagnostics)
	{
		foreach (ParameterSymbol parameter in Parameters)
		{
			if (parameter.RefKind != RefKind.None && parameter.RefKind != RefKind.In)
			{
				diagnostics.Add(ErrorCode.ERR_IllegalRefParam, Locations[0]);
				break;
			}
		}
	}

	private void CheckOperatorSignatures(DiagnosticBag diagnostics)
	{
		if (DoesOperatorHaveCorrectArity(Name, ParameterCount))
		{
			switch (Name)
			{
			case "op_Implicit":
			case "op_Explicit":
				CheckUserDefinedConversionSignature(diagnostics);
				break;
			case "op_UnaryNegation":
			case "op_UnaryPlus":
			case "op_LogicalNot":
			case "op_OnesComplement":
				CheckUnarySignature(diagnostics);
				break;
			case "op_True":
			case "op_False":
				CheckTrueFalseSignature(diagnostics);
				break;
			case "op_Increment":
			case "op_Decrement":
				CheckIncrementDecrementSignature(diagnostics);
				break;
			case "op_LeftShift":
			case "op_RightShift":
				CheckShiftSignature(diagnostics);
				break;
			default:
				CheckBinarySignature(diagnostics);
				break;
			}
		}
	}

	private static bool DoesOperatorHaveCorrectArity(string name, int parameterCount)
	{
		switch (name)
		{
		case "op_Increment":
		case "op_Decrement":
		case "op_UnaryNegation":
		case "op_UnaryPlus":
		case "op_LogicalNot":
		case "op_OnesComplement":
		case "op_True":
		case "op_False":
		case "op_Implicit":
		case "op_Explicit":
			return parameterCount == 1;
		default:
			return parameterCount == 2;
		}
	}

	private void CheckUserDefinedConversionSignature(DiagnosticBag diagnostics)
	{
		if (ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.ERR_OperatorCantReturnVoid, Locations[0]);
		}
		TypeSymbol typeSymbol = ParameterTypes[0];
		TypeSymbol returnType = ReturnType;
		TypeSymbol typeSymbol2 = typeSymbol.StrippedType();
		TypeSymbol typeSymbol3 = returnType.StrippedType();
		if (typeSymbol2.IsInterfaceType() || typeSymbol3.IsInterfaceType())
		{
			diagnostics.Add(ErrorCode.ERR_ConversionWithInterface, Locations[0], this);
			return;
		}
		if (typeSymbol2.TupleUnderlyingTypeOrSelf() != ContainingType && typeSymbol3.TupleUnderlyingTypeOrSelf() != ContainingType && typeSymbol != ContainingType && returnType != ContainingType)
		{
			diagnostics.Add(ErrorCode.ERR_ConversionNotInvolvingContainedType, Locations[0]);
			return;
		}
		if ((ContainingType.SpecialType == SpecialType.System_Nullable_T) ? (typeSymbol == returnType) : (typeSymbol2 == typeSymbol3))
		{
			diagnostics.Add(ErrorCode.ERR_IdentityConversion, Locations[0]);
			return;
		}
		if (typeSymbol.IsDynamic() || returnType.IsDynamic())
		{
			diagnostics.Add(ErrorCode.ERR_BadDynamicConversion, Locations[0], this);
			return;
		}
		TypeSymbol typeSymbol4;
		TypeSymbol typeSymbol5;
		if (typeSymbol2 == ContainingType)
		{
			typeSymbol4 = typeSymbol;
			typeSymbol5 = returnType;
		}
		else
		{
			typeSymbol4 = returnType;
			typeSymbol5 = typeSymbol;
		}
		if (typeSymbol5.IsClassType())
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			if (typeSymbol4.IsDerivedFrom(typeSymbol5, TypeCompareKind.IgnoreTupleNames, ref useSiteDiagnostics))
			{
				diagnostics.Add(ErrorCode.ERR_ConversionWithBase, Locations[0], this);
			}
			else if (typeSymbol5.IsDerivedFrom(typeSymbol4, TypeCompareKind.IgnoreTupleNames, ref useSiteDiagnostics))
			{
				diagnostics.Add(ErrorCode.ERR_ConversionWithDerived, Locations[0], this);
			}
			diagnostics.Add(Locations[0], useSiteDiagnostics);
		}
	}

	private void CheckUnarySignature(DiagnosticBag diagnostics)
	{
		if (ParameterTypes[0].StrippedType().TupleUnderlyingTypeOrSelf() != ContainingType)
		{
			diagnostics.Add(ErrorCode.ERR_BadUnaryOperatorSignature, Locations[0]);
		}
		if (ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.ERR_OperatorCantReturnVoid, Locations[0]);
		}
	}

	private void CheckTrueFalseSignature(DiagnosticBag diagnostics)
	{
		if (ReturnType.SpecialType != SpecialType.System_Boolean)
		{
			diagnostics.Add(ErrorCode.ERR_OpTFRetType, Locations[0]);
		}
		if (ParameterTypes[0].StrippedType().TupleUnderlyingTypeOrSelf() != ContainingType)
		{
			diagnostics.Add(ErrorCode.ERR_BadUnaryOperatorSignature, Locations[0]);
		}
	}

	private void CheckIncrementDecrementSignature(DiagnosticBag diagnostics)
	{
		TypeSymbol type = ParameterTypes[0];
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (type.StrippedType().TupleUnderlyingTypeOrSelf() != ContainingType)
		{
			diagnostics.Add(ErrorCode.ERR_BadIncDecSignature, Locations[0]);
		}
		else if (!ReturnType.EffectiveTypeNoUseSiteDiagnostics.IsEqualToOrDerivedFrom(type, TypeCompareKind.ConsiderEverything, ref useSiteDiagnostics))
		{
			diagnostics.Add(ErrorCode.ERR_BadIncDecRetType, Locations[0]);
		}
		diagnostics.Add(Locations[0], useSiteDiagnostics);
	}

	private void CheckShiftSignature(DiagnosticBag diagnostics)
	{
		if (ParameterTypes[0].StrippedType().TupleUnderlyingTypeOrSelf() != ContainingType || ParameterTypes[1].StrippedType().SpecialType != SpecialType.System_Int32)
		{
			diagnostics.Add(ErrorCode.ERR_BadShiftOperatorSignature, Locations[0]);
		}
		if (ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.ERR_OperatorCantReturnVoid, Locations[0]);
		}
	}

	private void CheckBinarySignature(DiagnosticBag diagnostics)
	{
		if (ParameterTypes[0].StrippedType().TupleUnderlyingTypeOrSelf() != ContainingType && ParameterTypes[1].StrippedType().TupleUnderlyingTypeOrSelf() != ContainingType)
		{
			diagnostics.Add(ErrorCode.ERR_BadBinaryOperatorSignature, Locations[0]);
		}
		if (ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.ERR_OperatorCantReturnVoid, Locations[0]);
		}
	}

	internal sealed override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(GetSyntax().AttributeLists);
	}

	internal sealed override void AfterAddingTypeMembersChecks(ConversionsBase conversions, DiagnosticBag diagnostics)
	{
		ReturnType.CheckAllConstraints(conversions, Locations[0], diagnostics);
		foreach (ParameterSymbol parameter in Parameters)
		{
			parameter.Type.CheckAllConstraints(conversions, parameter.Locations[0], diagnostics);
		}
		ParameterHelpers.EnsureIsReadOnlyAttributeExists(Parameters, diagnostics, modifyCompilationForRefReadOnly: true);
	}
}
