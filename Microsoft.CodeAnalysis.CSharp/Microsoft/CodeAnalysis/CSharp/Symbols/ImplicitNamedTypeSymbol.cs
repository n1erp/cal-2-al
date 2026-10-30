using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ImplicitNamedTypeSymbol : SourceMemberContainerTypeSymbol
{
	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics
	{
		get
		{
			if (!IsScriptClass)
			{
				return DeclaringCompilation.GetSpecialType(SpecialType.System_Object);
			}
			return null;
		}
	}

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => ImmutableArray<TypeSymbol>.Empty;

	internal override bool HasTypeArgumentsCustomModifiers => false;

	internal override bool IsComImport => false;

	internal override NamedTypeSymbol ComImportCoClass => null;

	internal override bool HasSpecialName => false;

	internal override bool ShouldAddWinRTMembers => false;

	internal sealed override bool IsWindowsRuntimeImport => false;

	public sealed override bool IsSerializable => false;

	internal sealed override TypeLayout Layout => default;

	internal bool HasStructLayoutAttribute => false;

	internal override CharSet MarshallingCharSet => DefaultMarshallingCharSet;

	internal sealed override bool HasDeclarativeSecurity => false;

	internal override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal override bool HasCodeAnalysisEmbeddedAttribute => false;

	internal ImplicitNamedTypeSymbol(NamespaceOrTypeSymbol containingSymbol, MergedTypeDeclaration declaration, DiagnosticBag diagnostics)
		: base(containingSymbol, declaration, diagnostics)
	{
		state.NotePartComplete(CompletionPart.EnumUnderlyingType);
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		state.NotePartComplete(CompletionPart.Attributes);
		return ImmutableArray<CSharpAttributeData>.Empty;
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		return AttributeUsageInfo.Null;
	}

	protected override Location GetCorrespondingBaseListLocation(NamedTypeSymbol @base)
	{
		return NoLocation.Singleton;
	}

	protected override void CheckBase(DiagnosticBag diagnostics)
	{
		DiagnosticInfo useSiteDiagnostic = DeclaringCompilation.GetSpecialType(SpecialType.System_Object).GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, Locations[0]);
		}
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<Symbol> basesBeingResolved)
	{
		return BaseTypeNoUseSiteDiagnostics;
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<Symbol> basesBeingResolved)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	protected override void CheckInterfaces(DiagnosticBag diagnostics)
	{
	}

	public override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		return GetEmptyTypeArgumentCustomModifiers(ordinal);
	}

	internal sealed override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}
}
