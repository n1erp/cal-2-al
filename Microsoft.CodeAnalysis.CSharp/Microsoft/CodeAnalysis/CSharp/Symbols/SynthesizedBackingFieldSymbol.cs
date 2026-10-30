using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedBackingFieldSymbol : FieldSymbolWithAttributesAndModifiers
{
	private readonly SourcePropertySymbol _property;

	private readonly string _name;

	internal bool HasInitializer { get; }

	protected override DeclarationModifiers Modifiers { get; }

	protected override IAttributeTargetSymbol AttributeOwner => _property;

	internal override Location ErrorLocation => _property.Location;

	protected override SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList => _property.CSharpSyntaxNode.AttributeLists;

	public override Symbol AssociatedSymbol => _property;

	public override ImmutableArray<Location> Locations => _property.Locations;

	internal override bool HasPointerType => _property.HasPointerType;

	public override string Name => _name;

	public override ImmutableArray<CustomModifier> CustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override Symbol ContainingSymbol => _property.ContainingSymbol;

	public override NamedTypeSymbol ContainingType => _property.ContainingType;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal override bool HasRuntimeSpecialName => false;

	public override bool IsImplicitlyDeclared => true;

	public SynthesizedBackingFieldSymbol(SourcePropertySymbol property, string name, bool isReadOnly, bool isStatic, bool hasInitializer)
	{
		_name = name;
		Modifiers = (DeclarationModifiers)(0x100 | (isReadOnly ? 1024 : 0) | (isStatic ? 4 : 0));
		_property = property;
		HasInitializer = hasInitializer;
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return _property.Type;
	}

	internal sealed override void DecodeWellKnownAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		if (arguments.Attribute.IsTargetAttribute(this, AttributeDescription.FixedBufferAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_DoNotUseFixedBufferAttrOnProperty, arguments.AttributeSyntaxOpt.Name.Location);
		}
		else
		{
			base.DecodeWellKnownAttribute(ref arguments);
		}
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (!ContainingType.IsImplicitlyDeclared)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		}
		Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDebuggerBrowsableNeverAttribute());
	}

	internal override ConstantValue GetConstantValue(ConstantFieldsInProgress inProgress, bool earlyDecodingWellKnownAttributes)
	{
		return null;
	}
}
