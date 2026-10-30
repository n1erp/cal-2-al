using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceChangeModifyControlSymbol : SourceChangeModifySymbol
{
	public override SymbolKind ChangeTargetKind => SymbolKind.Control;

	private new ControlModifyChangeSyntax Syntax => (ControlModifyChangeSyntax)base.Syntax;

	protected override SyntaxNode TargetSyntax => Syntax.Name;

	private SourceChangeModifyControlSymbol(Symbol owner, ControlModifyChangeSyntax syntax, PropertyListSyntax propertyListSyntax, SyntaxList<TriggerDeclarationSyntax> triggers)
		: base(owner, syntax, syntax.Name, propertyListSyntax, triggers)
	{
	}

	public static SourceChangeModifyControlSymbol Create(Symbol owner, ControlModifyChangeSyntax syntax)
	{
		return new SourceChangeModifyControlSymbol(owner, syntax, syntax.PropertyList, syntax.Triggers);
	}

	protected override ISymbol LazyGetTarget(DiagnosticBag diagnostics)
	{
		PageTypeSymbol targetPage = this.GetTargetPage();
		if (targetPage == null)
		{
			return null;
		}
		return this.FindSymbolInTargetPage(targetPage, Syntax.Name, (Symbol s) => s.Kind == SymbolKind.Control && ((ControlSymbol)s).ControlKind != ControlKind.Area, ErrorCode.ERR_ControlNotFound, ErrorCode.INF_PERS_ControlNotFound, diagnostics);
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		ControlSymbol controlSymbol = Target as ControlSymbol;
		if (controlSymbol == null)
		{
			return null;
		}
		switch (controlSymbol.ControlKind)
		{
		case ControlKind.Field:
			return TriggerDefinitions.GetControlExtensionTriggerInfo(name);
		case ControlKind.Area:
		case ControlKind.Group:
		case ControlKind.CueGroup:
		case ControlKind.Repeater:
		case ControlKind.Fixed:
		case ControlKind.Grid:
		case ControlKind.Part:
		case ControlKind.SystemPart:
		case ControlKind.UserControl:
		case ControlKind.ChartPart:
			return null;
		default:
			throw ExceptionUtilities.UnexpectedValue(controlSymbol.ControlKind);
		}
	}

	protected override void CheckModifySymbol(DiagnosticBag diagnostics)
	{
		ViewSymbol containingSymbolOfKind = ContainingSymbol.GetContainingSymbolOfKind<ViewSymbol>(SymbolKind.View);
		if (!(containingSymbolOfKind != null))
		{
			return;
		}
		foreach (PropertySymbol property in Properties)
		{
			BoundPropertyValue propertyValue = property.Property.PropertyValue;
			if (propertyValue.Kind == BoundKind.CodeExpressionPropertyValue)
			{
				BoundCodeExpressionPropertyValue boundCodeExpressionPropertyValue = (BoundCodeExpressionPropertyValue)propertyValue;
				new ViewExpressionVisitor(containingSymbolOfKind, property, diagnostics).Visit(boundCodeExpressionPropertyValue.Expression);
			}
		}
		foreach (MethodSymbol trigger in Triggers)
		{
			diagnostics.Add(ErrorCode.ERR_CannotHaveMethodsOnPageViews, trigger.GetLocation());
		}
	}
}
