using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IModuleReference : IUnitReference, IReference, INamedEntity
{
	IAssemblyReference GetContainingAssembly(EmitContext context);
}
