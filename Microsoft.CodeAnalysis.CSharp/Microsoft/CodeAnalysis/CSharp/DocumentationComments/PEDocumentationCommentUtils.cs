using System;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

namespace Microsoft.CodeAnalysis.CSharp.DocumentationComments;

internal static class PEDocumentationCommentUtils
{
	internal static string GetDocumentationComment(Symbol symbol, PEModuleSymbol containingPEModule, CultureInfo preferredCulture, CancellationToken cancellationToken, ref Tuple<CultureInfo, string> lazyDocComment)
	{
		if (lazyDocComment == null)
		{
			Interlocked.CompareExchange(ref lazyDocComment, Tuple.Create(preferredCulture, containingPEModule.DocumentationProvider.GetDocumentationForSymbol(symbol.GetDocumentationCommentId(), preferredCulture, cancellationToken)), null);
		}
		if (object.Equals(lazyDocComment.Item1, preferredCulture))
		{
			return lazyDocComment.Item2;
		}
		return containingPEModule.DocumentationProvider.GetDocumentationForSymbol(symbol.GetDocumentationCommentId(), preferredCulture, cancellationToken);
	}
}
