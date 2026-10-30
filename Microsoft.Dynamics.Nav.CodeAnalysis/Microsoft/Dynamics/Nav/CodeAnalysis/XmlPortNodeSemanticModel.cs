using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class XmlPortNodeSemanticModel : MemberSemanticModel
{
	public override bool IsSpeculativeSemanticModel => false;

	public override int OriginalPositionForSpeculation => 0;

	private XmlPortNodeSemanticModel(Compilation compilation, Symbol owner, Binder rootBinder, SyntaxNode syntax)
		: base(compilation, syntax, owner, rootBinder)
	{
		DebugAssertHelper.Assert((object)owner != null);
		DebugAssertHelper.Assert(owner.Kind == SymbolKind.XmlPortNode);
		DebugAssertHelper.Assert(syntax != null);
	}

	protected override SyntaxNode GetBindingRoot(SyntaxNode node)
	{
		return Root;
	}

	internal static XmlPortNodeSemanticModel Create(Compilation compilation, TypeSymbol owner, Binder rootBinder, SyntaxNode syntax)
	{
		ExecutableCodeBinder rootBinder2 = new ExecutableCodeBinder(syntax, owner, rootBinder);
		return new XmlPortNodeSemanticModel(compilation, owner, rootBinder2, syntax);
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, MemberSyntax method, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	internal override SymbolInfo GetSymbolInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (node.Kind.IsXmlPortNode())
		{
			return new SymbolInfo(MemberSymbol);
		}
		SourceXmlPortNodeSymbol sourceXmlPortNodeSymbol = MemberSymbol as SourceXmlPortNodeSymbol;
		if (node.Parent.Kind == SyntaxKind.ObjectReference && (object)sourceXmlPortNodeSymbol != null && sourceXmlPortNodeSymbol.SourceTypeKind == XmlPortSourceTypeKind.Table && sourceXmlPortNodeSymbol.Type.NavTypeKind == NavTypeKind.Record)
		{
			return new SymbolInfo(((RecordTypeSymbol)sourceXmlPortNodeSymbol.Type).OriginalDefinition, CandidateReason.None);
		}
		return base.GetSymbolInfoWorker(node, cancellationToken);
	}
}
