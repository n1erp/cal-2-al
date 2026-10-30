using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceEventFieldSymbol : SourceMemberFieldSymbolFromDeclarator
{
	private readonly SourceEventSymbol _associatedEvent;

	public override bool IsImplicitlyDeclared => true;

	protected override IAttributeTargetSymbol AttributeOwner => _associatedEvent;

	public override Symbol AssociatedSymbol => _associatedEvent;

	internal SourceEventFieldSymbol(SourceEventSymbol associatedEvent, VariableDeclaratorSyntax declaratorSyntax, DiagnosticBag discardedDiagnostics)
		: base(associatedEvent.containingType, declaratorSyntax, (associatedEvent.Modifiers & ~DeclarationModifiers.AccessibilityMask) | DeclarationModifiers.Private, modifierErrors: true, discardedDiagnostics)
	{
		_associatedEvent = associatedEvent;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDebuggerBrowsableNeverAttribute());
	}
}
