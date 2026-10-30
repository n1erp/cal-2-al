using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.IO.Txt;
using Txt2AlConverter;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class ProcedureExporter : IElementExporter
{
	private readonly VariableSectionExporter variableExporter;

	private readonly CommentedVariableSectionExporter commentedVariableSectionExporter;

	private const string OnBeforeTestRunTriggerName = "OnBeforeTestRun";

	private const string OnAfterTestRunTriggerName = "OnAfterTestRun";

	private static readonly string[] OnAfterRunTriggerSignature = new string[5] { "Integer", "Text", "Text", "TestPermissions", "Boolean" };

	private static readonly string[] OnBeforeRunTriggerSignature = new string[4] { "Integer", "Text", "Text", "TestPermissions" };

	public ProcedureExporter()
	{
		variableExporter = new VariableSectionExporter();
		commentedVariableSectionExporter = new CommentedVariableSectionExporter();
	}

	public virtual void Export(IElement element, IAlWriter writer)
	{
		Export(element, writer, commentedOut: false);
	}

	protected void Export(IElement element, IAlWriter writer, bool commentedOut)
	{
		if (writer.PendingEmptyLine)
		{
			writer.WriteLine();
		}
		ExportFunctionHeader(writer, element, commentedOut);
		ExportFunctionBody(writer, element, commentedOut);
		writer.PendingEmptyLine = true;
	}

	public void ExportFunctionBody(IAlWriter writer, IElement function, bool commentedOut = false)
	{
		writer.PendingEmptyLine = false;
		ExportVariables(function, writer, commentedOut);
		writer.WriteLinePossiblyCommentedOut("begin", commentedOut);
		writer.PushIndent(writer.Indentation);
		string[] array = function.GetCodeLines().ToArray();
		if (commentedOut)
		{
			writer.WriteLine("/*");
			string[] array2 = array;
			foreach (string value in array2)
			{
				writer.WriteLine(value);
			}
			writer.WriteLine("*/");
		}
		else
		{
			ReplaceCommentsAndExportBody(array, writer);
		}
		writer.PopIndent();
		writer.WriteLinePossiblyCommentedOut("end;", commentedOut);
	}

	private static string[] GetAttributeParameters(string attrib)
	{
		int num = attrib.IndexOf("(", StringComparison.OrdinalIgnoreCase);
		if (num < 0)
		{
			return new string[0];
		}
		int num2 = attrib.LastIndexOf(")", StringComparison.OrdinalIgnoreCase);
		if (num2 < 0)
		{
			return new string[0];
		}
		int num3 = num2 - num - 1;
		if (num3 < 0)
		{
			return new string[0];
		}
		return attrib.Substring(num + 1, num3).Split(',');
	}

	private string ResolveParameterDefaultBooleanValue(string[] parameters, int i, string defaultValue)
	{
		string text = ((parameters.Length > i) ? parameters[i] : defaultValue);
		if (text.Equals("Default", StringComparison.InvariantCultureIgnoreCase))
		{
			text = defaultValue;
		}
		return text;
	}

	private string ResolveParameterDefaultSkipErrorValue(string[] parameters, int i)
	{
		return ((parameters.Length > i) ? parameters[i] : string.Empty).ToUpperInvariant() switch
		{
			"SKIP" => true.ToString().ToLowerInvariant(), 
			_ => false.ToString().ToLowerInvariant(), 
		};
	}

	private void ExportAttributes(IAlWriter writer, IElement function, bool commentedOut)
	{
		if (function.GetPropertySetState(PropertyType.Attributes))
		{
			string stringProperty = function.GetStringProperty(PropertyType.Attributes);
			if (!string.IsNullOrEmpty(stringProperty))
			{
				string[] array = stringProperty.Replace("][", "]|[").Split('|');
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					if (text.StartsWith("[LineStart", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					if (text.StartsWith("[Integration", StringComparison.OrdinalIgnoreCase))
					{
						string[] attributeParameters = GetAttributeParameters(text);
						string arg = ResolveParameterDefaultBooleanValue(attributeParameters, 0, false.ToString().ToLowerInvariant());
						string arg2 = ResolveParameterDefaultBooleanValue(attributeParameters, 1, false.ToString().ToLowerInvariant());
						text = string.Format(CultureInfo.InvariantCulture, "[IntegrationEvent({0}, {1})]", arg, arg2);
					}
					if (text.StartsWith("[Business", StringComparison.OrdinalIgnoreCase))
					{
						string[] attributeParameters2 = GetAttributeParameters(text);
						string arg3 = ResolveParameterDefaultBooleanValue(attributeParameters2, 0, false.ToString().ToLowerInvariant());
						text = string.Format(CultureInfo.InvariantCulture, "[BusinessEvent({0})]", arg3);
					}
					if (text.StartsWith("[EventSubscriber", StringComparison.OrdinalIgnoreCase))
					{
						string[] attributeParameters3 = GetAttributeParameters(text);
						string value = ((attributeParameters3.Length > 3) ? attributeParameters3[3] : string.Empty);
						string text2 = ResolveParameterDefaultSkipErrorValue(attributeParameters3, 4);
						string text3 = ResolveParameterDefaultSkipErrorValue(attributeParameters3, 5);
						text = string.Format(CultureInfo.InvariantCulture, "[EventSubscriber(ObjectType::{0}, {1}, '{2}', '{3}', {4}, {5})]", attributeParameters3[0], attributeParameters3[1], AlUtilities.RemoveIdentifierQuotes(attributeParameters3[2]), AlUtilities.RemoveIdentifierQuotes(value), text2, text3);
					}
					if (!text.StartsWith("[External", StringComparison.OrdinalIgnoreCase))
					{
						if (text.StartsWith("[Internal", StringComparison.OrdinalIgnoreCase))
						{
							text = AlUtilities.GetCompilationScopeAttribute(writer.State.TargetRuntimeVersion, text);
						}
						if (text.StartsWith("[HandlerFunctions", StringComparison.OrdinalIgnoreCase))
						{
							string[] attributeParameters4 = GetAttributeParameters(text);
							string arg4 = string.Join(",", attributeParameters4);
							text = string.Format(CultureInfo.InvariantCulture, "[HandlerFunctions('{0}')]", arg4);
						}
						if (text.StartsWith("[SendNotificationHandler", StringComparison.OrdinalIgnoreCase))
						{
							text = GetNotificationHandlerAttribute("SendNotificationHandler", text);
						}
						if (text.StartsWith("[RecallNotificationHandler", StringComparison.OrdinalIgnoreCase))
						{
							text = GetNotificationHandlerAttribute("RecallNotificationHandler", text);
						}
						if (text.StartsWith("[TestPermissions", StringComparison.OrdinalIgnoreCase))
						{
							string[] attributeParameters5 = GetAttributeParameters(text);
							CultureInfo invariantCulture = CultureInfo.InvariantCulture;
							object[] args = attributeParameters5;
							text = string.Format(invariantCulture, "[TestPermissions(TestPermissions::{0})]", args);
						}
						if (text.StartsWith("[TransactionModel", StringComparison.OrdinalIgnoreCase))
						{
							string[] attributeParameters6 = GetAttributeParameters(text);
							CultureInfo invariantCulture2 = CultureInfo.InvariantCulture;
							object[] args = attributeParameters6;
							text = string.Format(invariantCulture2, "[TransactionModel(TransactionModel::{0})]", args);
						}
						if (text.StartsWith("[NonDebuggableAttribute", StringComparison.OrdinalIgnoreCase))
						{
							text = "[NonDebuggable]";
						}
						writer.WriteLinePossiblyCommentedOut(text, commentedOut);
					}
				}
			}
		}
		SetDefaultScope(writer, function, commentedOut);
	}

	private static void SetDefaultScope(IAlWriter writer, IElement function, bool commentedOut)
	{
		if (ProcedureNeedsDefaultScopeAttribute(function))
		{
			writer.WriteLinePossiblyCommentedOut(AlUtilities.GetCompilationScopeAttribute(writer.State.TargetRuntimeVersion), commentedOut);
		}
	}

	public static bool ProcedureNeedsDefaultScopeAttribute(IElement function)
	{
		if (function.ElementTypeInfo.ElementType != ElementType.Procedure)
		{
			return false;
		}
		bool flag = false;
		if (function.GetPropertySetState(PropertyType.Attributes))
		{
			string stringProperty = function.GetStringProperty(PropertyType.Attributes);
			if (!string.IsNullOrEmpty(stringProperty))
			{
				flag = stringProperty.Contains("[Internal]") || stringProperty.Contains("[External]");
			}
		}
		if (flag)
		{
			return false;
		}
		if (function.GetContainingApplicationObjectElement().ElementTypeInfo.ElementType == ElementType.Modification)
		{
			return false;
		}
		return !function.GetPropertySetState(PropertyType.Local);
	}

	private static string GetNotificationHandlerAttribute(string alAttributeName, string attributeString)
	{
		string[] notificationHandlerArguments = GetNotificationHandlerArguments(attributeString);
		if (notificationHandlerArguments.Length != 0)
		{
			string arg = string.Join(",", notificationHandlerArguments);
			return string.Format(CultureInfo.InvariantCulture, "[{0}({1})]", alAttributeName, arg);
		}
		return string.Format(CultureInfo.InvariantCulture, "[{0}]", alAttributeName);
	}

	private static string[] GetNotificationHandlerArguments(string attrib)
	{
		string[] attributeParameters = GetAttributeParameters(attrib);
		if (attributeParameters != null && attributeParameters.Length != 0)
		{
			attributeParameters[0] = (string.Equals(attributeParameters[0], "Optional", StringComparison.OrdinalIgnoreCase) ? "true" : "false");
		}
		return attributeParameters;
	}

	internal void ExportFunctionHeader(IAlWriter writer, IElement function, bool commentedOut = false)
	{
		bool flag = IsTestRunnerTrigger(function);
		if (!flag)
		{
			ExportAttributes(writer, function, commentedOut);
		}
		if (commentedOut)
		{
			writer.Write("//");
		}
		if (function.IsTrue(PropertyType.Local))
		{
			writer.Write("local ");
		}
		if (function.ElementTypeInfo.ElementType == ElementType.Event)
		{
			writer.Write("trigger ");
			if (function.IsDotNetEventTrigger())
			{
				string stringProperty = function.GetStringProperty(PropertyType.EventSource);
				writer.Write(AlUtilities.QuoteIdentifierIfNeeded(stringProperty));
				writer.Write("::");
			}
			WriteIdentifierName(function, writer, prependNameWithSpace: false);
		}
		else if (flag)
		{
			writer.Write("trigger ");
			WriteIdentifierName(function, writer, prependNameWithSpace: false);
		}
		else
		{
			writer.Write("procedure ");
			WriteIdentifierName(function, writer, prependNameWithSpace: false);
		}
		writer.Write('(');
		bool flag2 = true;
		foreach (IElement subElement in function.GetSubElements(ElementType.Parameter))
		{
			if (!flag2)
			{
				writer.Write(';');
			}
			if (subElement.IsTrue(PropertyType.AsVar))
			{
				writer.Write("var ");
			}
			WriteParameterDeclaration(subElement, writer, flag);
			flag2 = false;
		}
		writer.Write(')');
		IElement element = function.GetSubElements(ElementType.ReturnValue).FirstOrDefault();
		if (element != null)
		{
			WriteReturnValueDeclaration(element, writer, prependNameWithSpace: true);
		}
		writer.WriteLine();
	}

	private bool IsTestRunnerTrigger(IElement function)
	{
		if (!function.GetPropertySetState(PropertyType.Name))
		{
			return false;
		}
		string stringProperty = function.GetStringProperty(PropertyType.Name);
		if (!SemanticFacts.IsSameName(stringProperty, "OnBeforeTestRun") && !SemanticFacts.IsSameName(stringProperty, "OnAfterTestRun"))
		{
			return false;
		}
		IElement containingApplicationObjectElement = function.GetContainingApplicationObjectElement();
		if (containingApplicationObjectElement.ElementTypeInfo.ElementType != ElementType.Codeunit)
		{
			return false;
		}
		if (!containingApplicationObjectElement.GetPropertySetState(PropertyType.CodeunitSubtype))
		{
			return false;
		}
		if (!SemanticFacts.IsSameName(containingApplicationObjectElement.GetStringProperty(PropertyType.CodeunitSubtype), "TestRunner"))
		{
			return false;
		}
		return HasTestRunnerTriggerSignature(function, stringProperty);
	}

	private bool HasTestRunnerTriggerSignature(IElement function, string functionName)
	{
		if (SemanticFacts.IsSameName(functionName, "OnBeforeTestRun"))
		{
			IElement element = function.GetSubElements(ElementType.ReturnValue).FirstOrDefault();
			if (element == null)
			{
				return false;
			}
			if (!SemanticFacts.IsSameName(element.GetStringProperty(PropertyType.DataType), "Boolean"))
			{
				return false;
			}
			return HasSameSignature(function, OnBeforeRunTriggerSignature);
		}
		if (SemanticFacts.IsSameName(functionName, "OnAfterTestRun"))
		{
			if (function.GetSubElements(ElementType.ReturnValue).FirstOrDefault() != null)
			{
				return false;
			}
			return HasSameSignature(function, OnAfterRunTriggerSignature);
		}
		return false;
	}

	private static bool HasSameSignature(IElement function, string[] paramTypes)
	{
		return (from element in function.GetSubElements(ElementType.Parameter)
			select element.GetStringProperty(PropertyType.DataType)).SequenceEqual(paramTypes);
	}

	private void WriteIdentifierName(IElement ident, IAlWriter writer, bool prependNameWithSpace)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AlUtilities.FormatIdentifierName(stringBuilder, ident, prependNameWithSpace);
		writer.Write(stringBuilder.ToString());
	}

	private void WriteReturnValueDeclaration(IElement ident, IAlWriter writer, bool prependNameWithSpace = false)
	{
		AlUtilities.WriteVariableDeclaration(ident, writer, prependNameWithSpace);
	}

	private void WriteParameterDeclaration(IElement element, IAlWriter writer, bool isRunTriggerParameter)
	{
		if (isRunTriggerParameter)
		{
			StringBuilder stringBuilder = new StringBuilder();
			AlUtilities.FormatIdentifierName(stringBuilder, element, prependNameWithSpace: false);
			writer.Write(stringBuilder.ToString());
			writer.Write(": ");
			writer.Write(element.GetStringProperty(PropertyType.DataType));
		}
		else
		{
			AlUtilities.WriteVariableDeclaration(element, writer, prependNameWithSpace: false, commentedOut: false, isParameter: true);
		}
	}

	private void ReplaceCommentsAndExportBody(IList<string> codeLines, IAlWriter writer)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string codeLine in codeLines)
		{
			stringBuilder.AppendLine(writer.Indent + codeLine);
		}
		string text = stringBuilder.ToString();
		IList<TokenInfo> list = (from t in AlUtilities.GetTokens(text, enablePartialScan: true)
			where t.IsMultiLineComment
			orderby t
			select t).ToList();
		if (list.Count == 0)
		{
			foreach (string codeLine2 in codeLines)
			{
				writer.WriteLine(codeLine2);
			}
			return;
		}
		StringBuilder stringBuilder2 = new StringBuilder(stringBuilder.Length + 2 * list.Count);
		int num = 0;
		foreach (TokenInfo item in list)
		{
			stringBuilder2.Append(text.Substring(num, item.Position - num));
			stringBuilder2.Append("/*");
			stringBuilder2.Append(text.Substring(item.Position + 1, item.Length - 2));
			stringBuilder2.Append("*/");
			num = item.Position + item.Length;
		}
		stringBuilder2.Append(text.Substring(num, text.Length - num));
		string value = stringBuilder2.ToString();
		writer.Write(value, singleQuoteValue: false, ignoreIndent: true);
		writer.WriteLine();
	}

	private void ExportVariables(IElement function, IAlWriter writer, bool commentedOut)
	{
		if (commentedOut)
		{
			commentedVariableSectionExporter.Export(function, writer);
		}
		else
		{
			variableExporter.Export(function, writer);
		}
	}
}
