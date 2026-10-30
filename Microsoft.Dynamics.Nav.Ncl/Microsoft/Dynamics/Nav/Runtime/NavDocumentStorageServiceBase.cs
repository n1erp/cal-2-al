using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.DocumentService.Types;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavDocumentStorageServiceBase : IDocumentStorageService, IDisposable
{
	public ICredentials AuthorizedCredentials { get; set; }

	protected string DocumentServiceType { get; set; }

	public IDocumentServiceHandler DocumentServiceHandler => CreateDocumentServiceHandlerForProperty();

	public void Reset()
	{
		AuthorizedCredentials = null;
	}

	private IDocumentServiceHandler CreateDocumentServiceHandlerForProperty()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		IDocumentServiceHandler val = CreateDocumentServiceHandler();
		ICredentials credentials = ProvideCredentials(AuthorizedCredentials, out var errorMessage);
		if (credentials == null)
		{
			AuthorizedCredentials = null;
			if (!string.IsNullOrEmpty(errorMessage))
			{
				throw new DocumentServiceConnectionException(errorMessage);
			}
			string text = val.Properties.GetProperty("Location") as string;
			text = text ?? string.Empty;
			throw new DocumentServiceConnectionException(string.Format(CultureInfo.CurrentCulture, Lang.DocumentServiceConnectionFailed, text));
		}
		AuthorizedCredentials = credentials;
		val.Credentials = AuthorizedCredentials;
		return val;
	}

	public void SetDocumentServiceType(string documentServiceType)
	{
		DocumentServiceType = documentServiceType;
	}

	public ICredentials ProvideCredentials(ICredentials suggestedCredentials, out string errorMessage)
	{
		return ProvideCredentialsBase(null, out errorMessage);
	}

	public bool AuthenticateCredentials(string userName, SecureString securePassword, out ICredentials credentials, out string errorMessage)
	{
		//IL_0072: Expected Obj, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		AssureEnabled();
		credentials = null;
		errorMessage = null;
		try
		{
			if (string.IsNullOrEmpty(userName) || securePassword.Length.Equals(0))
			{
				errorMessage = string.Format(CultureInfo.CurrentCulture, Lang.DocumentServiceUserNamePassword);
				return false;
			}
			IDocumentServiceHandler val = CreateDocumentServiceHandler();
			val.Credentials = (ICredentials)new DocumentServiceCredentialsUserPassword
			{
				UserName = userName,
				SecurePassword = securePassword
			};
			credentials = val.AuthenticateCredentials();
			return credentials != null;
		}
		catch (DocumentServiceException ex)
		{
			DocumentServiceException ex2 = ex;
			errorMessage = ((Exception)(object)ex2).Message;
		}
		return false;
	}

	protected abstract IDocumentServiceHandler CreateDocumentServiceHandlerBase();

	internal IDocumentServiceHandler CreateDocumentServiceHandler()
	{
		IDocumentServiceHandler val = CreateDocumentServiceHandlerBase();
		if (val == null)
		{
			throw new InvalidOperationException("The Document Service Handler has not been applied.");
		}
		ApplyProperties(val.Properties);
		return val;
	}

	protected virtual ICredentials ProvideCredentialsBase(ICredentials suggestedCredentials, out string errorMessage)
	{
		errorMessage = string.Empty;
		return null;
	}

	public static void ApplyProperties(DocumentServiceProperties properties)
	{
		if (properties == null)
		{
			throw new ArgumentNullException("properties");
		}
		NavRecord documentServiceRecord = FindDocumentServiceRecord();
		Action<string, int> action = (string propertyName, int fieldName) =>
		{
			properties.SetProperty(propertyName, (object)GetFieldValue(documentServiceRecord, fieldName));
		};
		action("Description", 3);
		action("Location", 5);
		action("User Name", 7);
		action("Password", 9);
		action("Document Repository", 11);
		action("Folder", 13);
	}

	protected static string GetFieldValue(NavRecord record, int fieldName)
	{
		return record.GetFieldValue(fieldName).ToString();
	}

	protected static NavRecord FindDocumentServiceRecord()
	{
		NavRecord navRecord = NavGlobal.NCLMetadata.GetMetaTableById(2000000114).CreateObjectInstance(NavCurrentThread.Session, isTemporary: false, null, string.Empty, SecurityFiltering.Validated);
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			return navRecord;
		}
		navRecord.Dispose();
		return null;
	}

	public bool Enabled()
	{
		return IsEnabled();
	}

	protected virtual bool IsEnabled()
	{
		NavRecord navRecord = FindDocumentServiceRecord();
		if (navRecord != null && !string.IsNullOrWhiteSpace(GetFieldValue(navRecord, 5)))
		{
			return !string.IsNullOrWhiteSpace(GetFieldValue(navRecord, 13));
		}
		return false;
	}

	protected void AssureEnabled()
	{
		if (!IsEnabled())
		{
			throw new NavDocumentServiceNotConfiguredException(Lang.DocumentServiceNotConfigured);
		}
	}

	public string Save(Stream source, string targetName, bool overwrite)
	{
		AssureEnabled();
		return CatchGeneralConnectionException(() => SaveDocumentCore(source, targetName, overwrite));
	}

	internal string SaveDocumentCore(Stream dataStream, string filename, bool overwrite)
	{
		IDocumentServiceHandler documentServiceHandler = DocumentServiceHandler;
		string text = documentServiceHandler.Save(dataStream, filename, overwrite);
		return documentServiceHandler.GenerateViewableDocumentAddress(text);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected abstract void Dispose(bool disposing);

	protected static T CatchGeneralConnectionException<T>(Func<T> func)
	{
		//IL_000a: Expected Obj, but got Unknown
		try
		{
			return func();
		}
		catch (DocumentServiceException ex)
		{
			DocumentServiceException ex2 = ex;
			throw new NavNCLDocumentServiceException(((Exception)(object)ex2).Message, (Exception)(object)ex2);
		}
	}
}
