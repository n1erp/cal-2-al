using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSequence : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => StaticCast<BoundNode>.From(SideEffects.Add(Value));

	public ImmutableArray<LocalSymbol> Locals { get; }

	public ImmutableArray<BoundExpression> SideEffects { get; }

	public BoundExpression Value { get; }

	public BoundSequence(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundExpression> sideEffects, BoundExpression value, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.Sequence, syntax, type, hasErrors || sideEffects.HasErrors() || value.HasErrors())
	{
		Locals = locals;
		SideEffects = sideEffects;
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSequence(this);
	}

	public BoundSequence Update(ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundExpression> sideEffects, BoundExpression value, TypeSymbol type)
	{
		if (locals != Locals || sideEffects != SideEffects || value != Value || type != Type)
		{
			return new BoundSequence(Syntax, locals, sideEffects, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
