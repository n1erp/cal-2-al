using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class AbstractLexer : IDisposable
{
	internal readonly SlidingTextWindow TextWindow;

	private List<SyntaxDiagnosticInfo> _errors;

	protected bool HasErrors => _errors != null;

	protected AbstractLexer(SourceText text)
	{
		TextWindow = new SlidingTextWindow(text);
	}

	public virtual void Dispose()
	{
		TextWindow.Dispose();
	}

	protected void Start()
	{
		TextWindow.Start();
		_errors = null;
	}

	protected SyntaxDiagnosticInfo[] GetErrors(int leadingTriviaWidth)
	{
		if (_errors != null)
		{
			if (leadingTriviaWidth > 0)
			{
				SyntaxDiagnosticInfo[] array = new SyntaxDiagnosticInfo[_errors.Count];
				for (int i = 0; i < _errors.Count; i++)
				{
					array[i] = _errors[i].WithOffset(_errors[i].Offset + leadingTriviaWidth);
				}
				return array;
			}
			return _errors.ToArray();
		}
		return null;
	}

	protected void AddError(int position, int width, ErrorCode code)
	{
		AddError(MakeError(position, width, code));
	}

	protected void AddError(int position, int width, ErrorCode code, params object[] args)
	{
		AddError(MakeError(position, width, code, args));
	}

	protected void AddError(int position, int width, XmlParseErrorCode code, params object[] args)
	{
		AddError(MakeError(position, width, code, args));
	}

	protected void AddError(ErrorCode code)
	{
		AddError(MakeError(code));
	}

	protected void AddError(ErrorCode code, params object[] args)
	{
		AddError(MakeError(code, args));
	}

	protected void AddError(XmlParseErrorCode code)
	{
		AddError(MakeError(code));
	}

	protected void AddError(XmlParseErrorCode code, params object[] args)
	{
		AddError(MakeError(code, args));
	}

	protected void AddError(SyntaxDiagnosticInfo error)
	{
		if (error != null)
		{
			if (_errors == null)
			{
				_errors = new List<SyntaxDiagnosticInfo>(8);
			}
			_errors.Add(error);
		}
	}

	protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code)
	{
		return new SyntaxDiagnosticInfo(GetLexemeOffsetFromPosition(position), width, code);
	}

	protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(GetLexemeOffsetFromPosition(position), width, code, args);
	}

	protected XmlSyntaxDiagnosticInfo MakeError(int position, int width, XmlParseErrorCode code, params object[] args)
	{
		return new XmlSyntaxDiagnosticInfo(GetLexemeOffsetFromPosition(position), width, code, args);
	}

	private int GetLexemeOffsetFromPosition(int position)
	{
		if (position < TextWindow.LexemeStartPosition)
		{
			return position;
		}
		return position - TextWindow.LexemeStartPosition;
	}

	protected static SyntaxDiagnosticInfo MakeError(ErrorCode code)
	{
		return new SyntaxDiagnosticInfo(code);
	}

	protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(code, args);
	}

	protected static XmlSyntaxDiagnosticInfo MakeError(XmlParseErrorCode code)
	{
		return new XmlSyntaxDiagnosticInfo(0, 0, code);
	}

	protected static XmlSyntaxDiagnosticInfo MakeError(XmlParseErrorCode code, params object[] args)
	{
		return new XmlSyntaxDiagnosticInfo(0, 0, code, args);
	}
}
