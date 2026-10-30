using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundOrderByPropertyValue : BoundPropertyValue
{
	public ImmutableArray<BoundOrderByExpression> Elements { get; private set; }

	internal override object ValueObject => Syntax.ToString();

	internal override string ValueText => Syntax.ToString();

	public BoundOrderByPropertyValue(SyntaxNode syntax, ImmutableArray<BoundOrderByExpression> elements, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.OrderByPropertyValue, syntax, type, hasErrors || elements.HasErrors())
	{
		Elements = elements;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitOrderByPropertyValue(this);
	}

	public BoundOrderByPropertyValue Update(ImmutableArray<BoundOrderByExpression> elements, TypeSymbol type)
	{
		if (elements != Elements || type != Type)
		{
			return new BoundOrderByPropertyValue(Syntax, elements, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
