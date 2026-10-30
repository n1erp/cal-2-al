using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class VariableSectionExporter : IElementExporter
{
	public void Export(IElement element, IAlWriter writer)
	{
		ExportVariableSectionOfElement(writer, element);
	}

	internal static void ExportVariableSectionOfElement(IAlWriter writer, IElement element, bool commentedOut = false)
	{
		IElement[] array = element.GetSubElements(ElementType.Variable).ToArray();
		if (array.Length != 0)
		{
			if (writer.PendingEmptyLine)
			{
				writer.WriteLine();
			}
			writer.WriteLinePossiblyCommentedOut(SyntaxKind.VarKeyword.GetText(), commentedOut);
			writer.PushIndent(writer.Indentation);
			IElement[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				AlUtilities.WriteVariableDeclaration(array2[i], writer, prependNameWithSpace: false, commentedOut);
				writer.WriteLine(SyntaxKind.SemicolonToken.GetText());
			}
			writer.PopIndent();
			writer.PendingEmptyLine = true;
		}
	}
}
