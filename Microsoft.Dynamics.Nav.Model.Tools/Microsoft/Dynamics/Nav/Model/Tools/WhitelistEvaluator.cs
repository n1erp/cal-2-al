using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Tools.Language;
using Microsoft.Dynamics.Nav.Model.Tools.SchemaGatingTools;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class WhitelistEvaluator : IDisposable
{
	private bool disposed;

	private static WhitelistEvaluator instance;

	private readonly DataTable calculator;

	private readonly DataRow calculatorRow;

	private AppObjectChangesFilter filter;

	private static readonly Dictionary<string, string> SymbolsToPurge = new Dictionary<string, string>
	{
		{ "%", "PCT" },
		{ "*", "AST" }
	};

	public static WhitelistEvaluator Instance => instance ?? (instance = new WhitelistEvaluator());

	private WhitelistEvaluator()
	{
		calculator = new DataTable();
		calculator.Locale = CultureInfo.InvariantCulture;
		calculator.Columns.Add(string.Empty, typeof(bool));
		calculatorRow = calculator.NewRow();
		calculator.Rows.Add(calculatorRow);
	}

	public void SetChangesFilter(AppObjectChangesFilter inFilter)
	{
		WhitelistValidator.CheckNotNull(inFilter, "inFilter");
		filter = inFilter;
	}

	public bool Evaluate(ChangeTypes changeType, string expression, IElement element)
	{
		WhitelistValidator.CheckNotNull(expression, "expression");
		WhitelistValidator.CheckNotNull(element, "element");
		switch (changeType)
		{
		case ChangeTypes.Deletion:
			return EvaluateForAppObject(filter.BeforeRoot, expression, element);
		case ChangeTypes.Insertion:
			return EvaluateForAppObject(filter.AfterRoot, expression, element);
		case ChangeTypes.PropertyDeletion:
		case ChangeTypes.PropertyInsertion:
		case ChangeTypes.PropertyModification:
			return EvaluateForPropertyChange(filter.BeforeRoot, filter.AfterRoot, expression, element);
		default:
			return false;
		}
	}

	internal static string GetProperty(IElement element, string typeStr)
	{
		PropertyType result = PropertyType.Unknown;
		if (typeStr.Equals(EvaluatorConstants.DataLengthName))
		{
			result = PropertyType.DataType;
		}
		else if (!Enum.TryParse<PropertyType>(typeStr, out result))
		{
			return null;
		}
		if (!element.TryGetStringProperty(result, out var value) || value == null)
		{
			string defaultPropertyValue = DefaultPropertyValues.GetDefaultPropertyValue(result);
			return string.Format(CultureInfo.InvariantCulture, "'{0}'", defaultPropertyValue);
		}
		if (typeStr.Equals(EvaluatorConstants.DataLengthName))
		{
			foreach (string item in PropertyChangesFilter.DataTypesWithLength)
			{
				if (value.StartsWith(item, StringComparison.Ordinal))
				{
					return value.Replace(item, string.Empty);
				}
			}
		}
		return string.Format(CultureInfo.InvariantCulture, "'{0}'", value);
	}

	private bool EvaluateForPropertyChange(IElement before, IElement after, string expression, IElement target)
	{
		string elementType = AppObjectChangesFilter.GetElementType(target);
		expression = expression.Replace(" ", string.Empty);
		IElement element;
		IElement element2;
		if (filter.IsIDChange(ChangeTypes.Deletion, target))
		{
			(element, element2) = filter.GetBeforeAndAfter(ChangeTypes.Deletion, target);
		}
		else
		{
			element = null;
			element2 = null;
		}
		foreach (string objectVersion in EvaluatorConstants.objectVersions)
		{
			IElement element3;
			IElement target2;
			switch (objectVersion)
			{
			case "Before":
				element3 = before;
				target2 = element ?? Find(element3, target);
				break;
			case "After":
				element3 = after;
				target2 = element2 ?? Find(element3, target);
				break;
			default:
				throw new InvalidOperationException("objectVersion");
			}
			expression = RemoveObjectVersionParameter(expression, objectVersion);
			if (EvaluatorConstants.subElementsWithCheckedProperties.Contains(elementType))
			{
				expression = EvaluateProperties(expression, target2);
			}
			expression = EvaluateProperties(expression, element3);
		}
		return EvaluateInternal(expression);
	}

	private static string RemoveObjectVersionParameter(string expression, string objectVersion)
	{
		string value = string.Format(CultureInfo.InvariantCulture, "{0})", objectVersion);
		string text = string.Empty;
		int length = objectVersion.Length;
		for (int num = expression.IndexOf(value, StringComparison.Ordinal); num > -1; num = expression.IndexOf(value, StringComparison.Ordinal))
		{
			string text2 = expression.Substring(0, num);
			text += text2.TrimEnd(' ').TrimEnd(',');
			expression = expression.Substring(num + length);
		}
		return text + expression;
	}

	private static IElement Find(IElement root, IElement target)
	{
		return root.GetElements().FirstOrDefault((IElement x) => x.ElementTypeInfo.Equals(target.ElementTypeInfo) && ((target.Id == 0) ? (x.Name?.Equals(target.Name) ?? (target.Name == null)) : x.Id.Equals(target.Id)));
	}

	private bool EvaluateForAppObject(IElement root, string expression, IElement target)
	{
		string elementType = AppObjectChangesFilter.GetElementType(target);
		if (EvaluatorConstants.subElementsWithCheckedProperties.Contains(elementType))
		{
			expression = EvaluateProperties(expression, target);
		}
		expression = EvaluateProperties(expression, root);
		return EvaluateInternal(expression);
	}

	private static string EvaluateProperties(string expression, IElement target)
	{
		string text = string.Empty;
		string elementType = AppObjectChangesFilter.GetElementType(target);
		string arg = string.Format(CultureInfo.InvariantCulture, "{0}Property", elementType);
		int num = 0;
		string pattern = string.Format(CultureInfo.InvariantCulture, "{0}\\(([^)]*)\\)", arg);
		foreach (Match item2 in Regex.Matches(expression, pattern))
		{
			string text2 = item2.Groups[1].Value.Trim();
			if (text2.Contains(","))
			{
				string[] array = text2.Split(EvaluatorConstants.ParamSplitString, StringSplitOptions.RemoveEmptyEntries);
				string item = array[array.Length - 1].Trim();
				if (EvaluatorConstants.objectVersions.Contains(item))
				{
					continue;
				}
			}
			string text3;
			if (elementType.Equals("Key") && text2.Equals("IsPrimary"))
			{
				text3 = string.Format(CultureInfo.InvariantCulture, "{0}", EvaluatorArtificialProperties.IsPrimary(target));
			}
			else if (elementType.Equals("Key") && text2.Equals("Name"))
			{
				text3 = string.Format(CultureInfo.InvariantCulture, "'{0}'", target.Name);
				text3 = PurgeKeyName(text3);
			}
			else if (elementType.Equals("Field") && text2.Equals("FirstKeyIndex"))
			{
				text3 = string.Format(CultureInfo.InvariantCulture, "{0}", EvaluatorArtificialProperties.FirstKeyIndex(target));
			}
			else if (elementType.Equals("Field") && text2.Equals("IsPartOfActiveKey"))
			{
				text3 = string.Format(CultureInfo.InvariantCulture, "{0}", EvaluatorArtificialProperties.IsPartOfActiveKey(target));
			}
			else
			{
				text3 = GetProperty(target, text2);
				text3 = text3 ?? string.Format(CultureInfo.InvariantCulture, NonLocalizable.CouldNotGetPropertyValue, text2, target);
			}
			int length = item2.Index - num;
			text = text + expression.Substring(num, length) + text3;
			num = item2.Index + item2.Length;
		}
		text += expression.Substring(num);
		return text.Trim();
	}

	private static string PurgeKeyName(string keyName)
	{
		foreach (KeyValuePair<string, string> item in SymbolsToPurge)
		{
			keyName = keyName.Replace(item.Key, item.Value);
		}
		return keyName;
	}

	private bool EvaluateInternal(string expression)
	{
		try
		{
			calculator.Columns[0].Expression = expression;
			return (bool)calculatorRow[0];
		}
		catch (Exception ex)
		{
			Console.WriteLine(string.Format(CultureInfo.InvariantCulture, NonLocalizable.ExceptionOnEvaluation, expression, ex.Message));
			return false;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				calculator.Dispose();
			}
			disposed = true;
		}
	}
}
