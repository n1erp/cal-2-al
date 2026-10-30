namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TupleParameterSymbol : WrappedParameterSymbol
{
	private readonly Symbol _container;

	public override Symbol ContainingSymbol => _container;

	public TupleParameterSymbol(Symbol container, ParameterSymbol underlyingParameter)
		: base(underlyingParameter)
	{
		_container = container;
	}

	public override int GetHashCode()
	{
		return _underlyingParameter.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as TupleParameterSymbol);
	}

	public bool Equals(TupleParameterSymbol other)
	{
		if ((object)other == this)
		{
			return true;
		}
		if ((object)other != null && _container == other._container)
		{
			return _underlyingParameter == other._underlyingParameter;
		}
		return false;
	}
}
