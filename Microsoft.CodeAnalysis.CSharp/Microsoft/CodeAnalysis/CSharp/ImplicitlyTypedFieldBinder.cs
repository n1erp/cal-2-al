using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ImplicitlyTypedFieldBinder : Binder
{
	private readonly ConsList<FieldSymbol> _fieldsBeingBound;

	internal override ConsList<FieldSymbol> FieldsBeingBound => _fieldsBeingBound;

	public ImplicitlyTypedFieldBinder(Binder next, ConsList<FieldSymbol> fieldsBeingBound)
		: base(next, next.Flags)
	{
		_fieldsBeingBound = fieldsBeingBound;
	}
}
