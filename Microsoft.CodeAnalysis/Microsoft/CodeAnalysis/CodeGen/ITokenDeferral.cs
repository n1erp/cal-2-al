using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal interface ITokenDeferral
{
	ArrayMethods ArrayMethods { get; }

	uint GetFakeStringTokenForIL(string value);

	uint GetFakeSymbolTokenForIL(IReference value, SyntaxNode syntaxNode, DiagnosticBag diagnostics);

	uint GetSourceDocumentIndexForIL(DebugSourceDocument document);

	IFieldReference GetFieldForData(ImmutableArray<byte> data, SyntaxNode syntaxNode, DiagnosticBag diagnostics);

	IMethodReference GetInitArrayHelper();

	string GetStringFromToken(uint token);

	IReference GetReferenceFromToken(uint token);
}
