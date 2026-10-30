namespace Microsoft.CodeAnalysis.CSharp;

internal class RegionReachableWalker : AbstractRegionControlFlowPass
{
	private bool? _regionStartPointIsReachable;

	private bool? _regionEndPointIsReachable;

	internal static void Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, out bool startPointIsReachable, out bool endPointIsReachable)
	{
		RegionReachableWalker regionReachableWalker = new RegionReachableWalker(compilation, member, node, firstInRegion, lastInRegion);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		bool badRegion = false;
		try
		{
			regionReachableWalker.Analyze(ref badRegion, instance);
			startPointIsReachable = badRegion || (regionReachableWalker._regionStartPointIsReachable ?? true);
			endPointIsReachable = badRegion || regionReachableWalker._regionEndPointIsReachable.GetValueOrDefault(regionReachableWalker.State.Alive);
		}
		finally
		{
			instance.Free();
			regionReachableWalker.Free();
		}
	}

	private RegionReachableWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
		: base(compilation, member, node, firstInRegion, lastInRegion)
	{
	}

	protected override void EnterRegion()
	{
		_regionStartPointIsReachable = State.Alive;
		base.EnterRegion();
	}

	protected override void LeaveRegion()
	{
		_regionEndPointIsReachable = State.Alive;
		base.LeaveRegion();
	}
}
