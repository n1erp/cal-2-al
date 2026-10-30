using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceChangeModifyViewSymbol : SourceChangeModifySymbol
{
	public override SymbolKind ChangeTargetKind { get; } = SymbolKind.View;

	private new ViewModifyChangeSyntax Syntax => (ViewModifyChangeSyntax)base.Syntax;

	protected override SyntaxNode TargetSyntax => Syntax.Name;

	private SourceChangeModifyViewSymbol(Symbol owner, ViewModifyChangeSyntax syntax, PropertyListSyntax propertyListSyntax)
		: base(owner, syntax, syntax.Name, propertyListSyntax)
	{
	}

	public static SourceChangeModifyViewSymbol Create(Symbol owner, ViewModifyChangeSyntax syntax)
	{
		return new SourceChangeModifyViewSymbol(owner, syntax, syntax.PropertyList);
	}

	protected override ISymbol LazyGetTarget(DiagnosticBag diagnostics)
	{
		PageTypeSymbol targetPage = this.GetTargetPage();
		if (targetPage == null)
		{
			return null;
		}
		return this.FindSymbolInTargetPage(targetPage, Syntax.Name, (Symbol s) => s.Kind == SymbolKind.View, ErrorCode.ERR_ViewNotFound, ErrorCode.INF_PERS_ViewNotFound, diagnostics);
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return null;
	}
}
