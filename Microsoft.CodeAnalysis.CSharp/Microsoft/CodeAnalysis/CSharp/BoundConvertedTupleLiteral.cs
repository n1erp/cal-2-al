using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConvertedTupleLiteral : BoundTupleExpression
{
	public TypeSymbol NaturalTypeOpt { get; }

	public BoundConvertedTupleLiteral(SyntaxNode syntax, TypeSymbol naturalTypeOpt, ImmutableArray<BoundExpression> arguments, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ConvertedTupleLiteral, syntax, arguments, type, hasErrors || arguments.HasErrors())
	{
		NaturalTypeOpt = naturalTypeOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConvertedTupleLiteral(this);
	}

	public BoundConvertedTupleLiteral Update(TypeSymbol naturalTypeOpt, ImmutableArray<BoundExpression> arguments, TypeSymbol type)
	{
		if (naturalTypeOpt != NaturalTypeOpt || arguments != Arguments || type != Type)
		{
			return new BoundConvertedTupleLiteral(Syntax, naturalTypeOpt, arguments, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
