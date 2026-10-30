using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp;

internal class CSharpControlFlowAnalysis : ControlFlowAnalysis
{
	private readonly RegionAnalysisContext _context;

	private ImmutableArray<SyntaxNode> _entryPoints;

	private ImmutableArray<SyntaxNode> _exitPoints;

	private object _regionStartPointIsReachable;

	private object _regionEndPointIsReachable;

	private bool? _succeeded;

	public override ImmutableArray<SyntaxNode> EntryPoints
	{
		get
		{
			if (_entryPoints == null)
			{
				_succeeded = !_context.Failed;
				ImmutableArray<SyntaxNode> value = (_context.Failed ? ImmutableArray<SyntaxNode>.Empty : ((IEnumerable<SyntaxNode>)EntryPointsWalker.Analyze(_context.Compilation, _context.Member, _context.BoundNode, _context.FirstInRegion, _context.LastInRegion, out _succeeded)).ToImmutableArray());
				ImmutableInterlocked.InterlockedInitialize(ref _entryPoints, value);
			}
			return _entryPoints;
		}
	}

	public override ImmutableArray<SyntaxNode> ExitPoints
	{
		get
		{
			if (_exitPoints == null)
			{
				ImmutableArray<SyntaxNode> value = (Succeeded ? ((IEnumerable<SyntaxNode>)ExitPointsWalker.Analyze(_context.Compilation, _context.Member, _context.BoundNode, _context.FirstInRegion, _context.LastInRegion)).ToImmutableArray() : ImmutableArray<SyntaxNode>.Empty);
				ImmutableInterlocked.InterlockedInitialize(ref _exitPoints, value);
			}
			return _exitPoints;
		}
	}

	public sealed override bool EndPointIsReachable
	{
		get
		{
			if (_regionEndPointIsReachable == null)
			{
				ComputeReachability();
			}
			return (bool)_regionEndPointIsReachable;
		}
	}

	public sealed override bool StartPointIsReachable
	{
		get
		{
			if (_regionStartPointIsReachable == null)
			{
				ComputeReachability();
			}
			return (bool)_regionStartPointIsReachable;
		}
	}

	public override ImmutableArray<SyntaxNode> ReturnStatements => ExitPoints.WhereAsArray((SyntaxNode s) => s.IsKind(SyntaxKind.ReturnStatement) || s.IsKind(SyntaxKind.YieldBreakStatement));

	public sealed override bool Succeeded
	{
		get
		{
			if (!_succeeded.HasValue)
			{
				_ = EntryPoints;
			}
			return _succeeded.Value;
		}
	}

	internal CSharpControlFlowAnalysis(RegionAnalysisContext context)
	{
		_context = context;
	}

	private void ComputeReachability()
	{
		bool startPointIsReachable;
		bool endPointIsReachable;
		if (Succeeded)
		{
			RegionReachableWalker.Analyze(_context.Compilation, _context.Member, _context.BoundNode, _context.FirstInRegion, _context.LastInRegion, out startPointIsReachable, out endPointIsReachable);
		}
		else
		{
			startPointIsReachable = (endPointIsReachable = true);
		}
		Interlocked.CompareExchange(ref _regionEndPointIsReachable, endPointIsReachable, null);
		Interlocked.CompareExchange(ref _regionStartPointIsReachable, startPointIsReachable, null);
	}
}
