namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class EventSymbolExtensions
{
	internal static MethodSymbol GetOwnOrInheritedAccessor(this EventSymbol @event, bool isAdder)
	{
		if (!isAdder)
		{
			return @event.GetOwnOrInheritedRemoveMethod();
		}
		return @event.GetOwnOrInheritedAddMethod();
	}
}
