using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class DiagnosticBagExtensions
{
	internal static NavDiagnosticInfo Add(this DiagnosticBag diagnostics, ErrorCode code, Location location)
	{
		NavDiagnosticInfo navDiagnosticInfo = new NavDiagnosticInfo(code);
		NavDiagnostic diag = new NavDiagnostic(navDiagnosticInfo, location);
		diagnostics.Add(diag);
		return navDiagnosticInfo;
	}

	internal static NavDiagnosticInfo Add(this DiagnosticBag diagnostics, ErrorCode code, Location location, params object[] args)
	{
		NavDiagnosticInfo navDiagnosticInfo = new NavDiagnosticInfo(code, args);
		NavDiagnostic diag = new NavDiagnostic(navDiagnosticInfo, location);
		diagnostics.Add(diag);
		return navDiagnosticInfo;
	}

	internal static NavDiagnosticInfo Add(this List<DiagnosticInfo> diagnostics, ErrorCode code, Location location, params object[] args)
	{
		NavDiagnosticInfo navDiagnosticInfo = new NavDiagnosticInfo(code, args);
		diagnostics.Add(navDiagnosticInfo);
		return navDiagnosticInfo;
	}

	internal static NavDiagnosticInfo Add(this DiagnosticBag diagnostics, ErrorCode code, Location location, ImmutableArray<Symbol> symbols, params object[] args)
	{
		NavDiagnosticInfo navDiagnosticInfo = new NavDiagnosticInfo(code, args, symbols, ImmutableArray<Location>.Empty);
		NavDiagnostic diag = new NavDiagnostic(navDiagnosticInfo, location);
		diagnostics.Add(diag);
		return navDiagnosticInfo;
	}

	internal static void Add(this DiagnosticBag diagnostics, NavDiagnosticInfo info, Location location)
	{
		NavDiagnostic diag = new NavDiagnostic(info, location);
		diagnostics.Add(diag);
	}

	internal static void Add(this DiagnosticBag diagnostics, DiagnosticInfo info, Location location)
	{
		NavDiagnostic diag = new NavDiagnostic(info, location);
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
			diagnostics.Add(new NavDiagnostic(useSiteDiagnostic, location));
		}
		return result;
	}
}
