using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class LocalFunctionSymbol : SourceMethodSymbol
{
	private readonly Binder _binder;

	private readonly LocalFunctionStatementSyntax _syntax;

	private readonly Symbol _containingSymbol;

	private readonly DeclarationModifiers _declarationModifiers;

	private readonly ImmutableArray<SourceMethodTypeParameterSymbol> _typeParameters;

	private readonly RefKind _refKind;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private bool _lazyIsVarArg;

	private ImmutableArray<TypeParameterConstraintClause> _lazyTypeParameterConstraints;

	private TypeSymbol _lazyReturnType;

	private TypeSymbol _iteratorElementType;

	private readonly DiagnosticBag _declarationDiagnostics;

	internal Binder ScopeBinder { get; }

	public Binder ParameterBinder => _binder;

	public override bool IsVararg
	{
		get
		{
			ComputeParameters();
			return _lazyIsVarArg;
		}
	}

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			ComputeParameters();
			return _lazyParameters;
		}
	}

	public override TypeSymbol ReturnType
	{
		get
		{
			ComputeReturnType();
			return _lazyReturnType;
		}
	}

	public override RefKind RefKind => _refKind;

	public override bool ReturnsVoid
	{
		get
		{
			TypeSymbol returnType = ReturnType;
			if ((object)returnType == null)
			{
				return false;
			}
			return returnType.SpecialType == SpecialType.System_Void;
		}
	}

	public override int Arity => TypeParameters.Length;

	public override ImmutableArray<TypeSymbol> TypeArguments => TypeParameters.Cast<TypeParameterSymbol, TypeSymbol>();

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters.Cast<SourceMethodTypeParameterSymbol, TypeParameterSymbol>();

	public override bool IsExtensionMethod
	{
		get
		{
			ParameterSyntax parameterSyntax = _syntax.ParameterList.Parameters.FirstOrDefault();
			if (parameterSyntax != null && !parameterSyntax.IsArgList)
			{
				return parameterSyntax.Modifiers.Any(SyntaxKind.ThisKeyword);
			}
			return false;
		}
	}

	internal override TypeSymbol IteratorElementType
	{
		get
		{
			return _iteratorElementType;
		}
		set
		{
			Interlocked.CompareExchange(ref _iteratorElementType, value, null);
		}
	}

	public override MethodKind MethodKind => MethodKind.LocalFunction;

	public sealed override Symbol ContainingSymbol => _containingSymbol;

	public override string Name => _syntax.Identifier.ValueText;

	public SyntaxToken NameToken => _syntax.Identifier;

	public Binder SignatureBinder => _binder;

	internal override bool HasSpecialName => false;

	public override bool HidesBaseMethodsByName => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray.Create(_syntax.Identifier.GetLocation());

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(_syntax.GetReference());

	internal override bool GenerateDebugInfo => true;

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal override CallingConvention CallingConvention => CallingConvention.Default;

	internal override bool HasDeclarativeSecurity => false;

	internal override bool RequiresSecurityObject => false;

	public override Symbol AssociatedSymbol => null;

	public override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(_declarationModifiers);

	public override bool IsAsync => (_declarationModifiers & DeclarationModifiers.Async) != 0;

	public override bool IsStatic => (_declarationModifiers & DeclarationModifiers.Static) != 0;

	public override bool IsVirtual => (_declarationModifiers & DeclarationModifiers.Virtual) != 0;

	public override bool IsOverride => (_declarationModifiers & DeclarationModifiers.Override) != 0;

	public override bool IsAbstract => (_declarationModifiers & DeclarationModifiers.Abstract) != 0;

	public override bool IsSealed => (_declarationModifiers & DeclarationModifiers.Sealed) != 0;

	public override bool IsExtern => (_declarationModifiers & DeclarationModifiers.Extern) != 0;

	public bool IsUnsafe => (_declarationModifiers & DeclarationModifiers.Unsafe) != 0;

	internal bool IsExpressionBodied
	{
		get
		{
			if (_syntax.Body == null)
			{
				return _syntax.ExpressionBody != null;
			}
			return false;
		}
	}

	public override ImmutableArray<TypeParameterConstraintClause> TypeParameterConstraintClauses
	{
		get
		{
			if (_lazyTypeParameterConstraints.IsDefault)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				ImmutableArray<TypeParameterConstraintClause> lazyTypeParameterConstraints = this.MakeTypeParameterConstraints(_binder, TypeParameters, _syntax.ConstraintClauses, _syntax.Identifier.GetLocation(), instance);
				lock (_declarationDiagnostics)
				{
					if (_lazyTypeParameterConstraints.IsDefault)
					{
						_declarationDiagnostics.AddRange(instance);
						_lazyTypeParameterConstraints = lazyTypeParameterConstraints;
					}
				}
				instance.Free();
			}
			return _lazyTypeParameterConstraints;
		}
	}

	public LocalFunctionSymbol(Binder binder, Symbol containingSymbol, LocalFunctionStatementSyntax syntax)
	{
		_syntax = syntax;
		_containingSymbol = containingSymbol;
		_declarationDiagnostics = new DiagnosticBag();
		_declarationModifiers = DeclarationModifiers.Static | DeclarationModifiers.Private | syntax.Modifiers.ToDeclarationModifiers(_declarationDiagnostics);
		this.CheckUnsafeModifier(_declarationModifiers, _declarationDiagnostics);
		ScopeBinder = binder;
		binder = binder.WithUnsafeRegionIfNecessary(syntax.Modifiers);
		if (_syntax.TypeParameterList != null)
		{
			binder = new WithMethodTypeParametersBinder(this, binder);
			_typeParameters = MakeTypeParameters(_declarationDiagnostics);
		}
		else
		{
			_typeParameters = ImmutableArray<SourceMethodTypeParameterSymbol>.Empty;
			Symbol.ReportErrorIfHasConstraints(_syntax.ConstraintClauses, _declarationDiagnostics);
		}
		if (IsExtensionMethod)
		{
			_declarationDiagnostics.Add(ErrorCode.ERR_BadExtensionAgg, Locations[0]);
		}
		foreach (ParameterSyntax parameter in syntax.ParameterList.Parameters)
		{
			ReportAttributesDisallowed(parameter.AttributeLists, _declarationDiagnostics);
		}
		if (syntax.ReturnType.Kind() == SyntaxKind.RefType)
		{
			if (((RefTypeSyntax)syntax.ReturnType).ReadOnlyKeyword.Kind() == SyntaxKind.ReadOnlyKeyword)
			{
				_refKind = RefKind.In;
			}
			else
			{
				_refKind = RefKind.Ref;
			}
		}
		else
		{
			_refKind = RefKind.None;
		}
		_binder = binder;
	}

	internal void GetDeclarationDiagnostics(DiagnosticBag addTo)
	{
		foreach (SourceMethodTypeParameterSymbol typeParameter in _typeParameters)
		{
			typeParameter.ForceComplete(null, default);
		}
		ComputeParameters();
		foreach (ParameterSymbol lazyParameter in _lazyParameters)
		{
			lazyParameter.ForceComplete(null, default);
		}
		ComputeReturnType();
		addTo.AddRange(_declarationDiagnostics);
	}

	internal override void AddDeclarationDiagnostics(DiagnosticBag diagnostics)
	{
		_declarationDiagnostics.AddRange(diagnostics);
	}

	private void ComputeParameters()
	{
		if (_lazyParameters != null)
		{
			return;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		ImmutableArray<ParameterSymbol> immutableArray = ParameterHelpers.MakeParameters(_binder, this, _syntax.ParameterList, out var arglistToken, instance, allowRefOrOut: true, allowThis: true, addRefReadOnlyModifier: false);
		ParameterHelpers.EnsureIsReadOnlyAttributeExists(immutableArray, instance, modifyCompilationForRefReadOnly: false);
		bool flag = arglistToken.Kind() == SyntaxKind.ArgListKeyword;
		if (flag)
		{
			instance.Add(ErrorCode.ERR_IllegalVarArgs, arglistToken.GetLocation());
		}
		if (IsAsync)
		{
			SourceOrdinaryMethodSymbol.ReportAsyncParameterErrors(immutableArray, instance, Locations[0]);
		}
		lock (_declarationDiagnostics)
		{
			if (_lazyParameters != null)
			{
				instance.Free();
				return;
			}
			_declarationDiagnostics.AddRangeAndFree(instance);
			_lazyIsVarArg = flag;
			_lazyParameters = immutableArray;
		}
	}

	internal void ComputeReturnType()
	{
		if (_lazyReturnType != null)
		{
			return;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		TypeSyntax syntax = _syntax.ReturnType.SkipRef();
		TypeSymbol typeSymbol = _binder.BindType(syntax, instance);
		if (IsAsync && typeSymbol.SpecialType != SpecialType.System_Void && !typeSymbol.IsNonGenericTaskType(_binder.Compilation) && !typeSymbol.IsGenericTaskType(_binder.Compilation))
		{
			instance.Add(ErrorCode.ERR_BadAsyncReturn, Locations[0]);
		}
		if (_refKind == RefKind.In)
		{
			DeclaringCompilation.EnsureIsReadOnlyAttributeExists(instance, _syntax.ReturnType.Location, modifyCompilationForRefReadOnly: false);
		}
		lock (_declarationDiagnostics)
		{
			if (_lazyReturnType != null)
			{
				instance.Free();
				return;
			}
			_declarationDiagnostics.AddRangeAndFree(instance);
			_lazyReturnType = typeSymbol;
		}
	}

	public override DllImportData GetDllImportData()
	{
		return null;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override bool TryGetThisParameter(out ParameterSymbol thisParameter)
	{
		thisParameter = null;
		return true;
	}

	private static void ReportAttributesDisallowed(SyntaxList<AttributeListSyntax> attributes, DiagnosticBag diagnostics)
	{
		foreach (AttributeListSyntax item in attributes)
		{
			diagnostics.Add(ErrorCode.ERR_AttributesInLocalFuncDecl, item.Location);
		}
	}

	private ImmutableArray<SourceMethodTypeParameterSymbol> MakeTypeParameters(DiagnosticBag diagnostics)
	{
		ArrayBuilder<SourceMethodTypeParameterSymbol> instance = ArrayBuilder<SourceMethodTypeParameterSymbol>.GetInstance();
		SeparatedSyntaxList<TypeParameterSyntax> parameters = _syntax.TypeParameterList.Parameters;
		for (int i = 0; i < parameters.Count; i++)
		{
			TypeParameterSyntax typeParameterSyntax = parameters[i];
			if (typeParameterSyntax.VarianceKeyword.Kind() != SyntaxKind.None)
			{
				diagnostics.Add(ErrorCode.ERR_IllegalVarianceSyntax, typeParameterSyntax.VarianceKeyword.GetLocation());
			}
			ReportAttributesDisallowed(typeParameterSyntax.AttributeLists, diagnostics);
			SyntaxToken identifier = typeParameterSyntax.Identifier;
			Location location = identifier.GetLocation();
			string valueText = identifier.ValueText;
			foreach (SourceMethodTypeParameterSymbol item2 in instance)
			{
				if (valueText == item2.Name)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateTypeParameter, location, valueText);
					break;
				}
			}
			TypeParameterSymbol typeParameterSymbol = ContainingSymbol.FindEnclosingTypeParameter(valueText);
			if ((object)typeParameterSymbol != null)
			{
				ErrorCode code = ((typeParameterSymbol.ContainingSymbol.Kind != SymbolKind.Method) ? ErrorCode.WRN_TypeParameterSameAsOuterTypeParameter : ErrorCode.WRN_TypeParameterSameAsOuterMethodTypeParameter);
				diagnostics.Add(code, location, valueText, typeParameterSymbol.ContainingSymbol);
			}
			SourceMethodTypeParameterSymbol item = new SourceMethodTypeParameterSymbol(this, valueText, i, ImmutableArray.Create(location), ImmutableArray.Create(typeParameterSyntax.GetReference()));
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	public override int GetHashCode()
	{
		return _syntax.GetHashCode();
	}

	public sealed override bool Equals(object symbol)
	{
		if (this == symbol)
		{
			return true;
		}
		if (symbol is LocalFunctionSymbol localFunctionSymbol)
		{
			return localFunctionSymbol._syntax == _syntax;
		}
		return false;
	}
}
