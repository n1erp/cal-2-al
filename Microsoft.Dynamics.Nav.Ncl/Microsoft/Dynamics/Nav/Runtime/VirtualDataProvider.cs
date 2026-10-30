using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Security.Permissions;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class VirtualDataProvider : DataProvider
{
	private class VirtualTablePermissionSet : IPermissionSet
	{
		bool IPermissionSet.HasPermissions(NavApplicationObjectBase securableObject, PermissionMask permissionMask, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty, PermissionMask additionalIndirectPermissions, bool countAsLimitedUserAllowedTable, PermissionValidationCategory validationCategory)
		{
			return true;
		}

		void IPermissionSet.VerifyPermissions(NavApplicationObjectBase securableObject, PermissionMask permissionMask, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty, PermissionMask additionalIndirectPermissions, bool countAsLimitedUserAllowedTable)
		{
		}
	}

	private NCLMetaTable metaTable;

	private static readonly NavBigInteger virtualTimeStamp = NavBigInteger.Create(1L);

	private static readonly VirtualTablePermissionSet PermissionSet = new VirtualTablePermissionSet();

	public abstract int TableId { get; }

	public NCLMetaTable MetaTable
	{
		get
		{
			if (metaTable == null)
			{
				metaTable = CreateMetaTable();
			}
			return metaTable;
		}
	}

	public static NavBigInteger VirtualTimeStamp => virtualTimeStamp;

	public override bool ShouldResultSetBufferRows => false;

	protected virtual NCLMetaTable CreateMetaTable()
	{
		return NavGlobal.NCLMetadata.GetMetaTableById(TableId, 0);
	}

	internal override IPermissionSet GetPermissionSet()
	{
		return PermissionSet;
	}

	public override FieldDictionary<NavValue> CalcNumeric(CalcNumericProviderRequest request)
	{
		throw new NotSupportedException();
	}

	public override FieldDictionary<NavValue> CalcMinMax(CalcMinMaxProviderRequest request)
	{
		throw new NotSupportedException();
	}

	public override FieldDictionary<NavValue> GetBlobContent(GetBlobContentProviderRequest request)
	{
		throw new NotSupportedException();
	}

	public override FieldDictionary<NavValue> CalcSums(CalcSumsProviderRequest request)
	{
		throw new NotSupportedException();
	}

	public override ModifyResult Modify(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		throw CreateReadOnlyException();
	}

	public override void ModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldDictionary<NavValue> newFieldValues)
	{
		throw CreateReadOnlyException();
	}

	public override DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction)
	{
		throw CreateReadOnlyException();
	}

	public override void DeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		throw CreateReadOnlyException();
	}

	public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		throw CreateReadOnlyException();
	}

	private NavCSideException CreateReadOnlyException()
	{
		return new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.TableIsReadOnly, MetaTable.TableCaptions.GetValueOrDefault()));
	}

	protected NavOption GetOptionValue(int fieldNo, int fieldValue)
	{
		return MetaTable.GetFieldByNo(fieldNo).FieldOptionMetadata.CreateNavOption(fieldValue);
	}

	protected NavText GetTruncatedTextValue(int fieldNo, string fieldValue)
	{
		int fieldDefinedLength = MetaTable.GetFieldByNo(fieldNo).FieldDefinedLength;
		return NavText.CreateTruncated(fieldDefinedLength, fieldValue);
	}

	protected NavCode GetTruncatedCodeValue(int fieldNo, string fieldValue)
	{
		int fieldDefinedLength = MetaTable.GetFieldByNo(fieldNo).FieldDefinedLength;
		return NavCode.CreateTruncated(fieldDefinedLength, fieldValue);
	}
}
