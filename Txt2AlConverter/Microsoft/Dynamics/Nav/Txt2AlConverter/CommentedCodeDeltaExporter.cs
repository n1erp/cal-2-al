using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class CommentedCodeDeltaExporter : ProcedureExporter
{
	private const bool commentedOut = true;

	public override void Export(IElement element, IAlWriter writer)
	{
		string value = null;
		element?.TryGetStringProperty(PropertyType.Target, out value);
		IElementExporter elementExporter;
		if (value != null && ModificationContentExporter.GetExtensionTargetType(element) == ElementType.Procedure)
		{
			elementExporter = new CommentedProcedureDeltaExporter();
		}
		else
		{
			elementExporter = ((value == null || ModificationContentExporter.GetExtensionTargetType(element) != ElementType.Variable) ? ((CommentedCodeDeltaExporter)new CommentedTriggerDeltaExporter()) : ((CommentedCodeDeltaExporter)new CommentedVariableDeltaExporter()));
		}
		elementExporter.Export(element, writer);
	}

	internal void ExportCommentedCode(IElement element, IAlWriter writer)
	{
		switch (element.GetChangeType())
		{
		case ChangeTypeKind.CodeModification:
		{
			IElement firstSubElement = element.GetFirstSubElement(ElementType.OriginalCode);
			IElement firstSubElement2 = element.GetFirstSubElement(ElementType.ModifiedCode);
			element.TryGetStringProperty(PropertyType.Target, out var fullName);
			string elementName = ReduceElementName(fullName);
			ExportCodeModification(element, writer, elementName, firstSubElement, firstSubElement2);
			break;
		}
		case ChangeTypeKind.Insertion:
		{
			IElement insertedCode = element.GetSubElements().First();
			string fullName = element.GetSubElements().First().ToString();
			string elementName = ReduceElementName(fullName);
			ExportCodeInsertion(element, writer, elementName, insertedCode);
			break;
		}
		}
	}

	internal void ExportCodeInsertion(IElement element, IAlWriter writer, string elementName, IElement insertedCode)
	{
		if (writer.PendingEmptyLine)
		{
			writer.WriteLine();
		}
		new WarningExporter("Code Insertion").Export(element, writer);
		WriteMethodSignature(element, writer, elementName, commentedOut: true);
		ExportFunctionBody(writer, insertedCode, commentedOut: true);
		writer.PendingEmptyLine = true;
	}

	internal void ExportCodeModification(IElement element, IAlWriter writer, string elementName, IElement originalCode, IElement modifiedCode)
	{
		if (writer.PendingEmptyLine)
		{
			writer.WriteLine();
		}
		new WarningExporter().Export(element, writer);
		WriteMethodSignature(element, writer, elementName, commentedOut: true);
		writer.WriteLinePossiblyCommentedOut(">>>> ORIGINAL CODE:", commentedOut: true);
		ExportFunctionBody(writer, originalCode, commentedOut: true);
		writer.WriteLinePossiblyCommentedOut(">>>> MODIFIED CODE:", commentedOut: true);
		ExportFunctionBody(writer, modifiedCode, commentedOut: true);
		writer.PendingEmptyLine = true;
	}

	internal virtual string ReduceElementName(string fullName)
	{
		return fullName;
	}

	internal virtual void WriteMethodSignature(IElement element, IAlWriter writer, string elementName, bool commentedOut)
	{
	}
}
