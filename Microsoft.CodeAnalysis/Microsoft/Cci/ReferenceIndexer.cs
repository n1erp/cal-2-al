using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.Cci;

internal abstract class ReferenceIndexer : ReferenceIndexerBase
{
	protected readonly MetadataWriter metadataWriter;

	private readonly HashSet<IImportScope> _alreadySeenScopes = new HashSet<IImportScope>();

	internal ReferenceIndexer(MetadataWriter metadataWriter)
		: base(metadataWriter.Context)
	{
		this.metadataWriter = metadataWriter;
	}

	public override void Visit(CommonPEModuleBuilder module)
	{
		Visit(module.GetSourceAssemblyAttributes(Context.IsRefAssembly));
		Visit(module.GetSourceAssemblySecurityAttributes());
		Visit(module.GetAssemblyReferences(Context));
		Visit(module.GetSourceModuleAttributes());
		Visit(module.GetTopLevelTypes(Context));
		foreach (ExportedType exportedType in module.GetExportedTypes(Context.Diagnostics))
		{
			VisitExportedType(exportedType.Type);
		}
		Visit(module.GetResources(Context));
		VisitImports(module.GetImports());
		Visit(module.GetFiles(Context));
	}

	private void VisitExportedType(ITypeReference exportedType)
	{
		IUnitReference definingUnitReference = MetadataWriter.GetDefiningUnitReference(exportedType, Context);
		if (definingUnitReference is IAssemblyReference assemblyReference)
		{
			Visit(assemblyReference);
			return;
		}
		IAssemblyReference containingAssembly = ((IModuleReference)definingUnitReference).GetContainingAssembly(Context);
		if (containingAssembly != null && containingAssembly != Context.Module.GetContainingAssembly(Context))
		{
			Visit(containingAssembly);
		}
	}

	public void VisitMethodBodyReference(IReference reference)
	{
		if (reference is ITypeReference typeReference)
		{
			typeReferenceNeedsToken = true;
			Visit(typeReference);
		}
		else if (reference is IFieldReference fieldReference)
		{
			if (fieldReference.IsContextualNamedEntity)
			{
				((IContextualNamedEntity)fieldReference).AssociateWithMetadataWriter(metadataWriter);
			}
			Visit(fieldReference);
		}
		else if (reference is IMethodReference methodReference)
		{
			Visit(methodReference);
		}
	}

	protected override void RecordAssemblyReference(IAssemblyReference assemblyReference)
	{
		metadataWriter.GetAssemblyReferenceHandle(assemblyReference);
	}

	protected override void ProcessMethodBody(IMethodDefinition method)
	{
		if (!method.HasBody() || metadataWriter.MetadataOnly)
		{
			return;
		}
		IMethodBody body = method.GetBody(Context);
		if (body != null)
		{
			Visit(body);
			IImportScope importScope = body.ImportScope;
			while (importScope != null && _alreadySeenScopes.Add(importScope))
			{
				VisitImports(importScope.GetUsedNamespaces());
				importScope = importScope.Parent;
			}
		}
		else if (!metadataWriter.MetadataOnly)
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private void VisitImports(ImmutableArray<UsedNamespaceOrType> imports)
	{
		foreach (UsedNamespaceOrType item in imports)
		{
			if (item.TargetAssemblyOpt != null)
			{
				Visit(item.TargetAssemblyOpt);
			}
			if (item.TargetTypeOpt != null)
			{
				typeReferenceNeedsToken = true;
				Visit(item.TargetTypeOpt);
			}
		}
	}

	protected override void RecordTypeReference(ITypeReference typeReference)
	{
		metadataWriter.GetTypeHandle(typeReference);
	}

	protected override void RecordTypeMemberReference(ITypeMemberReference typeMemberReference)
	{
		metadataWriter.GetMemberReferenceHandle(typeMemberReference);
	}

	protected override void RecordFileReference(IFileReference fileReference)
	{
		metadataWriter.GetAssemblyFileHandle(fileReference);
	}

	protected override void ReserveMethodToken(IMethodReference methodReference)
	{
		metadataWriter.GetMethodHandle(methodReference);
	}

	protected override void ReserveFieldToken(IFieldReference fieldReference)
	{
		metadataWriter.GetFieldHandle(fieldReference);
	}

	protected override void RecordModuleReference(IModuleReference moduleReference)
	{
		metadataWriter.GetModuleReferenceHandle(moduleReference.Name);
	}

	public override void Visit(IPlatformInvokeInformation platformInvokeInformation)
	{
		metadataWriter.GetModuleReferenceHandle(platformInvokeInformation.ModuleName);
	}
}
