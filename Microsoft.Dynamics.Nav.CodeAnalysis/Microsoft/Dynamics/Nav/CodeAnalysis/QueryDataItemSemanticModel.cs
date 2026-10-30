using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class QueryDataItemSemanticModel : MemberSemanticModel
{
	public override bool IsSpeculativeSemanticModel => false;

	public override int OriginalPositionForSpeculation => 0;

	private QueryDataItemSemanticModel(Compilation compilation, Symbol owner, Binder rootBinder, SyntaxNode syntax)
		: base(compilation, syntax, owner, rootBinder)
	{
		DebugAssertHelper.Assert((object)owner != null);
		DebugAssertHelper.Assert(owner.Kind == SymbolKind.QueryDataItem);
		DebugAssertHelper.Assert(syntax != null);
	}

	internal static QueryDataItemSemanticModel Create(Compilation compilation, TypeSymbol owner, Binder rootBinder, SyntaxNode syntax)
	{
		ExecutableCodeBinder rootBinder2 = new ExecutableCodeBinder(syntax, owner, rootBinder);
		return new QueryDataItemSemanticModel(compilation, owner, rootBinder2, syntax);
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, MemberSyntax method, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	internal override SymbolInfo GetSymbolInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (node.Kind == SyntaxKind.QueryDataItem)
		{
			return new SymbolInfo(MemberSymbol);
		}
		SourceQueryDataItemSymbol sourceQueryDataItemSymbol = MemberSymbol as SourceQueryDataItemSymbol;
		if (sourceQueryDataItemSymbol != null && sourceQueryDataItemSymbol.RelatedTable != null)
		{
			return new SymbolInfo(sourceQueryDataItemSymbol.RelatedTable.OriginalDefinition, CandidateReason.None);
		}
		return base.GetSymbolInfoWorker(node, cancellationToken);
	}

	protected override SyntaxNode GetBindingRoot(SyntaxNode node)
	{
		return Root;
	}
}
