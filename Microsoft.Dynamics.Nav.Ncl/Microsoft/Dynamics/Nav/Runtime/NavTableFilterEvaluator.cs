using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTableFilterEvaluator : NavValueEvaluator<NavTableFilter>
{
	private static readonly NavTableFilterEvaluator instance = new NavTableFilterEvaluator();

	internal static NavTableFilterEvaluator Instance => instance;

	private NavTableFilterEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavTableFilter value, INavValueMetadata metadata, string source, int number)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			value = NavTableFilter.Default;
			return true;
		}
		int position = 0;
		bool flag = source[0] == '\'' && source[source.Length - 1] == '\'';
		if (!NavValueEvaluator.TryReadTokenUntilDelimiter(source, ref position, ':', delimiterRequired: true, out var result))
		{
			throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.TableFilter, source);
		}
		NavValueEvaluator.ConsumeWhitespace(source, ref position);
		int tableId = NavValueEvaluator.EvaluateTableIdentifierString(result, flag);
		NCLMetaTable metaTableById;
		try
		{
			metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(tableId, -1, requireCompiled: false);
		}
		catch (NavMetadataNotFoundException innerException)
		{
			throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.TableFilter, source, innerException);
		}
		List<Tuple<INavFieldMetadata, FilterExpression>> list = new List<Tuple<INavFieldMetadata, FilterExpression>>();
		while (position < source.Length)
		{
			if (!NavValueEvaluator.TryReadTokenUntilDelimiter(source, ref position, '=', delimiterRequired: true, out var result2))
			{
				value = null;
				return false;
			}
			NCLMetaField nCLMetaField;
			try
			{
				nCLMetaField = metaTableById.FindFieldMatch(result2, prefixFallback: true);
			}
			catch (NavNCLFieldNotFoundException innerException2)
			{
				throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.TableFilter, source, innerException2);
			}
			NavValueEvaluator.ConsumeWhitespace(source, ref position);
			if (!NavValueEvaluator.TryReadTokenUntilDelimiter(source, ref position, ',', delimiterRequired: false, out var result3))
			{
				value = null;
				return false;
			}
			int length = result3.Length;
			if (flag && position == source.Length && length >= 1 && result3[length - 1] == '\'')
			{
				result3 = result3.Substring(0, length - 1);
			}
			FilterExpression item;
			try
			{
				item = FilterExpressionParser.Parse(result3, nCLMetaField.ExpressionContext);
			}
			catch (NavInvalidFilterExpressionException innerException3)
			{
				throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.TableFilter, source, innerException3);
			}
			list.Add(new Tuple<INavFieldMetadata, FilterExpression>(nCLMetaField, item));
		}
		FilterFieldDictionary fieldFilters = ((list.Count == 0) ? FilterFieldDictionary.Empty : new FilterFieldDictionary(list));
		value = new NavTableFilter(metaTableById, fieldFilters);
		return true;
	}

	protected override NavTableFilter GetDefaultValue(INavValueMetadata metadata)
	{
		return NavTableFilter.Default;
	}
}
