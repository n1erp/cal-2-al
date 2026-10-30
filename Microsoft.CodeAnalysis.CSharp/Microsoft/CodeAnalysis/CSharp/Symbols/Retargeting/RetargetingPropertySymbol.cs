using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingPropertySymbol : WrappedPropertySymbol
{
	private readonly RetargetingModuleSymbol _retargetingModule;

	private ImmutableArray<PropertySymbol> _lazyExplicitInterfaceImplementations;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private CustomModifiersTuple _lazyCustomModifiers;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private DiagnosticInfo _lazyUseSiteDiagnostic = CSDiagnosticInfo.EmptyErrorInfo;

	private TypeSymbol _lazyType;

	private RetargetingModuleSymbol.RetargetingSymbolTranslator RetargetingTranslator => _retargetingModule.RetargetingTranslator;

	public RetargetingModuleSymbol RetargetingModule => _retargetingModule;

	public override TypeSymbol Type
	{
		get
		{
			if ((object)_lazyType == null)
			{
				TypeSymbol type = RetargetingTranslator.Retarget(_underlyingProperty.Type, RetargetOptions.RetargetPrimitiveTypesByTypeCode);
				_lazyType = type.AsDynamicIfNoPia(ContainingType);
			}
			return _lazyType;
		}
	}

	public override ImmutableArray<CustomModifier> TypeCustomModifiers => CustomModifiersTuple.TypeCustomModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => CustomModifiersTuple.RefCustomModifiers;

	private CustomModifiersTuple CustomModifiersTuple => RetargetingTranslator.RetargetModifiers(_underlyingProperty.TypeCustomModifiers, _underlyingProperty.RefCustomModifiers, ref _lazyCustomModifiers);

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

	public override MethodSymbol GetMethod
	{
		get
		{
			if ((object)_underlyingProperty.GetMethod != null)
			{
				return RetargetingTranslator.Retarget(_underlyingProperty.GetMethod);
			}
			return null;
		}
	}

	public override MethodSymbol SetMethod
	{
		get
		{
			if ((object)_underlyingProperty.SetMethod != null)
			{
				return RetargetingTranslator.Retarget(_underlyingProperty.SetMethod);
			}
			return null;
		}
	}

	internal override bool IsExplicitInterfaceImplementation => _underlyingProperty.IsExplicitInterfaceImplementation;

	public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations
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

	public override Symbol ContainingSymbol => RetargetingTranslator.Retarget(_underlyingProperty.ContainingSymbol);

	public override AssemblySymbol ContainingAssembly => _retargetingModule.ContainingAssembly;

	internal override ModuleSymbol ContainingModule => _retargetingModule;

	internal override bool MustCallMethodsDirectly => _underlyingProperty.MustCallMethodsDirectly;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public RetargetingPropertySymbol(RetargetingModuleSymbol retargetingModule, PropertySymbol underlyingProperty)
		: base(underlyingProperty)
	{
		_retargetingModule = retargetingModule;
	}

	private ImmutableArray<ParameterSymbol> RetargetParameters()
	{
		ImmutableArray<ParameterSymbol> parameters = _underlyingProperty.Parameters;
		int length = parameters.Length;
		if (length == 0)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		ParameterSymbol[] array = new ParameterSymbol[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = new RetargetingPropertyParameterSymbol(this, parameters[i]);
		}
		return array.AsImmutableOrNull();
	}

	private ImmutableArray<PropertySymbol> RetargetExplicitInterfaceImplementations()
	{
		ImmutableArray<PropertySymbol> explicitInterfaceImplementations = _underlyingProperty.ExplicitInterfaceImplementations;
		if (explicitInterfaceImplementations.IsEmpty)
		{
			return explicitInterfaceImplementations;
		}
		ArrayBuilder<PropertySymbol> instance = ArrayBuilder<PropertySymbol>.GetInstance();
		for (int i = 0; i < explicitInterfaceImplementations.Length; i++)
		{
			PropertySymbol propertySymbol = RetargetingTranslator.Retarget(explicitInterfaceImplementations[i], MemberSignatureComparer.RetargetedExplicitImplementationComparer);
			if ((object)propertySymbol != null)
			{
				instance.Add(propertySymbol);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return RetargetingTranslator.GetRetargetedAttributes(_underlyingProperty.GetAttributes(), ref _lazyCustomAttributes);
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return RetargetingTranslator.RetargetAttributes(_underlyingProperty.GetCustomAttributesToEmit(moduleBuilder));
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
}
