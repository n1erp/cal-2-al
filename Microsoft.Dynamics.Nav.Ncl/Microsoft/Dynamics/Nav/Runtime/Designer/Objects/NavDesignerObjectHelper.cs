using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer.Objects;

internal static class NavDesignerObjectHelper
{
	internal static int GetObjectId(this ApplicationObjectSyntax applicationObjectSyntax)
	{
		return (int)(applicationObjectSyntax?.ObjectId?.Value.Value ?? ((object)0));
	}

	internal static string GetObjectName(this ObjectSyntax objectSyntax)
	{
		return objectSyntax?.Name?.Identifier.ValueText;
	}
}
