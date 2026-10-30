using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavDocumentServiceSystemRecord : NavRecord
{
	private void TestRequiredFields()
	{
		ALTestField(5);
		ALTestField(13);
		if (ALTestFieldNotBlankOrZero(7))
		{
			ALTestField(11);
		}
		else
		{
			ALTestFieldMustBeBlankOrZero(11);
			SetFieldValue(9, new NavText(string.Empty));
			ALTestFieldMustBeBlankOrZero(9);
		}
		string text = GetFieldValue(5).ToString();
		if (!Uri.IsWellFormedUriString(text, UriKind.Absolute))
		{
			NCLMetaField fieldByNo = MetaTable.GetFieldByNo(5);
			throw NavTestFieldException.CreateMustBeUri(NavCurrentThread.Session.WindowsCulture, ALFieldCaption(fieldByNo.FieldNo), fieldByNo.Parent.TableCaptionSafe, text);
		}
	}

	protected NavDocumentServiceSystemRecord(ITreeObject parent, int tableId, NCLMetaTable metaTable, bool isTemporary, NavRecord sharedTable, string companyName, SecurityFiltering securityFiltering)
		: base(parent, tableId, metaTable, isTemporary, sharedTable, companyName, securityFiltering)
	{
	}

	public override bool Insert(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger)
	{
		if (runApplicationTrigger)
		{
			TestRequiredFields();
		}
		if (Session.DocumentStorageService != null)
		{
			DocumentServiceManagement.Reset(Session);
		}
		return base.Insert(errorLevel, runApplicationTrigger, runGlobalTrigger);
	}

	public override bool Modify(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger)
	{
		if (runApplicationTrigger)
		{
			TestRequiredFields();
		}
		if (Session.DocumentStorageService != null)
		{
			DocumentServiceManagement.Reset(Session);
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
