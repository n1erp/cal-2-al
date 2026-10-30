using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingEventSymbol : WrappedEventSymbol
{
	private readonly RetargetingModuleSymbol _retargetingModule;

	private ImmutableArray<EventSymbol> _lazyExplicitInterfaceImplementations;

	private DiagnosticInfo _lazyUseSiteDiagnostic = CSDiagnosticInfo.EmptyErrorInfo;

	private RetargetingModuleSymbol.RetargetingSymbolTranslator RetargetingTranslator => _retargetingModule.RetargetingTranslator;

	public override TypeSymbol Type => RetargetingTranslator.Retarget(_underlyingEvent.Type, RetargetOptions.RetargetPrimitiveTypesByTypeCode);

	public override MethodSymbol AddMethod
	{
		get
		{
			if ((object)_underlyingEvent.AddMethod != null)
			{
				return RetargetingTranslator.Retarget(_underlyingEvent.AddMethod);
			}
			return null;
		}
	}

	public override MethodSymbol RemoveMethod
	{
		get
		{
			if ((object)_underlyingEvent.RemoveMethod != null)
			{
				return RetargetingTranslator.Retarget(_underlyingEvent.RemoveMethod);
			}
			return null;
		}
	}

	internal override FieldSymbol AssociatedField
	{
		get
		{
			if ((object)_underlyingEvent.AssociatedField != null)
			{
				return RetargetingTranslator.Retarget(_underlyingEvent.AssociatedField);
			}
			return null;
		}
	}

	internal override bool IsExplicitInterfaceImplementation => _underlyingEvent.IsExplicitInterfaceImplementation;

	public override ImmutableArray<EventSymbol> ExplicitInterfaceImplementations
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

	public override Symbol ContainingSymbol => RetargetingTranslator.Retarget(_underlyingEvent.ContainingSymbol);

	public override AssemblySymbol ContainingAssembly => _retargetingModule.ContainingAssembly;

	internal override ModuleSymbol ContainingModule => _retargetingModule;

	internal override bool MustCallMethodsDirectly => _underlyingEvent.MustCallMethodsDirectly;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public RetargetingEventSymbol(RetargetingModuleSymbol retargetingModule, EventSymbol underlyingEvent)
		: base(underlyingEvent)
	{
		_retargetingModule = retargetingModule;
	}

	private ImmutableArray<EventSymbol> RetargetExplicitInterfaceImplementations()
	{
		ImmutableArray<EventSymbol> explicitInterfaceImplementations = _underlyingEvent.ExplicitInterfaceImplementations;
		if (explicitInterfaceImplementations.IsEmpty)
		{
			return explicitInterfaceImplementations;
		}
		ArrayBuilder<EventSymbol> instance = ArrayBuilder<EventSymbol>.GetInstance();
		for (int i = 0; i < explicitInterfaceImplementations.Length; i++)
		{
			EventSymbol eventSymbol = RetargetingTranslator.Retarget(explicitInterfaceImplementations[i]);
			if ((object)eventSymbol != null)
			{
				instance.Add(eventSymbol);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _underlyingEvent.GetAttributes();
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return RetargetingTranslator.RetargetAttributes(_underlyingEvent.GetCustomAttributesToEmit(moduleBuilder));
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
