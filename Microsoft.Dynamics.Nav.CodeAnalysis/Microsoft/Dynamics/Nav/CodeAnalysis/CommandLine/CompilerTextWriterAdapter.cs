using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

internal class CompilerTextWriterAdapter : CompilerTextWriter
{
	private readonly TextWriter textWriter;

	public CompilerTextWriterAdapter(TextWriter textWriter)
	{
		if (textWriter == null)
		{
			throw new ArgumentNullException("textWriter");
		}
		this.textWriter = textWriter;
	}

	public override void WriteLine()
	{
		textWriter.WriteLine();
	}

	public override void WriteLine(string text)
	{
		textWriter.WriteLine(text);
	}

	public override void WriteLine(string text, params object[] args)
	{
		textWriter.WriteLine(text, args);
	}
}
