using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNamespaceExpression : BoundExpression
{
	public override Symbol ExpressionSymbol => (Symbol)(((object)AliasOpt) ?? ((object)NamespaceSymbol));

	public NamespaceSymbol NamespaceSymbol { get; }

	public AliasSymbol AliasOpt { get; }

	public BoundNamespaceExpression(SyntaxNode syntax, NamespaceSymbol namespaceSymbol, bool hasErrors = false)
		: this(syntax, namespaceSymbol, null, hasErrors)
	{
	}

	public BoundNamespaceExpression(SyntaxNode syntax, NamespaceSymbol namespaceSymbol)
		: this(syntax, namespaceSymbol, null)
	{
	}

	public BoundNamespaceExpression Update(NamespaceSymbol namespaceSymbol)
	{
		return Update(namespaceSymbol, AliasOpt);
	}

	public BoundNamespaceExpression(SyntaxNode syntax, NamespaceSymbol namespaceSymbol, AliasSymbol aliasOpt, bool hasErrors)
		: base(BoundKind.NamespaceExpression, syntax, null, hasErrors)
	{
		NamespaceSymbol = namespaceSymbol;
		AliasOpt = aliasOpt;
	}

	public BoundNamespaceExpression(SyntaxNode syntax, NamespaceSymbol namespaceSymbol, AliasSymbol aliasOpt)
		: base(BoundKind.NamespaceExpression, syntax, null)
	{
		NamespaceSymbol = namespaceSymbol;
		AliasOpt = aliasOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNamespaceExpression(this);
	}

	public BoundNamespaceExpression Update(NamespaceSymbol namespaceSymbol, AliasSymbol aliasOpt)
	{
		if (namespaceSymbol != NamespaceSymbol || aliasOpt != AliasOpt)
		{
			return new BoundNamespaceExpression(Syntax, namespaceSymbol, aliasOpt, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
