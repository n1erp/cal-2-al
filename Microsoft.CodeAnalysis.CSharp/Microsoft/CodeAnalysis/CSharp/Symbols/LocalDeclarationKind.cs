namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal enum LocalDeclarationKind : byte
{
	None,
	RegularVariable,
	Constant,
	FixedVariable,
	UsingVariable,
	CatchVariable,
	ForEachIterationVariable,
	PatternVariable,
	DeconstructionVariable,
	OutVariable,
	DeclarationExpressionVariable
}
