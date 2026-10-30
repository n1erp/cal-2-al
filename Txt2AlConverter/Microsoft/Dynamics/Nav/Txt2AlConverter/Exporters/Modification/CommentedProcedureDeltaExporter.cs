using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class CommentedProcedureDeltaExporter : CommentedCodeDeltaExporter
{
	public override void Export(IElement element, IAlWriter writer)
	{
		ExportCommentedCode(element, writer);
	}

	internal override string ReduceElementName(string fullName)
	{
		if (!fullName.Contains('('))
		{
			return fullName;
		}
		return fullName.Split('(')[0];
	}

	internal override void WriteMethodSignature(IElement element, IAlWriter writer, string procedureName, bool commentedOut)
	{
		string value = SyntaxKind.ProcedureKeyword.GetText() + " " + procedureName + "();";
		writer.WriteLinePossiblyCommentedOut(value, commentedOut);
		writer.WriteLinePossiblyCommentedOut("Parameters and return type have not been exported.", commentedOut: true);
	}
}
