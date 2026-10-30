using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class WrappedParameterSymbol : ParameterSymbol
{
	protected readonly ParameterSymbol _underlyingParameter;

	public ParameterSymbol UnderlyingParameter => _underlyingParameter;

	public override TypeSymbol Type => _underlyingParameter.Type;

	public sealed override RefKind RefKind => _underlyingParameter.RefKind;

	internal sealed override bool IsMetadataIn => _underlyingParameter.IsMetadataIn;

	internal sealed override bool IsMetadataOut => _underlyingParameter.IsMetadataOut;

	public sealed override ImmutableArray<Location> Locations => _underlyingParameter.Locations;

	public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _underlyingParameter.DeclaringSyntaxReferences;

	internal sealed override ConstantValue ExplicitDefaultConstantValue => _underlyingParameter.ExplicitDefaultConstantValue;

	public override int Ordinal => _underlyingParameter.Ordinal;

	public override bool IsParams => _underlyingParameter.IsParams;

	internal override bool IsMetadataOptional => _underlyingParameter.IsMetadataOptional;

	public override bool IsImplicitlyDeclared => _underlyingParameter.IsImplicitlyDeclared;

	public sealed override string Name => _underlyingParameter.Name;

	public sealed override string MetadataName => _underlyingParameter.MetadataName;

	public override ImmutableArray<CustomModifier> CustomModifiers => _underlyingParameter.CustomModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _underlyingParameter.RefCustomModifiers;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => _underlyingParameter.MarshallingInformation;

	internal override UnmanagedType MarshallingType => _underlyingParameter.MarshallingType;

	internal override bool IsIDispatchConstant => _underlyingParameter.IsIDispatchConstant;

	internal override bool IsIUnknownConstant => _underlyingParameter.IsIUnknownConstant;

	internal override bool IsCallerLineNumber => _underlyingParameter.IsCallerLineNumber;

	internal override bool IsCallerFilePath => _underlyingParameter.IsCallerFilePath;

	internal override bool IsCallerMemberName => _underlyingParameter.IsCallerMemberName;

	protected WrappedParameterSymbol(ParameterSymbol underlyingParameter)
	{
		_underlyingParameter = underlyingParameter;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _underlyingParameter.GetAttributes();
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		_underlyingParameter.AddSynthesizedAttributes(moduleBuilder, ref attributes);
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _underlyingParameter.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
	}
}
