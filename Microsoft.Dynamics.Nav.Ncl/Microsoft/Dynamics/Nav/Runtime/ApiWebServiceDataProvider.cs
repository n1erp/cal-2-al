using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ApiWebServiceDataProvider : EagerVirtualDataProvider
{
	public static readonly ApiWebServiceDataProvider Instance = new ApiWebServiceDataProvider();

	public override int TableId => 2000000193;

	private NavOption PageType => NavOption.Create(MetaTable.GetFieldByNo(3).FieldOptionMetadata, 8);

	private NavOption QueryType => NavOption.Create(MetaTable.GetFieldByNo(3).FieldOptionMetadata, 9);

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = false;
		IEnumerable<ApiWebServiceEntry> apiWebServicesInstalled = WebServiceHelper.GetApiWebServicesInstalled();
		IEnumerable<ReadOnlyRecordBuffer> result;
		if (apiWebServicesInstalled == null)
		{
			IEnumerable<ReadOnlyRecordBuffer> enumerable = new List<ReadOnlyRecordBuffer>();
			result = enumerable;
		}
		else
		{
			result = apiWebServicesInstalled.Select((ApiWebServiceEntry s) => GetRecord(s));
		}
		return result;
	}

	private ReadOnlyRecordBuffer GetRecord(ApiWebServiceEntry apiWebService)
	{
		return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, (apiWebService.ObjectType == ObjectType.Page) ? PageType : QueryType, NavInteger.Create(apiWebService.ObjectId), NavText.Create(apiWebService.Name), NavText.Create(apiWebService.Publisher), NavText.Create(apiWebService.Group), NavText.Create(apiWebService.Version), NavGuid.CreateFromObject(((WrappedValue<Guid>)(object)apiWebService.PackageId).Value), NavBoolean.Create(value: true));
	}
}
