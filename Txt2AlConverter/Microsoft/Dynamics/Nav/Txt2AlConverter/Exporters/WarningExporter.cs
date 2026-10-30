using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;

internal class WarningExporter : IElementExporter
{
	private static string additionalInfo;

	internal WarningExporter(object info = null)
	{
		additionalInfo = info?.ToString();
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportWarningElement(element, writer);
	}

	private static void ExportWarningElement(IElement element, IAlWriter writer)
	{
		string text = additionalInfo ?? element.GetChangeTypeText();
		string changeValueInformation = GetChangeValueInformation(element);
		string targetInformation = GetTargetInformation(element);
		targetInformation = ((targetInformation != null) ? (" on \"" + targetInformation + "\"") : string.Empty);
		changeValueInformation = ((changeValueInformation != null) ? (" (" + changeValueInformation + ")") : string.Empty);
		string text2 = "Unsupported feature: " + text + changeValueInformation + targetInformation + ".";
		writer.WriteLine();
		writer.WriteLinePossiblyCommentedOut(text2, commentedOut: true);
		writer.WriteLine();
		DisplayConsoleWarning(text2);
	}

	internal static void DisplayConsoleWarning(string message)
	{
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.DarkYellow;
		Console.Error.WriteLine();
		Console.Error.WriteLine("Warning: " + message);
		Console.Error.WriteLine();
		Console.ResetColor();
	}

	private static string GetTargetInformation(IElement element)
	{
		string stringProperty = element.GetStringProperty(PropertyType.Target);
		if (stringProperty != null)
		{
			return stringProperty;
		}
		stringProperty = element.GetStringProperty(PropertyType.TargetValue);
		if (stringProperty != null)
		{
			return stringProperty;
		}
		List<IElement> list = element.GetSubElements().ToList();
		if (list.Count > 0)
		{
			return list.First().ToString();
		}
		return null;
	}

	private static string GetChangeValueInformation(IElement element)
	{
		return element.GetStringProperty(PropertyType.Property) ?? element.GetStringProperty(PropertyType.AddedChildren);
	}
}
