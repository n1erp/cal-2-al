using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class MethodBodySemanticModel : MemberSemanticModel
{
	public override bool IsSpeculativeSemanticModel => false;

	public override int OriginalPositionForSpeculation => 0;

	private MethodBodySemanticModel(Compilation compilation, Symbol owner, Binder rootBinder, SyntaxNode syntax)
		: base(compilation, syntax, owner, rootBinder)
	{
		DebugAssertHelper.Assert((object)owner != null);
		DebugAssertHelper.Assert(owner.Kind == SymbolKind.Method);
		DebugAssertHelper.Assert(syntax != null);
	}

	internal static MethodBodySemanticModel Create(Compilation compilation, MethodSymbol owner, Binder rootBinder, SyntaxNode syntax)
	{
		ExecutableCodeBinder rootBinder2 = new ExecutableCodeBinder(syntax, owner, rootBinder);
		return new MethodBodySemanticModel(compilation, owner, rootBinder2, syntax);
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, MemberSyntax method, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}
}
