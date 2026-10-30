using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classifiers;

internal abstract class AbstractSyntaxClassifier : ISyntaxClassifier
{
	public virtual IEnumerable<Type> SyntaxNodeTypes => null;

	public virtual IEnumerable<int> SyntaxTokenKinds => null;

	protected static string GetClassificationForType(ITypeSymbol type)
	{
		return type.GetClassification();
	}

	public virtual IEnumerable<ClassifiedSpan> ClassifyNode(SyntaxNode syntax, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		return null;
	}

	public virtual IEnumerable<ClassifiedSpan> ClassifyToken(SyntaxToken syntax, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		return null;
	}
}
