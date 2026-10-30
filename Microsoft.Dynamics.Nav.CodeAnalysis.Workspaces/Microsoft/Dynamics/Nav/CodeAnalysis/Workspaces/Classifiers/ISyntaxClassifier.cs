using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classifiers;

internal interface ISyntaxClassifier
{
	IEnumerable<Type> SyntaxNodeTypes { get; }

	IEnumerable<int> SyntaxTokenKinds { get; }

	IEnumerable<ClassifiedSpan> ClassifyNode(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken);

	IEnumerable<ClassifiedSpan> ClassifyToken(SyntaxToken token, SemanticModel semanticModel, CancellationToken cancellationToken);
}
