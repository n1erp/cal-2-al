using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavMediaSystemRecord : NavRecord
{
	private void ValidateFieldInformation()
	{
		ALTestField(2);
		ALTestField(5);
		ALTestField(6);
		ALTestField(1);
	}

	protected NavMediaSystemRecord(ITreeObject parent, int tableId, NCLMetaTable metaTable, bool isTemporary, NavRecord sharedTable, string companyName, SecurityFiltering securityFiltering)
		: base(parent, tableId, metaTable, isTemporary, sharedTable, companyName, securityFiltering)
	{
	}

	public override bool Insert(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger)
	{
		if (runApplicationTrigger)
		{
			ValidateFieldInformation();
		}
		return base.Insert(errorLevel, runApplicationTrigger, runGlobalTrigger);
	}

	public override bool Delete(DataError errorLevel, bool runApplicationTrigger, bool isCalledFromUI)
	{
		RemoveThumbnails();
		return base.Delete(errorLevel, runApplicationTrigger, isCalledFromUI);
	}

	private void RemoveThumbnails()
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000185 }))
		{
			using NavRecordHandle navRecordHandle = new NavRecordHandle(this, 2000000185, temporary: false, SecurityFiltering.Ignored);
			navRecordHandle.Target.ALSetFilter(2, "%1", GetFieldValue(1));
			if (navRecordHandle.Target.ALFind(DataError.TrapError, "-"))
			{
				navRecordHandle.Target.ALDeleteAll();
			}
		}
	}

	public override bool Delete(DataError errorLevel, bool runApplicationTrigger, bool isCalledFromUI, bool isBulkDelete)
	{
		RemoveThumbnails();
		return base.Delete(errorLevel, runApplicationTrigger, isCalledFromUI, isBulkDelete);
	}

	protected override void OnRename()
	{
		throw new NavNotSupportedException("Cannot change the key of a media record");
	}

	protected override void OnModify()
	{
		throw new NavNotSupportedException("Cannot modify a media record");
	}
}
