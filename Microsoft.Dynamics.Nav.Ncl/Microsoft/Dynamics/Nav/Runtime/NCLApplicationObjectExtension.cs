using System;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NCLApplicationObjectExtension : NCLMetaApplicationObject
{
	private readonly NavAppRuntimeMetadata runtimeAppMetadata;

	public int RuntimeAppId => OwningApp.RuntimeAppId;

	protected NavAppObjectMetadataRuntimeDeltas Deltas { get; set; }

	protected NCLApplicationObjectExtension(INCLMetaApplicationObjectLoader loader, ObjectType objectType, int objectId, NavAppRuntimeMetadata runtimeAppMetadata)
		: base(loader, objectType, objectId, NavAppGroup.ExtensionGroup, -1)
	{
		this.runtimeAppMetadata = runtimeAppMetadata ?? throw new ArgumentNullException("runtimeAppMetadata");
	}

	protected override NavAppRuntimeMetadata ResolveOwningApp()
	{
		return runtimeAppMetadata;
	}
}
public abstract class NCLApplicationObjectExtension<TSubject> : NCLApplicationObjectExtension, INCLApplicationObject<TSubject>
{
	protected NCLApplicationObjectExtension(INCLMetaApplicationObjectLoader loader, ObjectType objectType, int objectId, NavAppRuntimeMetadata runtimeAppMetadata)
		: base(loader, objectType, objectId, runtimeAppMetadata)
	{
	}

	protected override void LoadMetadata()
	{
		base.LoadMetadata();
		NavAppObjectMetadataTimestampRecord<NavAppObjectMetadataRuntimeDeltas> extensionDeltasForAppObject = ObjectLoader.XmlMetadataLoader.GetExtensionDeltasForAppObject(ApplicationObjectId, OwningApp);
		Deltas = extensionDeltasForAppObject?.MetadataObject;
		Timestamp = extensionDeltasForAppObject?.Timestamp ?? 0;
	}

	public void ApplyRuntimeDeltas(ApplicationObjectId subjectId, TSubject subject)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected Obj, but got Unknown
		if (Deltas != null)
		{
			MetadataRuntimeDeltaApplicator sharedInstanceWithAllAdapters = MetadataRuntimeDeltaApplicator.GetSharedInstanceWithAllAdapters(ApplicationObjectType);
			DeltaApplicationContext val = new DeltaApplicationContext();
			val.SubjectId = subjectId;
			val.Subject = subject;
			val.Deltas = new NavAppObjectMetadataRuntimeDeltas[1] { Deltas };
			val.SourceExtensionId = ApplicationObjectId;
			sharedInstanceWithAllAdapters.ApplyDeltas(val, Array.Empty<IDeltaValidationFilter>());
		}
	}
}
public abstract class NCLApplicationObjectExtension<TSubject, TParent, TExtension> : NCLApplicationObjectExtension<TSubject>, INCLApplicationObject<TSubject> where TParent : ITreeObject
{
	protected delegate TExtension CreateExtensionObjectInstance(ITreeObject parent, NavRecord record);

	protected override Type ApplicationObjectConstructorDelegateType => typeof(CreateExtensionObjectInstance);

	protected NCLApplicationObjectExtension(INCLMetaApplicationObjectLoader loader, ObjectType objectType, int objectId, NavAppRuntimeMetadata runtimeAppMetadata)
		: base(loader, objectType, objectId, runtimeAppMetadata)
	{
	}

	protected override Type LoadClrType()
	{
		return ObjectLoader.AppClrTypeRetriever.GetClrType(OwningApp.PackageId, ApplicationObjectId);
	}

	public void CreateExtensionInstanceAndBindToParent(TParent parent, NavRecord record = null)
	{
		if (ApplicationObjectClrType != null)
		{
			AddExtensionType(CreateExtensionObject(parent, record), parent);
		}
	}

	protected abstract void AddExtensionType(TExtension extension, TParent parent);

	protected virtual TExtension CreateExtensionObject(TParent parent, NavRecord record = null)
	{
		return ((CreateExtensionObjectInstance)ApplicationObjectConstructor)(parent, record);
	}
}
