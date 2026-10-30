using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPatternSwitchSection : BoundStatementList
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public ImmutableArray<BoundPatternSwitchLabel> SwitchLabels { get; }

	public BoundPatternSwitchSection(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundPatternSwitchLabel> switchLabels, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: base(BoundKind.PatternSwitchSection, syntax, statements, hasErrors || switchLabels.HasErrors() || statements.HasErrors())
	{
		Locals = locals;
		SwitchLabels = switchLabels;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPatternSwitchSection(this);
	}

	public BoundPatternSwitchSection Update(ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundPatternSwitchLabel> switchLabels, ImmutableArray<BoundStatement> statements)
	{
		if (locals != Locals || switchLabels != SwitchLabels || statements != Statements)
		{
			return new BoundPatternSwitchSection(Syntax, locals, switchLabels, statements, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
