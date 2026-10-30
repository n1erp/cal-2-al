using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Model.Tools.Language;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public sealed class MergeInfoCollection : IEnumerable<MergeInfo>, IEnumerable
{
	private readonly IList<MergeInfo> infos;

	private readonly int deltaChangeCount;

	private readonly int conflictChangeCount;

	public string Summary => ToString();

	public MergeInfoCollection(IEnumerable<MergeInfo> collection, int deltaChangeCount, int conflictChangeCount)
	{
		infos = new List<MergeInfo>(collection);
		this.deltaChangeCount = deltaChangeCount;
		this.conflictChangeCount = conflictChangeCount;
	}

	public IEnumerator<MergeInfo> GetEnumerator()
	{
		return infos.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return infos.GetEnumerator();
	}

	public override string ToString()
	{
		decimal num = ((deltaChangeCount == 0) ? 100m : (((decimal)deltaChangeCount - (decimal)conflictChangeCount) / (decimal)deltaChangeCount * 100m));
		return string.Format(CultureInfo.InvariantCulture, NonLocalizable.MergeSummaryTemplate, infos.Count, deltaChangeCount, num, infos.Count((MergeInfo m) => m.MergeResult == MergeResult.Merged), infos.Count((MergeInfo m) => m.MergeResult == MergeResult.Conflict), infos.Count((MergeInfo m) => m.MergeResult == MergeResult.Inserted), infos.Count((MergeInfo m) => m.MergeResult == MergeResult.Deleted), infos.Count((MergeInfo m) => m.MergeResult == MergeResult.Unchanged), infos.Count((MergeInfo m) => m.MergeResult == MergeResult.Failed), conflictChangeCount);
	}
}
