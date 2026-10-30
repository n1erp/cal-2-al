using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

internal class RuleSetReducer
{
	internal RuleSet Reduce(InternalRuleSet rootRuleSet)
	{
		return GetEffectiveRuleSet(rootRuleSet);
	}

	private static RuleSet GetEffectiveRuleSet(InternalRuleSet rootRuleSet)
	{
		ReportDiagnostic reportDiagnostic = rootRuleSet.GeneralAction;
		Dictionary<string, ReportDiagnostic> dictionary = new Dictionary<string, ReportDiagnostic>();
		if (rootRuleSet.IncludedRuleSets.IsDefaultOrEmpty)
		{
			return new RuleSet(rootRuleSet.GeneralAction, rootRuleSet.SpecificActions);
		}
		foreach (InternalIncludedRuleSet includedRuleSet in rootRuleSet.IncludedRuleSets)
		{
			if (includedRuleSet.Action == ReportDiagnostic.Suppress)
			{
				continue;
			}
			RuleSet effectiveRuleSet = GetEffectiveRuleSet(includedRuleSet.RuleSet);
			effectiveRuleSet = ApplyEffectiveAction(effectiveRuleSet, includedRuleSet.Action);
			if (effectiveRuleSet == null)
			{
				continue;
			}
			if (IsStricterThan(effectiveRuleSet.GeneralDiagnosticOption, reportDiagnostic))
			{
				reportDiagnostic = effectiveRuleSet.GeneralDiagnosticOption;
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
		foreach (KeyValuePair<string, ReportDiagnostic> specificAction in rootRuleSet.SpecificActions)
		{
			dictionary[specificAction.Key] = specificAction.Value;
		}
		return new RuleSet(reportDiagnostic, dictionary);
	}

	private static RuleSet ApplyEffectiveAction(RuleSet ruleSet, ReportDiagnostic action)
	{
		switch (action)
		{
		case ReportDiagnostic.Default:
			return ruleSet;
		case ReportDiagnostic.Error:
		case ReportDiagnostic.Warn:
		case ReportDiagnostic.Info:
		case ReportDiagnostic.Hidden:
		{
			ReportDiagnostic generalOption = ((ruleSet.GeneralDiagnosticOption != ReportDiagnostic.Default) ? action : ReportDiagnostic.Default);
			ImmutableDictionary<string, ReportDiagnostic>.Builder builder = ruleSet.SpecificDiagnosticOptions.ToBuilder();
			foreach (KeyValuePair<string, ReportDiagnostic> specificDiagnosticOption in ruleSet.SpecificDiagnosticOptions)
			{
				if (specificDiagnosticOption.Value != ReportDiagnostic.Suppress && specificDiagnosticOption.Value != ReportDiagnostic.Default)
				{
					builder[specificDiagnosticOption.Key] = action;
				}
			}
			return new RuleSet(generalOption, builder);
		}
		default:
			return null;
		}
	}

	private static bool IsStricterThan(ReportDiagnostic action1, ReportDiagnostic action2)
	{
		switch (action1)
		{
		case ReportDiagnostic.Error:
			return true;
		case ReportDiagnostic.Warn:
			if (action2 != ReportDiagnostic.Suppress && action2 != ReportDiagnostic.Default && action2 != ReportDiagnostic.Hidden)
			{
				return action2 == ReportDiagnostic.Info;
			}
			return true;
		case ReportDiagnostic.Info:
			if (action2 != ReportDiagnostic.Suppress && action2 != ReportDiagnostic.Default)
			{
				return action2 == ReportDiagnostic.Hidden;
			}
			return true;
		case ReportDiagnostic.Hidden:
			if (action2 != ReportDiagnostic.Suppress)
			{
				return action2 == ReportDiagnostic.Default;
			}
			return true;
		case ReportDiagnostic.Default:
			return action2 == ReportDiagnostic.Suppress;
		case ReportDiagnostic.Suppress:
			return false;
		default:
			return false;
		}
	}
}
