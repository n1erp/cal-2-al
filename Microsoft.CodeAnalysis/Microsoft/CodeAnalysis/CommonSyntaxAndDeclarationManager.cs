using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal abstract class CommonSyntaxAndDeclarationManager
{
	internal readonly ImmutableArray<SyntaxTree> ExternalSyntaxTrees;

	internal readonly string ScriptClassName;

	internal readonly SourceReferenceResolver Resolver;

	internal readonly CommonMessageProvider MessageProvider;

	internal readonly bool IsSubmission;

	public CommonSyntaxAndDeclarationManager(ImmutableArray<SyntaxTree> externalSyntaxTrees, string scriptClassName, SourceReferenceResolver resolver, CommonMessageProvider messageProvider, bool isSubmission)
	{
		ExternalSyntaxTrees = externalSyntaxTrees;
		ScriptClassName = scriptClassName ?? "";
		Resolver = resolver;
		MessageProvider = messageProvider;
		IsSubmission = isSubmission;
	}
}
