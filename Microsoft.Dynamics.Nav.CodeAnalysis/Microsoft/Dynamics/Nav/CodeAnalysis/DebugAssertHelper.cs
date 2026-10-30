using System;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class DebugAssertHelper
{
	private const string DefaultAssertFailure = "DebugAssertHelper.Assert failed";

	public static void Assert(bool condition, string message = "DebugAssertHelper.Assert failed")
	{
		if (!condition)
		{
			GlobalLogger.LogError(message + Environment.NewLine + new StackTrace(fNeedFileInfo: true));
		}
	}

	public static void Assert(bool condition, Symbol node)
	{
		if (CanLogOrAssert(condition))
		{
			Assert(condition, node?.ToReportingString());
		}
	}

	public static void Assert(bool condition, SyntaxNode node)
	{
		if (CanLogOrAssert(condition))
		{
			Assert(condition, node?.ToReportingString());
		}
	}

	public static void Fail(string message)
	{
		Assert(condition: false, message);
	}

	private static bool CanLogOrAssert(bool condition)
	{
		if (condition)
		{
			return false;
		}
		if (!GlobalLogger.Enabled)
		{
			return false;
		}
		return true;
	}
}
