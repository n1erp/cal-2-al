using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ApiWebhookEntityDataProvider : EagerVirtualDataProvider
{
	public static readonly ApiWebhookEntityDataProvider Instance = new ApiWebhookEntityDataProvider();

	public override int TableId => 2000000097;

	private NavOption PageType => NavOption.Create(MetaTable.GetFieldByNo(10).FieldOptionMetadata, 8);

	private NavOption QueryType => NavOption.Create(MetaTable.GetFieldByNo(10).FieldOptionMetadata, 9);

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

	public override FieldDictionary<NavValue> GetBlobContent(GetBlobContentProviderRequest request)
	{
		Tuple<INavFieldMetadata, NavValue>[] array = new Tuple<INavFieldMetadata, NavValue>[request.BlobsToGet.Count];
		if (request.RecordBuffer is MutableRecordBuffer { ReadOnlyBuffer: var readOnlyBuffer })
		{
			for (int i = 0; i < request.BlobsToGet.Count; i++)
			{
				NCLMetaField nCLMetaField = (NCLMetaField)request.BlobsToGet[i];
				if (nCLMetaField.FieldNclType != NavNclType.NavBlob)
				{
					throw new NavNCLConversionException(nCLMetaField.FieldNavType.GetType(), typeof(NavBLOB));
				}
				NavBLOB navBLOB = (NavBLOB)readOnlyBuffer[nCLMetaField.FieldIndex];
				NavBLOB item = navBLOB.DeepCopy();
				array[i] = new Tuple<INavFieldMetadata, NavValue>(nCLMetaField, item);
			}
		}
		return new FieldDictionary<NavValue>(array);
	}

	private ReadOnlyRecordBuffer GetRecord(ApiWebServiceEntry apiWebService)
	{
		byte[] value = ((apiWebService.TableFilter != null) ? Encoding.UTF8.GetBytes(apiWebService.TableFilter) : new byte[0]);
		return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavText.Create(apiWebService.Publisher), NavText.Create(apiWebService.Group), NavText.Create(apiWebService.Version), NavText.Create(apiWebService.Name), NavInteger.Create(apiWebService.TableId), NavBoolean.Create(apiWebService.TableTemporary), NavBLOB.CreateFromObject(value), NavText.Create(apiWebService.KeyFields), NavBoolean.Create(apiWebService.ODataKeySpecified), (apiWebService.ObjectType == ObjectType.Page) ? PageType : QueryType, NavInteger.Create(apiWebService.ObjectId), NavGuid.CreateFromObject(((WrappedValue<Guid>)(object)apiWebService.PackageId).Value));
	}
}
