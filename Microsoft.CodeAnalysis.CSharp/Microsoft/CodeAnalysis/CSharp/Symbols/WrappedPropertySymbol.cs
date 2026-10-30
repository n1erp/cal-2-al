using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class WrappedPropertySymbol : PropertySymbol
{
	protected readonly PropertySymbol _underlyingProperty;

	public PropertySymbol UnderlyingProperty => _underlyingProperty;

	public override bool IsImplicitlyDeclared => _underlyingProperty.IsImplicitlyDeclared;

	public override RefKind RefKind => _underlyingProperty.RefKind;

	public override bool IsIndexer => _underlyingProperty.IsIndexer;

	internal override CallingConvention CallingConvention => _underlyingProperty.CallingConvention;

	public override string Name => _underlyingProperty.Name;

	internal override bool HasSpecialName => _underlyingProperty.HasSpecialName;

	public override ImmutableArray<Location> Locations => _underlyingProperty.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _underlyingProperty.DeclaringSyntaxReferences;

	public override Accessibility DeclaredAccessibility => _underlyingProperty.DeclaredAccessibility;

	public override bool IsStatic => _underlyingProperty.IsStatic;

	public override bool IsVirtual => _underlyingProperty.IsVirtual;

	public override bool IsOverride => _underlyingProperty.IsOverride;

	public override bool IsAbstract => _underlyingProperty.IsAbstract;

	public override bool IsSealed => _underlyingProperty.IsSealed;

	public override bool IsExtern => _underlyingProperty.IsExtern;

	internal override ObsoleteAttributeData ObsoleteAttributeData => _underlyingProperty.ObsoleteAttributeData;

	public override string MetadataName => _underlyingProperty.MetadataName;

	internal override bool HasRuntimeSpecialName => _underlyingProperty.HasRuntimeSpecialName;

	public WrappedPropertySymbol(PropertySymbol underlyingProperty)
	{
		_underlyingProperty = underlyingProperty;
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _underlyingProperty.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
	}
}
