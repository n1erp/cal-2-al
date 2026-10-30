namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal struct FieldOrPropertyInitializer(FieldSymbol fieldOpt, SyntaxNode syntax, int precedingInitializersLength)
{
	internal readonly FieldSymbol FieldOpt = fieldOpt;

	internal readonly SyntaxReference Syntax = syntax.GetReference();

	internal readonly int PrecedingInitializersLength = precedingInitializersLength;
}
