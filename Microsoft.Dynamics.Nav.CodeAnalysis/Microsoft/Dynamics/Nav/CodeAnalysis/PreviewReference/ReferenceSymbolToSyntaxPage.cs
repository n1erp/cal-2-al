using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;

internal class ReferenceSymbolToSyntaxPage : ReferenceSymbolToSyntax
{
	protected override ApplicationObjectSyntax ConvertApplicationSymbol(ISymbol symbol)
	{
		return ReferenceSymbolToSyntax.ConvertApplicationSymbol<PageTypeSymbol>(symbol, ConvertPage);
	}

	private PageSyntax ConvertPage(PageTypeSymbol page)
	{
		PageSyntax pageSyntax = SyntaxFactory.Page(SyntaxFactory.ObjectId(page.Id), page.Name);
		ImmutableArray<Symbol> members = page.GetMembers();
		if (!page.Properties.IsDefaultOrEmpty)
		{
			PageSyntax pageSyntax2 = pageSyntax;
			PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(page.Properties, page);
			pageSyntax = pageSyntax2.AddPropertyListProperties(items);
		}
		SyntaxList<PageAreaSyntax> syntaxList = ConvertPageLayout(members.Where((Symbol m) => m.Kind == SymbolKind.Control).OfType<ControlSymbol>());
		if (!syntaxList.IsEmpty())
		{
			pageSyntax = pageSyntax.WithLayout(SyntaxFactory.PageLayout(syntaxList));
		}
		SyntaxList<PageActionAreaSyntax> syntaxList2 = ConvertPageActionAreas(members.Where((Symbol m) => m.Kind == SymbolKind.Action).OfType<ActionSymbol>());
		if (!syntaxList2.IsEmpty())
		{
			pageSyntax = pageSyntax.WithActions(SyntaxFactory.PageActionList(syntaxList2));
		}
		SyntaxList<PageViewSyntax> syntaxList3 = ReferenceSymbolToSyntax.ConvertPageViews(members.Where((Symbol m) => m.Kind == SymbolKind.View).OfType<ViewSymbol>());
		if (!syntaxList3.IsEmpty())
		{
			pageSyntax = pageSyntax.WithViews(SyntaxFactory.PageViewList(syntaxList3));
		}
		if (members != null && !members.IsEmpty)
		{
			pageSyntax = pageSyntax.WithMembers(ConvertMembers(members));
		}
		return pageSyntax;
	}

	private static SyntaxList<PageActionAreaSyntax> ConvertPageActionAreas(IEnumerable<ActionSymbol> actions)
	{
		SyntaxListBuilder<PageActionAreaSyntax> syntaxListBuilder = SyntaxListBuilder<PageActionAreaSyntax>.Create();
		foreach (ActionSymbol action in actions)
		{
			ImmutableArray<ActionSymbol> actions2 = action.Actions;
			if (action.ActionKind == ActionKind.Area)
			{
				if (!actions2.IsEmpty)
				{
					PageActionAreaSyntax pageActionAreaSyntax = SyntaxFactory.PageActionArea(action.Name);
					pageActionAreaSyntax = pageActionAreaSyntax.WithActions(ReferenceSymbolToSyntax.ConvertPageActions(actions2));
					PageActionAreaSyntax pageActionAreaSyntax2 = pageActionAreaSyntax;
					PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(action.Properties, action);
					pageActionAreaSyntax = pageActionAreaSyntax2.AddPropertyListProperties(items);
					syntaxListBuilder.Add(pageActionAreaSyntax);
				}
				continue;
			}
			throw ExceptionUtilities.UnexpectedValue(action.ActionKind);
		}
		return syntaxListBuilder.ToList();
	}

	private static SyntaxList<PageAreaSyntax> ConvertPageLayout(IEnumerable<ControlSymbol> controls)
	{
		SyntaxListBuilder<PageAreaSyntax> syntaxListBuilder = SyntaxListBuilder<PageAreaSyntax>.Create();
		foreach (ControlSymbol control in controls)
		{
			ImmutableArray<ControlSymbol> controls2 = control.Controls;
			if (control.ControlKind == ControlKind.Area)
			{
				if (!controls2.IsEmpty)
				{
					PageAreaSyntax pageAreaSyntax = SyntaxFactory.PageArea(control.Name);
					pageAreaSyntax = pageAreaSyntax.WithControls(ReferenceSymbolToSyntax.ConvertPageControls(controls2));
					PageAreaSyntax pageAreaSyntax2 = pageAreaSyntax;
					PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(control.Properties, control);
					pageAreaSyntax = pageAreaSyntax2.AddPropertyListProperties(items);
					syntaxListBuilder.Add(pageAreaSyntax);
				}
				continue;
			}
			throw ExceptionUtilities.UnexpectedValue(control.ControlKind);
		}
		return syntaxListBuilder.ToList();
	}
}
