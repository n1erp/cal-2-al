using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common.Exchange;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Exchange.WebServices.Data;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ExchangeDataProvider : ExternalDataProvider
{
	private readonly NavSession navSession;

	private readonly ExchangeTableConnection connection;

	private readonly NCLMetaTable table;

	private IExchangeDataProviderHelper providerHelper;

	private WellKnownFolderName knownFolderName;

	internal ExchangeDataProvider(NavSession session, NCLMetaTable table)
		: base(table)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		navSession = session;
		connection = navSession.TableConnectionManager.GetCurrentTableConnection<ExchangeTableConnection>(TableConnectionType.Exchange);
		this.table = table;
		switch (table.ExternalName)
		{
		case "Contact":
			providerHelper = (IExchangeDataProviderHelper)(object)new ExchangeDataProviderHelper<IContact>();
			knownFolderName = (WellKnownFolderName)1;
			break;
		case "BookingService":
			providerHelper = (IExchangeDataProviderHelper)(object)new ExchangeDataProviderHelper<IBookingService>();
			knownFolderName = (WellKnownFolderName)5781234;
			break;
		case "BookingStaff":
			providerHelper = (IExchangeDataProviderHelper)(object)new ExchangeDataProviderHelper<IBookingStaff>();
			knownFolderName = (WellKnownFolderName)5781236;
			break;
		case "BookingMailbox":
			providerHelper = (IExchangeDataProviderHelper)(object)new ExchangeDataProviderHelper<IBookingMailbox>();
			knownFolderName = (WellKnownFolderName)5781233;
			break;
		default:
			throw new NavNCLArgumentException(string.Format(CultureInfo.InvariantCulture, "Unknown ExternalName ({0}) for table {1}", table.ExternalName, table.TableName));
		}
	}

	private IExchangeFolder GetFolder()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected Obj, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (connection != null)
		{
			return NavExchangeServiceBase.CatchServiceResponseException<IExchangeFolder>((Func<IExchangeFolder>)(() =>
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				return connection.GetService().GetFolder(knownFolderName, connection.FolderId);
			}));
		}
		throw new NavNCLEwsException("Unable to retrieve folder", new NavEwsException("Connection for Exchange not initialized or folder does not exist."));
	}

	public override bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer)
	{
		IExchangeFolder folder = GetFolder();
		recordBuffer = null;
		IEnumerable<ReadOnlyRecordBuffer> source = ExchangeItemsToRecordBuffer(folder.ConvertToExchangeDataItem(folder.Read()), null, null, request.RecordId);
		if (source.Count() == 1)
		{
			recordBuffer = source.First();
			return true;
		}
		recordBuffer = null;
		return false;
	}

	public override int Count(DataProviderRequest request)
	{
		return NavExchangeServiceBase.CatchServiceResponseException<int>((Func<int>)(() => GetFolder().TotalCount));
	}

	public override ModifyResult Modify(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		IExchangeDataItem item = GetChangedItem(recordBuffer);
		if (item == null)
		{
			outputRecordBuffer = null;
			return ModifyResult.RecordNotFound;
		}
		NavExchangeServiceBase.CatchServiceResponseException((Action)(() =>
		{
			connection.GetService().UpdateItem(item);
		}));
		NavValue[] immutableFields = recordBuffer.ToArray();
		outputRecordBuffer = new ReadOnlyRecordBuffer(recordBuffer.MetaApplicationObject, immutableFields);
		return ModifyResult.Success;
	}

	public override void ModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldDictionary<NavValue> newFieldValues)
	{
		IEnumerable<ReadOnlyRecordBuffer> enumerable = ExchangeItemsToRecordBuffer(GetFolder().ConvertToExchangeDataItem(GetFolder().Read()));
		foreach (ReadOnlyRecordBuffer item in enumerable)
		{
			if (item.PassesFieldFiltersAndMarks(filtersAndMarks))
			{
				IExchangeDataItem exchangeItem = NavExchangeServiceBase.CatchServiceResponseException<IExchangeDataItem>((Func<IExchangeDataItem>)(() => GetMatchingItem(item)));
				Tuple<INavFieldMetadata, NavValue>[] items = newFieldValues.Items;
				foreach (Tuple<INavFieldMetadata, NavValue> tuple in items)
				{
					NCLMetaField fieldByIndex = table.GetFieldByIndex(tuple.Item1.ColumnIndex);
					string text = fieldByIndex.ExternalName ?? fieldByIndex.FieldName;
					((IExternalDataItem)exchangeItem).SetProperty(text, tuple.Item2.ValueAsObject);
				}
				NavExchangeServiceBase.CatchServiceResponseException((Action)(() =>
				{
					connection.GetService().UpdateItem(exchangeItem);
				}));
			}
		}
	}

	public override DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction)
	{
		IExchangeDataItem item = NavExchangeServiceBase.CatchServiceResponseException<IExchangeDataItem>((Func<IExchangeDataItem>)(() => GetMatchingItem(recordBuffer)));
		if (item != null)
		{
			NavExchangeServiceBase.CatchServiceResponseException((Action)(() =>
			{
				connection.GetService().DeleteItem(item);
			}));
			return DeleteResult.Success;
		}
		return DeleteResult.RecordNotFound;
	}

	public override void DeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		IEnumerable<ReadOnlyRecordBuffer> enumerable = ExchangeItemsToRecordBuffer(GetFolder().ConvertToExchangeDataItem(GetFolder().Read()));
		foreach (ReadOnlyRecordBuffer item in enumerable)
		{
			if (item.PassesFieldFiltersAndMarks(filtersAndMarks))
			{
				IExchangeDataItem exchangeItem = NavExchangeServiceBase.CatchServiceResponseException<IExchangeDataItem>((Func<IExchangeDataItem>)(() => GetMatchingItem(item)));
				NavExchangeServiceBase.CatchServiceResponseException((Action)(() =>
				{
					connection.GetService().DeleteItem(exchangeItem);
				}));
			}
		}
	}

	public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		IExchangeDataItem val = NavExchangeServiceBase.CatchServiceResponseException<IExchangeDataItem>((Func<IExchangeDataItem>)(() => GetMatchingItem(recordBuffer)));
		if (val != null)
		{
			outputRecordBuffer = new ReadOnlyRecordBuffer(recordBuffer.ToArray());
			return InsertResult.RecordAlreadyExists;
		}
		IExchangeDataItem newContact = NavExchangeServiceBase.CatchServiceResponseException<IExchangeDataItem>((Func<IExchangeDataItem>)(() => providerHelper.GetNewItem(connection.GetService())));
		newContact = ApplyBufferToDataItem(recordBuffer, newContact);
		outputRecordBuffer = ExchangeItemToRecordBuffer(newContact, recordBuffer);
		NavExchangeServiceBase.CatchServiceResponseException((Action)(() =>
		{
			connection.GetService().SaveItem(newContact, GetFolder().Id);
		}));
		return InsertResult.Success;
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> FindByPositionImplementation(PositionedFindProviderRequest request)
	{
		IExchangeFolder folder = GetFolder();
		IEnumerable<ReadOnlyRecordBuffer> records = ExchangeItemsToRecordBuffer(folder.ConvertToExchangeDataItem(folder.Read()), null, request.FiltersAndMarks);
		records = records.OrderBySortingFields(request.SortingFields, FindType.Normal);
		if (request.StartingPosition != null)
		{
			records = records.SkipWhile((ReadOnlyRecordBuffer x) => x.GetRecordId() != request.StartingPosition.GetRecordId());
			if (!request.IncludeCurrent)
			{
				records = records.Where((ReadOnlyRecordBuffer x) => !x.GetRecordId().Equals(request.StartingPosition.GetRecordId()));
			}
		}
		return records;
	}

	public IExchangeDataItem GetChangedItem(MutableRecordBuffer buffer)
	{
		IExchangeDataItem matchingItem = GetMatchingItem(buffer);
		buffer.OptimizeChanges();
		return ApplyBufferToDataItem(buffer, matchingItem);
	}

	private IExchangeDataItem ApplyBufferToDataItem(MutableRecordBuffer buffer, IExchangeDataItem dataItem)
	{
		for (int i = 1; i < table.FieldCount; i++)
		{
			if (buffer.GetChangedFieldValue(i) != null)
			{
				NCLMetaField fieldByIndex = table.GetFieldByIndex(i);
				string text = (string.IsNullOrEmpty(fieldByIndex.ExternalName) ? fieldByIndex.FieldName : fieldByIndex.ExternalName);
				((IExternalDataItem)dataItem).SetProperty(text, buffer.GetChangedFieldValue(i).ValueAsObject);
			}
		}
		return dataItem;
	}

	private IExchangeDataItem GetMatchingItem(IRecordBuffer buffer)
	{
		IExchangeFolder folder = GetFolder();
		NCLMetaField nCLMetaField = table.PrimaryKey.KeyFieldsList[0];
		object keyValue = buffer.GetPrimaryKeyValues()[0].ValueAsObject;
		IEnumerable<PropertyInfo> keyProperties = providerHelper.GetMatchingProperties(new string[2] { nCLMetaField.FieldName, nCLMetaField.ExternalName });
		IExchangeDataItem val = ((keyProperties.Count() > 0) ? (from x in folder.ConvertToExchangeDataItem(folder.Read())
			where keyProperties.First().GetValue(x).Equals(keyValue)
			select x).SingleOrDefault() : null);
		keyProperties = providerHelper.GetStartMatchingProperties(new string[2] { nCLMetaField.FieldName, nCLMetaField.ExternalName });
		if (val == null && keyProperties.Count() > 0)
		{
			string fieldName = (string.IsNullOrEmpty(nCLMetaField.ExternalName) ? nCLMetaField.FieldName : nCLMetaField.ExternalName);
			IEnumerable<IExchangeDataItem> source = from x in folder.ConvertToExchangeDataItem(folder.Read())
				where ((IExternalDataItem)x).GetProperty(fieldName)?.Equals(keyValue) ?? false
				select x;
			val = ((source.Count() > 0) ? source.First() : null);
		}
		return val;
	}

	public ReadOnlyRecordBuffer ExchangeItemToRecordBuffer(IExchangeDataItem exchangeItem, IRecordBuffer buffer = null, FiltersAndMarks filtermarks = null, NavRecordId recordid = null)
	{
		NavValue[] array = ((buffer == null) ? new NavValue[table.FieldCount] : buffer.ToArray());
		bool flag = true;
		array[0] = NavBigInteger.CreateFromObject(((IExternalDataItem)exchangeItem).Timestamp);
		for (int i = 1; i < table.FieldCount; i++)
		{
			NCLMetaField fieldByIndex = table.GetFieldByIndex(i);
			string text = ((fieldByIndex.ExternalName != null) ? fieldByIndex.ExternalName : string.Empty);
			string fieldName = fieldByIndex.FieldName;
			if (!fieldByIndex.FieldActive || fieldByIndex.FieldObsoleted || fieldByIndex.FieldClass == FieldClass.FlowField)
			{
				continue;
			}
			string text2 = (string.IsNullOrEmpty(text) ? fieldName : text);
			array[i] = ExternalDataItemConverter.GetNavValueFromType(exchangeItem.GetPropertyType(text2), ((IExternalDataItem)exchangeItem).GetProperty(text2));
			if (fieldByIndex.FieldIsPartOfPrimaryKey && recordid != null && !recordid.IsNull && !recordid.IsZeroOrEmpty && !recordid.FieldValueByIndex(fieldByIndex, 0).ValueAsObject.Equals(array[i].ValueAsObject) && !recordid.FieldValueByIndex(fieldByIndex, 0).IsZeroOrEmpty)
			{
				flag = false;
				break;
			}
			if (filtermarks != null && !filtermarks.IsEmpty && filtermarks.Filters.AnyFiltersOnNormalFields && filtermarks.Filters.FieldNames.Contains(fieldName))
			{
				FilterExpression filterOnField = filtermarks.Filters.GetFilterOnField(fieldByIndex);
				if (!filterOnField.Evaluate(array[i]))
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			return new ReadOnlyRecordBuffer(table, array);
		}
		return null;
	}

	public IEnumerable<ReadOnlyRecordBuffer> ExchangeItemsToRecordBuffer(IEnumerable<IExchangeDataItem> exchangeItems, IRecordBuffer buffer = null, FiltersAndMarks filtermarks = null, NavRecordId recordid = null)
	{
		foreach (IExchangeDataItem exchangeItem in exchangeItems)
		{
			ReadOnlyRecordBuffer readOnlyRecordBuffer = ExchangeItemToRecordBuffer(exchangeItem, buffer, filtermarks, recordid);
			if (readOnlyRecordBuffer != null)
			{
				yield return readOnlyRecordBuffer;
			}
		}
	}
}
