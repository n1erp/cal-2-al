using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Security;
using System.Text;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.Xrm.Proxy;
using Microsoft.Dynamics.Nav.Xrm.Proxy.Query;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CrmDataProvider : ExternalDataProvider
{
	private const string AttributeTypeCustomer = "Customer";

	private const string AttributeTypeOwner = "Owner";

	private const string AttributeTypeLookup = "Lookup";

	private const string AttributeTypeUniqueidentifier = "Uniqueidentifier";

	private const string AttributeTypeString = "String";

	private const string AttributeTypeStatus = "Status";

	private const string AttributeTypeState = "State";

	private const string AttributeTypePicklist = "Picklist";

	private const string AttributeTypeInteger = "Integer";

	private const string AttributeTypeBigInt = "BigInt";

	private const string AttributeTypeMoney = "Money";

	private const string AttributeTypeDecimal = "Decimal";

	private const string AttributeTypeDouble = "Double";

	private const string AttributeTypeBoolean = "Boolean";

	private const string AttributeTypeDateTime = "DateTime";

	private const string AttributeTypeEntityName = "EntityName";

	private const string AttributeTypeMemo = "Memo";

	public const int ErrAuthenticationFailure = -2146233087;

	private const int PageSize = 50;

	private readonly NavSession navSession;

	private readonly NCLMetaTable table;

	private readonly SortingFieldList primaryKeyFieldSortingFields;

	private readonly NCLMetaField primaryKeyField;

	private readonly NCLMetaField stateField;

	private readonly NCLMetaField statusField;

	private readonly Dictionary<int, NCLMetaField> fieldIsAttributeOf;

	private readonly CrmTableConnection connection;

	private static NavMetric2Dimensions crmTaskDurationMetric;

	private static NavMetric2Dimensions CrmTaskDurationMetricInstance => crmTaskDurationMetric ?? (crmTaskDurationMetric = new NavMetric2Dimensions("CrmDuration", "Method", "Task"));

	private bool IsIntersectEntity => !string.IsNullOrEmpty(table.ExternalSchema);

	public override bool SupportsAutoCalcFields => true;

	internal CrmDataProvider(NavSession session, NCLMetaTable table)
		: base(table)
	{
		navSession = session;
		connection = navSession.TableConnectionManager.GetCurrentTableConnection<CrmTableConnection>(TableConnectionType.CRM);
		this.table = table;
		primaryKeyFieldSortingFields = this.table.PrimaryKey.DefinedSortingFields;
		primaryKeyField = this.table.GetFieldByIndex(this.table.PrimaryKey.KeyFieldsList[0].FieldIndex);
		stateField = this.table.Fields.SingleOrDefault((NCLMetaField f) => f.ExternalType == "State");
		statusField = this.table.Fields.SingleOrDefault((NCLMetaField f) => f.ExternalType == "Status");
		if (stateField != null && statusField == null)
		{
			throw new NavCrmException(string.Format(CultureInfo.CurrentCulture, Lang.CrmStatusFieldMissing));
		}
		fieldIsAttributeOf = BuildFieldIsAttributeOf();
	}

	private IXrmConnection GetService()
	{
		return connection.GetXrmConnection();
	}

	public override FieldDictionary<NavValue> GetBlobContent(GetBlobContentProviderRequest request)
	{
		Tuple<INavFieldMetadata, NavValue>[] array = new Tuple<INavFieldMetadata, NavValue>[request.BlobsToGet.Count];
		ReadOnlyRecordBuffer recordBuffer = null;
		if (request.RecordBuffer is MutableRecordBuffer mutableRecordBuffer)
		{
			recordBuffer = mutableRecordBuffer.ReadOnlyBuffer;
		}
		else if (!TryGetByEntityId(request.RecordBuffer[primaryKeyField.FieldIndex].ToGuid(), out recordBuffer))
		{
			throw RecordImplementationHelper.GetRecordNotFoundException(table, new Tuple<NCLMetaField, NavValue>[1]
			{
				new Tuple<NCLMetaField, NavValue>(primaryKeyField, request.RecordBuffer[primaryKeyField.FieldIndex])
			}, navSession.Tenant?.Id);
		}
		for (int i = 0; i < request.BlobsToGet.Count; i++)
		{
			NCLMetaField nCLMetaField = (NCLMetaField)request.BlobsToGet[i];
			if (nCLMetaField.FieldNclType != NavNclType.NavBlob)
			{
				throw new InvalidOperationException("This method can only get the content of blobs");
			}
			NavBLOB navBLOB = (NavBLOB)recordBuffer[nCLMetaField.FieldIndex];
			NavBLOB item = new NavBLOB((navBLOB != null) ? navBLOB.GetBytes() : new byte[0]);
			array[i] = new Tuple<INavFieldMetadata, NavValue>(nCLMetaField, item);
		}
		return new FieldDictionary<NavValue>(array);
	}

	public override bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer)
	{
		recordBuffer = null;
		Guid idFromRecordId = GetIdFromRecordId(request.RecordId);
		if (Guid.Empty != idFromRecordId)
		{
			return TryGetByEntityId(idFromRecordId, out recordBuffer);
		}
		return false;
	}

	private bool TryGetByEntityId(Guid id, out ReadOnlyRecordBuffer recordBuffer)
	{
		if (IsIntersectEntity)
		{
			return TryGetIntersectEntityByEntityId(id, out recordBuffer);
		}
		ReadOnlyRecordBuffer buffer = null;
		bool result = ExecuteServiceCallWithTrace((EventTask)26, (IXrmConnection service) =>
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected Obj, but got Unknown
			IXrmEntity entity = ((IXrmOrganizationService)service).Retrieve(table.ExternalName, id, (IXrmColumnSet)new XrmColumnSet(true));
			buffer = EntityToRecordBuffer(entity);
			return true;
		}, null, (Exception exception) =>
		{
			XrmFaultException ex = (XrmFaultException)(object)((exception is XrmFaultException) ? exception : null);
			return ex != null && ex.ErrorCode == -2147220969;
		});
		recordBuffer = buffer;
		return result;
	}

	private bool TryGetIntersectEntityByEntityId(Guid id, out ReadOnlyRecordBuffer recordBuffer)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected Obj, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected Obj, but got Unknown
		//IL_004b: Expected Obj, but got Unknown
		recordBuffer = null;
		XrmQueryExpression query = new XrmQueryExpression(table.ExternalName)
		{
			ColumnSet = (IXrmColumnSet)new XrmColumnSet(true),
			PageInfo = (IXrmPagingInfo)new XrmPagingInfo
			{
				PagingCookie = null,
				PageNumber = 1,
				Count = 1
			}
		};
		query.Criteria.AddCondition(primaryKeyField.ExternalName, (XrmConditionOperator)0, new object[1] { id });
		IXrmEntityCollection val = ExecuteServiceCallWithTrace((EventTask)25, (IXrmConnection s) => ((IXrmOrganizationService)s).RetrieveMultiple((IXrmQueryExpression)(object)query));
		if (val.Entities.Count != 1)
		{
			return false;
		}
		recordBuffer = EntityToRecordBuffer(val.Entities[0]);
		return true;
	}

	public override int Count(DataProviderRequest request)
	{
		XrmQueryExpression query = CreateFilterQuery(request.FiltersAndMarks);
		query.PageInfo.Count = 1;
		query.PageInfo.ReturnTotalRecordCount = true;
		return ExecuteServiceCallWithTrace((EventTask)25, (IXrmConnection service) => ((IXrmOrganizationService)service).RetrieveMultiple((IXrmQueryExpression)(object)query).TotalRecordCount, query);
	}

	public override FieldDictionary<NavValue> CalcNumeric(CalcNumericProviderRequest request)
	{
		DataProvider.ValidateNotNull(request, "request");
		int count = request.FieldsToCalculate.Count;
		if (request.FieldsToCalculate.Any((INavFieldMetadata field) =>
		{
			NCLMetaCalculationMethod calculationMethod2 = ((NCLMetaField)field).CalculationFormula.CalculationMethod;
			return calculationMethod2 == NCLMetaCalculationMethod.Average || calculationMethod2 == NCLMetaCalculationMethod.Sum;
		}))
		{
			return base.CalcNumeric(request);
		}
		int num = Count(request);
		Tuple<INavFieldMetadata, NavValue>[] array = new Tuple<INavFieldMetadata, NavValue>[count];
		for (int num2 = 0; num2 < count; num2++)
		{
			NCLMetaField nCLMetaField = (NCLMetaField)request.FieldsToCalculate[num2];
			NavValue item = null;
			try
			{
				NCLMetaCalculationMethod calculationMethod = nCLMetaField.CalculationFormula.CalculationMethod;
				if (calculationMethod == NCLMetaCalculationMethod.Count)
				{
					item = NavValue.CreateNavValueFromObject(nCLMetaField, num);
				}
			}
			catch (OverflowException)
			{
				throw new NavNCLCalculationOverflowException(string.Format(CultureInfo.CurrentCulture, Lang.FieldCalculationOverflow, nCLMetaField.FieldName));
			}
			array[num2] = new Tuple<INavFieldMetadata, NavValue>(request.FieldsToCalculate[num2], item);
		}
		return new FieldDictionary<NavValue>(array);
	}

	public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected Obj, but got Unknown
		outputRecordBuffer = null;
		if (IsIntersectEntity)
		{
			return InsertIntersectEntity(recordBuffer, out outputRecordBuffer);
		}
		IXrmEntity entity = (IXrmEntity)new XrmEntity(table.ExternalName);
		RecordBufferToEntity(recordBuffer, entity, isUpdate: false);
		bool duplicate = false;
		Guid id = ExecuteServiceCallWithTrace((EventTask)27, (IXrmConnection service) => ((IXrmOrganizationService)service).Create(entity), null, (Exception exception) =>
		{
			XrmFaultException ex = (XrmFaultException)(object)((exception is XrmFaultException) ? exception : null);
			if (ex != null && ex.ErrorCode == -2147220937)
			{
				duplicate = true;
				return true;
			}
			return false;
		});
		if (duplicate)
		{
			return InsertResult.RecordAlreadyExists;
		}
		entity = ExecuteServiceCallWithTrace((EventTask)26, (IXrmConnection service) =>
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected Obj, but got Unknown
			return ((IXrmOrganizationService)service).Retrieve(table.ExternalName, id, (IXrmColumnSet)new XrmColumnSet(true));
		});
		outputRecordBuffer = EntityToRecordBuffer(entity);
		return InsertResult.Success;
	}

	private InsertResult InsertIntersectEntity(MutableRecordBuffer recordBuffer, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected Obj, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected Obj, but got Unknown
		//IL_00a9: Expected Obj, but got Unknown
		outputRecordBuffer = null;
		GetForeignTablesAndFields(out var field1, out var field2, out var table1, out var table2);
		bool duplicate = false;
		ExecuteServiceCallWithTrace((EventTask)30, (IXrmConnection service) =>
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Expected Obj, but got Unknown
			//IL_0077: Expected Obj, but got Unknown
			((IXrmOrganizationService)service).Associate(table1.ExternalName, recordBuffer[field1.FieldIndex].ToGuid(), (IXrmRelationship)new XrmRelationship(table.ExternalSchema), (IList<IXrmEntityReference>)new List<IXrmEntityReference> { (IXrmEntityReference)new XrmEntityReference(table2.ExternalName, recordBuffer[field2.FieldIndex].ToGuid()) });
		}, (Exception exception) =>
		{
			XrmFaultException ex = (XrmFaultException)(object)((exception is XrmFaultException) ? exception : null);
			if (ex != null && ex.ErrorCode == -2147220937)
			{
				duplicate = true;
				return true;
			}
			return false;
		});
		if (duplicate)
		{
			return InsertResult.RecordAlreadyExists;
		}
		XrmQueryExpression query = new XrmQueryExpression(table.ExternalName)
		{
			ColumnSet = (IXrmColumnSet)new XrmColumnSet(true),
			PageInfo = (IXrmPagingInfo)new XrmPagingInfo
			{
				PagingCookie = null,
				PageNumber = 1,
				Count = 1
			}
		};
		query.Criteria.AddCondition(field1.ExternalName, (XrmConditionOperator)0, new object[1] { recordBuffer[field1.FieldIndex].ToGuid() });
		query.Criteria.AddCondition(field2.ExternalName, (XrmConditionOperator)0, new object[1] { recordBuffer[field2.FieldIndex].ToGuid() });
		IXrmEntityCollection val = ExecuteServiceCallWithTrace((EventTask)25, (IXrmConnection s) => ((IXrmOrganizationService)s).RetrieveMultiple((IXrmQueryExpression)(object)query));
		outputRecordBuffer = EntityToRecordBuffer(val.Entities[0]);
		return InsertResult.Success;
	}

	private void GetForeignTablesAndFields(out NCLMetaField field1, out NCLMetaField field2, out NCLMetaTable table1, out NCLMetaTable table2)
	{
		NCLMetaField[] array = table.Fields.Where((NCLMetaField f) => f.FieldRelations != null && f.FieldRelations.Count == 1).Reverse().ToArray();
		if (array.Count() != 2)
		{
			throw new NavCrmException(string.Format(CultureInfo.CurrentCulture, Lang.CrmInvalidMetadataForIntersectEntity, table.TableId, table.ExternalName));
		}
		field1 = array[0];
		field2 = array[1];
		table1 = navSession.NCLMetadata.GetMetaTableById(field1.FieldRelations[0].SourceTableId, -1, requireCompiled: false);
		table2 = navSession.NCLMetadata.GetMetaTableById(field2.FieldRelations[0].SourceTableId, -1, requireCompiled: false);
	}

	public override ModifyResult Modify(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Expected Obj, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected Obj, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected Obj, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected Obj, but got Unknown
		outputRecordBuffer = null;
		bool flag = recordBuffer.MetaTable.Fields.Any((NCLMetaField fi) => fi != stateField && recordBuffer.IsChangedValueSameAsOriginalValue(fi.FieldIndex));
		if (stateField != null && statusField != null && !recordBuffer.IsChangedValueSameAsOriginalValue(stateField.FieldIndex))
		{
			IXrmOrganizationRequest setStateRequest = (IXrmOrganizationRequest)new XrmOrganizationRequest
			{
				RequestName = "SetState"
			};
			setStateRequest.Parameters["EntityMoniker"] = (object)new XrmEntityReference(MetaTable.ExternalName, GetIdFromRecordBuffer(recordBuffer));
			setStateRequest.Parameters["State"] = GetOptionSetValue(recordBuffer[stateField.FieldIndex], stateField);
			if (!recordBuffer.IsChangedValueSameAsOriginalValue(statusField.FieldIndex))
			{
				setStateRequest.Parameters["Status"] = GetOptionSetValue(recordBuffer[statusField.FieldIndex], statusField);
			}
			else
			{
				setStateRequest.Parameters["Status"] = (object)new XrmOptionSetValue(-1);
			}
			ExecuteServiceCallWithTrace((EventTask)32, (IXrmConnection service) => ((IXrmOrganizationService)service).Execute(setStateRequest), null, null, recordBuffer);
			if (flag)
			{
				if (!TryGetByEntityId(GetIdFromRecordBuffer(recordBuffer), out outputRecordBuffer))
				{
					return ModifyResult.RecordNotFound;
				}
				return ModifyResult.Success;
			}
		}
		XrmColumnSet cols = new XrmColumnSet(Array.Empty<string>());
		for (int num = 0; num < table.FieldCount; num++)
		{
			if (!recordBuffer.IsChangedValueSameAsOriginalValue(num))
			{
				cols.AddColumn(table.GetFieldByIndex(num).ExternalName);
			}
		}
		Guid id = GetIdFromRecordBuffer(recordBuffer);
		IXrmEntity entity = ExecuteServiceCallWithTrace((EventTask)26, (IXrmConnection service) => ((IXrmOrganizationService)service).Retrieve(table.ExternalName, id, (IXrmColumnSet)(object)cols), null, (Exception exception) =>
		{
			XrmFaultException ex = (XrmFaultException)(object)((exception is XrmFaultException) ? exception : null);
			return ex != null && ex.ErrorCode == -2147220969;
		});
		if (entity == null)
		{
			return ModifyResult.RecordNotFound;
		}
		RecordBufferToEntity(recordBuffer, entity, isUpdate: true, (NCLMetaField fi) => !recordBuffer.IsChangedValueSameAsOriginalValue(fi.FieldIndex));
		ExecuteServiceCallWithTrace((EventTask)28, (IXrmConnection service) =>
		{
			((IXrmOrganizationService)service).Update(entity);
		}, null, recordBuffer);
		if (!TryGetByEntityId(id, out outputRecordBuffer))
		{
			return ModifyResult.RecordNotFound;
		}
		return ModifyResult.Success;
	}

	public override void ModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldDictionary<NavValue> newFieldValues)
	{
		ForeachEntity(filtersAndMarks, (IXrmEntity e) =>
		{
			Tuple<INavFieldMetadata, NavValue>[] items = newFieldValues.Items;
			foreach (Tuple<INavFieldMetadata, NavValue> tuple in items)
			{
				AssignEntityAttribute(e, metaTable.GetFieldByIndex(tuple.Item1.ColumnIndex), tuple.Item2, isUpdate: true, null);
			}
			ExecuteServiceCallWithTrace((EventTask)28, (IXrmConnection service) =>
			{
				((IXrmOrganizationService)service).Update(e);
			});
		});
	}

	public override DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction)
	{
		if (IsIntersectEntity)
		{
			return DeleteIntersectEntity(recordBuffer);
		}
		if (!ExecuteServiceCallWithTrace((EventTask)29, (IXrmConnection service) =>
		{
			((IXrmOrganizationService)service).Delete(table.ExternalName, GetIdFromRecordBuffer(recordBuffer));
			return true;
		}, null, (Exception exception) =>
		{
			XrmFaultException ex = (XrmFaultException)(object)((exception is XrmFaultException) ? exception : null);
			return ex != null && ex.ErrorCode == -2147220969;
		}))
		{
			return DeleteResult.RecordNotFound;
		}
		return DeleteResult.Success;
	}

	private DeleteResult DeleteIntersectEntity(MutableRecordBuffer recordBuffer)
	{
		GetForeignTablesAndFields(out var field1, out var field2, out var table1, out var table2);
		ExecuteServiceCallWithTrace((EventTask)31, (IXrmConnection service) =>
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Expected Obj, but got Unknown
			//IL_0077: Expected Obj, but got Unknown
			((IXrmOrganizationService)service).Disassociate(table1.ExternalName, recordBuffer[field1.FieldIndex].ToGuid(), (IXrmRelationship)new XrmRelationship(table.ExternalSchema), (IList<IXrmEntityReference>)new List<IXrmEntityReference> { (IXrmEntityReference)new XrmEntityReference(table2.ExternalName, recordBuffer[field2.FieldIndex].ToGuid()) });
		});
		return DeleteResult.Success;
	}

	public override void DeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		ForeachEntity(filtersAndMarks, (IXrmEntity e) =>
		{
			ExecuteServiceCallWithTrace((EventTask)29, (IXrmConnection service) =>
			{
				((IXrmOrganizationService)service).Delete(table.ExternalName, e.Id);
			});
		});
	}

	private void ForeachEntity(FiltersAndMarks filtersAndMarks, Action<IXrmEntity> action)
	{
		XrmQueryExpression query = CreateFilterQuery(filtersAndMarks);
		while (true)
		{
			IXrmEntityCollection val = ExecuteServiceCallWithTrace((EventTask)25, (IXrmConnection service) => ((IXrmOrganizationService)service).RetrieveMultiple((IXrmQueryExpression)(object)query), query);
			if (val.Entities == null)
			{
				break;
			}
			foreach (IXrmEntity entity in val.Entities)
			{
				action(entity);
			}
			if (val.MoreRecords)
			{
				IXrmPagingInfo pageInfo = query.PageInfo;
				int pageNumber = pageInfo.PageNumber;
				pageInfo.PageNumber = pageNumber + 1;
				query.PageInfo.PagingCookie = val.PagingCookie;
				continue;
			}
			break;
		}
	}

	private static XrmConditionOperator MapConditionOperator(FilterExpressionType exprType)
	{
		return exprType switch
		{
			FilterExpressionType.Equal => (XrmConditionOperator)0, 
			FilterExpressionType.NotEqual => (XrmConditionOperator)1, 
			FilterExpressionType.GreaterThan => (XrmConditionOperator)2, 
			FilterExpressionType.GreaterThanOrEqual => (XrmConditionOperator)4, 
			FilterExpressionType.LessThan => (XrmConditionOperator)3, 
			FilterExpressionType.LessThanOrEqual => (XrmConditionOperator)5, 
			_ => throw new NotSupportedException(), 
		};
	}

	private void AddFilter(IXrmFilterExpression parent, FilterExpression filter)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		switch (filter.ExpressionType)
		{
		case FilterExpressionType.And:
		case FilterExpressionType.Or:
		{
			IXrmFilterExpression parent2 = parent.AddFilter((XrmLogicalOperator)(filter.ExpressionType != FilterExpressionType.And));
			BinaryFilterExpression binaryFilterExpression = (BinaryFilterExpression)filter;
			AddFilter(parent2, binaryFilterExpression.Left);
			AddFilter(parent2, binaryFilterExpression.Right);
			break;
		}
		case FilterExpressionType.Equal:
		case FilterExpressionType.NotEqual:
		case FilterExpressionType.GreaterThan:
		case FilterExpressionType.LessThan:
		case FilterExpressionType.GreaterThanOrEqual:
		case FilterExpressionType.LessThanOrEqual:
		{
			UnaryFilterExpression unaryFilterExpression = (UnaryFilterExpression)filter;
			NCLMetaField fieldByIndex = table.GetFieldByIndex(unaryFilterExpression.ExpressionContext.Metadata.ColumnIndex);
			if (!UseNullFilter(ref parent, fieldByIndex, unaryFilterExpression))
			{
				parent.AddCondition(fieldByIndex.ExternalName, MapConditionOperator(filter.ExpressionType), new object[1] { NavValueToCrmValue(fieldByIndex, unaryFilterExpression.Value) });
			}
			break;
		}
		case FilterExpressionType.Wildcard:
		{
			WildcardFilterExpression wildcardFilterExpression = (WildcardFilterExpression)filter;
			NCLMetaField fieldByIndex = table.GetFieldByIndex(wildcardFilterExpression.ExpressionContext.Metadata.ColumnIndex);
			string text = NavSqlStatementHelper.WildcardText(wildcardFilterExpression.Pattern);
			parent.AddCondition(fieldByIndex.ExternalName, (XrmConditionOperator)(wildcardFilterExpression.IsNegated ? 7 : 6), new object[1] { text });
			break;
		}
		case FilterExpressionType.RangeBetweenInclusive:
		{
			RangeFilterExpression rangeFilterExpression3 = (RangeFilterExpression)filter;
			IXrmFilterExpression val = parent.AddFilter((XrmLogicalOperator)0);
			NCLMetaField fieldByIndex = table.GetFieldByIndex(rangeFilterExpression3.ExpressionContext.Metadata.ColumnIndex);
			if (string.IsNullOrEmpty(rangeFilterExpression3.LowValue.ToString()) & string.IsNullOrEmpty(rangeFilterExpression3.HighValue.ToString()))
			{
				val.AddCondition(fieldByIndex.ExternalName, (XrmConditionOperator)12, Array.Empty<object>());
				break;
			}
			val.AddCondition(fieldByIndex.ExternalName, (XrmConditionOperator)4, new object[1] { NavValueToCrmValue(fieldByIndex, rangeFilterExpression3.LowValue) });
			val.AddCondition(fieldByIndex.ExternalName, (XrmConditionOperator)5, new object[1] { NavValueToCrmValue(fieldByIndex, rangeFilterExpression3.HighValue) });
			break;
		}
		case FilterExpressionType.RangeFromInclusive:
		{
			RangeFilterExpression rangeFilterExpression2 = (RangeFilterExpression)filter;
			NCLMetaField fieldByIndex = table.GetFieldByIndex(rangeFilterExpression2.ExpressionContext.Metadata.ColumnIndex);
			parent.AddCondition(fieldByIndex.ExternalName, (XrmConditionOperator)4, new object[1] { NavValueToCrmValue(fieldByIndex, rangeFilterExpression2.LowValue) });
			break;
		}
		case FilterExpressionType.RangeToInclusive:
		{
			RangeFilterExpression rangeFilterExpression = (RangeFilterExpression)filter;
			NCLMetaField fieldByIndex = table.GetFieldByIndex(rangeFilterExpression.ExpressionContext.Metadata.ColumnIndex);
			parent.AddCondition(fieldByIndex.ExternalName, (XrmConditionOperator)5, new object[1] { NavValueToCrmValue(fieldByIndex, rangeFilterExpression.HighValue) });
			break;
		}
		default:
			throw new NavNotSupportedException(filter.ExpressionType.ToString());
		}
	}

	private static bool UseNullFilter(ref IXrmFilterExpression parent, NCLMetaField field, UnaryFilterExpression unary)
	{
		if (!unary.Value.IsNull)
		{
			switch (unary.Value.NclType)
			{
			case NavNclType.NavText:
				if (!string.IsNullOrEmpty(unary.Value.ToString()))
				{
					return false;
				}
				break;
			case NavNclType.NavGuid:
				if (unary.Value.ToGuid() != Guid.Empty)
				{
					return false;
				}
				break;
			case NavNclType.NavDate:
				if (!((NavDate)unary.Value).IsZeroOrEmpty)
				{
					return false;
				}
				break;
			case NavNclType.NavDateTime:
				if (!((NavDateTime)unary.Value).IsZeroOrEmpty)
				{
					return false;
				}
				break;
			default:
				return false;
			}
		}
		switch (unary.ExpressionType)
		{
		case FilterExpressionType.Equal:
			parent.AddCondition(field.ExternalName, (XrmConditionOperator)12, Array.Empty<object>());
			break;
		case FilterExpressionType.NotEqual:
			parent.AddCondition(field.ExternalName, (XrmConditionOperator)13, Array.Empty<object>());
			break;
		default:
		{
			NavNclType nclType = unary.Value.NclType;
			if (nclType == NavNclType.NavText || nclType == NavNclType.NavGuid)
			{
				return false;
			}
			throw new NavNotSupportedException(unary.ExpressionType.ToString());
		}
		}
		return true;
	}

	private XrmQueryExpression CreateFilterQuery(FiltersAndMarks filtersAndMarks, MutableRecordBuffer startingPosition = null, SortingFieldList sortingFields = null, bool includeCurrent = true)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected Obj, but got Unknown
		//IL_0050: Expected Obj, but got Unknown
		XrmQueryExpression query = new XrmQueryExpression(table.ExternalName)
		{
			ColumnSet = (IXrmColumnSet)new XrmColumnSet(true),
			PageInfo = (IXrmPagingInfo)new XrmPagingInfo
			{
				PagingCookie = null,
				PageNumber = 1,
				Count = 50
			}
		};
		if (sortingFields == null)
		{
			sortingFields = primaryKeyFieldSortingFields;
		}
		IEnumerable<XrmOrderExpression> source = ((IEnumerable<INavSortingField>)sortingFields).Select((Func<INavSortingField, XrmOrderExpression>)((INavSortingField sf) =>
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected Obj, but got Unknown
			return new XrmOrderExpression(table.GetFieldByIndex(sf.Field.ColumnIndex).ExternalName, (XrmOrderType)(sf.Order != SortOrder.Ascending));
		}));
		source.ForEach((XrmOrderExpression o) =>
		{
			query.Orders.Add((IXrmOrderExpression)(object)o);
		});
		FilterExpression filterExpression = ExternalDataProvider.GetCombinedExpressionFilter(filtersAndMarks, null);
		if (filterExpression == null && startingPosition == null)
		{
			return query;
		}
		if (startingPosition != null)
		{
			FilterExpression right = DataProviderHelper.BuildPositioningFilterFromRecordBuffer(startingPosition, sortingFields, includeCurrent);
			filterExpression = FilterExpression.And(filterExpression, right);
		}
		AddFilter(query.Criteria, filterExpression);
		return query;
	}

	private XrmQueryExpression CreateFilterQuery(PositionedFindProviderRequest request)
	{
		return CreateFilterQuery(request.FiltersAndMarks, request.StartingPosition, request.SortingFields, request.IncludeCurrent);
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> FindByPositionImplementation(PositionedFindProviderRequest request)
	{
		XrmQueryExpression query = CreateFilterQuery(request);
		while (true)
		{
			IXrmEntityCollection result = ExecuteServiceCallWithTrace((EventTask)25, (IXrmConnection service) => ((IXrmOrganizationService)service).RetrieveMultiple((IXrmQueryExpression)(object)query), query);
			if (result.Entities == null)
			{
				break;
			}
			foreach (IXrmEntity entity in result.Entities)
			{
				yield return EntityToRecordBuffer(entity);
			}
			if (result.MoreRecords)
			{
				IXrmPagingInfo pageInfo = query.PageInfo;
				int pageNumber = pageInfo.PageNumber;
				pageInfo.PageNumber = pageNumber + 1;
				query.PageInfo.PagingCookie = result.PagingCookie;
				continue;
			}
			break;
		}
	}

	private Guid GetIdFromRecordBuffer(IRecordBuffer recordBuffer)
	{
		return recordBuffer[primaryKeyField.FieldIndex].ToGuid();
	}

	private Guid GetIdFromRecordId(NavRecordId recordId)
	{
		return ((NavGuid)recordId.FieldValueByIndex(primaryKeyField, 0)).Value;
	}

	private ReadOnlyRecordBuffer EntityToRecordBuffer(IXrmEntity entity, IRecordBuffer originalBuffer = null)
	{
		NavValue[] array = ((originalBuffer == null) ? new NavValue[table.FieldCount] : originalBuffer.ToArray());
		array[0] = NavBigInteger.Create(entity.GetAttributeValue<DateTime>("modifiedon").Ticks);
		for (int i = 1; i < table.FieldCount; i++)
		{
			NCLMetaField fi = table.GetFieldByIndex(i);
			if (!fi.FieldActive || fi.FieldObsoleted || !entity.Contains(fi.ExternalName) || fi.FieldClass == FieldClass.FlowField)
			{
				continue;
			}
			try
			{
				switch (fi.ExternalType)
				{
				case "Customer":
				case "Owner":
				case "Lookup":
				{
					bool flag = false;
					try
					{
						IXrmEntityReference attributeValue4 = entity.GetAttributeValue<IXrmEntityReference>(fi.ExternalName);
						if (attributeValue4 != null)
						{
							array[i] = new NavGuid(attributeValue4.Id);
							flag = true;
						}
					}
					catch (InvalidCastException)
					{
					}
					if (!flag)
					{
						array[i] = new NavGuid(entity.GetAttributeValue<Guid>(fi.ExternalName));
					}
					break;
				}
				case "Uniqueidentifier":
					try
					{
						array[i] = new NavGuid(entity.GetAttributeValue<Guid>(fi.ExternalName));
					}
					catch (InvalidCastException)
					{
						IXrmEntityReference attributeValue3 = entity.GetAttributeValue<IXrmEntityReference>(fi.ExternalName);
						array[i] = new NavGuid(attributeValue3.Id);
					}
					break;
				case "String":
					array[i] = new NavText(fi.FieldDefinedLength, TrimText(entity.GetAttributeValue<string>(fi.ExternalName), fi.FieldDefinedLength));
					break;
				case "State":
				case "Status":
				case "Picklist":
					array[i] = NavOption.Create(fi.FieldOptionMetadata, fi.MapExternalOptionValueToNavOptionValue(entity.GetAttributeValue<IXrmOptionSetValue>(fi.ExternalName).Value));
					break;
				case "Integer":
					array[i] = NavInteger.Create(entity.GetAttributeValue<int>(fi.ExternalName));
					break;
				case "BigInt":
					array[i] = NavBigInteger.Create(entity.GetAttributeValue<long>(fi.ExternalName));
					break;
				case "Money":
					array[i] = NavDecimal.Create(entity.GetAttributeValue<XrmMoney>(fi.ExternalName).Value);
					break;
				case "Decimal":
					array[i] = NavDecimal.Create(entity.GetAttributeValue<decimal>(fi.ExternalName));
					break;
				case "Double":
					array[i] = NavDecimal.Create(entity.GetAttributeValue<double>(fi.ExternalName));
					break;
				case "Boolean":
					array[i] = NavBoolean.Create(entity.GetAttributeValue<bool>(fi.ExternalName));
					break;
				case "DateTime":
					if (fi.FieldNclType == NavNclType.NavDate)
					{
						DateTime attributeValue2 = entity.GetAttributeValue<DateTime>(fi.ExternalName);
						array[i] = ALSystemDate.ALDMY2Date(attributeValue2.Day, attributeValue2.Month, attributeValue2.Year);
					}
					else
					{
						array[i] = NavDateTime.Create(entity.GetAttributeValue<DateTime>(fi.ExternalName));
					}
					break;
				case "Memo":
				{
					NavBLOB navBLOB = new NavBLOB();
					BlobSubType blobSubType = fi.BlobSubType;
					if (blobSubType == BlobSubType.Memo)
					{
						string attributeValue = entity.GetAttributeValue<string>(fi.ExternalName);
						if (attributeValue == null)
						{
							throw new NavCrmException(string.Format(CultureInfo.CurrentCulture, Lang.CrmUnsupportedBlobDataType, entity.Attributes.FirstOrDefault((KeyValuePair<string, object> t) => t.Key == fi.ExternalName).Value?.GetType(), fi.FieldName, table.TableName, table.TableId));
						}
						navBLOB.AssignFromBytes(Encoding.Unicode.GetBytes(attributeValue));
						navBLOB.IsDirty = false;
						array[i] = navBLOB;
						break;
					}
					throw new NavCrmException(string.Format(CultureInfo.CurrentCulture, Lang.CrmUnsupportedBlobSubType, fi.BlobSubType));
				}
				default:
					throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.CrmUnsupportedExternalType, fi.ExternalType));
				case "EntityName":
					break;
				}
			}
			catch (InvalidCastException exception)
			{
				string content = string.Format(CultureInfo.InvariantCulture, "Invalid type cast for crm entity {0}, expected type {1}. Default value will be returned to the field of type {2}", fi.ExternalName, fi.ExternalType, fi.Field.NavType);
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001NV", Category.Crm, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata(content));
				array[i] = fi.InitValue;
			}
		}
		for (int num = 1; num < table.FieldCount; num++)
		{
			if (array[num] == null)
			{
				NCLMetaField fieldByIndex = table.GetFieldByIndex(num);
				if (!UpdateRelatedFields(entity, fieldByIndex, array))
				{
					array[num] = NavValue.GetDefaultNavValue(fieldByIndex, nullSupport: true);
				}
			}
		}
		return new ReadOnlyRecordBuffer(table, array);
	}

	private bool UpdateRelatedFields(IXrmEntity entity, NCLMetaField field, NavValue[] values)
	{
		if (field.FieldRelations == null)
		{
			return false;
		}
		NCLMetaField attributeOfField = GetAttributeOfField(field);
		if (attributeOfField == null)
		{
			return false;
		}
		IXrmEntityReference attributeValue = entity.GetAttributeValue<IXrmEntityReference>(attributeOfField.ExternalName);
		if (attributeValue == null)
		{
			return false;
		}
		switch (attributeOfField.ExternalType)
		{
		case "Customer":
		case "Owner":
			values[field.FieldIndex] = field.FieldOptionMetadata.CreateNavOption(field.FieldOptionMetadata.GetIndexFromOption(attributeValue.LogicalName));
			return true;
		case "Lookup":
			switch (field.FieldNclType)
			{
			case NavNclType.NavText:
				values[field.FieldIndex] = new NavText(field.FieldDefinedLength, (field.FieldClass == FieldClass.FlowField) ? attributeValue.Name : attributeValue.LogicalName);
				return true;
			case NavNclType.NavOption:
				values[field.FieldIndex] = field.FieldOptionMetadata.CreateNavOption(field.FieldOptionMetadata.GetIndexFromOption(attributeValue.LogicalName));
				return true;
			}
			break;
		}
		return false;
	}

	private static object NavValueToCrmValue(NCLMetaField fieldInfo, NavValue value)
	{
		switch (fieldInfo.ExternalType)
		{
		case "Customer":
		case "Owner":
		case "Lookup":
		case "Uniqueidentifier":
			return value.ToGuid();
		case "EntityName":
		case "String":
			return value.ToString();
		case "Integer":
			return value.ToInt32();
		case "State":
		case "Status":
		case "Picklist":
			return fieldInfo.MapNavOptionValueToExternalOptionValue(value.ToInt32());
		case "Money":
			return (decimal)value.ToDecimal();
		case "Decimal":
			return (decimal)value.ToDecimal();
		case "Double":
			return (double)(decimal)value.ToDecimal();
		case "Boolean":
			return value.ToBoolean();
		case "DateTime":
		{
			if (value.NclType == NavNclType.NavDateTime)
			{
				if (NavDateTime.Default == (NavDateTime)value)
				{
					return null;
				}
				return value.ToDateTime();
			}
			if (NavDate.Default == (NavDate)value)
			{
				return null;
			}
			DateTime dateTime = value.ToDateTime();
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Utc);
		}
		default:
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.CrmUnsupportedExternalType, fieldInfo.ExternalType));
		}
	}

	private void RecordBufferToEntity(MutableRecordBuffer recordBuffer, IXrmEntity entity, bool isUpdate, Predicate<NCLMetaField> shouldTransfer = null)
	{
		for (int i = 0; i < table.FieldCount; i++)
		{
			NCLMetaField fieldByIndex = table.GetFieldByIndex(i);
			if (shouldTransfer == null || shouldTransfer(fieldByIndex))
			{
				AssignEntityAttribute(entity, fieldByIndex, recordBuffer[i], isUpdate, recordBuffer);
			}
		}
	}

	private void AssignEntityAttribute(IXrmEntity entity, NCLMetaField fi, NavValue navValue, bool isUpdate, IRecordBuffer recordBuffer)
	{
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Expected Obj, but got Unknown
		//IL_02bb: Expected Obj, but got Unknown
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Expected Obj, but got Unknown
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Expected Obj, but got Unknown
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Expected Obj, but got Unknown
		if (fi.FieldIsTimestamp || fi.ExternalAccess == ExternalAccess.Read || ((fi.ExternalAccess == ExternalAccess.Insert) & isUpdate) || (fi.ExternalAccess == ExternalAccess.Modify && !isUpdate))
		{
			return;
		}
		if (navValue == null)
		{
			navSession.Diagnostics.SendTraceTag("00008SY", Category.Crm, Verbosity.Warning, "Crm AssignEntityAttribute with null value: entity={0}, field={1}", DiagnosticParameter.SystemMetadata(entity.LogicalName), DiagnosticParameter.SystemMetadata(fi.FieldName));
		}
		else
		{
			if (navValue.IsNull && !isUpdate)
			{
				return;
			}
			string externalType = fi.ExternalType;
			if (externalType == null)
			{
				return;
			}
			object obj;
			switch (externalType)
			{
			default:
				return;
			case "State":
			case "Status":
			case "Picklist":
			{
				int num = navValue.ToInt32();
				obj = (object)((num == 0 && !(fi.FieldOptionMetadata.GetCaptionFromIndex(0) != " ")) ? ((XrmOptionSetValue)null) : new XrmOptionSetValue(fi.MapNavOptionValueToExternalOptionValue(num)));
				break;
			}
			case "Money":
				obj = (object)new XrmMoney((decimal)navValue.ToDecimal());
				break;
			case "String":
			case "Integer":
			case "Decimal":
			case "Double":
			case "Boolean":
			case "DateTime":
				obj = (navValue.IsNull ? null : NavValueToCrmValue(fi, navValue));
				break;
			case "Lookup":
			{
				Guid guid2 = navValue.ToGuid();
				obj = null;
				if (!(guid2 != Guid.Empty))
				{
					break;
				}
				if (fi.FieldRelations.Count == 1)
				{
					obj = (object)new XrmEntityReference(fi.FieldRelations[0].SourceTable.ExternalName, guid2);
					break;
				}
				NCLMetaField nCLMetaField = GetAttributeFields(fi).Single((NCLMetaField f) => f.ExternalType == "EntityName");
				obj = (object)new XrmEntityReference(recordBuffer[nCLMetaField.FieldIndex].ToString(), guid2);
				break;
			}
			case "Customer":
			case "Owner":
				if (recordBuffer == null)
				{
					throw new NavNotSupportedException();
				}
				obj = null;
				if (navValue.ToGuid() != Guid.Empty)
				{
					NCLMetaField nCLMetaField2 = GetAttributeFields(fi).Single((NCLMetaField f) => f.ExternalType == "EntityName");
					obj = (object)new XrmEntityReference(nCLMetaField2.FieldOptionMetadata.GetOptionFromIndex(recordBuffer[nCLMetaField2.FieldIndex].ToInt32()), navValue.ToGuid());
				}
				break;
			case "Uniqueidentifier":
			{
				Guid guid = navValue.ToGuid();
				if (fi.FieldIsPartOfPrimaryKey && guid == Guid.Empty)
				{
					return;
				}
				obj = guid;
				break;
			}
			case "Memo":
			{
				NavBLOB navBLOB = navValue as NavBLOB;
				obj = null;
				if (navBLOB.IsZeroOrEmpty)
				{
					break;
				}
				BlobSubType blobSubType = fi.BlobSubType;
				if (blobSubType == BlobSubType.Memo)
				{
					byte[] bytes = navBLOB.GetBytes();
					if (bytes.Length != 0)
					{
						string text = Encoding.Unicode.GetString(bytes, 0, bytes.Length);
						if (text.Length > 0 && text[text.Length - 1] == '\0')
						{
							text = text.Substring(0, text.Length - 1);
						}
						obj = text;
					}
					navBLOB.IsDirty = false;
					break;
				}
				throw new NavCrmException(string.Format(CultureInfo.CurrentCulture, Lang.CrmUnsupportedBlobSubType, fi.BlobSubType));
			}
			}
			entity[fi.ExternalName] = obj;
		}
	}

	private Dictionary<int, NCLMetaField> BuildFieldIsAttributeOf()
	{
		Dictionary<int, NCLMetaField> dictionary = new Dictionary<int, NCLMetaField>();
		for (int i = 0; i < table.FieldCount; i++)
		{
			NCLMetaField fieldByIndex = table.GetFieldByIndex(i);
			switch (fieldByIndex.ExternalType)
			{
			case "Customer":
			case "Owner":
			{
				NCLMetaField customerOwnerDiscriminatorField2 = GetCustomerOwnerDiscriminatorField(fieldByIndex);
				if (customerOwnerDiscriminatorField2 != null)
				{
					dictionary.Add(customerOwnerDiscriminatorField2.FieldIndex, fieldByIndex);
				}
				break;
			}
			case "Lookup":
			{
				NCLMetaField customerOwnerDiscriminatorField = GetCustomerOwnerDiscriminatorField(fieldByIndex);
				if (customerOwnerDiscriminatorField != null)
				{
					dictionary.Add(customerOwnerDiscriminatorField.FieldIndex, fieldByIndex);
				}
				customerOwnerDiscriminatorField = GetNameField(fieldByIndex);
				if (customerOwnerDiscriminatorField != null)
				{
					dictionary.Add(customerOwnerDiscriminatorField.FieldIndex, fieldByIndex);
				}
				break;
			}
			}
		}
		return dictionary;
	}

	private NCLMetaField GetCustomerOwnerDiscriminatorField(NCLMetaField field)
	{
		if (field.FieldRelations.Count <= 1)
		{
			return null;
		}
		int fieldNo = 0;
		foreach (NCLMetaFieldRelation fieldRelation in field.FieldRelations)
		{
			fieldNo = fieldRelation.Conditions[0].SourceFieldId;
		}
		return table.GetFieldByNo(fieldNo);
	}

	private NCLMetaField GetNameField(NCLMetaField field)
	{
		if (field.FieldRelations.Count == 0)
		{
			return null;
		}
		return table.CalculatedFields.Select((int fno) => table.GetFieldByNo(fno)).SingleOrDefault((NCLMetaField f) => f.CalculationFormula != null && f.CalculationFormula.CalculationMethod == NCLMetaCalculationMethod.Lookup && ((NCLMetaFilterField)f.CalculationFormula.Filters[0]).ValueFieldId == field.FieldRelations[0].ReferencingFieldId && f.CalculationFormula.TableId == field.FieldRelations[0].SourceTableId);
	}

	private NCLMetaField GetAttributeOfField(NCLMetaField field)
	{
		if (!fieldIsAttributeOf.TryGetValue(field.FieldIndex, out var value))
		{
			return null;
		}
		return value;
	}

	private IEnumerable<NCLMetaField> GetAttributeFields(NCLMetaField field)
	{
		return from v in fieldIsAttributeOf
			where v.Value.FieldIndex == field.FieldIndex
			select table.GetFieldByIndex(v.Key);
	}

	private static string TrimText(string text, int length)
	{
		if (text != null && text.Length >= length)
		{
			return text.Substring(0, length);
		}
		return text;
	}

	private static XrmOptionSetValue GetOptionSetValue(NavValue value, NCLMetaField field)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected Obj, but got Unknown
		if (value.ToInt32() != 0 || field.FieldOptionMetadata.GetCaptionFromIndex(0) != " ")
		{
			return new XrmOptionSetValue(field.MapNavOptionValueToExternalOptionValue(value.ToInt32()));
		}
		return null;
	}

	private T ExecuteServiceCallWithTrace<T>(EventTask task, Func<IXrmConnection, T> function, XrmQueryExpression query = null, Func<Exception, bool> handleException = null, IRecordBuffer recordBuffer = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		string text = NavServerEventSource.TaskNames[task];
		using (new IfxTimingScope(CrmTaskDurationMetricInstance, "CrmExecuteServiceCallGeneric", text))
		{
			navSession.Diagnostics.SendTraceTag("00000PG", Category.Crm, Verbosity.Verbose, "Crm Service: task={0}, TableId={1}, TableName={2}, PageInfoCount={3}", DiagnosticParameter.SystemMetadata(text), DiagnosticParameter.SystemMetadata(MetaTable.TableId), DiagnosticParameter.SystemMetadata(MetaTable.TableName), DiagnosticParameter.SystemMetadata((query != null) ? query.PageInfo.Count : 0));
			try
			{
				return ExecuteServiceCall(function, handleException, recordBuffer);
			}
			catch (Exception ex)
			{
				Verbosity verbosity = ((ex is NavCrmException && ex.InnerException is XrmFaultException) ? Verbosity.Warning : Verbosity.Error);
				string content = string.Format(CultureInfo.InvariantCulture, "Crm Service: task={0}, TableId={1}, TableName={2}, PageInfoCount={3}, QueryMetadata={4}", text, MetaTable.TableId, MetaTable.TableName, (query != null) ? query.PageInfo.Count : 0, BuildQueryTraceString((IXrmQueryExpression)(object)query));
				navSession.Diagnostics.SendExceptionTag("000079N", Category.Crm, ex, verbosity, DiagnosticParameter.SystemMetadata(content));
				throw;
			}
		}
	}

	private T ExecuteServiceCall<T>(Func<IXrmConnection, T> function, Func<Exception, bool> handleException = null, IRecordBuffer recordBuffer = null)
	{
		try
		{
			int num = 0;
			while (true)
			{
				try
				{
					lock (this)
					{
						return NavThread.RunExternalAction(() => function(GetService()));
					}
				}
				catch (ObjectDisposedException exception)
				{
					navSession.Diagnostics.SendExceptionTag("00000ND", Category.Crm, exception, Verbosity.Warning);
					connection.FaultOrganizationService();
					if (num++ == 3)
					{
						throw;
					}
				}
			}
		}
		catch (Exception ex)
		{
			connection.FaultOrganizationService();
			if (handleException != null && handleException(ex))
			{
				navSession.Diagnostics.SendExceptionTag("00000NE", Category.Crm, ex, Verbosity.Warning);
				return default;
			}
			MapException(ex, recordBuffer);
			throw;
		}
	}

	private void ExecuteServiceCallWithTrace(EventTask task, Action<IXrmConnection> action, Func<Exception, bool> handleException = null, IRecordBuffer recordBuffer = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		string text = NavServerEventSource.TaskNames[task];
		using (new IfxTimingScope(CrmTaskDurationMetricInstance, "CrmExecuteServiceCall", text))
		{
			navSession.Diagnostics.SendTraceTag("00000PI", Category.Crm, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Crm Service: task={0}, TableId={1}, TableName={2}", text, MetaTable.TableId, MetaTable.TableName);
			ExecuteServiceCall(action, handleException, recordBuffer);
		}
	}

	private void ExecuteServiceCall(Action<IXrmConnection> action, Func<Exception, bool> handleException = null, IRecordBuffer recordBuffer = null)
	{
		try
		{
			action(GetService());
		}
		catch (Exception ex)
		{
			connection.FaultOrganizationService();
			if (handleException != null && handleException(ex))
			{
				return;
			}
			MapException(ex, recordBuffer);
			throw;
		}
	}

	private void MapException(Exception e, IRecordBuffer recordBuffer)
	{
		if (e is NavBaseException)
		{
			return;
		}
		if (e is MessageSecurityException { InnerException: not null } ex && ex.InnerException.HResult == -2146233087)
		{
			throw new NavCrmException(string.Format(CultureInfo.CurrentCulture, Lang.CrmConnectionFailure, ex.InnerException.Message), ex.InnerException);
		}
		XrmFaultException ex2 = (XrmFaultException)(object)((e is XrmFaultException) ? e : null);
		if (ex2 != null)
		{
			int errorCode = ex2.ErrorCode;
			if (errorCode == -2147187704 && recordBuffer != null && stateField != null && statusField != null)
			{
				throw new NavCrmException(string.Format(CultureInfo.CurrentCulture, Lang.CrmInvalidStatusForState, recordBuffer[statusField.FieldIndex], statusField.FieldCaption, recordBuffer[stateField.FieldIndex], stateField.FieldCaption), (Exception)(object)ex2);
			}
		}
		throw NavCrmException.CreateForRequestFailure(e.Message, e);
	}

	[Conditional("DEBUG")]
	private void EmitQueryDebugTrace(IXrmQueryExpression query)
	{
		string text = BuildQueryTraceString(query);
		navSession.Diagnostics.SendTraceTag("00001U2", Category.Crm, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Service query: {0}", text);
	}

	private static string BuildQueryTraceString(IXrmQueryExpression query)
	{
		if (query == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "EntityName: {0}", query.EntityName ?? string.Empty);
		stringBuilder.Append(" Order: ");
		stringBuilder.AppendJoin(",", query.Orders.Select((IXrmOrderExpression o) =>
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return string.Format(CultureInfo.InvariantCulture, "{0}({1})", o.OrderType, o.AttributeName);
		}));
		stringBuilder.Append(" Filter: ");
		stringBuilder.Append(FilterExpressionToString(query.Criteria));
		return stringBuilder.ToString();
	}

	private static string FilterExpressionToString(IXrmFilterExpression filter)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		if (filter.Conditions != null)
		{
			stringBuilder.AppendJoin(string.Format(CultureInfo.InvariantCulture, " {0} ", filter.FilterOperator), filter.Conditions.Select((IXrmConditionExpression c) =>
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				return string.Format(CultureInfo.InvariantCulture, "({0} {1} ?)", c.AttributeName, c.Operator);
			}));
		}
		if (filter.Filters != null)
		{
			stringBuilder.AppendJoin(string.Format(CultureInfo.InvariantCulture, " {0} ", filter.FilterOperator), filter.Filters.Select((IXrmFilterExpression f) => string.Format(CultureInfo.InvariantCulture, "({0})", FilterExpressionToString(f))));
		}
		return stringBuilder.ToString();
	}
}
