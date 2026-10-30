using System.Collections.Generic;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IReference
{
	IEnumerable<ICustomAttribute> GetAttributes(EmitContext context);

	void Dispatch(MetadataVisitor visitor);

	IDefinition AsDefinition(EmitContext context);
}
