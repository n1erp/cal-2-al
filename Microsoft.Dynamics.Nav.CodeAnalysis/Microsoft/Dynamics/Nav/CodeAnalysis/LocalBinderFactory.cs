using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class LocalBinderFactory : SyntaxVisitor
{
	private readonly SmallDictionary<SyntaxNode, Binder> map;

	private readonly MethodSymbol method;

	private Binder enclosing;

	private void Visit(SyntaxNode syntax, Binder enclosingParameter)
	{
		if (enclosing == enclosingParameter)
		{
			Visit(syntax);
			return;
		}
		Binder binder = enclosing;
		enclosing = enclosingParameter;
		Visit(syntax);
		enclosing = binder;
	}

	public static SmallDictionary<SyntaxNode, Binder> BuildMap(MethodSymbol method, SyntaxNode syntax, Binder enclosing)
	{
		LocalBinderFactory localBinderFactory = new LocalBinderFactory(method, enclosing);
		localBinderFactory.Visit(syntax);
		return localBinderFactory.map;
	}

	private LocalBinderFactory(MethodSymbol method, Binder enclosing)
	{
		DebugAssertHelper.Assert((object)method != null);
		map = new SmallDictionary<SyntaxNode, Binder>(ReferenceEqualityComparer.Instance);
		this.method = method;
		this.enclosing = enclosing;
	}

	public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		VisitBlock(node.Body);
	}

	public override void VisitTriggerDeclaration(TriggerDeclarationSyntax node)
	{
		VisitBlock(node.Body);
	}

	public override void VisitBlock(BlockSyntax node)
	{
		DebugAssertHelper.Assert((object)method == enclosing.ContainingMember);
		BlockBinder blockBinder = new BlockBinder(enclosing, node.Statements);
		AddToMap(node, blockBinder);
		foreach (StatementSyntax statement in node.Statements)
		{
			Visit(statement, blockBinder);
		}
	}

	public override void VisitWhileStatement(WhileStatementSyntax node)
	{
		DebugAssertHelper.Assert((object)method == enclosing.ContainingMember);
		WhileBinder whileBinder = new WhileBinder(enclosing, node);
		AddToMap(node, whileBinder);
		VisitPossibleEmbeddedStatement(node.Statement, whileBinder);
	}

	public override void VisitRepeatStatement(RepeatStatementSyntax node)
	{
		DebugAssertHelper.Assert((object)method == enclosing.ContainingMember);
		RepeatBinder repeatBinder = new RepeatBinder(enclosing, node);
		AddToMap(node, repeatBinder);
		foreach (StatementSyntax statement in node.Statements)
		{
			Visit(statement, repeatBinder);
		}
	}

	public override void VisitForStatement(ForStatementSyntax node)
	{
		DebugAssertHelper.Assert((object)method == enclosing.ContainingMember);
		ForLoopBinder forLoopBinder = new ForLoopBinder(enclosing, node);
		AddToMap(node, forLoopBinder);
		VisitPossibleEmbeddedStatement(node.Statement, forLoopBinder);
	}

	public override void VisitForEachStatement(ForEachStatementSyntax node)
	{
		DebugAssertHelper.Assert((object)method == enclosing.ContainingMember);
		ForEachLoopBinder forEachLoopBinder = new ForEachLoopBinder(enclosing, node);
		AddToMap(node, forEachLoopBinder);
		VisitPossibleEmbeddedStatement(node.Statement, forEachLoopBinder);
	}

	public override void VisitIfStatement(IfStatementSyntax node)
	{
		VisitPossibleEmbeddedStatement(node.Statement, enclosing);
		Visit(node.ElseStatement, enclosing);
	}

	public override void VisitCaseStatement(CaseStatementSyntax node)
	{
		CaseBinder caseBinder = new CaseBinder(enclosing, node);
		AddToMap(node, caseBinder);
		Visit(node.Expression, enclosing);
		foreach (CaseLineSyntax caseLine in node.CaseLines)
		{
			VisitPossibleEmbeddedStatement(caseLine.Statement, caseBinder);
		}
		if (node.CaseElse != null)
		{
			foreach (StatementSyntax elseStatement in node.CaseElse.ElseStatements)
			{
				VisitPossibleEmbeddedStatement(elseStatement, caseBinder);
			}
		}
	}

	public override void VisitWithStatement(WithStatementSyntax node)
	{
		WithBinder withBinder = new WithBinder(enclosing, node);
		AddToMap(node, withBinder);
		VisitPossibleEmbeddedStatement(node.Statement, withBinder);
	}

	public override void VisitAssertErrorStatement(AssertErrorStatementSyntax node)
	{
		VisitPossibleEmbeddedStatement(node.Statement, enclosing);
	}

	private void AddToMap(SyntaxNode node, Binder binder)
	{
		DebugAssertHelper.Assert(!map.TryGetValue(node, out var value) || value == binder.Next);
		map[node] = binder;
	}

	private void VisitPossibleEmbeddedStatement(StatementSyntax statement, Binder enclosingParameter)
	{
		Visit(statement, enclosingParameter);
	}
}
