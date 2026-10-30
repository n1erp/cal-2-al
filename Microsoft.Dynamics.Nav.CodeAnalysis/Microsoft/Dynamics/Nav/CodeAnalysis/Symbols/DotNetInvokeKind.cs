namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal enum DotNetInvokeKind
{
	InvokeMethod = 1,
	InvokePropertyGet = 2,
	InvokePropertySet = 4,
	InvokePropertyGetOrSet = 8,
	InvokeConstructor = 0x10,
	InvokeEnumerator = 0x20
}
