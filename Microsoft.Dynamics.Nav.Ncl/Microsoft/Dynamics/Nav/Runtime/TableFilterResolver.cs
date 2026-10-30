using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class TableFilterResolver
{
	private readonly List<Tuple<INavFieldMetadata, FilterExpression>> filters = new List<Tuple<INavFieldMetadata, FilterExpression>>();

	private readonly NCLMetaTable table;

	private readonly string input;

	protected abstract string Description { get; }

	protected string TableName => table.TableName;

	protected int TableId => table.TableId;

	public IReadOnlyList<Tuple<INavFieldMetadata, FilterExpression>> Filters => filters;

	protected TableFilterResolver(NCLMetaTable table, string input)
	{
		this.table = table;
		this.input = input;
	}

	public NavCSideException GetInvalidExpressionException()
	{
		return new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidTableViewOrPositionExpression, Description, input));
	}

	public virtual void AddFilter(string fieldName, NCLMetaFilterType filterType, string expressionString, bool isCultureInvariant)
	{
		ValidateFilterType(filterType);
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		NCLMetaField nCLMetaField;
		if ((nCLMetaField = ResolveField(fieldName, trapError: true)) == null)
		{
			mostSpecificInstance.SendTraceTag("000010F", Category.FilterParsing, Verbosity.Verbose, "Filter field {0} for table {1} (id={2}) does not exist in expression {3}.", DiagnosticParameter.SystemMetadata(fieldName), DiagnosticParameter.SystemMetadata(TableName), DiagnosticParameter.SystemMetadata(TableId), DiagnosticParameter.CustomerContent(expressionString));
			return;
		}
		FilterExpression item;
		try
		{
			item = filterType switch
			{
				NCLMetaFilterType.Const => FilterExpression.Equal(nCLMetaField.ExpressionContext.Parse(expressionString, isCultureInvariant), nCLMetaField.ExpressionContext), 
				NCLMetaFilterType.Expression => (!isCultureInvariant) ? FilterExpressionParser.Parse(expressionString, nCLMetaField.ExpressionContext) : FilterExpressionParser.ParseInvariant(expressionString, nCLMetaField.ExpressionContext), 
				NCLMetaFilterType.Field => FilterExpression.FieldEqualsField(ResolveReferencedField(expressionString), nCLMetaField.ExpressionContext), 
				_ => throw new NotSupportedException(), 
			};
		}
		catch (Exception exception) when (mostSpecificInstance.SendExceptionTagExceptionFilter("00000XA", Category.FilterParsing, exception))
		{
			throw;
		}
		filters.Add(Tuple.Create((INavFieldMetadata)nCLMetaField, item));
	}

	protected abstract void ValidateFilterType(NCLMetaFilterType filterType);

	protected NCLMetaField ResolveField(string field, bool trapError)
	{
		return table.FindFieldMatch(field, prefixFallback: true, trapError);
	}

	protected virtual INavFieldMetadata ResolveReferencedField(string fieldName)
	{
		throw new NotSupportedException();
	}

	protected void AddResolvedFilter(INavFieldMetadata resolvedField, FilterExpression resolvedExpression)
	{
		filters.Add(Tuple.Create(resolvedField, resolvedExpression));
	}
}
