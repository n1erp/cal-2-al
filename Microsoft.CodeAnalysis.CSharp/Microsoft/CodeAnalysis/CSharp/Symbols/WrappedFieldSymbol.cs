using System.Collections.Immutable;
using System.Globalization;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class WrappedFieldSymbol : FieldSymbol
{
	protected readonly FieldSymbol _underlyingField;

	public FieldSymbol UnderlyingField => _underlyingField;

	public override bool IsImplicitlyDeclared => _underlyingField.IsImplicitlyDeclared;

	public override Accessibility DeclaredAccessibility => _underlyingField.DeclaredAccessibility;

	public override string Name => _underlyingField.Name;

	internal override bool HasSpecialName => _underlyingField.HasSpecialName;

	internal override bool HasRuntimeSpecialName => _underlyingField.HasRuntimeSpecialName;

	internal override bool IsNotSerialized => _underlyingField.IsNotSerialized;

	internal override bool IsMarshalledExplicitly => _underlyingField.IsMarshalledExplicitly;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => _underlyingField.MarshallingInformation;

	internal override ImmutableArray<byte> MarshallingDescriptor => _underlyingField.MarshallingDescriptor;

	internal override int? TypeLayoutOffset => _underlyingField.TypeLayoutOffset;

	public override bool IsReadOnly => _underlyingField.IsReadOnly;

	public override bool IsVolatile => _underlyingField.IsVolatile;

	public override bool IsConst => _underlyingField.IsConst;

	internal override ObsoleteAttributeData ObsoleteAttributeData => _underlyingField.ObsoleteAttributeData;

	public override object ConstantValue => _underlyingField.ConstantValue;

	public override ImmutableArray<Location> Locations => _underlyingField.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _underlyingField.DeclaringSyntaxReferences;

	public override bool IsStatic => _underlyingField.IsStatic;

	public WrappedFieldSymbol(FieldSymbol underlyingField)
	{
		_underlyingField = underlyingField;
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _underlyingField.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
	}

	internal override ConstantValue GetConstantValue(ConstantFieldsInProgress inProgress, bool earlyDecodingWellKnownAttributes)
	{
		return _underlyingField.GetConstantValue(inProgress, earlyDecodingWellKnownAttributes);
	}
}
