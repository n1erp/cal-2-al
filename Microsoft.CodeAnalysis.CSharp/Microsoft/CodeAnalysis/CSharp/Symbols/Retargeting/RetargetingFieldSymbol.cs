using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingFieldSymbol : WrappedFieldSymbol
{
	private readonly RetargetingModuleSymbol _retargetingModule;

	private ImmutableArray<CustomModifier> _lazyCustomModifiers;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private DiagnosticInfo _lazyUseSiteDiagnostic = CSDiagnosticInfo.EmptyErrorInfo;

	private RetargetingModuleSymbol.RetargetingSymbolTranslator RetargetingTranslator => _retargetingModule.RetargetingTranslator;

	public RetargetingModuleSymbol RetargetingModule => _retargetingModule;

	public override ImmutableArray<CustomModifier> CustomModifiers => RetargetingTranslator.RetargetModifiers(_underlyingField.CustomModifiers, ref _lazyCustomModifiers);

	public override Symbol ContainingSymbol => RetargetingTranslator.Retarget(_underlyingField.ContainingSymbol);

	public override AssemblySymbol ContainingAssembly => _retargetingModule.ContainingAssembly;

	internal override ModuleSymbol ContainingModule => _retargetingModule;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => RetargetingTranslator.Retarget(_underlyingField.MarshallingInformation);

	public override Symbol AssociatedSymbol
	{
		get
		{
			Symbol associatedSymbol = _underlyingField.AssociatedSymbol;
			if ((object)associatedSymbol != null)
			{
				return RetargetingTranslator.Retarget(associatedSymbol);
			}
			return null;
		}
	}

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public RetargetingFieldSymbol(RetargetingModuleSymbol retargetingModule, FieldSymbol underlyingField)
		: base(underlyingField)
	{
		_retargetingModule = retargetingModule;
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return RetargetingTranslator.Retarget(_underlyingField.GetFieldType(fieldsBeingBound), RetargetOptions.RetargetPrimitiveTypesByTypeCode);
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return RetargetingTranslator.GetRetargetedAttributes(_underlyingField.GetAttributes(), ref _lazyCustomAttributes);
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return RetargetingTranslator.RetargetAttributes(_underlyingField.GetCustomAttributesToEmit(moduleBuilder));
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
