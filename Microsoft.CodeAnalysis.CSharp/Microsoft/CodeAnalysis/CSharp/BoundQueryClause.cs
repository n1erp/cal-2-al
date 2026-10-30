using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundQueryClause : BoundExpression
{
	public BoundExpression Operation { get; }

	public BoundExpression Cast { get; }

	public BoundExpression UnoptimizedForm { get; }

	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Value);

	public BoundExpression Value { get; }

	public RangeVariableSymbol DefinedSymbol { get; }

	public Binder Binder { get; }

	public BoundQueryClause(SyntaxNode syntax, BoundExpression value, RangeVariableSymbol definedSymbol, BoundExpression queryInvocation, BoundExpression castInvocation, Binder binder, BoundExpression unoptimizedForm, TypeSymbol type, bool hasErrors = false)
		: this(syntax, value, definedSymbol, binder, type, hasErrors)
	{
		Operation = queryInvocation;
		Cast = castInvocation;
		UnoptimizedForm = unoptimizedForm;
	}

	public BoundQueryClause Update(BoundExpression value, RangeVariableSymbol definedSymbol, BoundExpression queryInvocation, BoundExpression castInvocation, Binder binder, BoundExpression unoptimizedForm, TypeSymbol type)
	{
		if (value != Value || definedSymbol != DefinedSymbol || queryInvocation != Operation || castInvocation != Cast || binder != Binder || unoptimizedForm != UnoptimizedForm || type != Type)
		{
			return new BoundQueryClause(Syntax, value, definedSymbol, queryInvocation, castInvocation, binder, unoptimizedForm, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public BoundQueryClause(SyntaxNode syntax, BoundExpression value, RangeVariableSymbol definedSymbol, Binder binder, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.QueryClause, syntax, type, hasErrors || value.HasErrors())
	{
		Value = value;
		DefinedSymbol = definedSymbol;
		Binder = binder;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitQueryClause(this);
	}

	public BoundQueryClause Update(BoundExpression value, RangeVariableSymbol definedSymbol, Binder binder, TypeSymbol type)
	{
		if (value != Value || definedSymbol != DefinedSymbol || binder != Binder || type != Type)
		{
			return new BoundQueryClause(Syntax, value, definedSymbol, binder, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
