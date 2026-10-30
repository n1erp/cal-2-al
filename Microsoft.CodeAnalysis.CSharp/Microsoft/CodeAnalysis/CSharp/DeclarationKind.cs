namespace Microsoft.CodeAnalysis.CSharp;

internal enum DeclarationKind : byte
{
	Namespace,
	Class,
	Interface,
	Struct,
	Enum,
	Delegate,
	Script,
	Submission,
	ImplicitClass
}
