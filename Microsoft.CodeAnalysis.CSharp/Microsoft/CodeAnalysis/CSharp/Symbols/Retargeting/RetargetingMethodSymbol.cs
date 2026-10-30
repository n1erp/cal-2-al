using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingMethodSymbol : WrappedMethodSymbol
{
	private readonly RetargetingModuleSymbol _retargetingModule;

	private readonly MethodSymbol _underlyingMethod;

	private ImmutableArray<TypeParameterSymbol> _lazyTypeParameters;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private CustomModifiersTuple _lazyCustomModifiers;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private ImmutableArray<CSharpAttributeData> _lazyReturnTypeCustomAttributes;

	private ImmutableArray<MethodSymbol> _lazyExplicitInterfaceImplementations;

	private DiagnosticInfo _lazyUseSiteDiagnostic = CSDiagnosticInfo.EmptyErrorInfo;

	private TypeSymbol _lazyReturnType;

	private RetargetingModuleSymbol.RetargetingSymbolTranslator RetargetingTranslator => _retargetingModule.RetargetingTranslator;

	public RetargetingModuleSymbol RetargetingModule => _retargetingModule;

	public override MethodSymbol UnderlyingMethod => _underlyingMethod;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters
	{
		get
		{
			if (_lazyTypeParameters.IsDefault)
			{
				if (!IsGenericMethod)
				{
					_lazyTypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
				}
				else
				{
					ImmutableInterlocked.InterlockedCompareExchange(ref _lazyTypeParameters, RetargetingTranslator.Retarget(_underlyingMethod.TypeParameters), default);
				}
			}
			return _lazyTypeParameters;
		}
	}

	public override ImmutableArray<TypeSymbol> TypeArguments
	{
		get
		{
			if (IsGenericMethod)
			{
				return StaticCast<TypeSymbol>.From(TypeParameters);
			}
			return ImmutableArray<TypeSymbol>.Empty;
		}
	}

	public override bool ReturnsVoid => _underlyingMethod.ReturnsVoid;

	public override TypeSymbol ReturnType
	{
		get
		{
			if ((object)_lazyReturnType == null)
			{
				TypeSymbol type = RetargetingTranslator.Retarget(_underlyingMethod.ReturnType, RetargetOptions.RetargetPrimitiveTypesByTypeCode);
				_lazyReturnType = type.AsDynamicIfNoPia(ContainingType);
			}
			return _lazyReturnType;
		}
	}

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => CustomModifiersTuple.TypeCustomModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => CustomModifiersTuple.RefCustomModifiers;

	private CustomModifiersTuple CustomModifiersTuple => RetargetingTranslator.RetargetModifiers(_underlyingMethod.ReturnTypeCustomModifiers, _underlyingMethod.RefCustomModifiers, ref _lazyCustomModifiers);

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_lazyParameters.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _lazyParameters, RetargetParameters(), default);
			}
			return _lazyParameters;
		}
	}

	public override Symbol AssociatedSymbol
	{
		get
		{
			Symbol associatedSymbol = _underlyingMethod.AssociatedSymbol;
			if ((object)associatedSymbol != null)
			{
				return RetargetingTranslator.Retarget(associatedSymbol);
			}
			return null;
		}
	}

	public override Symbol ContainingSymbol => RetargetingTranslator.Retarget(_underlyingMethod.ContainingSymbol);

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => _retargetingModule.RetargetingTranslator.Retarget(_underlyingMethod.ReturnValueMarshallingInformation);

	public override AssemblySymbol ContainingAssembly => _retargetingModule.ContainingAssembly;

	internal override ModuleSymbol ContainingModule => _retargetingModule;

	internal override bool IsExplicitInterfaceImplementation => _underlyingMethod.IsExplicitInterfaceImplementation;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations
	{
		get
		{
			if (_lazyExplicitInterfaceImplementations.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _lazyExplicitInterfaceImplementations, RetargetExplicitInterfaceImplementations(), default);
			}
			return _lazyExplicitInterfaceImplementations;
		}
	}

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	internal override bool GenerateDebugInfo => false;

	public RetargetingMethodSymbol(RetargetingModuleSymbol retargetingModule, MethodSymbol underlyingMethod)
	{
		_retargetingModule = retargetingModule;
		_underlyingMethod = underlyingMethod;
	}

	private ImmutableArray<ParameterSymbol> RetargetParameters()
	{
		ImmutableArray<ParameterSymbol> parameters = _underlyingMethod.Parameters;
		int length = parameters.Length;
		if (length == 0)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		ParameterSymbol[] array = new ParameterSymbol[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = new RetargetingMethodParameterSymbol(this, parameters[i]);
		}
		return array.AsImmutableOrNull();
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return RetargetingTranslator.GetRetargetedAttributes(_underlyingMethod.GetAttributes(), ref _lazyCustomAttributes);
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return RetargetingTranslator.RetargetAttributes(_underlyingMethod.GetCustomAttributesToEmit(moduleBuilder));
	}

	public override ImmutableArray<CSharpAttributeData> GetReturnTypeAttributes()
	{
		return RetargetingTranslator.GetRetargetedAttributes(_underlyingMethod.GetReturnTypeAttributes(), ref _lazyReturnTypeCustomAttributes);
	}

	private ImmutableArray<MethodSymbol> RetargetExplicitInterfaceImplementations()
	{
		ImmutableArray<MethodSymbol> explicitInterfaceImplementations = _underlyingMethod.ExplicitInterfaceImplementations;
		if (explicitInterfaceImplementations.IsEmpty)
		{
			return explicitInterfaceImplementations;
		}
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		for (int i = 0; i < explicitInterfaceImplementations.Length; i++)
		{
			MethodSymbol methodSymbol = RetargetingTranslator.Retarget(explicitInterfaceImplementations[i], MemberSignatureComparer.RetargetedExplicitImplementationComparer);
			if ((object)methodSymbol != null)
			{
				instance.Add(methodSymbol);
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (_lazyUseSiteDiagnostic == CSDiagnosticInfo.EmptyErrorInfo)
		{
			DiagnosticInfo result = null;
			CalculateUseSiteDiagnostic(ref result);
			_lazyUseSiteDiagnostic = result;
		}
		return _lazyUseSiteDiagnostic;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
