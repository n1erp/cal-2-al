using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class RecordImplementationHelper
{
	internal const string VERSION = "VERSION(1)";

	private static readonly int SqlMinYear = SqlDateTime.MinValue.Value.Year;

	internal static string FormatKeyFieldsAndValues(NCLMetaKey metaKey, IEnumerable<NavValue> keyValues)
	{
		IEnumerable<Tuple<NCLMetaField, NavValue>> fieldValues = metaKey.KeyFieldsList.Zip(keyValues, Tuple.Create);
		return FormatKeyFieldsAndValues(fieldValues);
	}

	internal static string FormatKeyFields(NCLMetaKey metaKey, IEnumerable<NavValue> keyValues)
	{
		IEnumerable<Tuple<NCLMetaField, NavValue>> fieldValues = metaKey.KeyFieldsList.Zip(keyValues, Tuple.Create);
		return FormatKeyFields(fieldValues);
	}

	internal static string FormatPrimaryKeyFieldsAndValues(IRecordBuffer recordBuffer)
	{
		NCLMetaTable nCLMetaTable = (NCLMetaTable)recordBuffer.MetaApplicationObject;
		return FormatKeyFieldsAndValues(nCLMetaTable.PrimaryKey.KeyFieldsList.Select((NCLMetaField f) => Tuple.Create(f, recordBuffer[f.FieldIndex] ?? f.InitValue)));
	}

	internal static string FormatKeyFieldsAndValues(IEnumerable<Tuple<NCLMetaField, NavValue>> fieldValues)
	{
		StringBuilder keyFieldsAndValues = new StringBuilder();
		keyFieldsAndValues.Join(",", fieldValues, (Tuple<NCLMetaField, NavValue> t) =>
		{
			keyFieldsAndValues.AppendFieldName(t.Item1).Append("='").Append(t.Item2 ?? t.Item1.InitValue)
				.Append('\'');
		});
		return keyFieldsAndValues.ToString();
	}

	private static string FormatKeyFields(IEnumerable<Tuple<NCLMetaField, NavValue>> fieldValues)
	{
		StringBuilder keyFields = new StringBuilder();
		keyFields.Join(",", fieldValues, (Tuple<NCLMetaField, NavValue> t) =>
		{
			keyFields.AppendFieldName(t.Item1);
		});
		return keyFields.ToString();
	}

	internal static NavValue[] RecordIdToValueArray(DataError errorLevel, NCLMetaTable table, NavRecordId recordId)
	{
		int num = recordId.FieldCount;
		FieldList definedKeyFields = table.PrimaryKey.DefinedKeyFields;
		if (num > definedKeyFields.Count)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.IncompatibleRecordId_InvalidNumberOfFields, table.TableCaptions.GetValueOrDefault()));
			}
			num = definedKeyFields.Count;
		}
		NavValue[] array = new NavValue[num];
		for (int i = 0; i < num; i++)
		{
			INavFieldMetadata navFieldMetadata = table.PrimaryKey.DefinedKeyFields[i];
			array[i] = recordId.FieldValueByIndex(navFieldMetadata, i);
			CheckForValidStringLength(navFieldMetadata, array[i]);
		}
		return array;
	}

	internal static void ValidateSearchMethod(string searchMethod)
	{
		if (searchMethod == null)
		{
			throw new ArgumentNullException("searchMethod");
		}
		switch (searchMethod)
		{
		case "+":
			return;
		case "-":
			return;
		case "=":
			return;
		case "=>":
			return;
		case "=><":
			return;
		case "=<":
			return;
		case "=<>":
			return;
		case ">":
			return;
		case ">=":
			return;
		case ">=<":
			return;
		case "><":
			return;
		case "><=":
			return;
		case "<":
			return;
		case "<=":
			return;
		case "<=>":
			return;
		case "<>":
			return;
		case "<>=":
			return;
		}
		throw new NavCSideException(18023424, string.Format(CultureInfo.CurrentCulture, Lang.InvalidSearchMethodError, searchMethod));
	}

	internal static NavCSideException GetRecordAlreadyExistsException(NCLMetaTable metaTable, NavValue[] primaryKeyValues)
	{
		string fieldsAndValues = FormatKeyFieldsAndValues(metaTable.PrimaryKey, primaryKeyValues);
		string content = FormatKeyFields(metaTable.PrimaryKey, primaryKeyValues);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000HZ", Category.Sql, Verbosity.Warning, "Duplicate key insertion in table {0}, Identification fields: {1}", DiagnosticParameter.SystemMetadata(metaTable.TableId), DiagnosticParameter.SystemMetadata(content));
		return NavCSideDuplicateKeyException.Create(metaTable.TableCaptions.GetValueOrDefault(), fieldsAndValues);
	}

	internal static NavCSideException GetUniqueConstraintException(NCLMetaTable metaTable, NCLMetaKey violatingKey, NavValue[] keyValues)
	{
		string fieldsAndValues = FormatKeyFieldsAndValues(violatingKey, keyValues);
		string content = FormatKeyFields(violatingKey, keyValues);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006NB", Category.Sql, Verbosity.Warning, "Unique constraint violation in table {0}, Identification fields: {1}", DiagnosticParameter.SystemMetadata(metaTable.TableId), DiagnosticParameter.SystemMetadata(content));
		return NavCSideDuplicateKeyException.CreateUniqueConstraint(metaTable.TableCaptions.GetValueOrDefault(), fieldsAndValues);
	}

	internal static NavCSideRecordNotFoundException GetRecordNotFoundException(NCLMetaTable metaTable, NavValue[] primaryKeyValues, string navTenantId = null)
	{
		string primaryKeyValues2 = FormatKeyFieldsAndValues(metaTable.PrimaryKey, primaryKeyValues);
		byte[] bytes = new NavRecordId(metaTable.ObjectId, primaryKeyValues).GetBytes();
		return NavCSideRecordNotFoundException.Create(85132272, metaTable.TableCaptions.GetValueOrDefault(), primaryKeyValues2, bytes, navTenantId);
	}

	internal static NavCSideRecordNotFoundException GetRecordNotFoundException(NCLMetaTable metaTable, IEnumerable<Tuple<NCLMetaField, NavValue>> fieldValues, string navTenantId = null)
	{
		string arg = FormatKeyFieldsAndValues(fieldValues);
		return new NavCSideRecordNotFoundException(85132272, string.Format(CultureInfo.CurrentCulture, Lang.RecordNotFoundError, metaTable.TableCaptions.GetValueOrDefault(), arg))
		{
			NavTenantId = navTenantId
		};
	}

	internal static Exception GetNoResultsInFindException(NCLMetaTable metaTable, FilterFieldDictionary filters)
	{
		if (filters.IsNullOrEmpty())
		{
			return new NavCSideDataException(18023438, string.Format(CultureInfo.CurrentCulture, Lang.TableEmpty, metaTable.TableCaptions.GetValueOrDefault()));
		}
		string filterDescription = GetFilterDescription(filters);
		throw NavCSideDataException.CreateForThisFilter(18023495, metaTable.TableCaptions.GetValueOrDefault(), filterDescription);
	}

	internal static Exception GetInvalidFilterException(NavRecord parentRecord, NCLMetaField metaField, string expression, Exception innerException)
	{
		throw new NavCSideException(18023434, string.Format(CultureInfo.CurrentCulture, Lang.InvalidFilterOnField, expression, parentRecord.ALFieldCaption(metaField.FieldNo), metaField.Parent.TableCaptions.GetValueOrDefault(), innerException.Message), innerException);
	}

	public static Exception GetKeyNotFoundException(NCLMetaTable metaTable, NCLMetaField[] fields)
	{
		StringBuilder sb = new StringBuilder();
		sb.Join(",", fields, (NCLMetaField f) =>
		{
			sb.AppendFieldName(f);
		});
		return new NavCSideException(85132271, string.Format(CultureInfo.CurrentCulture, Lang.KeyNotFoundError, metaTable.TableCaptions.GetValueOrDefault(), sb));
	}

	public static Exception GetCannotSortOnCertainFieldsException(NCLMetaTable metaTable, IEnumerable<NCLMetaField> fields, string resourceId)
	{
		StringBuilder sb = new StringBuilder();
		sb.Join(",", fields, (NCLMetaField f) =>
		{
			sb.AppendFieldName(f);
		});
		return new NavSortingException(string.Format(CultureInfo.InvariantCulture, resourceId, sb, metaTable.TableCaptions.GetValueOrDefault()));
	}

	public static Exception GetDuplicatePrimaryKeyException(NCLMetaTable metaTable, NavValue[] primaryKeyValues, bool remappable = true)
	{
		string fieldsAndValues = FormatKeyFieldsAndValues(metaTable.PrimaryKey, primaryKeyValues);
		string content = FormatKeyFields(metaTable.PrimaryKey, primaryKeyValues);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000I0", Category.Sql, Verbosity.Warning, "Duplicate key insertion in table {0} Identification fields: {1}", DiagnosticParameter.SystemMetadata(metaTable.TableId), DiagnosticParameter.SystemMetadata(content));
		return NavCSideDuplicateKeyException.Create(metaTable.TableCaptions.GetValueOrDefault(), fieldsAndValues, remappable);
	}

	public static Exception GetInvalidPositionStringException(string positionString, NCLMetaTable metaTable)
	{
		return new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidPositionString, positionString, metaTable));
	}

	private static string GetFilterDescription(FilterFieldDictionary currentFilterGroup)
	{
		StringBuilder sb = new StringBuilder();
		return sb.Join(", ", currentFilterGroup.Items, (Tuple<INavFieldMetadata, FilterExpression> t) =>
		{
			sb.Append(((NCLMetaField)t.Item1).FieldCaption).Append(": ").Append(t.Item2.ToString());
		}).ToString();
	}

	public static Exception GetNoResultsInFindFromPositionException(NCLMetaTable metaTable, string searchMethod, NavValue[] primaryKeyValues)
	{
		string arg = FormatKeyFieldsAndValues(metaTable.PrimaryKey, primaryKeyValues);
		throw new NavCSideException(85132272, string.Format(CultureInfo.CurrentCulture, Lang.PositionedFindEmpty, metaTable.TableCaptions.GetValueOrDefault(), (searchMethod == "=") ? string.Empty : searchMethod, arg));
	}

	public static Exception GetRecordStaleModifiedByAnotherUserException(NCLMetaTable metaTable, NavValue[] primaryKeyValues)
	{
		string content = FormatKeyFields(metaTable.PrimaryKey, primaryKeyValues);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001IB", Category.Sql, Verbosity.Warning, "Another user has modified the record for this {0} after it was retrieved from the database. Identification fields: {1}", DiagnosticParameter.SystemMetadata(metaTable.TableId), DiagnosticParameter.SystemMetadata(content));
		throw new NavCSideException(85132286, string.Format(CultureInfo.CurrentCulture, Lang.AnotherUserChangedTheDataError));
	}

	public static Exception GetRecordStaleModifiedByThisUserException(NCLMetaTable metaTable, NavValue[] primaryKeyValues)
	{
		string fieldsAndValues = FormatKeyFieldsAndValues(metaTable.PrimaryKey, primaryKeyValues);
		throw NavCSideException.CreateAndLogRecordChangedInDatabase(85132286, Lang.ThisUserChangedTheDataError, metaTable.TableCaptions.GetValueOrDefault(), fieldsAndValues, SessionContextHelper.GetALCallStack());
	}

	public static Exception GetRecordStaleUnknownReasonException(NCLMetaTable metaTable, NavValue[] primaryKeyValues)
	{
		string fieldsAndValues = FormatKeyFieldsAndValues(metaTable.PrimaryKey, primaryKeyValues);
		throw NavCSideException.CreateAndLogRecordChangedInDatabase(85132286, Lang.ThisOrAnotherUserChangedTheDataError, metaTable.TableCaptions.GetValueOrDefault(), fieldsAndValues, SessionContextHelper.GetALCallStack());
	}

	public static Exception GetFieldError(NavRecord parentRecord, NCLMetaField field, MutableRecordBuffer recordBuffer, string originalMessage, string originalDiagnosticsMessage)
	{
		string text = parentRecord.ALFieldCaption(field.FieldNo);
		string valueOrDefault = field.Parent.TableCaptions.GetValueOrDefault();
		string text2 = FormatPrimaryKeyFieldsAndValues(recordBuffer);
		string message;
		string text3;
		if (!string.IsNullOrEmpty(originalMessage))
		{
			message = string.Format(CultureInfo.CurrentCulture, Lang.FieldErrorWithAlMessage, text, originalMessage, valueOrDefault, text2);
			text3 = "FieldError on field " + valueOrDefault + "." + text;
		}
		else
		{
			NavValue navValue = recordBuffer[field.FieldIndex];
			switch (field.FieldNclType)
			{
			case NavNclType.NavOemText:
			case NavNclType.NavOemCode:
			case NavNclType.NavText:
			case NavNclType.NavCode:
			case NavNclType.NavDate:
			case NavNclType.NavTime:
			case NavNclType.NavDateTime:
			case NavNclType.NavDateFormula:
				if (navValue == null || navValue.IsZeroOrEmpty)
				{
					message = string.Format(CultureInfo.CurrentCulture, Lang.FieldErrorEmpty, text, valueOrDefault, text2);
					text3 = "FieldError for unspecified value of " + valueOrDefault + "." + text;
					break;
				}
				goto default;
			default:
				message = string.Format(CultureInfo.CurrentCulture, Lang.FieldErrorNonEmpty, text, navValue ?? field.InitValue, valueOrDefault, text2);
				text3 = "FieldError for incorrect value of " + valueOrDefault + "." + text;
				break;
			}
		}
		if (!string.IsNullOrEmpty(originalDiagnosticsMessage))
		{
			text3 = text3 + ", message: " + originalDiagnosticsMessage;
		}
		return new NavCSideException(23004237, message)
		{
			DiagnosticsMessage = text3
		};
	}

	internal static SortOrder Reverse(this SortOrder sortOrder)
	{
		if (sortOrder != SortOrder.Ascending)
		{
			return SortOrder.Ascending;
		}
		return SortOrder.Descending;
	}

	internal static string GetTableViewString(SortingFieldList sortingFields, FilterFieldDictionary currentFilterGroup, bool useCaptions)
	{
		StringBuilder sb = new StringBuilder();
		bool isAscendingOnAllFields = sortingFields.IsAscendingOnAllFields;
		bool isDescendingOnAllFields = sortingFields.IsDescendingOnAllFields;
		sb.AppendFormat(CultureInfo.InvariantCulture, "{0} ", "VERSION(1)");
		sb.Append("SORTING(").AppendSortingFields(sortingFields, useCaptions).Append(')');
		if (!isAscendingOnAllFields)
		{
			sb.Append(" ORDER(");
			string descendingDesc = (useCaptions ? "Descending" : "1");
			string ascendingDesc = (useCaptions ? "Ascending" : "0");
			if (isDescendingOnAllFields)
			{
				sb.Append(descendingDesc);
			}
			else
			{
				sb.Join(",", sortingFields, (INavSortingField sortingField) =>
				{
					sb.Append((sortingField.Order == SortOrder.Ascending) ? ascendingDesc : descendingDesc);
				});
			}
			sb.Append(')');
		}
		FilterExpressionStringOptions filterExpressionStringOptions = ((!useCaptions) ? FilterExpressionStringOptions.OptionsAsIntegers : FilterExpressionStringOptions.None);
		filterExpressionStringOptions |= FilterExpressionStringOptions.FormatCultureInvariant;
		if (currentFilterGroup != null && currentFilterGroup.Items.Length != 0)
		{
			sb.Append(" WHERE(").AppendFieldFiterStrings(NCLMetaFilterType.Expression, currentFilterGroup, filterExpressionStringOptions).Append(')');
		}
		return sb.ToString();
	}

	internal static string GetPositionString(FilterFieldDictionary primaryKeyFields, bool useCaptions)
	{
		FilterExpressionStringOptions filterExpressionStringOptions = (FilterExpressionStringOptions)(((!useCaptions) ? 1 : 2) | 0x10 | 8);
		return new StringBuilder().AppendFieldFiterStrings(NCLMetaFilterType.Const, primaryKeyFields, filterExpressionStringOptions).ToString();
	}

	internal static string GetSortingFieldsString(SortingFieldList key)
	{
		return new StringBuilder().AppendSortingFields(key, useCaptions: true).ToString();
	}

	private static StringBuilder AppendFieldFiterStrings(this StringBuilder sb, NCLMetaFilterType filterType, FilterFieldDictionary expressions, FilterExpressionStringOptions filterExpressionStringOptions)
	{
		return sb.Join(",", expressions.Items, (Tuple<INavFieldMetadata, FilterExpression> f) =>
		{
			sb.AppendFieldFilter(f.Item1, f.Item2, filterType, filterExpressionStringOptions);
		});
	}

	private static StringBuilder AppendSortingFields(this StringBuilder sb, SortingFieldList sortingFields, bool useCaptions)
	{
		return sb.Join(",", sortingFields, (INavSortingField f) =>
		{
			sb.AppendFieldName((NCLMetaField)f.Field, useCaptions, ",");
		});
	}

	private static StringBuilder AppendFieldFilter(this StringBuilder sb, INavFieldMetadata field, FilterExpression filterExpression, NCLMetaFilterType filterType, FilterExpressionStringOptions filterExpressionStringOptions)
	{
		string value = filterExpression.ToString(filterExpressionStringOptions);
		bool flag = (filterExpressionStringOptions & FilterExpressionStringOptions.OptionsAsIntegers) != FilterExpressionStringOptions.OptionsAsIntegers;
		return sb.AppendFieldName((NCLMetaField)field, flag, "=").Append('=').Append(filterType.FilterTypeToString(flag))
			.Append('(')
			.Append(value)
			.Append(')');
	}

	private static string FilterTypeToString(this NCLMetaFilterType filterType, bool useNames)
	{
		if (!useNames)
		{
			int num = (int)filterType;
			return num.ToString(CultureInfo.InvariantCulture);
		}
		return filterType switch
		{
			NCLMetaFilterType.Const => "CONST", 
			NCLMetaFilterType.Expression => "FILTER", 
			NCLMetaFilterType.Field => "FIELD", 
			_ => throw new NotSupportedException(), 
		};
	}

	private static StringBuilder AppendFieldName(this StringBuilder sb, NCLMetaField field)
	{
		return sb.AppendFieldName(field, useCaption: true, null);
	}

	private static StringBuilder AppendFieldName(this StringBuilder sb, NCLMetaField field, bool useCaption, string escapeIfContains)
	{
		if (useCaption)
		{
			string fieldCaption = field.FieldCaption;
			if (escapeIfContains != null && fieldCaption.Contains(escapeIfContains))
			{
				string value = fieldCaption.Replace("\"", "\"\"");
				return sb.Append('"').Append(value).Append('"');
			}
			return sb.Append(fieldCaption);
		}
		return sb.Append("Field").Append(field.FieldNo);
	}

	internal static StringBuilder Join<T>(this StringBuilder stringBuilder, string separator, IEnumerable<T> items, Action<T> writeElement)
	{
		bool flag = true;
		foreach (T item in items)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.Append(separator);
			}
			writeElement(item);
		}
		return stringBuilder;
	}

	internal static string GetNonViewFilterString(FilterFieldDictionary currentFilterGroup, bool markedRecordsOnly)
	{
		StringBuilder sb = new StringBuilder();
		if (markedRecordsOnly)
		{
			sb.Append(Lang.Marked).Append(": ").Append(Lang.Yes);
		}
		if (currentFilterGroup.IsNullOrEmpty())
		{
			return sb.ToString();
		}
		if (markedRecordsOnly)
		{
			sb.Append(", ");
		}
		return sb.Join(", ", currentFilterGroup.Items, (Tuple<INavFieldMetadata, FilterExpression> t) =>
		{
			sb.AppendFieldName((NCLMetaField)t.Item1).Append(": ").Append(t.Item2);
		}).ToString();
	}

	internal static string GetUrlFilterString(FilterFieldDictionary currentFilterGroup)
	{
		if (currentFilterGroup.IsNullOrEmpty())
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		Tuple<INavFieldMetadata, FilterExpression>[] items = currentFilterGroup.Items;
		foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.Append(" AND ");
			}
			FilterExpressionStringOptions filterExpressionStringOptions = FilterExpressionStringOptions.OptionsAsIntegers | FilterExpressionStringOptions.TwoQuotesAsEmpty;
			stringBuilder.AppendFormat("'{0}' IS '{1}'", EscapeUrlChars(((NCLMetaField)tuple.Item1).FieldName), EscapeUrlChars(tuple.Item2.ToString(filterExpressionStringOptions)));
		}
		return stringBuilder.ToString();
	}

	private static string EscapeUrlChars(string value)
	{
		return value.Replace("'", "''");
	}

	internal static NavValue TransferFieldValue(NCLMetaField destField, NCLMetaField sourceField, NavValue sourceValue)
	{
		if (destField.IsStringType && sourceField.IsStringType)
		{
			string value = ((NavStringValue)sourceValue).Value;
			if (destField.FieldDefinedLength < value.Length)
			{
				throw new NavCSideException(18023443, string.Format(CultureInfo.CurrentCulture, Lang.CannotCopyTextFieldsBecauseOfLength, sourceField.FieldCaption, destField.FieldCaption, destField.FieldNavType, destField.FieldDefinedLength, sourceField.FieldNavType, sourceField.FieldDefinedLength, destField.Parent.TableCaptions.GetValueOrDefault(), sourceField.Parent.TableCaptions.GetValueOrDefault()));
			}
			if (destField.FieldNclType == sourceField.FieldNclType)
			{
				return sourceValue;
			}
			return NavValue.CreateNavValueFromObject(destField, value);
		}
		if (destField.FieldNclType == sourceField.FieldNclType)
		{
			return sourceValue;
		}
		throw new NavCSideException(18023443, string.Format(CultureInfo.CurrentCulture, Lang.FieldTypeMismatchError, destField.FieldCaption, sourceField.FieldCaption, destField.Parent.TableCaptions.GetValueOrDefault(), sourceField.Parent.TableCaptions.GetValueOrDefault(), destField.FieldNavType, sourceField.FieldNavType));
	}

	internal static void MapException(NavCSideException originalException, NavDatabase database, int companyToken, NCLMetaTable metaTable, IRecordBuffer recordBuffer, bool makeNewExceptionRemappable = true)
	{
		if (!originalException.Remappable)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000130", Category.Runtime, originalException, Verbosity.Warning);
			return;
		}
		switch (originalException.ErrorCode)
		{
		case 22926086:
			throw new NavCSideException(originalException.ErrorCode, string.Format(CultureInfo.CurrentCulture, Lang.BadBLOBDataError, metaTable.TableCaptions.GetValueOrDefault(), FormatPrimaryKeyFieldsAndValues(recordBuffer)), originalException, makeNewExceptionRemappable);
		case 22926090:
			throw new NavCSideException(originalException.ErrorCode, string.Format(CultureInfo.CurrentCulture, Lang.LockTimeOutError, metaTable.TableCaptions.GetValueOrDefault()), originalException, makeNewExceptionRemappable);
		case 22926089:
			throw new NavCSideException(originalException.ErrorCode, string.Format(CultureInfo.CurrentCulture, Lang.DeadLockError, metaTable.TableCaptions.GetValueOrDefault()), originalException, makeNewExceptionRemappable);
		case 18023796:
			throw NavCSideException.CreateAutoIncrementFieldError(originalException, metaTable.AutoIncrementField.FieldCaption, metaTable.TableCaptions.GetValueOrDefault(), FormatPrimaryKeyFieldsAndValues(recordBuffer), makeNewExceptionRemappable);
		case 22928070:
		case 22928072:
			throw NavCSideException.CreateForInvalidValueInField(originalException, metaTable.TableCaptions.GetValueOrDefault(), FormatPrimaryKeyFieldsAndValues(recordBuffer), makeNewExceptionRemappable);
		case 85132273:
			if (originalException is NavCSideDuplicateKeyException ex)
			{
				if (ex.UniqueIndexViolation)
				{
					throw new NavCSideException(Lang.DuplicateKeyErrorDuringModify);
				}
				if (!string.IsNullOrEmpty(ex.SqlExceptionMessage))
				{
					foreach (NCLMetaKey item in metaTable.Keys.Where((NCLMetaKey k) => k.Unique))
					{
						string value = NavSqlStatementHelper.AppendIndexName(new StringBuilder("'"), item, database, companyToken, NavAppTableContext.Main).Append("'").ToString();
						if (ex.SqlExceptionMessage.Contains(value))
						{
							throw GetUniqueConstraintException(metaTable, item, item.KeyFieldsList.Select((NCLMetaField kf) => recordBuffer[kf.ColumnIndex]).ToArray());
						}
					}
				}
				throw GetDuplicatePrimaryKeyException(metaTable, recordBuffer.GetPrimaryKeyValues(), remappable: false);
			}
			throw NavCSideDuplicateKeyException.CreateRemappable(metaTable.TableCaptions.GetValueOrDefault(), FormatPrimaryKeyFieldsAndValues(recordBuffer), originalException, makeNewExceptionRemappable);
		}
	}

	internal static bool CheckForValidStringLength(INavFieldMetadata metadata, NavValue primaryKeyValue, bool trapError = false)
	{
		if (primaryKeyValue.NclType != metadata.NclType && (primaryKeyValue.NclType != NavNclType.NavCode || metadata.NclType != NavNclType.NavText) && (primaryKeyValue.NclType != NavNclType.NavText || metadata.NclType != NavNclType.NavCode))
		{
			return true;
		}
		if (metadata.NclType != NavNclType.NavOemText && metadata.NclType != NavNclType.NavOemCode && metadata.NclType != NavNclType.NavCode && metadata.NclType != NavNclType.NavText)
		{
			return true;
		}
		NavStringValue navStringValue = primaryKeyValue as NavStringValue;
		if (navStringValue == null || metadata.NavDefinedLengthMetadata >= navStringValue.Value.Length)
		{
			return true;
		}
		if (navStringValue.Value.Contains('|'))
		{
			int num = 0;
			while (true)
			{
				int num2 = navStringValue.Value.IndexOf("|", num, StringComparison.OrdinalIgnoreCase);
				int num3 = ((num2 < 0) ? (navStringValue.Value.Length - num) : (num2 - num));
				if (num3 > metadata.NavDefinedLengthMetadata)
				{
					if (trapError)
					{
						return false;
					}
					string value = navStringValue.Value.Substring(num, num3);
					throw new NavNCLStringLengthExceededException(metadata.NavDefinedLengthMetadata, navStringValue.Value.Length, value);
				}
				if (num2 < 0)
				{
					break;
				}
				num = num2 + 1;
			}
			return true;
		}
		if (navStringValue.Value.Length > metadata.NavDefinedLengthMetadata)
		{
			if (trapError)
			{
				return false;
			}
			throw new NavNCLStringLengthExceededException(metadata.NavDefinedLengthMetadata, navStringValue.Value.Length, navStringValue);
		}
		return true;
	}

	internal static bool NavValuesEqual(NavValue leftNavValue, NavValue rightNavValue, bool enforceSqlEquality)
	{
		if (leftNavValue == null || rightNavValue == null)
		{
			return leftNavValue == rightNavValue;
		}
		if (!enforceSqlEquality)
		{
			return leftNavValue.Equals(rightNavValue);
		}
		if (!(leftNavValue is NavDateTimeValue navDateTimeValue))
		{
			return leftNavValue.Equals(rightNavValue);
		}
		NavDateTimeValue navDateTimeValue2 = rightNavValue as NavDateTimeValue;
		DateTime value = navDateTimeValue.Value;
		DateTime value2 = navDateTimeValue2.Value;
		SqlDateTime sqlDateTime = new SqlDateTime(Math.Max(value.Year, SqlMinYear), value.Month, value.Day, value.Hour, value.Minute, value.Second, (double)value.Millisecond);
		SqlDateTime value3 = new SqlDateTime(Math.Max(value2.Year, SqlMinYear), value2.Month, value2.Day, value2.Hour, value2.Minute, value2.Second, (double)value2.Millisecond);
		return sqlDateTime.CompareTo(value3) == 0;
	}
}
