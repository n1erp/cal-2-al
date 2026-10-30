using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;

internal class ReferenceSymbolToSyntaxPageExtension : ReferenceSymbolToSyntax
{
	protected override ApplicationObjectSyntax ConvertApplicationSymbol(ISymbol symbol)
	{
		return ReferenceSymbolToSyntax.ConvertApplicationSymbol<PageExtensionTypeSymbol>(symbol, ConvertPageExtension);
	}

	private PageExtensionSyntax ConvertPageExtension(PageExtensionTypeSymbol pageExtension)
	{
		PageExtensionSyntax pageExtensionSyntax = SyntaxFactory.PageExtension(SyntaxFactory.ObjectId(pageExtension.Id), pageExtension.Name, SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(pageExtension.Target.Name)));
		ImmutableArray<Symbol> members = pageExtension.GetMembers();
		if (!pageExtension.Properties.IsDefaultOrEmpty)
		{
			PageExtensionSyntax pageExtensionSyntax2 = pageExtensionSyntax;
			PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(pageExtension.Properties, pageExtension);
			pageExtensionSyntax = pageExtensionSyntax2.AddPropertyListProperties(items);
		}
		SyntaxList<ControlChangeBaseSyntax> changes = ConvertPageExtensionControls(members.Where(IsControlChange).OfType<ChangeSymbol>());
		if (changes.Any())
		{
			pageExtensionSyntax = pageExtensionSyntax.WithLayout(SyntaxFactory.PageExtensionLayout(changes));
		}
		if (members != null && !members.IsEmpty)
		{
			pageExtensionSyntax = pageExtensionSyntax.WithMembers(ConvertMembers(members));
		}
		return pageExtensionSyntax;
	}

	private static bool IsControlChange(Symbol symbol)
	{
		if (!(symbol is ReferenceChangeAddControlSymbol))
		{
			return symbol is SourceChangeAddControlSymbol;
		}
		return true;
	}

	internal static SyntaxList<ControlChangeBaseSyntax> ConvertPageExtensionControls(IEnumerable<ChangeSymbol> controls)
	{
		SyntaxListBuilder<ControlChangeBaseSyntax> syntaxListBuilder = SyntaxListBuilder<ControlChangeBaseSyntax>.Create();
		foreach (ChangeSymbol control in controls)
		{
			if (control is ChangeAddSymbol changeAddSymbol)
			{
				ControlAddChangeSyntax controlAddChangeSyntax = SyntaxFactory.ControlAddChange(KindToToken(changeAddSymbol.ChangeKind), changeAddSymbol.Anchor.Name);
				SyntaxList<ControlBaseSyntax> controls2 = ReferenceSymbolToSyntax.ConvertPageControls(changeAddSymbol.AddedSymbols.OfType<ControlSymbol>());
				controlAddChangeSyntax = controlAddChangeSyntax.WithControls(controls2);
				syntaxListBuilder.Add(controlAddChangeSyntax);
			}
			else if (control is ChangeMoveSymbol changeMoveSymbol)
			{
				ControlMoveChangeSyntax node = SyntaxFactory.ControlMoveChange(KindToToken(changeMoveSymbol.ChangeKind), SyntaxFactory.IdentifierName(changeMoveSymbol.Anchor.Name), ReferenceSymbolToSyntax.ConvertPageControlIdentifiers(changeMoveSymbol.MovedSymbols.OfType<ControlSymbol>()));
				syntaxListBuilder.Add(node);
			}
			else if (control is ChangeModifySymbol changeModifySymbol)
			{
				ControlModifyChangeSyntax controlModifyChangeSyntax = SyntaxFactory.ControlModifyChange(changeModifySymbol.Target.Name);
				ControlModifyChangeSyntax controlModifyChangeSyntax2 = controlModifyChangeSyntax;
				PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(changeModifySymbol.Properties, changeModifySymbol);
				controlModifyChangeSyntax = controlModifyChangeSyntax2.AddPropertyListProperties(items);
				syntaxListBuilder.Add(controlModifyChangeSyntax);
			}
		}
		return syntaxListBuilder.ToList();
	}

	private static SyntaxToken KindToToken(ChangeKind kind)
	{
		return kind switch
		{
			ChangeKind.AddAfter => SyntaxFactory.Token(SyntaxKind.AddAfterKeyword), 
			ChangeKind.AddBefore => SyntaxFactory.Token(SyntaxKind.AddBeforeKeyword), 
			ChangeKind.AddFirst => SyntaxFactory.Token(SyntaxKind.AddFirstKeyword), 
			ChangeKind.AddLast => SyntaxFactory.Token(SyntaxKind.AddLastKeyword), 
			ChangeKind.MoveAfter => SyntaxFactory.Token(SyntaxKind.MoveAfterKeyword), 
			ChangeKind.MoveBefore => SyntaxFactory.Token(SyntaxKind.MoveBeforeKeyword), 
			ChangeKind.MoveFirst => SyntaxFactory.Token(SyntaxKind.MoveFirstKeyword), 
			ChangeKind.MoveLast => SyntaxFactory.Token(SyntaxKind.MoveLastKeyword), 
			_ => default, 
		};
	}
}
