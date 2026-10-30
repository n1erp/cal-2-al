namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IXmlPortNodeAccess : IOperation
{
	IOperation Instance { get; }

	IXmlPortNodeSymbol XmlPortNodeSymbol { get; }
}
