using System;
using System.IO;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Report;

internal sealed class WordLayoutExporter
{
	public void Export(IElement element, Stream stream)
	{
		foreach (string wordLayoutLine in element.GetWordLayoutLines())
		{
			byte[] array = Convert.FromBase64String(wordLayoutLine.Trim());
			stream.Write(array, 0, array.Length);
		}
	}
}
