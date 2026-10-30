using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class AdditionalTextFile : AdditionalText
{
	private readonly CommonCompiler compiler;

	private readonly object lockObject = new object();

	private readonly CommandLineSourceFile sourceFile;

	private SourceText text;

	public override string Path => sourceFile.Path;

	internal List<DiagnosticInfo> Diagnostics { get; private set; }

	public AdditionalTextFile(CommandLineSourceFile sourceFile, CommonCompiler compiler)
	{
		if (compiler == null)
		{
			throw new ArgumentNullException("compiler");
		}
		this.sourceFile = sourceFile;
		this.compiler = compiler;
		Diagnostics = new List<DiagnosticInfo>();
	}

	public override SourceText GetText(CancellationToken cancellationToken = default(CancellationToken))
	{
		lock (lockObject)
		{
			if (text == null)
			{
				List<DiagnosticInfo> diagnostics = new List<DiagnosticInfo>();
				text = compiler.ReadFileContent(sourceFile, diagnostics);
				Diagnostics = diagnostics;
			}
		}
		return text;
	}
}
