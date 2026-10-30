using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Roslyn.Utilities;

internal class NoThrowStreamDisposer : IDisposable
{
	private bool? _failed;

	private readonly string _filePath;

	private readonly DiagnosticBag _diagnostics;

	private readonly CommonMessageProvider _messageProvider;

	public Stream Stream { get; }

	public bool HasFailedToDispose => _failed == true;

	public NoThrowStreamDisposer(Stream stream, string filePath, DiagnosticBag diagnostics, CommonMessageProvider messageProvider)
	{
		Stream = stream;
		_failed = null;
		_filePath = filePath;
		_diagnostics = diagnostics;
		_messageProvider = messageProvider;
	}

	public void Dispose()
	{
		try
		{
			Stream.Dispose();
			if (!_failed.HasValue)
			{
				_failed = false;
			}
		}
		catch (Exception e)
		{
			_messageProvider.ReportStreamWriteException(e, _filePath, _diagnostics);
			_failed = true;
		}
	}
}
