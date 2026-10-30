using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class AdditionalTextFile : AdditionalText
{
	private readonly CommandLineSourceFile _sourceFile;

	private readonly CommonCompiler _compiler;

	private SourceText _text;

	private IList<DiagnosticInfo> _diagnostics;

	private readonly object _lockObject = new object();

	public override string Path => _sourceFile.Path;

	internal IList<DiagnosticInfo> Diagnostics => _diagnostics;

	public AdditionalTextFile(CommandLineSourceFile sourceFile, CommonCompiler compiler)
	{
		if (compiler == null)
		{
			throw new ArgumentNullException("compiler");
		}
		_sourceFile = sourceFile;
		_compiler = compiler;
		_diagnostics = SpecializedCollections.EmptyList<DiagnosticInfo>();
	}

	public override SourceText GetText(CancellationToken cancellationToken = default(CancellationToken))
	{
		lock (_lockObject)
		{
			if (_text == null)
			{
				List<DiagnosticInfo> diagnostics = new List<DiagnosticInfo>();
				_text = _compiler.TryReadFileContent(_sourceFile, diagnostics);
				_diagnostics = diagnostics;
			}
		}
		return _text;
	}
}
