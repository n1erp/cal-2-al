using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class ReportDataItemSemanticModel : MemberSemanticModel
{
	public override bool IsSpeculativeSemanticModel => false;

	public override int OriginalPositionForSpeculation => 0;

	private ReportDataItemSemanticModel(Compilation compilation, Symbol owner, Binder rootBinder, SyntaxNode syntax)
		: base(compilation, syntax, owner, rootBinder)
	{
		DebugAssertHelper.Assert((object)owner != null);
		DebugAssertHelper.Assert(owner.Kind == SymbolKind.ReportDataItem);
		DebugAssertHelper.Assert(syntax != null);
	}

	internal static ReportDataItemSemanticModel Create(Compilation compilation, TypeSymbol owner, Binder rootBinder, SyntaxNode syntax)
	{
		ExecutableCodeBinder rootBinder2 = new ExecutableCodeBinder(syntax, owner, rootBinder);
		return new ReportDataItemSemanticModel(compilation, owner, rootBinder2, syntax);
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
		if (node.Kind == SyntaxKind.ReportDataItem)
		{
			return new SymbolInfo(MemberSymbol);
		}
		ReportDataItemSymbol reportDataItemSymbol = MemberSymbol as ReportDataItemSymbol;
		if (reportDataItemSymbol != null && reportDataItemSymbol.RelatedTable != null)
		{
			return new SymbolInfo(reportDataItemSymbol.RelatedTable, CandidateReason.None);
		}
		return base.GetSymbolInfoWorker(node, cancellationToken);
	}
}
