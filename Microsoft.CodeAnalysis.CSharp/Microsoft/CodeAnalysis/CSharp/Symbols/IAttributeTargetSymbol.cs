namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal interface IAttributeTargetSymbol
{
	IAttributeTargetSymbol AttributesOwner { get; }

	AttributeLocation AllowedAttributeLocations { get; }

	AttributeLocation DefaultAttributeLocation { get; }
}
