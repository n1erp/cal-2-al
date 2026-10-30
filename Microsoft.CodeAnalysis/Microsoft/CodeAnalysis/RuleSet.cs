using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Microsoft.CodeAnalysis;

public class RuleSet
{
	private readonly string _filePath;

	private readonly ReportDiagnostic _generalDiagnosticOption;

	private readonly ImmutableDictionary<string, ReportDiagnostic> _specificDiagnosticOptions;

	private readonly ImmutableArray<RuleSetInclude> _includes;

	public string FilePath => _filePath;

	public ReportDiagnostic GeneralDiagnosticOption => _generalDiagnosticOption;

	public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions => _specificDiagnosticOptions;

	public ImmutableArray<RuleSetInclude> Includes => _includes;

	public RuleSet(string filePath, ReportDiagnostic generalOption, ImmutableDictionary<string, ReportDiagnostic> specificOptions, ImmutableArray<RuleSetInclude> includes)
	{
		_filePath = filePath;
		_generalDiagnosticOption = generalOption;
		_specificDiagnosticOptions = ((specificOptions == null) ? ImmutableDictionary<string, ReportDiagnostic>.Empty : specificOptions);
		_includes = includes.NullToEmpty();
	}

	public RuleSet WithEffectiveAction(ReportDiagnostic action)
	{
		if (!_includes.IsEmpty)
		{
			throw new ArgumentException("Effective action cannot be applied to rulesets with Includes");
		}
		switch (action)
		{
		case ReportDiagnostic.Default:
			return this;
		case ReportDiagnostic.Suppress:
			return null;
		case ReportDiagnostic.Error:
		case ReportDiagnostic.Warn:
		case ReportDiagnostic.Info:
		case ReportDiagnostic.Hidden:
		{
			ReportDiagnostic generalOption = ((_generalDiagnosticOption != ReportDiagnostic.Default) ? action : ReportDiagnostic.Default);
			ImmutableDictionary<string, ReportDiagnostic>.Builder builder = _specificDiagnosticOptions.ToBuilder();
			foreach (KeyValuePair<string, ReportDiagnostic> specificDiagnosticOption in _specificDiagnosticOptions)
			{
				if (specificDiagnosticOption.Value != ReportDiagnostic.Suppress && specificDiagnosticOption.Value != ReportDiagnostic.Default)
				{
					builder[specificDiagnosticOption.Key] = action;
				}
			}
			return new RuleSet(FilePath, generalOption, builder.ToImmutable(), _includes);
		}
		default:
			return null;
		}
	}

	private RuleSet GetEffectiveRuleSet(HashSet<string> includedRulesetPaths)
	{
		ReportDiagnostic generalDiagnosticOption = _generalDiagnosticOption;
		Dictionary<string, ReportDiagnostic> dictionary = new Dictionary<string, ReportDiagnostic>();
		if (_includes.IsEmpty)
		{
			return this;
		}
		foreach (RuleSetInclude include in _includes)
		{
			if (include.Action == ReportDiagnostic.Suppress)
			{
				continue;
			}
			RuleSet ruleSet = include.LoadRuleSet(this);
			if (ruleSet == null || includedRulesetPaths.Contains(ruleSet.FilePath.ToLowerInvariant()))
			{
				continue;
			}
			includedRulesetPaths.Add(ruleSet.FilePath.ToLowerInvariant());
			RuleSet effectiveRuleSet = ruleSet.GetEffectiveRuleSet(includedRulesetPaths);
			effectiveRuleSet = effectiveRuleSet.WithEffectiveAction(include.Action);
			if (IsStricterThan(effectiveRuleSet.GeneralDiagnosticOption, generalDiagnosticOption))
			{
				generalDiagnosticOption = effectiveRuleSet.GeneralDiagnosticOption;
			}
			foreach (KeyValuePair<string, ReportDiagnostic> specificDiagnosticOption in effectiveRuleSet.SpecificDiagnosticOptions)
			{
				if (dictionary.TryGetValue(specificDiagnosticOption.Key, out var value))
				{
					if (IsStricterThan(specificDiagnosticOption.Value, value))
					{
						dictionary[specificDiagnosticOption.Key] = specificDiagnosticOption.Value;
					}
				}
				else
				{
					dictionary.Add(specificDiagnosticOption.Key, specificDiagnosticOption.Value);
				}
			}
		}
		foreach (KeyValuePair<string, ReportDiagnostic> specificDiagnosticOption2 in _specificDiagnosticOptions)
		{
			if (dictionary.ContainsKey(specificDiagnosticOption2.Key))
			{
				dictionary[specificDiagnosticOption2.Key] = specificDiagnosticOption2.Value;
			}
			else
			{
				dictionary.Add(specificDiagnosticOption2.Key, specificDiagnosticOption2.Value);
			}
		}
		return new RuleSet(_filePath, generalDiagnosticOption, dictionary.ToImmutableDictionary(), ImmutableArray<RuleSetInclude>.Empty);
	}

	private ImmutableArray<string> GetEffectiveIncludes()
	{
		ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>();
		GetEffectiveIncludesCore(builder);
		return builder.ToImmutable();
	}

	private void GetEffectiveIncludesCore(ImmutableArray<string>.Builder arrayBuilder)
	{
		arrayBuilder.Add(FilePath);
		foreach (RuleSetInclude include in _includes)
		{
			RuleSet ruleSet = include.LoadRuleSet(this);
			if (ruleSet != null && !arrayBuilder.Contains(ruleSet.FilePath, StringComparer.OrdinalIgnoreCase))
			{
				ruleSet.GetEffectiveIncludesCore(arrayBuilder);
			}
		}
	}

	private static bool IsStricterThan(ReportDiagnostic action1, ReportDiagnostic action2)
	{
		switch (action2)
		{
		case ReportDiagnostic.Suppress:
			return true;
		case ReportDiagnostic.Default:
			if (action1 != ReportDiagnostic.Warn && action1 != ReportDiagnostic.Error && action1 != ReportDiagnostic.Info)
			{
				return action1 == ReportDiagnostic.Hidden;
			}
			return true;
		case ReportDiagnostic.Hidden:
			if (action1 != ReportDiagnostic.Warn && action1 != ReportDiagnostic.Error)
			{
				return action1 == ReportDiagnostic.Info;
			}
			return true;
		case ReportDiagnostic.Info:
			if (action1 != ReportDiagnostic.Warn)
			{
				return action1 == ReportDiagnostic.Error;
			}
			return true;
		case ReportDiagnostic.Warn:
			return action1 == ReportDiagnostic.Error;
		case ReportDiagnostic.Error:
			return false;
		default:
			return false;
		}
	}

	public static RuleSet LoadEffectiveRuleSetFromFile(string filePath)
	{
		return RuleSetProcessor.LoadFromFile(filePath)?.GetEffectiveRuleSet(new HashSet<string>());
	}

	public static ImmutableArray<string> GetEffectiveIncludesFromFile(string filePath)
	{
		return RuleSetProcessor.LoadFromFile(filePath)?.GetEffectiveIncludes() ?? ImmutableArray<string>.Empty;
	}

	public static ReportDiagnostic GetDiagnosticOptionsFromRulesetFile(string rulesetFileFullPath, out Dictionary<string, ReportDiagnostic> specificDiagnosticOptions)
	{
		return GetDiagnosticOptionsFromRulesetFile(rulesetFileFullPath, out specificDiagnosticOptions, null, null);
	}

	internal static ReportDiagnostic GetDiagnosticOptionsFromRulesetFile(string rulesetFileFullPath, out Dictionary<string, ReportDiagnostic> diagnosticOptions, IList<Diagnostic> diagnosticsOpt, CommonMessageProvider messageProviderOpt)
	{
		diagnosticOptions = new Dictionary<string, ReportDiagnostic>();
		if (rulesetFileFullPath == null)
		{
			return ReportDiagnostic.Default;
		}
		return GetDiagnosticOptionsFromRulesetFile(diagnosticOptions, rulesetFileFullPath, diagnosticsOpt, messageProviderOpt);
	}

	private static ReportDiagnostic GetDiagnosticOptionsFromRulesetFile(Dictionary<string, ReportDiagnostic> diagnosticOptions, string resolvedPath, IList<Diagnostic> diagnosticsOpt, CommonMessageProvider messageProviderOpt)
	{
		ReportDiagnostic result = ReportDiagnostic.Default;
		try
		{
			RuleSet ruleSet = LoadEffectiveRuleSetFromFile(resolvedPath);
			result = ruleSet.GeneralDiagnosticOption;
			foreach (KeyValuePair<string, ReportDiagnostic> specificDiagnosticOption in ruleSet.SpecificDiagnosticOptions)
			{
				diagnosticOptions.Add(specificDiagnosticOption.Key, specificDiagnosticOption.Value);
			}
		}
		catch (InvalidRuleSetException ex)
		{
			if (diagnosticsOpt != null && messageProviderOpt != null)
			{
				diagnosticsOpt.Add(Diagnostic.Create(messageProviderOpt, messageProviderOpt.ERR_CantReadRulesetFile, resolvedPath, ex.Message));
			}
		}
		catch (IOException ex2)
		{
			if (ex2 is FileNotFoundException || ((object)ex2).GetType().Name == "DirectoryNotFoundException")
			{
				if (diagnosticsOpt != null && messageProviderOpt != null)
				{
					diagnosticsOpt.Add(Diagnostic.Create(messageProviderOpt, messageProviderOpt.ERR_CantReadRulesetFile, resolvedPath, new CodeAnalysisResourcesLocalizableErrorArgument("FileNotFound")));
				}
			}
			else if (diagnosticsOpt != null && messageProviderOpt != null)
			{
				diagnosticsOpt.Add(Diagnostic.Create(messageProviderOpt, messageProviderOpt.ERR_CantReadRulesetFile, resolvedPath, ex2.Message));
			}
		}
		return result;
	}
}
