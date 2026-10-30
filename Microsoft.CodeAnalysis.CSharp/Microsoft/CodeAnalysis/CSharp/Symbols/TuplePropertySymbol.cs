using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TuplePropertySymbol : WrappedPropertySymbol
{
	private readonly TupleTypeSymbol _containingType;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	public override bool IsTupleProperty => true;

	public override PropertySymbol TupleUnderlyingProperty => _underlyingProperty;

	public override TypeSymbol Type => _underlyingProperty.Type;

	public override ImmutableArray<CustomModifier> TypeCustomModifiers => _underlyingProperty.TypeCustomModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _underlyingProperty.RefCustomModifiers;

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

	public override MethodSymbol GetMethod => _containingType.GetTupleMemberSymbolForUnderlyingMember(_underlyingProperty.GetMethod);

	public override MethodSymbol SetMethod => _containingType.GetTupleMemberSymbolForUnderlyingMember(_underlyingProperty.SetMethod);

	internal override bool IsExplicitInterfaceImplementation => _underlyingProperty.IsExplicitInterfaceImplementation;

	public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations => _underlyingProperty.ExplicitInterfaceImplementations;

	public override Symbol ContainingSymbol => _containingType;

	internal override bool MustCallMethodsDirectly => _underlyingProperty.MustCallMethodsDirectly;

	public TuplePropertySymbol(TupleTypeSymbol container, PropertySymbol underlyingProperty)
		: base(underlyingProperty)
	{
		_containingType = container;
	}

	private ImmutableArray<ParameterSymbol> CreateParameters()
	{
		ImmutableArray<ParameterSymbol> parameters = _underlyingProperty.Parameters;
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(parameters.Length);
		foreach (ParameterSymbol item in parameters)
		{
			instance.Add(new TupleParameterSymbol(this, item));
		}
		return instance.ToImmutableAndFree();
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		DiagnosticInfo result = base.GetUseSiteDiagnostic();
		MergeUseSiteDiagnostics(ref result, _underlyingProperty.GetUseSiteDiagnostic());
		return result;
	}

	public override int GetHashCode()
	{
		return _underlyingProperty.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as TuplePropertySymbol);
	}

	public bool Equals(TuplePropertySymbol other)
	{
		if ((object)other == this)
		{
			return true;
		}
		if ((object)other != null && _containingType == other._containingType)
		{
			return _underlyingProperty == other._underlyingProperty;
		}
		return false;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _underlyingProperty.GetAttributes();
	}
}
