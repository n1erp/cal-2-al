using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classifiers;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

internal static class SyntaxClassifier
{
	public static readonly IEnumerable<ISyntaxClassifier> DefaultSyntaxClassifiers = ImmutableArray.Create((ISyntaxClassifier)new NameSyntaxClassifier());
}
