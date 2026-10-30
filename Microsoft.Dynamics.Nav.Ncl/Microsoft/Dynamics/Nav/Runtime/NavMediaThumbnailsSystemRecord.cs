using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavMediaThumbnailsSystemRecord : NavRecord
{
	private void TestRequiredFields()
	{
	}

	protected NavMediaThumbnailsSystemRecord(ITreeObject parent, int tableId, NCLMetaTable metaTable, bool isTemporary, NavRecord sharedTable, string companyName, SecurityFiltering securityFiltering)
		: base(parent, tableId, metaTable, isTemporary, sharedTable, companyName, securityFiltering)
	{
	}

	public override bool Insert(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger)
	{
		if (runApplicationTrigger)
		{
			TestRequiredFields();
		}
		return base.Insert(errorLevel, runApplicationTrigger, runGlobalTrigger);
	}

	public override bool Modify(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger)
	{
		if (runApplicationTrigger)
		{
			TestRequiredFields();
		}
		return Modify(errorLevel, runApplicationTrigger, runGlobalTrigger, isBulkModify: false);
	}

	public override bool Modify(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger, bool isBulkModify)
	{
		if (runApplicationTrigger)
		{
			TestRequiredFields();
		}
		return base.Modify(errorLevel, runApplicationTrigger, runGlobalTrigger, isBulkModify);
	}

	protected override void OnModify()
	{
	}
}
