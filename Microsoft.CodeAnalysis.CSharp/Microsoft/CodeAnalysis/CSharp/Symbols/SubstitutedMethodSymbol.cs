using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SubstitutedMethodSymbol : WrappedMethodSymbol
{
	private readonly NamedTypeSymbol _containingType;

	private readonly MethodSymbol _underlyingMethod;

	private readonly TypeMap _inputMap;

	private readonly MethodSymbol _constructedFrom;

	private TypeSymbol _lazyReturnType;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeMap _lazyMap;

	private ImmutableArray<TypeParameterSymbol> _lazyTypeParameters;

	private ImmutableArray<MethodSymbol> _lazyExplicitInterfaceImplementations;

	private OverriddenOrHiddenMembersResult _lazyOverriddenOrHiddenMembers;

	private int _hashCode;

	public override MethodSymbol UnderlyingMethod => _underlyingMethod;

	public override MethodSymbol ConstructedFrom => _constructedFrom;

	private TypeMap Map
	{
		get
		{
			EnsureMapAndTypeParameters();
			return _lazyMap;
		}
	}

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters
	{
		get
		{
			EnsureMapAndTypeParameters();
			return _lazyTypeParameters;
		}
	}

	public sealed override AssemblySymbol ContainingAssembly => OriginalDefinition.ContainingAssembly;

	public override ImmutableArray<TypeSymbol> TypeArguments => TypeParameters.Cast<TypeParameterSymbol, TypeSymbol>();

	public sealed override MethodSymbol OriginalDefinition => _underlyingMethod;

	internal sealed override MethodSymbol CallsiteReducedFromMethod => OriginalDefinition.ReducedFrom?.Construct(TypeArguments);

	public override TypeSymbol ReceiverType
	{
		get
		{
			MethodSymbol callsiteReducedFromMethod = CallsiteReducedFromMethod;
			if ((object)callsiteReducedFromMethod == null)
			{
				return ContainingType;
			}
			return callsiteReducedFromMethod.Parameters[0].Type;
		}
	}

	public sealed override MethodSymbol ReducedFrom => OriginalDefinition.ReducedFrom;

	public sealed override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public sealed override Symbol AssociatedSymbol => OriginalDefinition.AssociatedSymbol?.SymbolAsMember(ContainingType);

	public sealed override bool ReturnsVoid => OriginalDefinition.ReturnsVoid;

	public sealed override TypeSymbol ReturnType
	{
		get
		{
			TypeSymbol lazyReturnType = _lazyReturnType;
			if (lazyReturnType != null)
			{
				return lazyReturnType;
			}
			lazyReturnType = Map.SubstituteTypeWithTupleUnification(OriginalDefinition.ReturnType).Type;
			return Interlocked.CompareExchange(ref _lazyReturnType, lazyReturnType, null) ?? lazyReturnType;
		}
	}

	public sealed override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => Map.SubstituteCustomModifiers(OriginalDefinition.ReturnType, OriginalDefinition.ReturnTypeCustomModifiers);

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => Map.SubstituteCustomModifiers(OriginalDefinition.RefCustomModifiers);

	public sealed override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_lazyParameters.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _lazyParameters, SubstituteParameters(), default);
			}
			return _lazyParameters;
		}
	}

	internal sealed override bool IsExplicitInterfaceImplementation => OriginalDefinition.IsExplicitInterfaceImplementation;

	public sealed override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations
	{
		get
		{
			if ((object)ConstructedFrom != this)
			{
				return ImmutableArray<MethodSymbol>.Empty;
			}
			if (_lazyExplicitInterfaceImplementations.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _lazyExplicitInterfaceImplementations, ExplicitInterfaceHelpers.SubstituteExplicitInterfaceImplementations(OriginalDefinition.ExplicitInterfaceImplementations, Map), default);
			}
			return _lazyExplicitInterfaceImplementations;
		}
	}

	internal sealed override OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers
	{
		get
		{
			if (_lazyOverriddenOrHiddenMembers == null)
			{
				Interlocked.CompareExchange(ref _lazyOverriddenOrHiddenMembers, this.MakeOverriddenOrHiddenMembers(), null);
			}
			return _lazyOverriddenOrHiddenMembers;
		}
	}

	internal sealed override TypeMap TypeSubstitution => Map;

	internal SubstitutedMethodSymbol(NamedTypeSymbol containingSymbol, MethodSymbol originalDefinition)
		: this(containingSymbol, containingSymbol.TypeSubstitution, originalDefinition, null)
	{
	}

	protected SubstitutedMethodSymbol(NamedTypeSymbol containingSymbol, TypeMap map, MethodSymbol originalDefinition, MethodSymbol constructedFrom)
	{
		_containingType = containingSymbol;
		_underlyingMethod = originalDefinition;
		_inputMap = map;
		if ((object)constructedFrom != null)
		{
			_constructedFrom = constructedFrom;
			_lazyTypeParameters = constructedFrom.TypeParameters;
			_lazyMap = map;
		}
		else
		{
			_constructedFrom = this;
		}
	}

	private void EnsureMapAndTypeParameters()
	{
		if (_lazyTypeParameters.IsDefault)
		{
			TypeMap value = _inputMap.WithAlphaRename(OriginalDefinition, this, out var newTypeParameters);
			TypeMap typeMap = Interlocked.CompareExchange(ref _lazyMap, value, null);
			if (typeMap != null)
			{
				newTypeParameters = typeMap.SubstituteTypeParameters(OriginalDefinition.TypeParameters);
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyTypeParameters, newTypeParameters, default);
		}
	}

	public override TypeSymbol GetTypeInferredDuringReduction(TypeParameterSymbol reducedFromTypeParameter)
	{
		OriginalDefinition.GetTypeInferredDuringReduction(reducedFromTypeParameter);
		return TypeArguments[reducedFromTypeParameter.Ordinal];
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return OriginalDefinition.GetAttributes();
	}

	public override ImmutableArray<CSharpAttributeData> GetReturnTypeAttributes()
	{
		return OriginalDefinition.GetReturnTypeAttributes();
	}

	internal sealed override bool CallsAreOmitted(SyntaxTree syntaxTree)
	{
		return OriginalDefinition.CallsAreOmitted(syntaxTree);
	}

	internal sealed override bool TryGetThisParameter(out ParameterSymbol thisParameter)
	{
		if (!OriginalDefinition.TryGetThisParameter(out var thisParameter2))
		{
			thisParameter = null;
			return false;
		}
		thisParameter = (((object)thisParameter2 != null) ? new ThisParameterSymbol(this) : null);
		return true;
	}

	private ImmutableArray<ParameterSymbol> SubstituteParameters()
	{
		ImmutableArray<ParameterSymbol> parameters = OriginalDefinition.Parameters;
		int length = parameters.Length;
		if (length == 0)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(length);
		TypeMap map = Map;
		foreach (ParameterSymbol item in parameters)
		{
			instance.Add(new SubstitutedParameterSymbol(this, map, item));
		}
		return instance.ToImmutableAndFree();
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable;
	}

	private int ComputeHashCode()
	{
		int hashCode = OriginalDefinition.GetHashCode();
		hashCode = Hash.Combine(ContainingType, hashCode);
		if ((object)ConstructedFrom != this)
		{
			foreach (TypeSymbol typeArgument in TypeArguments)
			{
				hashCode = Hash.Combine(typeArgument, hashCode);
			}
		}
		return hashCode;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is SubstitutedMethodSymbol substitutedMethodSymbol))
		{
			return false;
		}
		if ((object)OriginalDefinition != substitutedMethodSymbol.OriginalDefinition && OriginalDefinition != substitutedMethodSymbol.OriginalDefinition)
		{
			return false;
		}
		if (ContainingType != substitutedMethodSymbol.ContainingType)
		{
			return false;
		}
		bool flag = (object)this == ConstructedFrom;
		bool flag2 = (object)substitutedMethodSymbol == substitutedMethodSymbol.ConstructedFrom;
		if (flag | flag2)
		{
			return flag & flag2;
		}
		int arity = Arity;
		for (int i = 0; i < arity; i++)
		{
			if (TypeArguments[i] != substitutedMethodSymbol.TypeArguments[i])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = _hashCode;
		if (num == 0)
		{
			num = ComputeHashCode();
			if (num == 0)
			{
				num++;
			}
			_hashCode = num;
		}
		return num;
	}
}
