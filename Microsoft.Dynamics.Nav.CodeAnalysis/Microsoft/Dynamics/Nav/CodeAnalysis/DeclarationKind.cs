namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal enum DeclarationKind : byte
{
	ObjectCollection,
	Codeunit,
	Table,
	TableExtension,
	Page,
	PageExtension,
	XmlPort,
	Report,
	Query,
	Profile,
	PageCustomization,
	ControlAddIn,
	DotNetPackage,
	EnumType,
	EnumExtensionType
}
