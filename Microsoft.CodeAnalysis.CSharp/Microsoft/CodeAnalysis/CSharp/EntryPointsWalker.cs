using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class EntryPointsWalker : AbstractRegionControlFlowPass
{
	private readonly HashSet<LabeledStatementSyntax> _entryPoints = new HashSet<LabeledStatementSyntax>();

	internal static IEnumerable<LabeledStatementSyntax> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, out bool? succeeded)
	{
		EntryPointsWalker entryPointsWalker = new EntryPointsWalker(compilation, member, node, firstInRegion, lastInRegion);
		bool badRegion = false;
		try
		{
			entryPointsWalker.Analyze(ref badRegion);
			HashSet<LabeledStatementSyntax> entryPoints = entryPointsWalker._entryPoints;
			succeeded = !badRegion;
			IEnumerable<LabeledStatementSyntax> result;
			if (!badRegion)
			{
				IEnumerable<LabeledStatementSyntax> enumerable = entryPoints;
				result = enumerable;
			}
			else
			{
				result = SpecializedCollections.EmptyEnumerable<LabeledStatementSyntax>();
			}
			return result;
		}
		finally
		{
			entryPointsWalker.Free();
		}
	}

	private new void Analyze(ref bool badRegion)
	{
		Scan(ref badRegion);
	}

	private EntryPointsWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
		: base(compilation, member, node, firstInRegion, lastInRegion)
	{
	}

	protected override void Free()
	{
		base.Free();
	}

	protected override void NoteBranch(PendingBranch pending, BoundNode gotoStmt, BoundStatement targetStmt)
	{
		if (!gotoStmt.WasCompilerGenerated && !targetStmt.WasCompilerGenerated && RegionContains(targetStmt.Syntax.Span) && !RegionContains(gotoStmt.Syntax.Span))
		{
			_entryPoints.Add((LabeledStatementSyntax)targetStmt.Syntax);
		}
	}
}
