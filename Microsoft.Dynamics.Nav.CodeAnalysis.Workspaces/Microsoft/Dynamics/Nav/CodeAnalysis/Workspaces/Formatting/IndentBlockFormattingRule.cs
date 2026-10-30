using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class IndentBlockFormattingRule : BaseFormattingRule
{
	internal const string Name = "AL IndentBlock Formatting Rule";

	public override void AddIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<IndentBlockOperation> nextOperation)
	{
		nextOperation.Invoke(list);
		AddPropertyIndentationOperation(list, node);
		AddAlignmentBlockOperation(list, node, optionSet);
		AddBlockIndentationOperation(list, node);
		AddEmbeddedStatementsIndentation(list, node);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "optionSet")]
	private void AddAlignmentBlockOperation(List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet)
	{
		if (node == null)
		{
			return;
		}
		if (!(node is VarSectionSyntax varSectionSyntax))
		{
			if (!(node is PropertyListSyntax propertyListSyntax))
			{
				if (!(node is RepeatStatementSyntax repeatStatementSyntax))
				{
					if (!(node is CaseLineSyntax caseLineSyntax))
					{
						if (!(node is CaseElseSyntax caseElseSyntax))
						{
							if (node is StatementSyntax statementSyntax)
							{
								StatementSyntax statementSyntax2 = statementSyntax;
								if (statementSyntax2.Parent.IsKind(SyntaxKind.CaseLine) || statementSyntax2.Parent.IsKind(SyntaxKind.CaseElse))
								{
									SyntaxToken firstToken = statementSyntax2.Parent.GetFirstToken(includeZeroWidth: true);
									SyntaxToken firstToken2 = statementSyntax2.GetFirstToken(includeZeroWidth: true);
									SyntaxToken lastToken = statementSyntax2.GetLastToken(includeZeroWidth: true);
									AddIndentBlockOperation(list, firstToken, firstToken2, lastToken);
								}
							}
						}
						else
						{
							SyntaxToken firstToken3 = caseElseSyntax.Parent.GetFirstToken(includeZeroWidth: true);
							SyntaxToken firstToken4 = caseElseSyntax.GetFirstToken(includeZeroWidth: true);
							SyntaxToken lastToken2 = caseElseSyntax.GetLastToken(includeZeroWidth: true);
							AddIndentBlockOperation(list, firstToken3, firstToken4, lastToken2);
						}
					}
					else
					{
						SyntaxToken firstToken5 = caseLineSyntax.Parent.GetFirstToken(includeZeroWidth: true);
						SyntaxToken firstToken6 = caseLineSyntax.GetFirstToken(includeZeroWidth: true);
						SyntaxToken lastToken3 = caseLineSyntax.GetLastToken(includeZeroWidth: true);
						AddIndentBlockOperation(list, firstToken5, firstToken6, lastToken3);
					}
					return;
				}
				RepeatStatementSyntax repeatStatementSyntax2 = repeatStatementSyntax;
				if (repeatStatementSyntax2.Statements.Count != 0)
				{
					SyntaxList<StatementSyntax> statements = repeatStatementSyntax2.Statements;
					StatementSyntax statementSyntax3 = statements.First();
					if (statementSyntax3.Kind == SyntaxKind.Block)
					{
						AddBlockIndentationOperation(list, statementSyntax3);
						return;
					}
					SyntaxToken firstToken7 = repeatStatementSyntax2.GetFirstToken(includeZeroWidth: true);
					SyntaxToken firstToken8 = statements.First().GetFirstToken(includeZeroWidth: true);
					SyntaxToken lastToken4 = statements.Last().GetLastToken(includeZeroWidth: true);
					AddIndentBlockOperation(list, firstToken7, firstToken8, lastToken4);
				}
			}
			else
			{
				PropertyListSyntax propertyListSyntax2 = propertyListSyntax;
				if (propertyListSyntax2.Properties.Count != 0)
				{
					SyntaxToken firstToken9 = propertyListSyntax2.Parent.GetFirstToken(includeZeroWidth: true);
					SyntaxToken firstToken10 = propertyListSyntax2.Properties.First().GetFirstToken(includeZeroWidth: true);
					SyntaxToken lastToken5 = propertyListSyntax2.Properties.Last().GetLastToken(includeZeroWidth: true);
					AddIndentBlockOperation(list, firstToken9, firstToken10, lastToken5);
				}
			}
			return;
		}
		VarSectionSyntax varSectionSyntax2 = varSectionSyntax;
		if (varSectionSyntax2.Variables.Count != 0)
		{
			SyntaxToken firstToken11 = varSectionSyntax2.GetFirstToken(includeZeroWidth: true);
			SyntaxToken lastToken6 = varSectionSyntax2.GetLastToken(includeZeroWidth: true);
			if (!firstToken11.IsMissing && !firstToken11.Equals(lastToken6))
			{
				SyntaxToken nextToken = firstToken11.GetNextToken(includeZeroWidth: true);
				AddIndentBlockOperation(list, firstToken11, nextToken, lastToken6);
			}
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "property")]
	private void AddPropertyIndentationOperation(List<IndentBlockOperation> list, SyntaxNode node)
	{
		if (node is PropertySyntax)
		{
			AddIndentBlockOperation(list, node.Parent.Parent.GetFirstToken(), node.GetFirstToken(includeZeroWidth: true), node.GetLastToken(includeZeroWidth: true), IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine);
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private void SetAlignmentBlockOperation(List<IndentBlockOperation> list, SyntaxNode baseNode, SyntaxNode body)
	{
		SyntaxToken firstToken = baseNode.GetFirstToken(includeZeroWidth: true);
		SyntaxToken firstToken2 = body.GetFirstToken(includeZeroWidth: true);
		SyntaxToken lastToken = body.GetLastToken(includeZeroWidth: true);
		SetAlignmentBlockOperation(list, firstToken, firstToken2, lastToken, IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine);
	}

	private void AddBlockIndentationOperation(List<IndentBlockOperation> list, SyntaxNode node)
	{
		(SyntaxToken, SyntaxToken) scopeDelimiters = node.GetScopeDelimiters();
		AddIndentBlockOperation(list, scopeDelimiters.Item1.GetNextToken(includeZeroWidth: true), scopeDelimiters.Item2.GetPreviousToken(includeZeroWidth: true));
	}

	private void AddEmbeddedStatementsIndentation(List<IndentBlockOperation> list, SyntaxNode node)
	{
		if (node is IfStatementSyntax ifStatementSyntax)
		{
			if (ifStatementSyntax.Statement != null && !(ifStatementSyntax.Statement is BlockSyntax))
			{
				AddEmbeddedStatementsIndentationOperation(list, ifStatementSyntax.Statement);
			}
			if (ifStatementSyntax.ElseStatement != null && !(ifStatementSyntax.ElseStatement is BlockSyntax))
			{
				AddEmbeddedStatementsIndentationOperation(list, ifStatementSyntax.ElseStatement);
			}
		}
		else if (node is WhileStatementSyntax { Statement: not null } whileStatementSyntax && !(whileStatementSyntax.Statement is BlockSyntax))
		{
			AddEmbeddedStatementsIndentationOperation(list, whileStatementSyntax.Statement);
		}
		else if (node is ForStatementSyntax { Statement: not null } forStatementSyntax && !(forStatementSyntax.Statement is BlockSyntax))
		{
			AddEmbeddedStatementsIndentationOperation(list, forStatementSyntax.Statement);
		}
		else if (node is ForEachStatementSyntax { Statement: not null } forEachStatementSyntax && !(forEachStatementSyntax.Statement is BlockSyntax))
		{
			AddEmbeddedStatementsIndentationOperation(list, forEachStatementSyntax.Statement);
		}
		else if (node is WithStatementSyntax { Statement: not null } withStatementSyntax && !(withStatementSyntax.Statement is BlockSyntax))
		{
			AddEmbeddedStatementsIndentationOperation(list, withStatementSyntax.Statement);
		}
	}

	private void AddEmbeddedStatementsIndentationOperation(List<IndentBlockOperation> list, SyntaxNode statement)
	{
		SyntaxToken firstToken = statement.GetFirstToken(includeZeroWidth: true);
		SyntaxToken lastToken = statement.GetLastToken(includeZeroWidth: true);
		if (lastToken.IsMissing)
		{
			AddIndentBlockOperation(list, firstToken, lastToken);
		}
		else
		{
			AddIndentBlockOperation(list, firstToken, lastToken, TextSpan.FromBounds(firstToken.FullSpan.Start, lastToken.FullSpan.End));
		}
	}
}
