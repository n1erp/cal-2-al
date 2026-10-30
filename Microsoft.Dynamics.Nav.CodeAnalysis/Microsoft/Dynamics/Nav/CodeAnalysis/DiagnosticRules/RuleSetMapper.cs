using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

internal class RuleSetMapper
{
	internal RuleSetLoader Loader { get; }

	internal RuleSetMapper(RuleSetLoader ruleSetLoader)
	{
		Loader = ruleSetLoader;
	}

	internal InternalRuleSet MapAndValidate(ExternalRuleSet ruleSet, string pathToRuleSet)
	{
		DebugAssertHelper.Assert(!string.IsNullOrEmpty(pathToRuleSet));
		DebugAssertHelper.Assert(ruleSet != null);
		return LoadAndValidate(ruleSet, pathToRuleSet, new HashSet<string> { pathToRuleSet });
	}

	private InternalRuleSet LoadAndValidate(ExternalRuleSet ruleSet, string pathToRuleSet, HashSet<string> pathsToIncludedRuleSets)
	{
		return new InternalRuleSet
		{
			Name = GetNonEmptyProperty(ruleSet.Name, "Name"),
			Description = ruleSet.Description,
			GeneralAction = GetAction(ruleSet.GeneralAction),
			IncludedRuleSets = GetIncludedRuleSets(pathToRuleSet, pathsToIncludedRuleSets, ruleSet.IncludedRuleSets),
			SpecificActions = GetSpecificRules(ruleSet.Rules)
		};
	}

	private ImmutableDictionary<string, ReportDiagnostic> GetSpecificRules(ExternalRule[] rules)
	{
		if (rules == null)
		{
			return ImmutableDictionary<string, ReportDiagnostic>.Empty;
		}
		PooledDictionary<string, ReportDiagnostic> instance = PooledDictionary<string, ReportDiagnostic>.GetInstance();
		foreach (ExternalRule externalRule in rules)
		{
			string nonEmptyProperty = GetNonEmptyProperty(externalRule.Id, "Id");
			ReportDiagnostic action = GetAction<RuleAction>(externalRule.Action);
			if (instance.TryGetValue(nonEmptyProperty, out var value))
			{
				if (value != action)
				{
					throw new InvalidRuleSetException(string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.ERR_RuleSetHasDuplicateRules, nonEmptyProperty, value, action));
				}
			}
			else
			{
				instance.Add(nonEmptyProperty, action);
			}
		}
		return instance.ToImmutableDictionaryAndFree();
	}

	private ReportDiagnostic GetAction<T>(T? action) where T : struct
	{
		if (!action.HasValue)
		{
			return ReportDiagnostic.Default;
		}
		return action.ToString() switch
		{
			"Error" => ReportDiagnostic.Error, 
			"Warning" => ReportDiagnostic.Warn, 
			"Info" => ReportDiagnostic.Info, 
			"Hidden" => ReportDiagnostic.Hidden, 
			"None" => ReportDiagnostic.Suppress, 
			"Default" => ReportDiagnostic.Default, 
			_ => throw ExceptionUtilities.UnexpectedValue(action), 
		};
	}

	private string GetNonEmptyProperty(string value, string propertyName)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new InvalidRuleSetException(string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.ERR_RuleSetMissingProperty, propertyName));
		}
		return value;
	}

	private ImmutableArray<InternalIncludedRuleSet> GetIncludedRuleSets(string pathToParentRuleSet, HashSet<string> pathsToIncludedRuleSets, ExternalIncludedRuleSet[] includedRuleSet)
	{
		if (includedRuleSet == null)
		{
			return ImmutableArray<InternalIncludedRuleSet>.Empty;
		}
		ArrayBuilder<InternalIncludedRuleSet> instance = ArrayBuilder<InternalIncludedRuleSet>.GetInstance();
		foreach (ExternalIncludedRuleSet externalIncludedRuleSet in includedRuleSet)
		{
			ReportDiagnostic action = GetAction<IncludeAction>(externalIncludedRuleSet.Action);
			if (action != ReportDiagnostic.Suppress)
			{
				string text = Loader.ResolvePath(pathToParentRuleSet, externalIncludedRuleSet.Path);
				if (!pathsToIncludedRuleSets.Contains(text))
				{
					pathsToIncludedRuleSets.Add(text);
					instance.Add(new InternalIncludedRuleSet
					{
						Action = action,
						RuleSet = GetIncludedRuleSet(text, pathsToIncludedRuleSets)
					});
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	private InternalRuleSet GetIncludedRuleSet(string pathToIncludedRuleSet, HashSet<string> pathsToIncludedRuleSets)
	{
		ExternalRuleSet ruleSet = Loader.LoadRuleSet(pathToIncludedRuleSet);
		return LoadAndValidate(ruleSet, pathToIncludedRuleSet, pathsToIncludedRuleSets);
	}
}
