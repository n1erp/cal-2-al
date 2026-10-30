using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceChangeModifyActionSymbol : SourceChangeModifySymbol
{
	public override SymbolKind ChangeTargetKind => SymbolKind.Action;

	private new ActionModifyChangeSyntax Syntax => (ActionModifyChangeSyntax)base.Syntax;

	protected override SyntaxNode TargetSyntax => Syntax.Name;

	private SourceChangeModifyActionSymbol(Symbol owner, ActionModifyChangeSyntax syntax, PropertyListSyntax propertyListSyntax, SyntaxList<TriggerDeclarationSyntax> triggers)
		: base(owner, syntax, syntax.Name, propertyListSyntax, triggers)
	{
	}

	public static SourceChangeModifyActionSymbol Create(Symbol owner, ActionModifyChangeSyntax syntax)
	{
		return new SourceChangeModifyActionSymbol(owner, syntax, syntax.PropertyList, syntax.Triggers);
	}

	protected override ISymbol LazyGetTarget(DiagnosticBag diagnostics)
	{
		PageTypeSymbol targetPage = this.GetTargetPage();
		if (targetPage == null)
		{
			return null;
		}
		return this.FindSymbolInTargetPage(targetPage, Syntax.Name, (Symbol s) => s.Kind == SymbolKind.Action && ((ActionSymbol)s).ActionKind != ActionKind.Area, ErrorCode.ERR_ActionNotFound, ErrorCode.INF_PERS_ActionNotFound, diagnostics);
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		ActionSymbol actionSymbol = Target as ActionSymbol;
		if (actionSymbol == null)
		{
			return null;
		}
		switch (actionSymbol.ActionKind)
		{
		case ActionKind.Action:
			return TriggerDefinitions.GetActionExtensionTriggerInfo(name);
		case ActionKind.Area:
		case ActionKind.Group:
		case ActionKind.Separator:
			return null;
		default:
			throw ExceptionUtilities.UnexpectedValue(actionSymbol.ActionKind);
		}
	}
}
