using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IXmlPortNodeSymbol : ISymbol, IEquatable<ISymbol>
{
	XmlPortSourceTypeKind SourceTypeKind { get; }

	XmlPortNodeXmlKind NodeXmlKind { get; }
}
