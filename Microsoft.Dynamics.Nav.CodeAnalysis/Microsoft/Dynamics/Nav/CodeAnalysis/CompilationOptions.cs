using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public sealed class CompilationOptions : IEquatable<CompilationOptions>
{
	public const CompilationGenerationOptions DefaultCompilationGenerationOptions = CompilationGenerationOptions.ReportLayout;

	public const CodeGenerationOption DefaultCodeGenerationOptions = CodeGenerationOption.Text;

	public bool ConcurrentEmit { get; }

	public bool ConcurrentBuild { get; }

	public bool ReportSuppressedDiagnostics { get; }

	public int WarningLevel { get; }

	public ReportDiagnostic GeneralDiagnosticOption { get; }

	public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; }

	public IdSpaces IdSpaces { get; }

	public CompilationTarget Target { get; }

	public bool ReadonlyErrorsAsWarnings { get; }

	public bool AllowExtensionAndTargetInSameModule { get; }

	public CompilerFeatures CompilerFeatures { get; }

	public bool ShowMyCode { get; }

	public CompilationGenerationOptions CompilationGenerateOptions { get; }

	public CodeGenerationOption CodeGenerationOptions { get; }

	public NavAppSupportedLocales SupportedLocales { get; }

	public string ContextSensitiveHelpUrl { get; }

	[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
	public CompilationOptions(bool concurrentBuild = true, int warningLevel = 4, ReportDiagnostic generalDiagnosticOption = ReportDiagnostic.Default, bool reportSuppressedDiagnostics = false, IdSpaces idSpaces = null, CompilationTarget target = CompilationTarget.Extension, bool allowExtensionAndTargetInSameModule = false, bool readonlyErrorsAsWarnings = true, CompilerFeatures compilerFeatures = CompilerFeatures.None, bool showMyCode = false, IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions = null, CompilationGenerationOptions generateOptions = CompilationGenerationOptions.ReportLayout, NavAppSupportedLocales supportedLocales = null, CodeGenerationOption codeGenerationOptions = CodeGenerationOption.Text, bool concurrentEmit = false, string contextSensitiveHelpUrl = null)
	{
		ConcurrentBuild = concurrentBuild;
		WarningLevel = warningLevel;
		GeneralDiagnosticOption = generalDiagnosticOption;
		ReportSuppressedDiagnostics = reportSuppressedDiagnostics;
		IdSpaces = idSpaces ?? IdSpaces.BaseAppDevelopmentIdSpaces;
		Target = target;
		AllowExtensionAndTargetInSameModule = allowExtensionAndTargetInSameModule;
		ReadonlyErrorsAsWarnings = readonlyErrorsAsWarnings;
		CompilerFeatures = compilerFeatures;
		ShowMyCode = showMyCode;
		SpecificDiagnosticOptions = specificDiagnosticOptions.ToImmutableDictionaryOrEmpty();
		CompilationGenerateOptions = generateOptions;
		ContextSensitiveHelpUrl = contextSensitiveHelpUrl;
		SupportedLocales = supportedLocales;
		CodeGenerationOptions = codeGenerationOptions;
		ConcurrentEmit = concurrentEmit;
	}

	private CompilationOptions(CompilationOptions other, bool? concurrentBuild = null, int? warningLevel = null, ReportDiagnostic? generalDiagnosticOption = null, bool? reportSuppressedDiagnostics = null, IdSpaces idSpaces = null, CompilationTarget? target = null, bool? allowExtensionAndTargetInSameModule = null, bool? readonlyErrorsAsWarnings = true, CompilerFeatures? compilerFeatures = null, ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = null, CompilationGenerationOptions? generateOptions = null, NavAppSupportedLocales supportedLocales = null, string contextSensitiveHelpUrl = null, CodeGenerationOption? codeGenerationOptions = null, bool? concurrentEmit = null, bool? showMyCode = null)
		: this(concurrentBuild ?? other.ConcurrentBuild, warningLevel ?? other.WarningLevel, generalDiagnosticOption ?? other.GeneralDiagnosticOption, reportSuppressedDiagnostics ?? other.ReportSuppressedDiagnostics, idSpaces ?? other.IdSpaces, target ?? other.Target, allowExtensionAndTargetInSameModule ?? other.AllowExtensionAndTargetInSameModule, compilerFeatures: compilerFeatures ?? other.CompilerFeatures, specificDiagnosticOptions: specificDiagnosticOptions ?? other.SpecificDiagnosticOptions, generateOptions: generateOptions ?? other.CompilationGenerateOptions, contextSensitiveHelpUrl: contextSensitiveHelpUrl ?? other.ContextSensitiveHelpUrl, supportedLocales: supportedLocales ?? other.SupportedLocales, codeGenerationOptions: codeGenerationOptions ?? other.CodeGenerationOptions, concurrentEmit: concurrentEmit ?? other.ConcurrentEmit, showMyCode: showMyCode ?? other.ShowMyCode, readonlyErrorsAsWarnings: readonlyErrorsAsWarnings ?? other.ReadonlyErrorsAsWarnings)
	{
	}

	internal Diagnostic FilterDiagnostic(Diagnostic diagnostic)
	{
		return DiagnosticFilter.Filter(diagnostic, WarningLevel, GeneralDiagnosticOption, SpecificDiagnosticOptions);
	}

	public CompilationOptions WithManifestOptions(NavAppManifest manifest)
	{
		return new CompilationOptions(this, null, null, null, null, compilerFeatures: manifest.CompilerFeatures, idSpaces: manifest.IdSpaces, contextSensitiveHelpUrl: manifest.ContextSensitiveHelpUrl ?? string.Empty, supportedLocales: manifest.AppSupportedLocales, showMyCode: manifest.ShowMyCode, target: manifest.Target, allowExtensionAndTargetInSameModule: null, readonlyErrorsAsWarnings: true);
	}

	public CompilationOptions WithSpecificDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic> values)
	{
		if (values == null)
		{
			values = ImmutableDictionary<string, ReportDiagnostic>.Empty;
		}
		if (SpecificDiagnosticOptions == values)
		{
			return this;
		}
		return new CompilationOptions(this, null, null, null, null, null, null, null, true, null, values);
	}

	public CompilationOptions WithGeneralDiagnosticOption(ReportDiagnostic value)
	{
		if (GeneralDiagnosticOption == value)
		{
			return this;
		}
		return new CompilationOptions(this, null, null, value, null, null, null, null, true);
	}

	public CompilationOptions WithWarningLevel(int warningLevel)
	{
		if (WarningLevel == warningLevel)
		{
			return this;
		}
		return new CompilationOptions(this, null, warningLevel, null, null, null, null, null, true);
	}

	public CompilationOptions WithConcurrentBuild(bool concurrentBuild)
	{
		if (ConcurrentBuild == concurrentBuild)
		{
			return this;
		}
		return new CompilationOptions(this, concurrentBuild, null, null, null, null, null, null, true);
	}

	public CompilationOptions WithReportSuppressedDiagnostics(bool reportSuppressedDiagnostics)
	{
		if (ReportSuppressedDiagnostics == reportSuppressedDiagnostics)
		{
			return this;
		}
		return new CompilationOptions(this, null, null, null, reportSuppressedDiagnostics, null, null, null, true);
	}

	public CompilationOptions WithIdSpaces(IdSpaces idSpaces)
	{
		if (IdSpaces == idSpaces)
		{
			return this;
		}
		return new CompilationOptions(this, null, null, null, null, idSpaces, null, null, true);
	}

	public CompilationOptions WithTarget(CompilationTarget target)
	{
		if (Target == target)
		{
			return this;
		}
		return new CompilationOptions(this, null, null, null, null, null, target, null, true);
	}

	public CompilationOptions WithAllowExtensionAndTargetInSameModule(bool allow)
	{
		if (AllowExtensionAndTargetInSameModule == allow)
		{
			return this;
		}
		return new CompilationOptions(this, null, null, null, null, null, null, allow, true);
	}

	public CompilationOptions WithCompilerFeatures(CompilerFeatures compilerFeatures)
	{
		if (CompilerFeatures == compilerFeatures)
		{
			return this;
		}
		return new CompilationOptions(this, null, null, null, null, null, null, null, true, compilerFeatures);
	}

	public CompilationOptions WithSupportedLocales(NavAppSupportedLocales supportedLocales)
	{
		if (SupportedLocales == supportedLocales)
		{
			return this;
		}
		return new CompilationOptions(this, null, null, null, null, null, null, null, true, null, null, null, supportedLocales);
	}

	public CompilationOptions WithGenerationOptions(CompilationGenerationOptions generateOptions)
	{
		if (CompilationGenerateOptions == generateOptions)
		{
			return this;
		}
		return new CompilationOptions(this, null, null, null, null, null, null, null, true, null, null, generateOptions);
	}

	public override bool Equals(object obj)
	{
		CompilationOptions compilationOptions = obj as CompilationOptions;
		if (compilationOptions == null)
		{
			return false;
		}
		return Equals(compilationOptions);
	}

	public bool Equals(CompilationOptions other)
	{
		if (other == null)
		{
			return false;
		}
		if (ConcurrentBuild == other.ConcurrentBuild && WarningLevel == other.WarningLevel && GeneralDiagnosticOption == other.GeneralDiagnosticOption && ReportSuppressedDiagnostics == other.ReportSuppressedDiagnostics && IdSpaces == other.IdSpaces && Target == other.Target && AllowExtensionAndTargetInSameModule == other.AllowExtensionAndTargetInSameModule && ReadonlyErrorsAsWarnings == other.ReadonlyErrorsAsWarnings && CompilerFeatures == other.CompilerFeatures && ShowMyCode == other.ShowMyCode && SpecificDiagnosticOptions.Keys.SequenceEqual(other.SpecificDiagnosticOptions.Keys, (string a, string b) => StringComparer.OrdinalIgnoreCase.Compare(a, b) == 0) && SupportedLocales == other.SupportedLocales && string.Equals(ContextSensitiveHelpUrl, other.ContextSensitiveHelpUrl, StringComparison.Ordinal) && CompilationGenerateOptions == other.CompilationGenerateOptions && CodeGenerationOptions == other.CodeGenerationOptions)
		{
			return ConcurrentEmit == other.ConcurrentEmit;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int h = Hash.Combine(ConcurrentBuild.GetHashCode(), WarningLevel, GeneralDiagnosticOption.GetHashCode(), ReportSuppressedDiagnostics.GetHashCode(), IdSpaces.GetHashCode(), (int)Target);
		int h2 = Hash.Combine(AllowExtensionAndTargetInSameModule.GetHashCode(), ReadonlyErrorsAsWarnings.GetHashCode(), CompilerFeatures.GetHashCode(), ShowMyCode.GetHashCode(), SpecificDiagnosticOptions.GetHashCode(), CompilationGenerateOptions.GetHashCode());
		return Hash.Combine(h, h2, SupportedLocales.GetHashCode(), CodeGenerationOptions.GetHashCode(), ConcurrentEmit.GetHashCode(), ContextSensitiveHelpUrl.GetHashCode());
	}

	public static bool operator ==(CompilationOptions first, CompilationOptions second)
	{
		if ((object)first == second)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Equals(second);
	}

	public static bool operator !=(CompilationOptions first, CompilationOptions second)
	{
		return !(first == second);
	}
}
