using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal sealed class NoPiaReferenceIndexer : ReferenceIndexerBase
{
	internal NoPiaReferenceIndexer(EmitContext context)
		: base(context)
	{
	}

	public override void Visit(CommonPEModuleBuilder module)
	{
		Visit(module.GetSourceAssemblyAttributes(Context.IsRefAssembly));
		Visit(module.GetSourceAssemblySecurityAttributes());
		Visit(module.GetSourceModuleAttributes());
	}

	protected override void RecordAssemblyReference(IAssemblyReference assemblyReference)
	{
	}

	protected override void RecordFileReference(IFileReference fileReference)
	{
	}

	protected override void RecordModuleReference(IModuleReference moduleReference)
	{
	}

	public override void Visit(IPlatformInvokeInformation platformInvokeInformation)
	{
	}

	protected override void ProcessMethodBody(IMethodDefinition method)
	{
	}

	protected override void RecordTypeReference(ITypeReference typeReference)
	{
	}

	protected override void ReserveFieldToken(IFieldReference fieldReference)
	{
	}

	protected override void ReserveMethodToken(IMethodReference methodReference)
	{
	}

	protected override void RecordTypeMemberReference(ITypeMemberReference typeMemberReference)
	{
	}
}
