using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceFieldSymbol : FieldSymbolWithAttributesAndModifiers
{
	protected readonly SourceMemberContainerTypeSymbol containingType;

	public abstract override string Name { get; }

	protected override IAttributeTargetSymbol AttributeOwner => this;

	internal sealed override bool RequiresCompletion => true;

	internal bool IsNew => (Modifiers & DeclarationModifiers.New) != 0;

	public override ImmutableArray<CustomModifier> CustomModifiers
	{
		get
		{
			if (!IsVolatile)
			{
				return ImmutableArray<CustomModifier>.Empty;
			}
			return ImmutableArray.Create(CSharpCustomModifier.CreateRequired(ContainingAssembly.GetSpecialType(SpecialType.System_Runtime_CompilerServices_IsVolatile)));
		}
	}

	public sealed override Symbol ContainingSymbol => containingType;

	public override NamedTypeSymbol ContainingType => containingType;

	internal sealed override bool HasRuntimeSpecialName => Name == "value__";

	protected SourceFieldSymbol(SourceMemberContainerTypeSymbol containingType)
	{
		this.containingType = containingType;
	}

	protected void CheckAccessibility(DiagnosticBag diagnostics)
	{
		CSDiagnosticInfo cSDiagnosticInfo = ModifierUtils.CheckAccessibility(Modifiers);
		if (cSDiagnosticInfo != null)
		{
			diagnostics.Add(new CSDiagnostic(cSDiagnosticInfo, ErrorLocation));
		}
	}

	protected void ReportModifiersDiagnostics(DiagnosticBag diagnostics)
	{
		if (ContainingType.IsSealed && DeclaredAccessibility.HasProtected())
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(containingType), ErrorLocation, this);
		}
		else if (IsVolatile && IsReadOnly)
		{
			diagnostics.Add(ErrorCode.ERR_VolatileAndReadonly, ErrorLocation, this);
		}
		else if (containingType.IsStatic && !IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_InstanceMemberInStaticClass, ErrorLocation, this);
		}
		else if (!IsStatic && !IsReadOnly && containingType.IsReadOnly)
		{
			diagnostics.Add(ErrorCode.ERR_FieldsInRoStruct, ErrorLocation);
		}
	}

	internal sealed override void DecodeWellKnownAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		if (arguments.Attribute.IsTargetAttribute(this, AttributeDescription.FixedBufferAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_DoNotUseFixedBufferAttr, arguments.AttributeSyntaxOpt.Name.Location);
		}
		else
		{
			base.DecodeWellKnownAttribute(ref arguments);
		}
	}
}
