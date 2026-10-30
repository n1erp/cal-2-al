using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TupleMethodSymbol : WrappedMethodSymbol
{
	private readonly TupleTypeSymbol _containingType;

	private readonly MethodSymbol _underlyingMethod;

	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	public override bool IsTupleMethod => true;

	public override MethodSymbol TupleUnderlyingMethod => _underlyingMethod.ConstructedFrom;

	public override MethodSymbol UnderlyingMethod => _underlyingMethod;

	public override Symbol AssociatedSymbol => _containingType.GetTupleMemberSymbolForUnderlyingMember(_underlyingMethod.ConstructedFrom.AssociatedSymbol);

	public override Symbol ContainingSymbol => _containingType;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => _underlyingMethod.ConstructedFrom.ExplicitInterfaceImplementations;

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_lazyParameters.IsDefault)
			{
				InterlockedOperations.Initialize(ref _lazyParameters, CreateParameters());
			}
			return _lazyParameters;
		}
	}

	public override bool ReturnsVoid => _underlyingMethod.ReturnsVoid;

	public override TypeSymbol ReturnType => _underlyingMethod.ReturnType;

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => _underlyingMethod.ReturnTypeCustomModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _underlyingMethod.RefCustomModifiers;

	public override ImmutableArray<TypeSymbol> TypeArguments => _typeParameters.Cast<TypeParameterSymbol, TypeSymbol>();

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	internal override bool IsExplicitInterfaceImplementation => _underlyingMethod.OriginalDefinition.IsExplicitInterfaceImplementation;

	public TupleMethodSymbol(TupleTypeSymbol container, MethodSymbol underlyingMethod)
	{
		_containingType = container;
		TypeMap.Empty.WithAlphaRename(underlyingMethod, this, out _typeParameters);
		_underlyingMethod = underlyingMethod.ConstructIfGeneric(TypeArguments);
	}

	private ImmutableArray<ParameterSymbol> CreateParameters()
	{
		ImmutableArray<ParameterSymbol> parameters = _underlyingMethod.Parameters;
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(parameters.Length);
		foreach (ParameterSymbol item in parameters)
		{
			instance.Add(new TupleParameterSymbol(this, item));
		}
		return instance.ToImmutableAndFree();
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _underlyingMethod.GetAttributes();
	}

	public override ImmutableArray<CSharpAttributeData> GetReturnTypeAttributes()
	{
		return _underlyingMethod.GetReturnTypeAttributes();
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		DiagnosticInfo result = base.GetUseSiteDiagnostic();
		MergeUseSiteDiagnostics(ref result, _underlyingMethod.GetUseSiteDiagnostic());
		return result;
	}

	public override int GetHashCode()
	{
		return _underlyingMethod.ConstructedFrom.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as TupleMethodSymbol);
	}

	public bool Equals(TupleMethodSymbol other)
	{
		if ((object)other == this)
		{
			return true;
		}
		if ((object)other != null && _containingType == other._containingType)
		{
			return _underlyingMethod.ConstructedFrom == other._underlyingMethod.ConstructedFrom;
		}
		return false;
	}
}
