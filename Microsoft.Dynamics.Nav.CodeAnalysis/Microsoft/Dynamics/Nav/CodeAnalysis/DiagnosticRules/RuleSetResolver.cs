using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

internal class RuleSetResolver
{
	private readonly RuleSetMapper mapper;

	private readonly RuleSetReducer reducer;

	public static RuleSet DefaultRuleSet { get; } = new RuleSet(ReportDiagnostic.Default, ImmutableDictionary<string, ReportDiagnostic>.Empty);

	internal RuleSetResolver(RuleSetMapper mapper, RuleSetReducer reducer)
	{
		this.mapper = mapper;
		this.reducer = reducer;
	}

	public static RuleSet LoadFromFile(string pathToRootRuleSet, IList<Diagnostic> diagnostics)
	{
		return new RuleSetResolver(new RuleSetMapper(new RuleSetLoader()), new RuleSetReducer()).LoadFromFileCore(pathToRootRuleSet, diagnostics);
	}

	internal RuleSet LoadFromFileCore(string pathToRootRuleSet, IList<Diagnostic> diagnostics)
	{
		try
		{
			string text = mapper.Loader.ResolvePath(null, pathToRootRuleSet);
			ExternalRuleSet externalRuleSet = mapper.Loader.LoadRuleSet(text);
			if (externalRuleSet == null)
			{
				return DefaultRuleSet;
			}
			InternalRuleSet rootRuleSet = mapper.MapAndValidate(externalRuleSet, text);
			return reducer.Reduce(rootRuleSet);
		}
		catch (Exception ex) when (ex is IOException || ex is InvalidRuleSetException)
		{
			diagnostics?.Add(Diagnostic.Create(MessageProvider.Instance, 1033, pathToRootRuleSet, ex.Message));
		}
		return DefaultRuleSet;
	}
}
