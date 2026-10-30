using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class LocalReportCache : TreeObject
{
	private Dictionary<int, ILocalReportHandle> localReportCache = new Dictionary<int, ILocalReportHandle>();

	private MetadataProvider provider;

	private long versionNumber;

	private Func<MetaReport, ILocalReportHandle> createLocalReportHandle;

	public LocalReportCache(ITreeObject parent, MetadataProvider provider, Func<MetaReport, ILocalReportHandle> createLocalReportHandle)
		: base(parent)
	{
		this.provider = provider ?? throw new ArgumentNullException("provider");
		this.createLocalReportHandle = createLocalReportHandle ?? throw new ArgumentNullException("createLocalReportHandle");
		versionNumber = provider.VersionNumber;
	}

	public ILocalReportHandle GetLocalReport(MetaReport metaReport)
	{
		if (provider.VersionNumber != versionNumber)
		{
			DisposeValues();
			versionNumber = provider.VersionNumber;
		}
		try
		{
			if (metaReport.VersionNumber != versionNumber)
			{
				return createLocalReportHandle(metaReport);
			}
			if (!localReportCache.TryGetValue(metaReport.Id, out var value))
			{
				value = createLocalReportHandle(metaReport);
				localReportCache.Add(metaReport.Id, value);
			}
			value.AddReference();
			return value;
		}
		catch (FileNotFoundException innerException)
		{
			throw new NavReportViewerNotInstalledException(innerException);
		}
	}

	[Conditional("DEBUG")]
	private static void VerifyRequestedRdlDebug(MetaReport metaReport, ILocalReportHandle localReport)
	{
		if (metaReport.RDLCRuntimeData != null)
		{
			using (localReport.ReportLayout)
			{
				return;
			}
		}
		using (localReport.ReportLayout)
		{
		}
	}

	private void DisposeValues()
	{
		foreach (ILocalReportHandle value in localReportCache.Values)
		{
			value.Dispose();
		}
		localReportCache.Clear();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			DisposeValues();
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
