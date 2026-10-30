namespace Microsoft.CodeAnalysis.Debugging;

internal enum ImportTargetKind
{
	Namespace,
	Type,
	NamespaceOrType,
	Assembly,
	XmlNamespace,
	MethodToken,
	CurrentNamespace,
	DefaultNamespace,
	Defunct
}
