using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SessionContextHelper
{
	internal static string GetALCallStack()
	{
		NavSession session = NavCurrentThread.Session;
		if (session != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			NavApplicationObjectBase navApplicationObjectBase = session.CurrentMethodScope?.ApplicationObject;
			if (navApplicationObjectBase != null)
			{
				stringBuilder.Append("  AppObjectType: ").AppendLine(navApplicationObjectBase.ObjectId.ObjectType.ToString());
				stringBuilder.Append("  AppObjectId: ").AppendLine(navApplicationObjectBase.ObjectId.ObjectNumber.ToString(CultureInfo.InvariantCulture));
			}
			string currentCallstack = session.GetCurrentCallstack();
			if (!string.IsNullOrWhiteSpace(currentCallstack))
			{
				stringBuilder.Append("  AL CallStack: ").Append(currentCallstack);
			}
			return stringBuilder.ToString();
		}
		return string.Empty;
	}

	internal static ALObjectScopeDiagnosticEntry GetALScope(NavSession session)
	{
		string aLMethodScope = string.Empty;
		int aLObjectId = 0;
		string aLObjectName = string.Empty;
		ObjectType aLObjectType = ObjectType.System;
		string extensionId = string.Empty;
		Version extensionVersion = null;
		string extensionName = string.Empty;
		string aLCallStack = string.Empty;
		string companyName = string.Empty;
		string clientType = string.Empty;
		if (session != null && session.CurrentMethodScope != null && !session.CurrentMethodScope.IsDisposed)
		{
			companyName = session.CompanyName;
			clientType = session.ClientConnectionType.ToString();
			NavApplicationObjectBase navApplicationObjectBase = session.CurrentMethodScope?.ApplicationObject;
			if (navApplicationObjectBase != null)
			{
				aLObjectId = navApplicationObjectBase.ObjectId.ObjectNumber;
				aLObjectType = navApplicationObjectBase.ObjectId.ObjectType;
				aLObjectName = navApplicationObjectBase.ObjectName;
				ApplicationObjectId applicationObjectId = ALCodeEnvironment.EffectiveObjectId(navApplicationObjectBase);
				NavAppRuntimeMetadata extensionMetadataForObject = NavGlobal.NCLMetadata.CodeEnvironment.GetExtensionMetadataForObject(applicationObjectId);
				if (extensionMetadataForObject != null)
				{
					extensionId = ((WrappedValue<Guid>)(object)extensionMetadataForObject.AppId)?.Value.ToString();
					extensionVersion = extensionMetadataForObject.Version;
					extensionName = extensionMetadataForObject.Name;
				}
			}
			aLCallStack = session.GetCurrentCallstack();
			aLMethodScope = session.CurrentMethodScope?.ScopeName;
		}
		return new ALObjectScopeDiagnosticEntry
		{
			ALObjectId = aLObjectId,
			ALObjectType = aLObjectType,
			ALObjectName = aLObjectName,
			ALCallStack = aLCallStack,
			ALMethodScope = aLMethodScope,
			ExtensionId = extensionId,
			ExtensionName = extensionName,
			ExtensionVersion = extensionVersion,
			CompanyName = companyName,
			ClientType = clientType
		};
	}
}
