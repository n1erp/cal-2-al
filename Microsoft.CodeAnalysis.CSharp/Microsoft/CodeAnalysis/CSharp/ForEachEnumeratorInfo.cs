using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ForEachEnumeratorInfo
{
	internal struct Builder
	{
		public TypeSymbol CollectionType;

		public TypeSymbol ElementType;

		public MethodSymbol GetEnumeratorMethod;

		public MethodSymbol CurrentPropertyGetter;

		public MethodSymbol MoveNextMethod;

		public bool NeedsDisposeMethod;

		public Conversion CollectionConversion;

		public Conversion CurrentConversion;

		public Conversion EnumeratorConversion;

		public ForEachEnumeratorInfo Build(BinderFlags location)
		{
			return new ForEachEnumeratorInfo(CollectionType, ElementType, GetEnumeratorMethod, CurrentPropertyGetter, MoveNextMethod, NeedsDisposeMethod, CollectionConversion, CurrentConversion, EnumeratorConversion, location);
		}
	}

	public readonly TypeSymbol CollectionType;

	public readonly TypeSymbol ElementType;

	public readonly MethodSymbol GetEnumeratorMethod;

	public readonly MethodSymbol CurrentPropertyGetter;

	public readonly MethodSymbol MoveNextMethod;

	public readonly bool NeedsDisposeMethod;

	public readonly Conversion CollectionConversion;

	public readonly Conversion CurrentConversion;

	public readonly Conversion EnumeratorConversion;

	public readonly BinderFlags Location;

	private ForEachEnumeratorInfo(TypeSymbol collectionType, TypeSymbol elementType, MethodSymbol getEnumeratorMethod, MethodSymbol currentPropertyGetter, MethodSymbol moveNextMethod, bool needsDisposeMethod, Conversion collectionConversion, Conversion currentConversion, Conversion enumeratorConversion, BinderFlags location)
	{
		CollectionType = collectionType;
		ElementType = elementType;
		GetEnumeratorMethod = getEnumeratorMethod;
		CurrentPropertyGetter = currentPropertyGetter;
		MoveNextMethod = moveNextMethod;
		NeedsDisposeMethod = needsDisposeMethod;
		CollectionConversion = collectionConversion;
		CurrentConversion = currentConversion;
		EnumeratorConversion = enumeratorConversion;
		Location = location;
	}
}
