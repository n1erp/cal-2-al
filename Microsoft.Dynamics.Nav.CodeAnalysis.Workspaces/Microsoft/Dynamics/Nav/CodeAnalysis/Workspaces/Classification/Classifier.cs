using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classifiers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

public static class Classifier
{
	[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
	public static async Task<IEnumerable<ClassifiedSpan>> GetClassifiedSpansAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetClassifiedSpans(await document.GetSemanticModelForSpanAsync(textSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), textSpan, document.Project.Solution.Workspace, cancellationToken);
	}

	public static IEnumerable<ClassifiedSpan> GetClassifiedSpans(SemanticModel semanticModel, TextSpan textSpan, Workspace workspace, CancellationToken cancellationToken = default(CancellationToken))
	{
		IClassificationService service = workspace.Services.GetLanguageServices("AL").GetService<IClassificationService>();
		List<ISyntaxClassifier> syntaxClassifiers = service.GetDefaultSyntaxClassifiers().ToList();
		Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers = CreateNodeGetter(syntaxClassifiers);
		Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers = CreateTokenGetter(syntaxClassifiers);
		List<ClassifiedSpan> list = new List<ClassifiedSpan>();
		List<ClassifiedSpan> list2 = new List<ClassifiedSpan>();
		service.AddSyntacticClassifications(semanticModel.SyntaxTree, textSpan, list, cancellationToken);
		service.AddSemanticClassifications(semanticModel, textSpan, workspace, getNodeClassifiers, getTokenClassifiers, list2, cancellationToken);
		List<ClassifiedSpan> list3 = new List<ClassifiedSpan>(list2.Where((ClassifiedSpan s) => s.TextSpan.OverlapsWith(textSpan)));
		ISet<TextSpan> semanticSet = list2.Select((ClassifiedSpan s) => s.TextSpan).ToSet();
		list3.AddRange(list.Where((ClassifiedSpan s) => s.TextSpan.OverlapsWith(textSpan) && !semanticSet.Contains(s.TextSpan)));
		list3.Sort((ClassifiedSpan s1, ClassifiedSpan s2) => s1.TextSpan.Start - s2.TextSpan.Start);
		return list3;
	}

	private static Func<SyntaxNode, List<ISyntaxClassifier>> CreateNodeGetter(IEnumerable<ISyntaxClassifier> syntaxClassifiers)
	{
		Func<Type, List<ISyntaxClassifier>> getter = (Type t1) => (from e in syntaxClassifiers
			let types = e.SyntaxNodeTypes
			where types != null
			where !types.Any() || types.Any((Type type) => t1 == type || t1.GetTypeInfo().IsSubclassOf(type))
			select e).ToList();
		return (SyntaxNode n) => getter(n.GetType());
	}

	private static Func<SyntaxToken, List<ISyntaxClassifier>> CreateTokenGetter(IEnumerable<ISyntaxClassifier> syntaxClassifiers)
	{
		Func<int, List<ISyntaxClassifier>> getter = (int k) => (from e in syntaxClassifiers
			let kinds = e.SyntaxTokenKinds
			where kinds != null
			where !kinds.Any() || kinds.Contains(k)
			select e).ToList();
		return (SyntaxToken n) => getter((int)n.Kind);
	}
}
