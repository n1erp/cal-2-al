using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AllProfileDataProvider : EagerVirtualDataProvider
{
	public override int TableId => 2000000178;

	private T EnsureMaximizedScope<T>(Func<T> myAction)
	{
		using (NavCurrentThread.Session.Permissions.HasMaximizedPermissions(TableId) ? new MaximizedPermissionScope(NavCurrentThread.Session, new int[2] { 2000000072, 2000000177 }) : null)
		{
			return myAction();
		}
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = false;
		return EnsureMaximizedScope(() => TenantProfileTableDataHandler.GetAllItems().Concat(ProfileTableDataHandler.GetAllItems(TenantProfileTableDataHandler.IsTenantProfileDefault)));
	}

	public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		InsertResult result = EnsureMaximizedScope(() =>
		{
			NavOption navOption = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)] as NavOption;
			if (navOption.Value == 0)
			{
				ProfileTableDataHandler.Insert(recordBuffer);
			}
			else
			{
				TenantProfileTableDataHandler.Insert(recordBuffer);
			}
			return InsertResult.Success;
		});
		outputRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, recordBuffer.ToArray());
		return result;
	}

	public override ModifyResult Modify(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		ModifyResult result = EnsureMaximizedScope(() =>
		{
			if (!recordBuffer.IsChangedValueSameAsOriginalValue(1))
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ModifyProfileScopeNotAllowed));
			}
			NavOption navOption = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)] as NavOption;
			if (navOption.Value == 0)
			{
				ProfileTableDataHandler.Modify(recordBuffer);
			}
			else
			{
				TenantProfileTableDataHandler.Modify(recordBuffer);
			}
			return ModifyResult.Success;
		});
		outputRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, recordBuffer.ToArray());
		return result;
	}

	public override DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction)
	{
		return EnsureMaximizedScope(() =>
		{
			NavOption navOption = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)] as NavOption;
			if (navOption.Value == 0)
			{
				ProfileTableDataHandler.Delete(recordBuffer);
			}
			else
			{
				TenantProfileTableDataHandler.Delete(recordBuffer);
			}
			return DeleteResult.Success;
		});
	}
}
