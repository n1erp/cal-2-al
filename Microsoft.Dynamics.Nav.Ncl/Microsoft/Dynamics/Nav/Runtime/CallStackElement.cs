using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Debugger;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CallStackElement
{
	private readonly ApplicationObjectId objectId;

	private readonly Type methodScopeType;

	private readonly int statementNumber;

	private readonly WeakReference applicationObjectWeakReference;

	private readonly bool applicationObjectRetrievalError;

	public CallStackElement(NavMethodScope scope)
	{
		try
		{
			NavCurrentThread.Session.IsDiagnosticsMode = true;
			objectId = NavGlobal.NCLMetadata.CodeEnvironment.GetApplicationObjectId(scope);
			applicationObjectWeakReference = new WeakReference(scope.ApplicationObject);
			methodScopeType = scope.GetType();
			statementNumber = scope.StatementNumber;
		}
		catch (NavBaseException)
		{
			applicationObjectRetrievalError = true;
		}
		finally
		{
			NavCurrentThread.Session.IsDiagnosticsMode = false;
		}
	}

	internal void GetText(StringBuilder sb, Func<NavApplicationObjectBase, string> getApplicationObjectName)
	{
		if (applicationObjectRetrievalError)
		{
			sb.AppendFormat(CultureInfo.CurrentCulture, Lang.MetadataRetrievalErrorForCurrentFrame);
			return;
		}
		NavApplicationObjectBase navApplicationObjectBase = (NavApplicationObjectBase)applicationObjectWeakReference.Target;
		string text = null;
		if (navApplicationObjectBase != null && !navApplicationObjectBase.IsDisposed && getApplicationObjectName != null)
		{
			try
			{
				text = getApplicationObjectName(navApplicationObjectBase);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (NavBaseException)
			{
			}
			catch (NullReferenceException exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000192", Category.Runtime, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata("NullReferenceException when retrieving application object name for object id " + navApplicationObjectBase.ObjectId.ToString()));
			}
		}
		if (text == null)
		{
			text = NavCurrentThread.Session.GetObjectNameForErrorMessage(objectId);
		}
		sb.Append(DebugEnvironment.QuoteNameIfNeeded(text));
		sb.Append('(');
		sb.Append(objectId.ObjectType);
		sb.Append(' ');
		sb.Append(objectId.ObjectNumber);
		sb.Append(").");
		if (objectId.ObjectNumber == 0)
		{
			sb.AppendLine("Unknown method scope");
			return;
		}
		ObjectSourceInfo sourceInfo = NavGlobal.NCLMetadata.CodeEnvironment.GetSourceInfo(objectId);
		string name = NavMethodScope.MethodName(methodScopeType);
		sb.Append(DebugEnvironment.QuoteNameIfNeeded(name));
		MethodSourceInfo methodSourceInfo = sourceInfo.Methods.FirstOrDefault((MethodSourceInfo method) => method.LocalsType == methodScopeType);
		if (methodSourceInfo != null)
		{
			if (methodSourceInfo.IsTrigger)
			{
				sb.Append("(Trigger)");
			}
			sb.Append(" line ");
			sb.Append(methodSourceInfo.GetStatement(statementNumber).RelativeSourceSpan.From.Line);
		}
		NavAppRuntimeMetadata extensionMetadataForObject = NavGlobal.NCLMetadata.CodeEnvironment.GetExtensionMetadataForObject(objectId);
		if (extensionMetadataForObject != null)
		{
			sb.Append(" - ");
			sb.Append(extensionMetadataForObject.Name);
			sb.Append(" by ");
			sb.Append(extensionMetadataForObject.Publisher);
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		GetText(stringBuilder, null);
		return stringBuilder.ToString();
	}
}
