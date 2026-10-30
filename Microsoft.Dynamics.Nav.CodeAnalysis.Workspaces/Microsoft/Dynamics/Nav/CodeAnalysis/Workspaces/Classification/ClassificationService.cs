using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classifiers;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

internal class ClassificationService : AbstractClassificationService
{
	public override IEnumerable<ISyntaxClassifier> GetDefaultSyntaxClassifiers()
	{
		return SyntaxClassifier.DefaultSyntaxClassifiers;
	}

	public override void AddLexicalClassifications(SourceText text, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken)
	{
		ClassificationHelpers.AddLexicalClassifications(text, textSpan, result, cancellationToken);
	}

	public override void AddSyntacticClassifications(SyntaxTree syntaxTree, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken)
	{
		Worker.CollectClassifiedSpans(syntaxTree.GetRoot(cancellationToken), textSpan, result, cancellationToken);
	}

	public override ClassifiedSpan FixClassification(SourceText rawText, ClassifiedSpan classifiedSpan)
	{
		return classifiedSpan;
	}
}
