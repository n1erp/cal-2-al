using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppObjectMetadataRecord
{
	public long SqlTimestamp { get; set; }

	public PackageId AppPackageId { get; set; }

	public ObjectType ObjectType { get; set; }

	public int ObjectId { get; set; }

	public NavAppObjectFormat Format
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public int MetadataVersion { get; set; }

	public string ObjectSubtype { get; set; }

	public string ObjectName { get; set; }

	public Sha256Hash MetadataHash { get; set; }

	public Sha256Hash UserCodeHash { get; set; }

	public NavAppObjectFlags ObjectFlags
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public ReadOnlyRecordBuffer RecordBuffer { get; set; }
}
