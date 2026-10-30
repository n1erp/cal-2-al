using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class CommentedVariableDeltaExporter : CommentedCodeDeltaExporter
{
	private const bool commentedOut = true;

	public override void Export(IElement element, IAlWriter writer)
	{
		ExportVariableModification(element, writer);
	}

	private void ExportVariableModification(IElement element, IAlWriter writer)
	{
		if (writer.PendingEmptyLine)
		{
			writer.WriteLine();
		}
		element.TryGetStringProperty(PropertyType.Target, out var value);
		new WarningExporter().Export(element, writer);
		element.TryGetStringProperty(PropertyType.OriginalValue, out var value2);
		element.TryGetStringProperty(PropertyType.ModifiedValue, out var value3);
		string elementName = ReduceElementName(value);
		writer.WriteLinePossiblyCommentedOut(SyntaxKind.VarKeyword.GetText(), commentedOut: true);
		writer.PushIndent(writer.Indentation);
		writer.WriteLinePossiblyCommentedOut(">>>> ORIGINAL VALUE:", commentedOut: true);
		WriteVariable(element, writer, elementName, value2);
		writer.WriteLinePossiblyCommentedOut(">>>> MODIFIED VALUE:", commentedOut: true);
		WriteVariable(element, writer, elementName, value3);
		writer.PopIndent();
		writer.PendingEmptyLine = true;
	}

	internal override string ReduceElementName(string fullName)
	{
		if (!fullName.Contains('('))
		{
			return fullName;
		}
		return fullName.Split('(')[0];
	}

	private static void WriteVariable(IElement element, IAlWriter writer, string elementName, string value)
	{
		writer.WriteLinePossiblyCommentedOut(elementName + " : " + value + ";", commentedOut: true);
		writer.WriteLinePossiblyCommentedOut("Variable type has not been exported.", commentedOut: true);
	}
}
