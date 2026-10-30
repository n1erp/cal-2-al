using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class DiagnosticBagExtensions
{
	internal static CSDiagnosticInfo Add(this DiagnosticBag diagnostics, ErrorCode code, Location location)
	{
		CSDiagnosticInfo cSDiagnosticInfo = new CSDiagnosticInfo(code);
		CSDiagnostic diag = new CSDiagnostic(cSDiagnosticInfo, location);
		diagnostics.Add(diag);
		return cSDiagnosticInfo;
	}

	internal static CSDiagnosticInfo Add(this DiagnosticBag diagnostics, ErrorCode code, Location location, params object[] args)
	{
		CSDiagnosticInfo cSDiagnosticInfo = new CSDiagnosticInfo(code, args);
		CSDiagnostic diag = new CSDiagnostic(cSDiagnosticInfo, location);
		diagnostics.Add(diag);
		return cSDiagnosticInfo;
	}

	internal static CSDiagnosticInfo Add(this DiagnosticBag diagnostics, ErrorCode code, Location location, ImmutableArray<Symbol> symbols, params object[] args)
	{
		CSDiagnosticInfo cSDiagnosticInfo = new CSDiagnosticInfo(code, args, symbols, ImmutableArray<Location>.Empty);
		CSDiagnostic diag = new CSDiagnostic(cSDiagnosticInfo, location);
		diagnostics.Add(diag);
		return cSDiagnosticInfo;
	}

	internal static void Add(this DiagnosticBag diagnostics, DiagnosticInfo info, Location location)
	{
		CSDiagnostic diag = new CSDiagnostic(info, location);
		diagnostics.Add(diag);
	}

	internal static bool Add(this DiagnosticBag diagnostics, SyntaxNode node, HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!useSiteDiagnostics.IsNullOrEmpty())
		{
			return diagnostics.Add(node.Location, useSiteDiagnostics);
		}
		return false;
	}

	internal static bool Add(this DiagnosticBag diagnostics, Location location, HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (useSiteDiagnostics.IsNullOrEmpty())
		{
			return false;
		}
		bool result = false;
		foreach (DiagnosticInfo useSiteDiagnostic in useSiteDiagnostics)
		{
			if (useSiteDiagnostic.Severity == DiagnosticSeverity.Error)
			{
				result = true;
			}
			diagnostics.Add(new CSDiagnostic(useSiteDiagnostic, location));
		}
		return result;
	}
}
