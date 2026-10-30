using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classifiers;

internal class NameSyntaxClassifier : AbstractSyntaxClassifier
{
	public override IEnumerable<Type> SyntaxNodeTypes
	{
		get
		{
			yield return typeof(NameSyntax);
		}
	}

	public override IEnumerable<ClassifiedSpan> ClassifyNode(SyntaxNode syntax, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		if (syntax is NameSyntax name)
		{
			return ClassifyTypeSyntax(name, semanticModel, cancellationToken);
		}
		return null;
	}

	private IEnumerable<ClassifiedSpan> ClassifyTypeSyntax(NameSyntax name, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(name, cancellationToken);
		if (TryClassifySymbol(name, symbolInfo, semanticModel, cancellationToken, out var result) || TryClassifyFromIdentifier(name, symbolInfo, out result))
		{
			return result;
		}
		return null;
	}

	private bool TryClassifySymbol(NameSyntax name, SymbolInfo symbolInfo, SemanticModel semanticModel, CancellationToken cancellationToken, out IEnumerable<ClassifiedSpan> result)
	{
		if (symbolInfo.CandidateReason == CandidateReason.Ambiguous)
		{
			return TryClassifyAmbiguousSymbol(name, symbolInfo, semanticModel, cancellationToken, out result);
		}
		ISymbol symbol = TryGetSymbol(name, symbolInfo, semanticModel);
		if (TryClassifySymbol(name, symbol, semanticModel, cancellationToken, out var classifiedSpan))
		{
			result = SpecializedCollections.SingletonEnumerable(classifiedSpan);
			return true;
		}
		result = null;
		return false;
	}

	private bool TryClassifyAmbiguousSymbol(NameSyntax name, SymbolInfo symbolInfo, SemanticModel semanticModel, CancellationToken cancellationToken, out IEnumerable<ClassifiedSpan> result)
	{
		HashSet<ClassifiedSpan> hashSet = new HashSet<ClassifiedSpan>();
		foreach (ISymbol candidateSymbol in symbolInfo.CandidateSymbols)
		{
			if (TryClassifySymbol(name, candidateSymbol, semanticModel, cancellationToken, out var classifiedSpan))
			{
				hashSet.Add(classifiedSpan);
			}
		}
		if (hashSet.Count == 1)
		{
			result = SpecializedCollections.SingletonEnumerable(hashSet.First());
			return true;
		}
		result = null;
		return false;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "symbol")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "semanticModel")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	private bool TryClassifySymbol(NameSyntax name, ISymbol symbol, SemanticModel semanticModel, CancellationToken cancellationToken, out ClassifiedSpan classifiedSpan)
	{
		classifiedSpan = default;
		return false;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "symbolInfo")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "semanticModel")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name")]
	private static ISymbol TryGetSymbol(NameSyntax name, SymbolInfo symbolInfo, SemanticModel semanticModel)
	{
		return null;
	}

	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "symbolInfo")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name")]
	private bool TryClassifyFromIdentifier(NameSyntax name, SymbolInfo symbolInfo, out IEnumerable<ClassifiedSpan> result)
	{
		result = new List<ClassifiedSpan>();
		return false;
	}
}
