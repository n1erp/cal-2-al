using System;
using System.Collections.Concurrent;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SourceContextInfo
{
	internal const int InvalidScopeId = -1;

	private readonly ConcurrentDictionary<int, string> methodNames = new ConcurrentDictionary<int, string>();

	private readonly bool isApplicationObject;

	private readonly ConcurrentDictionary<Tuple<int, int>, Tuple<int, string>> statements = new ConcurrentDictionary<Tuple<int, int>, Tuple<int, string>>();

	internal AppQualifiedApplicationObjectId AppQualifiedObjectId { get; private set; }

	internal string[] SourceLines { get; private set; }

	public SourceContextInfo(AppQualifiedApplicationObjectId appQualifiedObjectId, string[] sourceLines, bool isApplicationObject)
	{
		AppQualifiedObjectId = appQualifiedObjectId;
		SourceLines = sourceLines;
		this.isApplicationObject = isApplicationObject;
	}

	internal bool TryGetMethodName(int scopeId, out string methodName)
	{
		return methodNames.TryGetValue(scopeId, out methodName);
	}

	internal bool TryGetLineNumberAndStatement(int scopeId, int statementNumber, out Tuple<int, string> lineNumberAndStatement)
	{
		return statements.TryGetValue(Tuple.Create(scopeId, statementNumber), out lineNumberAndStatement);
	}

	internal string AddOrUpdateMethodName(int scopeId, string methodName)
	{
		return methodNames.AddOrUpdate(scopeId, methodName, (int tempScopeId, string tempMethodName) => tempMethodName);
	}

	internal Tuple<int, string> AddOrUpdateLineNumberAndStatement(int scopeId, int statementNumber, SourceSpan sourceSpan)
	{
		string statement = GetStatement(sourceSpan);
		Tuple<int, string> lineAndSatatement = Tuple.Create((int)sourceSpan.From.Line, statement);
		return statements.AddOrUpdate(Tuple.Create(scopeId, statementNumber), lineAndSatatement, (Tuple<int, int> k, Tuple<int, string> s) => lineAndSatatement);
	}

	internal void Clear()
	{
		statements.Clear();
		methodNames.Clear();
	}

	private static string TrimStatementSourceCode(string statement)
	{
		int num = statement.IndexOf("//", StringComparison.OrdinalIgnoreCase);
		if (num > 0)
		{
			statement = statement.Substring(0, num);
		}
		return statement.Trim();
	}

	private string GetStatement(SourceSpan sourceSpan)
	{
		if (sourceSpan.From > sourceSpan.To || SourceLines.Length <= sourceSpan.From.Line || SourceLines.Length <= sourceSpan.To.Line)
		{
			return string.Empty;
		}
		if (sourceSpan.From.Line != sourceSpan.To.Line)
		{
			string text = SourceLines[sourceSpan.From.Line];
			int num = text.Length - sourceSpan.From.Column;
			if (num < 0)
			{
				return string.Empty;
			}
			string statement = text.Substring(sourceSpan.From.Column, num);
			statement = TrimStatementSourceCode(statement);
			for (int i = sourceSpan.From.Line + 1; i < sourceSpan.To.Line; i++)
			{
				statement += TrimStatementSourceCode(SourceLines[i]);
			}
			string text2 = SourceLines[sourceSpan.To.Line];
			int endColumnPosition = GetEndColumnPosition(sourceSpan.To.Column);
			if (text2.Length < endColumnPosition)
			{
				return string.Empty;
			}
			return statement + TrimStatementSourceCode(text2.Substring(0, endColumnPosition));
		}
		string text3 = SourceLines[sourceSpan.From.Line];
		int endColumnPosition2 = GetEndColumnPosition(sourceSpan.To.Column);
		if (text3.Length < endColumnPosition2)
		{
			return string.Empty;
		}
		int num2 = sourceSpan.To.Column - sourceSpan.From.Column;
		if (!isApplicationObject)
		{
			num2++;
		}
		return text3.Substring(sourceSpan.From.Column, num2);
	}

	private int GetEndColumnPosition(int columnPosition)
	{
		if (!isApplicationObject)
		{
			return columnPosition + 1;
		}
		return columnPosition;
	}
}
