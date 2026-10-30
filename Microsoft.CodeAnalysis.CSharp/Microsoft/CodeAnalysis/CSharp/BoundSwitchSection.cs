using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSwitchSection : BoundStatementList
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public ImmutableArray<BoundSwitchLabel> SwitchLabels { get; }

	public BoundSwitchSection(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundSwitchLabel> switchLabels, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: base(BoundKind.SwitchSection, syntax, statements, hasErrors || switchLabels.HasErrors() || statements.HasErrors())
	{
		Locals = locals;
		SwitchLabels = switchLabels;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSwitchSection(this);
	}

	public BoundSwitchSection Update(ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundSwitchLabel> switchLabels, ImmutableArray<BoundStatement> statements)
	{
		if (locals != Locals || switchLabels != SwitchLabels || statements != Statements)
		{
			return new BoundSwitchSection(Syntax, locals, switchLabels, statements, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
