using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppDataArchiveRecord
{
	public Guid AppId { get; set; }

	public int TableId { get; set; }

	public string CompanyName { get; set; }

	public Version AppVersion { get; set; }

	public string ArchiveTableName { get; set; }

	public NavBLOB MetadataSnapshot { get; set; }

	public int TableBlobVersion { get; set; }
}
