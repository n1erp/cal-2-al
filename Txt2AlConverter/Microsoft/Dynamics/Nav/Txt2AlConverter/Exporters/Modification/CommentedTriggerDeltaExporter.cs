using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class CommentedTriggerDeltaExporter : CommentedCodeDeltaExporter
{
	public override void Export(IElement element, IAlWriter writer)
	{
		ExportCommentedCode(element, writer);
	}

	internal override string ReduceElementName(string fullName)
	{
		if (!fullName.Contains('.'))
		{
			return fullName;
		}
		return fullName.Split('.')[1];
	}

	internal override void WriteMethodSignature(IElement element, IAlWriter writer, string triggerName, bool commentedOut)
	{
		string value = AlUtilities.GetTriggerTypeInfo(element, triggerName ?? string.Empty)?.GetSignatureString() ?? string.Empty;
		if (!string.IsNullOrEmpty(value))
		{
			writer.WriteLinePossiblyCommentedOut(value, commentedOut);
			return;
		}
		value = ConstructFakeSignatureFromName(triggerName);
		writer.WriteLinePossiblyCommentedOut(value, commentedOut);
		writer.WriteLinePossiblyCommentedOut("Parameters and return type have not been exported.", commentedOut: true);
	}

	private static string ConstructFakeSignatureFromName(string triggerName)
	{
		return SyntaxKind.TriggerKeyword.GetText() + " " + triggerName + "()";
	}
}
