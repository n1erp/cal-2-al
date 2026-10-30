using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class ReportColumnSemanticModel : MemberSemanticModel
{
	public override bool IsSpeculativeSemanticModel => false;

	public override int OriginalPositionForSpeculation => 0;

	private ReportColumnSemanticModel(Compilation compilation, Symbol owner, Binder rootBinder, SyntaxNode syntax)
		: base(compilation, syntax, owner, rootBinder)
	{
		DebugAssertHelper.Assert((object)owner != null);
		DebugAssertHelper.Assert(owner.Kind == SymbolKind.ReportColumn);
		DebugAssertHelper.Assert(syntax != null);
	}

	internal static ReportColumnSemanticModel Create(Compilation compilation, TypeSymbol owner, Binder rootBinder, ReportColumnSyntax syntax)
	{
		ExecutableCodeBinder rootBinder2 = new ExecutableCodeBinder(syntax, owner, rootBinder);
		return new ReportColumnSemanticModel(compilation, owner, rootBinder2, syntax);
	}

	internal override BoundNode Bind(Binder binder, SyntaxNode node, DiagnosticBag diagnostics)
	{
		if (node.Kind != SyntaxKind.ReportColumn)
		{
			return base.Bind(binder, node, diagnostics);
		}
		return binder.BindExpression(((ReportColumnSyntax)node).SourceExpression, diagnostics);
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, MemberSyntax method, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	protected override SyntaxNode GetBindingRoot(SyntaxNode node)
	{
		return Root;
	}

	internal override SymbolInfo GetSymbolInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (node.Kind == SyntaxKind.ReportColumn)
		{
			return new SymbolInfo(MemberSymbol);
		}
		return base.GetSymbolInfoWorker(node, cancellationToken);
	}
}
