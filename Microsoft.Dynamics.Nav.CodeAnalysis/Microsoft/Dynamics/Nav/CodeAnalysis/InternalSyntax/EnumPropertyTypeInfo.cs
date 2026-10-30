using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal sealed class EnumPropertyTypeInfo : PropertyTypeInfo
{
	private readonly IDictionary<string, SymbolCompilationScope> optionValues = new Dictionary<string, SymbolCompilationScope>(StringComparer.OrdinalIgnoreCase);

	internal ImmutableArray<string> Options { get; private set; }

	internal SymbolCompilationScope GetScope(string optionValue)
	{
		if (!optionValues.TryGetValue(optionValue, out var value))
		{
			return SymbolCompilationScope.Personalization;
		}
		return value;
	}

	internal EnumPropertyTypeInfo(PropertyKind kind, string name, string typeName, string parserName, ParseFunc parseFunc, IList<string> dependentProperties = null, IList<string> dependentParentProperties = null, IList<string> dependentDeclaringApplicationObjectProperties = null, string options = null, string optionScopes = null, string defaultValue = null, bool isObsolete = false, bool generateMetadata = true, ModificationKind modification = ModificationKind.None, Func<string, string, PropertyValueSyntax, DiagnosticBag, bool> validator = null, string metadataName = null, Func<IEnumerable<string>> recommendationFunc = null, string compilationScope = null, bool emitAsAttribute = true, bool prependObjectName = false, string syntaxOwner = null, VersionCompatibility compatibility = null)
		: base(kind, name, typeName, parserName, parseFunc, dependentProperties, dependentParentProperties, dependentDeclaringApplicationObjectProperties, defaultValue, isObsolete, generateMetadata, modification, validator, metadataName, recommendationFunc, compilationScope, emitAsAttribute, null, prependObjectName, syntaxOwner, compatibility)
	{
		if (options == null)
		{
			return;
		}
		Options = options.Split(',').ToImmutableArray();
		IList<SymbolCompilationScope> list = null;
		if (optionScopes != null)
		{
			list = (from scopeString in optionScopes.Split(',')
				select (!Enum.TryParse<SymbolCompilationScope>(scopeString, out var result)) ? SymbolCompilationScope.Personalization : result).ToList();
		}
		bool flag = list?.Count == Options.Length;
		for (int num = 0; num < Options.Length; num++)
		{
			string key = Options[num];
			DebugAssertHelper.Assert(!optionValues.ContainsKey(key));
			SymbolCompilationScope value = ((!flag) ? SymbolCompilationScope.Personalization : list[num]);
			optionValues.Add(key, value);
		}
	}

	internal override string GetTypeDescription()
	{
		return "Option " + string.Join(",", optionValues.Keys);
	}
}
