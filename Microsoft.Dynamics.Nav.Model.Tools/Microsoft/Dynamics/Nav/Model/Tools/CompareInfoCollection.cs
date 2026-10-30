using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Model.Tools.Language;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class CompareInfoCollection : IEnumerable<CompareInfo>, IEnumerable
{
	private IList<CompareInfo> infos;

	public string Summary => ToString();

	public CompareInfoCollection(IEnumerable<CompareInfo> collection)
	{
		infos = new List<CompareInfo>(collection);
	}

	public IEnumerator<CompareInfo> GetEnumerator()
	{
		return infos.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return infos.GetEnumerator();
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, NonLocalizable.ProcessedMessage + Environment.NewLine, infos.Count) + string.Format(CultureInfo.InvariantCulture, "  {0,-11}{1} {2}\r\n", NonLocalizable.InsertedLabel, infos.Count((CompareInfo c) => c.CompareResult == CompareResult.Inserted), NonLocalizable.Objects) + string.Format(CultureInfo.InvariantCulture, "  {0,-11}{1} {2}\r\n", NonLocalizable.DeletedLabel, infos.Count((CompareInfo c) => c.CompareResult == CompareResult.Deleted), NonLocalizable.Objects) + string.Format(CultureInfo.InvariantCulture, "  {0,-11}{1} {2}\r\n", NonLocalizable.ChangedLabel, infos.Count((CompareInfo c) => c.CompareResult == CompareResult.Delta), NonLocalizable.Objects) + string.Format(CultureInfo.InvariantCulture, "  {0,-11}{1} {2}\r\n", NonLocalizable.IdenticalLabel, infos.Count((CompareInfo c) => c.CompareResult == CompareResult.Identical), NonLocalizable.Objects) + string.Format(CultureInfo.InvariantCulture, "  {0,-11}{1} {2}\r\n", NonLocalizable.FailedLabel, infos.Count((CompareInfo c) => c.CompareResult == CompareResult.Failed), NonLocalizable.Objects);
	}
}
