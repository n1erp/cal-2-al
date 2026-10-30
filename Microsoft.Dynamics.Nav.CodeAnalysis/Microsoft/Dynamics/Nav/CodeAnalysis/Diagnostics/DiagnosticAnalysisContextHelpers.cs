using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal static class DiagnosticAnalysisContextHelpers
{
	internal static void VerifyArguments<TContext>(Action<TContext> action)
	{
		VerifyAction(action);
	}

	internal static void VerifyArguments<TContext>(Action<TContext> action, ImmutableArray<SymbolKind> symbolKinds)
	{
		VerifyAction(action);
		VerifySymbolKinds(symbolKinds);
	}

	internal static void VerifyArguments<TContext, TLanguageKindEnum>(Action<TContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) where TLanguageKindEnum : struct
	{
		VerifyAction(action);
		VerifySyntaxKinds(syntaxKinds);
	}

	internal static void VerifyArguments(Diagnostic diagnostic, Func<Diagnostic, bool> isSupportedDiagnostic)
	{
		if (!(diagnostic is DiagnosticWithInfo))
		{
			if (diagnostic == null)
			{
				throw new ArgumentNullException("diagnostic");
			}
			if (!isSupportedDiagnostic(diagnostic))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.UnsupportedDiagnosticReported, diagnostic.Id), "diagnostic");
			}
		}
	}

	internal static void VerifyArguments(Diagnostic diagnostic, Compilation compilationOpt, Func<Diagnostic, bool> isSupportedDiagnostic)
	{
		if (!(diagnostic is DiagnosticWithInfo))
		{
			if (diagnostic == null)
			{
				throw new ArgumentNullException("diagnostic");
			}
			if (compilationOpt != null)
			{
				VerifyDiagnosticLocationsInCompilation(diagnostic, compilationOpt);
			}
			if (!isSupportedDiagnostic(diagnostic))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.UnsupportedDiagnosticReported, diagnostic.Id), "diagnostic");
			}
			if (!UnicodeCharacterUtilities.IsValidIdentifier(diagnostic.Id))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.UnsupportedDiagnosticReported, diagnostic.Id), "diagnostic");
			}
		}
	}

	internal static void VerifyDiagnosticLocationsInCompilation(Diagnostic diagnostic, Compilation compilation)
	{
		VerifyDiagnosticLocationInCompilation(diagnostic.Id, diagnostic.Location, compilation);
		if (diagnostic.AdditionalLocations == null)
		{
			return;
		}
		foreach (Location additionalLocation in diagnostic.AdditionalLocations)
		{
			VerifyDiagnosticLocationInCompilation(diagnostic.Id, additionalLocation, compilation);
		}
	}

	[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
	private static void VerifyDiagnosticLocationInCompilation(string id, Location location, Compilation compilation)
	{
		if (location.IsInSource && !compilation.ContainsSyntaxTree(location.SourceTree))
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.InvalidDiagnosticLocationReported, id, location.SourceTree.FilePath), "location");
		}
	}

	private static void VerifyAction<TContext>(Action<TContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
	}

	private static void VerifySymbolKinds(ImmutableArray<SymbolKind> symbolKinds)
	{
		if (symbolKinds.IsDefault)
		{
			throw new ArgumentNullException("symbolKinds");
		}
		if (symbolKinds.IsEmpty)
		{
			throw new ArgumentException(CodeAnalysisResources.ArgumentCannotBeEmpty, "symbolKinds");
		}
	}

	private static void VerifySyntaxKinds<TLanguageKindEnum>(ImmutableArray<TLanguageKindEnum> syntaxKinds) where TLanguageKindEnum : struct
	{
		if (syntaxKinds.IsDefault)
		{
			throw new ArgumentNullException("syntaxKinds");
		}
		if (syntaxKinds.IsEmpty)
		{
			throw new ArgumentException(CodeAnalysisResources.ArgumentCannotBeEmpty, "syntaxKinds");
		}
	}
}
