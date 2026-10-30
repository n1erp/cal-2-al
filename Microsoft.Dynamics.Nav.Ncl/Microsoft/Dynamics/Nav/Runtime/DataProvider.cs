using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class DataProvider
{
	public virtual bool SupportsAutoCalcFields => false;

	public virtual bool IsDataModifiedInSqlServer => false;

	public virtual bool ShouldResultSetBufferRows => true;

	public virtual bool SupportReversedLoading => true;

	public abstract bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer);

	public abstract IEnumerable<ReadOnlyRecordBuffer> Find(FindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null);

	public abstract IEnumerable<ReadOnlyRecordBuffer> FindFromPosition(PositionedFindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null);

	public abstract bool Exists(ExistsProviderRequest request);

	public abstract int Count(DataProviderRequest request);

	public virtual FieldDictionary<NavValue> CalcNumeric(CalcNumericProviderRequest request)
	{
		ValidateNotNull(request, "request");
		NCLMetaTable nCLMetaTable = ValidateIsTable(request.MetaApplicationObject);
		int count = request.FieldsToCalculate.Count;
		Decimal18[] array = new Decimal18[count];
		int num = 0;
		foreach (ReadOnlyRecordBuffer item in Find(new FindProviderRequest(request, FieldList.Empty, nCLMetaTable.PrimaryKey.SortingFieldsWithPrimaryKeyFields, FindType.Normal)))
		{
			num++;
			for (int i = 0; i < request.FieldsToCalculate.Count; i++)
			{
				NCLMetaField nCLMetaField = (NCLMetaField)request.FieldsToCalculate[i];
				if (nCLMetaField.CalculationFormula.CalculationMethod == NCLMetaCalculationMethod.Average || nCLMetaField.CalculationFormula.CalculationMethod == NCLMetaCalculationMethod.Sum)
				{
					NCLMetaField sourceField = nCLMetaField.CalculationFormula.SourceField;
					if (sourceField.FieldClass != FieldClass.Normal)
					{
						throw new NavNCLFlowFieldValidationException(string.Format(CultureInfo.InvariantCulture, "The FieldClass for the {0} field in the {1} table must be Normal.", sourceField.FieldCaption, sourceField.Parent.TableCaptionSafe));
					}
					if (item[sourceField.ColumnIndex] != null)
					{
						array[i] += item[sourceField.ColumnIndex].ToDecimal();
					}
				}
			}
		}
		Tuple<INavFieldMetadata, NavValue>[] array2 = new Tuple<INavFieldMetadata, NavValue>[count];
		for (int j = 0; j < count; j++)
		{
			NCLMetaField nCLMetaField2 = (NCLMetaField)request.FieldsToCalculate[j];
			NavValue navValue = null;
			try
			{
				switch (nCLMetaField2.CalculationFormula.CalculationMethod)
				{
				case NCLMetaCalculationMethod.Count:
					navValue = NavValue.CreateNavValueFromObject(nCLMetaField2, num);
					break;
				case NCLMetaCalculationMethod.Sum:
					navValue = NavValue.CreateNavValueFromObject(nCLMetaField2, array[j]);
					break;
				case NCLMetaCalculationMethod.Average:
				{
					Decimal18 @decimal = ((num == 0) ? ((Decimal18)0) : (array[j] / num));
					if (nCLMetaField2.CalculationFormula.SourceField.FieldNclType != NavNclType.NavDecimal)
					{
						@decimal = Decimal18.Floor(@decimal);
					}
					navValue = NavValue.CreateNavValueFromObject(nCLMetaField2, @decimal);
					break;
				}
				}
				if (nCLMetaField2.CalculationFormula.NegateResult)
				{
					navValue = FlowFieldsHelper.NegateValue(navValue, nCLMetaField2.CalculationFormula.SourceField.FieldNavType);
				}
			}
			catch (OverflowException)
			{
				throw new NavNCLCalculationOverflowException(string.Format(CultureInfo.CurrentCulture, Lang.FieldCalculationOverflow, nCLMetaField2.FieldName));
			}
			array2[j] = new Tuple<INavFieldMetadata, NavValue>(request.FieldsToCalculate[j], navValue);
		}
		return new FieldDictionary<NavValue>(array2);
	}

	public virtual FieldDictionary<NavValue> CalcMinMax(CalcMinMaxProviderRequest request)
	{
		ValidateNotNull(request, "request");
		NCLMetaTable nCLMetaTable = ValidateIsTable(request.MetaApplicationObject);
		int count = request.FieldsToCalculate.Count;
		NavValue[] array = new NavValue[count];
		foreach (ReadOnlyRecordBuffer item in Find(new FindProviderRequest(request, FieldList.Empty, nCLMetaTable.PrimaryKey.SortingFieldsWithPrimaryKeyFields, FindType.Normal)))
		{
			for (int i = 0; i < request.FieldsToCalculate.Count; i++)
			{
				NCLMetaField nCLMetaField = (NCLMetaField)request.FieldsToCalculate[i];
				NavValue navValue = item[nCLMetaField.CalculationFormula.SourceField.ColumnIndex];
				if (array[i] == null)
				{
					array[i] = navValue;
					continue;
				}
				int num = navValue.CompareTo(array[i]);
				if ((nCLMetaField.CalculationFormula.CalculationMethod == NCLMetaCalculationMethod.Min && num == -1) || (nCLMetaField.CalculationFormula.CalculationMethod == NCLMetaCalculationMethod.Max && num == 1))
				{
					array[i] = navValue;
				}
			}
		}
		Tuple<INavFieldMetadata, NavValue>[] array2 = new Tuple<INavFieldMetadata, NavValue>[count];
		for (int j = 0; j < count; j++)
		{
			INavFieldMetadata navFieldMetadata = request.FieldsToCalculate[j];
			array2[j] = new Tuple<INavFieldMetadata, NavValue>(navFieldMetadata, array[j] ?? navFieldMetadata.EmptyValue);
		}
		return new FieldDictionary<NavValue>(array2);
	}

	public abstract FieldDictionary<NavValue> GetBlobContent(GetBlobContentProviderRequest request);

	public virtual FieldDictionary<NavValue> CalcSums(CalcSumsProviderRequest request)
	{
		ValidateNotNull(request, "request");
		NCLMetaTable nCLMetaTable = ValidateIsTable(request.MetaApplicationObject);
		int count = request.FieldsToCalculate.Count;
		Decimal18[] array = new Decimal18[count];
		foreach (ReadOnlyRecordBuffer item in Find(new FindProviderRequest(request, FieldList.Empty, nCLMetaTable.PrimaryKey.SortingFieldsWithPrimaryKeyFields, FindType.Normal)))
		{
			for (int i = 0; i < request.FieldsToCalculate.Count; i++)
			{
				array[i] += item[request.FieldsToCalculate[i].ColumnIndex].ToDecimal();
			}
		}
		Tuple<INavFieldMetadata, NavValue>[] array2 = new Tuple<INavFieldMetadata, NavValue>[count];
		for (int j = 0; j < count; j++)
		{
			array2[j] = new Tuple<INavFieldMetadata, NavValue>(request.FieldsToCalculate[j], NavDecimal.Create(array[j]));
		}
		return new FieldDictionary<NavValue>(array2);
	}

	public abstract ModifyResult Modify(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out ReadOnlyRecordBuffer outputRecordBuffer);

	public abstract void ModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldDictionary<NavValue> newFieldValues);

	public abstract DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction);

	public abstract void DeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering);

	public abstract InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer);

	public virtual int GetQueryCount(NCLMetaQueryDefinition queryDefinition, int companyToken, int topNumberOfRows, int skipNumberOfRows, FiltersAndMarks queryFilters, TableFilterDictionary globalAndSecurityFilters, MutableRecordBuffer startingPosition, SecurityFiltering securityFiltering, bool addExplicitJoinHints = false)
	{
		throw new NotSupportedException();
	}

	public virtual IEnumerable<ReadOnlyRecordBuffer> GetQueryResult(NCLMetaQueryDefinition queryDefinition, int companyToken, int topNumberOfRows, int skipNumberOfRows, FiltersAndMarks queryFilters, TableFilterDictionary globalAndSecurityFilters, MutableRecordBuffer startingPosition, SecurityFiltering securityFiltering, bool addExplicitJoinHints = false, bool includeCurrent = false, SortingFieldList sortingFields = null)
	{
		throw new NotSupportedException();
	}

	public virtual void BeginTransaction(TransactionType transactionType)
	{
		throw new NotSupportedException();
	}

	public virtual void EndTransaction(bool commit)
	{
		throw new NotSupportedException();
	}

	public virtual void FlushBufferedWrites(NCLMetaApplicationObject applicationObject)
	{
	}

	public virtual void FlushBufferedWrites(MutableRecordBuffer recordBuffer)
	{
	}

	public virtual void FlushBufferedWrites()
	{
	}

	internal virtual IPermissionSet GetPermissionSet()
	{
		return null;
	}

	protected internal static void ValidateNotNull(object instance, string name)
	{
		if (instance == null)
		{
			throw new ArgumentNullException(name);
		}
	}

	protected internal static NCLMetaTable ValidateIsTable(NCLMetaApplicationObject metaApplicationObject)
	{
		if (!(metaApplicationObject is NCLMetaTable result))
		{
			throw new ArgumentOutOfRangeException("metaApplicationObject");
		}
		return result;
	}
}
