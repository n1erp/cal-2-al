using System.Diagnostics;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class StatementSourceInfo
{
	private readonly MethodSourceInfo method;

	private readonly int statementNumber;

	private readonly SourceSpan sourceSpan;

	public MethodSourceInfo Method
	{
		[DebuggerStepThrough]
		get
		{
			return method;
		}
	}

	public int StatementNumber
	{
		[DebuggerStepThrough]
		get
		{
			return statementNumber;
		}
	}

	public SourceSpan SourceSpan
	{
		[DebuggerStepThrough]
		get
		{
			return sourceSpan;
		}
	}

	public SourceSpan RelativeSourceSpan => new SourceSpan(new SourcePosition((ushort)(sourceSpan.From.Line - Method.SignatureLineNumber), sourceSpan.From.Column), new SourcePosition((ushort)(sourceSpan.To.Line - Method.SignatureLineNumber), sourceSpan.To.Column));

	public StatementSourceInfo(MethodSourceInfo method, int statementNumber, SourceSpan sourceSpan)
	{
		this.method = method;
		this.statementNumber = statementNumber;
		this.sourceSpan = sourceSpan;
	}
}
