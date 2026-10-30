using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal delegate InternalPropertyValueSyntax ParseFunc(ObjectParser op, PropertyTypeInfo pti, ref InternalSyntaxToken eT);
