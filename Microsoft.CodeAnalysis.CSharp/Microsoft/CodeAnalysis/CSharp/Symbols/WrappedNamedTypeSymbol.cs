using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class WrappedNamedTypeSymbol : NamedTypeSymbol
{
	protected readonly NamedTypeSymbol _underlyingType;

	public NamedTypeSymbol UnderlyingNamedType => _underlyingType;

	public override bool IsImplicitlyDeclared => _underlyingType.IsImplicitlyDeclared;

	public override int Arity => _underlyingType.Arity;

	public override bool MightContainExtensionMethods => _underlyingType.MightContainExtensionMethods;

	public override string Name => _underlyingType.Name;

	public override string MetadataName => _underlyingType.MetadataName;

	internal override bool HasSpecialName => _underlyingType.HasSpecialName;

	internal override bool MangleName => _underlyingType.MangleName;

	public override Accessibility DeclaredAccessibility => _underlyingType.DeclaredAccessibility;

	public override TypeKind TypeKind => _underlyingType.TypeKind;

	internal override bool IsInterface => _underlyingType.IsInterface;

	public override ImmutableArray<Location> Locations => _underlyingType.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _underlyingType.DeclaringSyntaxReferences;

	public override bool IsStatic => _underlyingType.IsStatic;

	public override bool IsAbstract => _underlyingType.IsAbstract;

	internal override bool IsMetadataAbstract => _underlyingType.IsMetadataAbstract;

	public override bool IsSealed => _underlyingType.IsSealed;

	internal override bool IsMetadataSealed => _underlyingType.IsMetadataSealed;

	internal override bool HasCodeAnalysisEmbeddedAttribute => _underlyingType.HasCodeAnalysisEmbeddedAttribute;

	internal override ObsoleteAttributeData ObsoleteAttributeData => _underlyingType.ObsoleteAttributeData;

	internal override bool ShouldAddWinRTMembers => _underlyingType.ShouldAddWinRTMembers;

	internal override bool IsWindowsRuntimeImport => _underlyingType.IsWindowsRuntimeImport;

	internal override TypeLayout Layout => _underlyingType.Layout;

	internal override CharSet MarshallingCharSet => _underlyingType.MarshallingCharSet;

	public override bool IsSerializable => _underlyingType.IsSerializable;

	internal override bool IsByRefLikeType => _underlyingType.IsByRefLikeType;

	internal override bool IsReadOnly => _underlyingType.IsReadOnly;

	internal override bool HasDeclarativeSecurity => _underlyingType.HasDeclarativeSecurity;

	public WrappedNamedTypeSymbol(NamedTypeSymbol underlyingType)
	{
		_underlyingType = underlyingType;
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _underlyingType.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		return _underlyingType.GetSecurityInformation();
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return _underlyingType.GetAppliedConditionalSymbols();
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		return _underlyingType.GetAttributeUsageInfo();
	}

	internal override bool GetGuidString(out string guidString)
	{
		return _underlyingType.GetGuidString(out guidString);
	}
}
