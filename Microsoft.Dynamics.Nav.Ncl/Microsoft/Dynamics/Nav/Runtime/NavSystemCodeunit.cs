using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavSystemCodeunit
{
	public enum Ids
	{
		VersionTriggers = 2000000001,
		GlobalTriggers,
		CompanyTriggers,
		UIHelperTriggers,
		ReportingTriggers,
		SystemActionTriggers,
		FilterHelperTriggers,
		UpgradeTriggers,
		DebuggerTriggers,
		ExtensionTriggers
	}

	internal static readonly object[] EmptyObjectArray = new object[0];

	private readonly NavCodeunitHandle codeunitHandle;

	protected NavSystemCodeunit(TreeObject parent, int codeunitId)
	{
		codeunitHandle = new NavCodeunitHandle(parent, codeunitId);
	}

	internal object Invoke(int methodId, object[] arguments)
	{
		codeunitHandle.Target.ExecutePermissionsValidated = true;
		try
		{
			using (new NavAppUsageEventSuppressionScope(NavCurrentThread.Session, string.Format(CultureInfo.InvariantCulture, "System Codeunit {0} Call", codeunitHandle.ObjectId.ObjectNumber)))
			{
				NavSession session = NavCurrentThread.Session;
				if (session.AlternativeSystemCodeunitInvoke != null)
				{
					return session.AlternativeSystemCodeunitInvoke(methodId, arguments);
				}
				return codeunitHandle.Target.Invoke(methodId, arguments);
			}
		}
		catch (InvalidCastException innerException)
		{
			throw NavNCLWrongSignatureOnCU1TriggerException.Create(CultureInfo.CurrentCulture, methodId, codeunitHandle.ObjectId.ObjectNumber, GetExpectedTriggerSignature(methodId, arguments), innerException);
		}
	}

	private string GetExpectedTriggerSignature(int methodId, object[] arguments)
	{
		string methodName = GetMethodName(methodId);
		StringBuilder stringBuilder = new StringBuilder(255);
		stringBuilder.Append(methodName).Append('(');
		for (int i = 0; i < arguments.Length; i++)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", arguments[i]);
			if (i < arguments.Length - 1)
			{
				stringBuilder.Append(", ");
			}
		}
		stringBuilder.Append(')');
		return stringBuilder.ToString();
	}

	protected abstract string GetMethodName(int methodId);
}
