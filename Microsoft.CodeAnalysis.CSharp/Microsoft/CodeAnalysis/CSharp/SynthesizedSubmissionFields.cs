using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SynthesizedSubmissionFields
{
	private readonly NamedTypeSymbol _declaringSubmissionClass;

	private readonly CSharpCompilation _compilation;

	private FieldSymbol _hostObjectField;

	private Dictionary<ImplicitNamedTypeSymbol, FieldSymbol> _previousSubmissionFieldMap;

	internal int Count
	{
		get
		{
			if (_previousSubmissionFieldMap != null)
			{
				return _previousSubmissionFieldMap.Count;
			}
			return 0;
		}
	}

	internal IEnumerable<FieldSymbol> FieldSymbols
	{
		get
		{
			if (_previousSubmissionFieldMap != null)
			{
				return _previousSubmissionFieldMap.Values;
			}
			return Array.Empty<FieldSymbol>();
		}
	}

	public SynthesizedSubmissionFields(CSharpCompilation compilation, NamedTypeSymbol submissionClass)
	{
		_declaringSubmissionClass = submissionClass;
		_compilation = compilation;
	}

	internal FieldSymbol GetHostObjectField()
	{
		if ((object)_hostObjectField != null)
		{
			return _hostObjectField;
		}
		TypeSymbol hostObjectTypeSymbol = _compilation.GetHostObjectTypeSymbol();
		if ((object)hostObjectTypeSymbol != null && hostObjectTypeSymbol.Kind != SymbolKind.ErrorType)
		{
			return _hostObjectField = new SynthesizedFieldSymbol(_declaringSubmissionClass, hostObjectTypeSymbol, "<host-object>", isPublic: false, isReadOnly: true);
		}
		return null;
	}

	internal FieldSymbol GetOrMakeField(ImplicitNamedTypeSymbol previousSubmissionType)
	{
		if (_previousSubmissionFieldMap == null)
		{
			_previousSubmissionFieldMap = new Dictionary<ImplicitNamedTypeSymbol, FieldSymbol>();
		}
		if (!_previousSubmissionFieldMap.TryGetValue(previousSubmissionType, out var value))
		{
			value = new SynthesizedFieldSymbol(_declaringSubmissionClass, previousSubmissionType, "<" + previousSubmissionType.Name + ">", isPublic: false, isReadOnly: true);
			_previousSubmissionFieldMap.Add(previousSubmissionType, value);
		}
		return value;
	}

	internal void AddToType(NamedTypeSymbol containingType, PEModuleBuilder moduleBeingBuilt)
	{
		foreach (FieldSymbol fieldSymbol in FieldSymbols)
		{
			moduleBeingBuilt.AddSynthesizedDefinition(containingType, fieldSymbol);
		}
		FieldSymbol hostObjectField = GetHostObjectField();
		if ((object)hostObjectField != null)
		{
			moduleBeingBuilt.AddSynthesizedDefinition(containingType, hostObjectField);
		}
	}
}
