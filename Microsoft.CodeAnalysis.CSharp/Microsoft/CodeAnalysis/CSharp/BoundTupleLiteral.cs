using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundTupleLiteral : BoundTupleExpression
{
	public ImmutableArray<string> ArgumentNamesOpt { get; }

	public ImmutableArray<bool> InferredNamesOpt { get; }

	public BoundTupleLiteral(SyntaxNode syntax, ImmutableArray<string> argumentNamesOpt, ImmutableArray<bool> inferredNamesOpt, ImmutableArray<BoundExpression> arguments, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TupleLiteral, syntax, arguments, type, hasErrors || arguments.HasErrors())
	{
		ArgumentNamesOpt = argumentNamesOpt;
		InferredNamesOpt = inferredNamesOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTupleLiteral(this);
	}

	public BoundTupleLiteral Update(ImmutableArray<string> argumentNamesOpt, ImmutableArray<bool> inferredNamesOpt, ImmutableArray<BoundExpression> arguments, TypeSymbol type)
	{
		if (argumentNamesOpt != ArgumentNamesOpt || inferredNamesOpt != InferredNamesOpt || arguments != Arguments || type != Type)
		{
			return new BoundTupleLiteral(Syntax, argumentNamesOpt, inferredNamesOpt, arguments, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
