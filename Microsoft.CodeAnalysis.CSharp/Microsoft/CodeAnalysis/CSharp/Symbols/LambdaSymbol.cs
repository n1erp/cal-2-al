using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class LambdaSymbol : SourceMethodSymbol
{
	private readonly Symbol _containingSymbol;

	private readonly MessageID _messageID;

	private readonly SyntaxNode _syntax;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	private RefKind _refKind;

	private TypeSymbol _returnType;

	private readonly bool _isSynthesized;

	private readonly bool _isAsync;

	internal static readonly TypeSymbol InferenceFailureReturnType = new UnsupportedMetadataTypeSymbol();

	public MessageID MessageID => _messageID;

	public override MethodKind MethodKind => MethodKind.AnonymousFunction;

	public override bool IsExtern => false;

	public override bool IsSealed => false;

	public override bool IsAbstract => false;

	public override bool IsVirtual => false;

	public override bool IsOverride => false;

	public override bool IsStatic => false;

	public override bool IsAsync => _isAsync;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal override bool IsMetadataFinal => false;

	public override bool IsVararg => false;

	internal override bool HasSpecialName => false;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal override bool RequiresSecurityObject => false;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal override bool HasDeclarativeSecurity => false;

	public override bool ReturnsVoid
	{
		get
		{
			if ((object)ReturnType != null)
			{
				return ReturnType.SpecialType == SpecialType.System_Void;
			}
			return false;
		}
	}

	public override RefKind RefKind => _refKind;

	public override TypeSymbol ReturnType => _returnType;

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override bool IsExplicitInterfaceImplementation => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override Symbol AssociatedSymbol => null;

	public override ImmutableArray<TypeSymbol> TypeArguments => ImmutableArray<TypeSymbol>.Empty;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override int Arity => 0;

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override ImmutableArray<Location> Locations => ImmutableArray.Create(_syntax.Location);

	internal Location DiagnosticLocation
	{
		get
		{
			switch (_syntax.Kind())
			{
			case SyntaxKind.AnonymousMethodExpression:
				return ((AnonymousMethodExpressionSyntax)_syntax).DelegateKeyword.GetLocation();
			case SyntaxKind.SimpleLambdaExpression:
			case SyntaxKind.ParenthesizedLambdaExpression:
				return ((LambdaExpressionSyntax)_syntax).ArrowToken.GetLocation();
			default:
				return Locations[0];
			}
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(_syntax.GetReference());

	public override Symbol ContainingSymbol => _containingSymbol;

	internal override CallingConvention CallingConvention => CallingConvention.Default;

	public override bool IsExtensionMethod => false;

	public override bool HidesBaseMethodsByName => false;

	public override bool IsImplicitlyDeclared => _isSynthesized;

	internal override bool GenerateDebugInfo => true;

	public override ImmutableArray<TypeParameterConstraintClause> TypeParameterConstraintClauses => ImmutableArray<TypeParameterConstraintClause>.Empty;

	public LambdaSymbol(CSharpCompilation compilation, Symbol containingSymbol, UnboundLambda unboundLambda, ImmutableArray<TypeSymbol> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, RefKind refKind, TypeSymbol returnType, DiagnosticBag diagnostics)
	{
		_containingSymbol = containingSymbol;
		_messageID = unboundLambda.Data.MessageID;
		_syntax = unboundLambda.Syntax;
		_refKind = refKind;
		_returnType = returnType;
		_isSynthesized = unboundLambda.WasCompilerGenerated;
		_isAsync = unboundLambda.IsAsync;
		_parameters = MakeParameters(compilation, unboundLambda, parameterTypes, parameterRefKinds, diagnostics);
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal sealed override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	public override DllImportData GetDllImportData()
	{
		return null;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal void SetInferredReturnType(RefKind refKind, TypeSymbol inferredReturnType)
	{
		_refKind = refKind;
		_returnType = inferredReturnType;
	}

	internal override bool TryGetThisParameter(out ParameterSymbol thisParameter)
	{
		thisParameter = null;
		return true;
	}

	private ImmutableArray<ParameterSymbol> MakeParameters(CSharpCompilation compilation, UnboundLambda unboundLambda, ImmutableArray<TypeSymbol> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, DiagnosticBag diagnostics)
	{
		if (!unboundLambda.HasSignature || unboundLambda.ParameterCount == 0)
		{
			return parameterTypes.SelectAsArray((TypeSymbol type, int ordinal, (LambdaSymbol owner, ImmutableArray<RefKind> refKinds) arg) => SynthesizedParameterSymbol.Create(arg.owner, type, ordinal, arg.refKinds[ordinal], GeneratedNames.LambdaCopyParameterName(ordinal)), (this, parameterRefKinds));
		}
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance();
		bool hasExplicitlyTypedParameterList = unboundLambda.HasExplicitlyTypedParameterList;
		int length = parameterTypes.Length;
		for (int num = 0; num < unboundLambda.ParameterCount; num++)
		{
			TypeSymbol parameterType;
			RefKind refKind;
			if (hasExplicitlyTypedParameterList)
			{
				parameterType = unboundLambda.ParameterType(num);
				refKind = unboundLambda.RefKind(num);
			}
			else if (num < length)
			{
				parameterType = parameterTypes[num];
				refKind = parameterRefKinds[num];
			}
			else
			{
				parameterType = new ExtendedErrorTypeSymbol(compilation, string.Empty, 0, null);
				refKind = RefKind.None;
			}
			string name = unboundLambda.ParameterName(num);
			ImmutableArray<Location> locations = ImmutableArray.Create(unboundLambda.ParameterLocation(num));
			SourceSimpleParameterSymbol item = new SourceSimpleParameterSymbol(this, parameterType, num, refKind, name, locations);
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	public sealed override bool Equals(object symbol)
	{
		if (this == symbol)
		{
			return true;
		}
		if (symbol is LambdaSymbol lambdaSymbol && lambdaSymbol._syntax == _syntax && lambdaSymbol._refKind == _refKind && lambdaSymbol.ReturnType == ReturnType && lambdaSymbol.ParameterTypes.SequenceEqual(ParameterTypes))
		{
			return object.Equals(lambdaSymbol.ContainingSymbol, ContainingSymbol);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _syntax.GetHashCode();
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
