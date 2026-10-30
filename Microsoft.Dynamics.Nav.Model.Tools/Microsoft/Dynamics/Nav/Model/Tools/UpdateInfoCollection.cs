using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Model.Tools.Language;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class UpdateInfoCollection : IEnumerable<UpdateInfo>, IEnumerable
{
	private IList<UpdateInfo> infos;

	private readonly int deltaChangeCount;

	private readonly int conflictChangeCount;

	public string Summary => ToString();

	internal int ConflictCount => conflictChangeCount;

	public UpdateInfoCollection(IEnumerable<UpdateInfo> collection, int deltaChangeCount, int conflictChangeCount)
	{
		infos = new List<UpdateInfo>(collection);
		this.deltaChangeCount = deltaChangeCount;
		this.conflictChangeCount = conflictChangeCount;
	}

	public IEnumerator<UpdateInfo> GetEnumerator()
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
		return string.Format(CultureInfo.InvariantCulture, NonLocalizable.UpdateSummaryTemplate, infos.Count, deltaChangeCount, num, infos.Count((UpdateInfo m) => m.UpdateResult == UpdateResult.Updated), infos.Count((UpdateInfo m) => m.UpdateResult == UpdateResult.Conflict), infos.Count((UpdateInfo m) => m.UpdateResult == UpdateResult.Inserted), infos.Count((UpdateInfo m) => m.UpdateResult == UpdateResult.Deleted), infos.Count((UpdateInfo m) => m.UpdateResult == UpdateResult.Unchanged), infos.Count((UpdateInfo m) => m.UpdateResult == UpdateResult.Failed), conflictChangeCount);
	}
}
