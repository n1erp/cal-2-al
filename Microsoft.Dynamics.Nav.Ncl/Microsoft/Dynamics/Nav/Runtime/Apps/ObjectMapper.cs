using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class ObjectMapper
{
	public static Dictionary<string, SyntaxToken> RunObjectTypeMapper = new Dictionary<string, SyntaxToken>
	{
		{
			"Table",
			SyntaxFactory.Token(SyntaxKind.TableKeyword)
		},
		{
			"Report",
			SyntaxFactory.Token(SyntaxKind.ReportKeyword)
		},
		{
			"Codeunit",
			SyntaxFactory.Token(SyntaxKind.CodeunitKeyword)
		},
		{
			"XmlPort",
			SyntaxFactory.Token(SyntaxKind.XmlPortKeyword)
		},
		{
			"Page",
			SyntaxFactory.Token(SyntaxKind.PageKeyword)
		}
	};

	public static Dictionary<string, SyntaxToken> AccessByObjectTypeMapper = new Dictionary<string, SyntaxToken>
	{
		{
			"Table",
			SyntaxFactory.Token(SyntaxKind.TableKeyword)
		},
		{
			"TableData",
			SyntaxFactory.Token(SyntaxKind.TableDataKeyword)
		},
		{
			"Report",
			SyntaxFactory.Token(SyntaxKind.ReportKeyword)
		},
		{
			"Codeunit",
			SyntaxFactory.Token(SyntaxKind.CodeunitKeyword)
		},
		{
			"XmlPort",
			SyntaxFactory.Token(SyntaxKind.XmlPortKeyword)
		},
		{
			"Page",
			SyntaxFactory.Token(SyntaxKind.PageKeyword)
		},
		{
			"Query",
			SyntaxFactory.Token(SyntaxKind.QueryKeyword)
		}
	};

	public static Dictionary<string, Func<IdentifierNameSyntax, IdentifierNameSyntax, PropertyExpressionSyntax>> RunPageLinkExpressionMapper = new Dictionary<string, Func<IdentifierNameSyntax, IdentifierNameSyntax, PropertyExpressionSyntax>>
	{
		{
			"FIELD",
			SyntaxFactory.SimpleFieldExpression
		},
		{
			"FILTER",
			SyntaxFactory.FieldFilterExpression
		},
		{
			"CONST",
			SyntaxFactory.FieldUpperLimitFilterExpression
		}
	};

	public static Dictionary<ControlContainerType, string> AreaKindMapper = new Dictionary<ControlContainerType, string>
	{
		{
			ControlContainerType.ContentArea,
			AreaKind.Content.ToString()
		},
		{
			ControlContainerType.FactBoxArea,
			AreaKind.FactBoxes.ToString()
		},
		{
			ControlContainerType.RoleCenterArea,
			AreaKind.RoleCenter.ToString()
		}
	};

	public static Dictionary<ActionContainerType, string> ActionAreaKindMapper = new Dictionary<ActionContainerType, string>
	{
		{
			ActionContainerType.ActionItems,
			ActionAreaKind.Processing.ToString()
		},
		{
			ActionContainerType.RelatedInformation,
			ActionAreaKind.Navigation.ToString()
		},
		{
			ActionContainerType.Reports,
			ActionAreaKind.Reporting.ToString()
		},
		{
			ActionContainerType.NewDocumentItems,
			ActionAreaKind.Creation.ToString()
		},
		{
			ActionContainerType.HomeItems,
			ActionAreaKind.Embedding.ToString()
		},
		{
			ActionContainerType.ActivityButtons,
			ActionAreaKind.Sections.ToString()
		}
	};
}
