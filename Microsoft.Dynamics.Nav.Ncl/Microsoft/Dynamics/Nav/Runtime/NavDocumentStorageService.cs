using System;
using System.Globalization;
using System.Net;
using System.Security;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.DocumentService;
using Microsoft.Dynamics.Nav.DocumentService.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDocumentStorageService : NavDocumentStorageServiceBase
{
	private readonly NavSession session;

	public NavDocumentStorageService(NavSession session)
	{
		this.session = session;
	}

	protected override IDocumentServiceHandler CreateDocumentServiceHandlerBase()
	{
		DocumentServiceType = DocumentServiceType ?? ServerUserSettings.Instance.DocumentServicesProvider.Value;
		return NavDocumentStorageServiceBase.CatchGeneralConnectionException(() => DocumentServiceFactory.CreateServiceDirect(DocumentServiceType));
	}

	protected override ICredentials ProvideCredentialsBase(ICredentials suggestedCredentials, out string errorMessage)
	{
		errorMessage = string.Empty;
		if (AuthorizedCredentials != null)
		{
			return AuthorizedCredentials;
		}
		AssureEnabled();
		IDocumentServiceHandler documentServiceHandler = CreateDocumentServiceHandler();
		ICredentials credentials = suggestedCredentials;
		ICredentials authorizedCredentials;
		if (credentials == null)
		{
			if (TryUsingSharedDocuments(documentServiceHandler, out authorizedCredentials, out var didCheckUsingSharedDocumentsProperties, out errorMessage))
			{
				return authorizedCredentials;
			}
			if (didCheckUsingSharedDocumentsProperties)
			{
				return null;
			}
		}
		DocumentServiceException documentServiceException;
		while (!TryAuthorizeCredentials(documentServiceHandler, credentials, out authorizedCredentials, out documentServiceException))
		{
			UserNamePasswordRequestOptions requestOptions = CreateRequestOptions(documentServiceHandler, credentials, documentServiceException);
			if (!TryObtainingCredentials(requestOptions, out credentials))
			{
				return null;
			}
		}
		AuthorizedCredentials = authorizedCredentials;
		return AuthorizedCredentials;
	}

	private static bool TryUsingSharedDocuments(IDocumentServiceHandler documentServiceHandler, out ICredentials authorizedCredentials, out bool didCheckUsingSharedDocumentsProperties, out string errorMessage)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected Obj, but got Unknown
		authorizedCredentials = null;
		didCheckUsingSharedDocumentsProperties = false;
		errorMessage = string.Empty;
		string text = documentServiceHandler.Properties.GetProperty("User Name") as string;
		SecureString secureString = SecureStringExtension.CreateFromUnsecureString(documentServiceHandler.Properties.GetProperty("Password") as string);
		if (!string.IsNullOrEmpty(text) || secureString.Length > 0)
		{
			didCheckUsingSharedDocumentsProperties = true;
			DocumentServiceCredentialsUserPassword suggestedCredentials = new DocumentServiceCredentialsUserPassword
			{
				UserName = text,
				SecurePassword = secureString
			};
			if (TryAuthorizeCredentials(documentServiceHandler, (ICredentials)suggestedCredentials, out authorizedCredentials, out var documentServiceException))
			{
				return true;
			}
			if (documentServiceException != null)
			{
				errorMessage = ((Exception)(object)documentServiceException).Message;
			}
		}
		return false;
	}

	private UserNamePasswordRequestOptions CreateRequestOptions(IDocumentServiceHandler documentServiceHandler, ICredentials nextSuggestedCredentials, DocumentServiceException documentServiceException)
	{
		string arg = documentServiceHandler.Properties.GetProperty("Location") as string;
		DocumentServiceCredentialsUserPassword val = (DocumentServiceCredentialsUserPassword)((nextSuggestedCredentials is DocumentServiceCredentialsUserPassword) ? nextSuggestedCredentials : null);
		string userName = ((val != null) ? val.UserName : FindAuthenticationEmail());
		return new UserNamePasswordRequestOptions
		{
			Description = string.Format(CultureInfo.CurrentCulture, Lang.DocumentServiceLoginDescription, arg, ProductNames.ProductNameShort),
			DialogCaption = Lang.DocumentServiceRequestForCredentials,
			RequestIsRetry = (nextSuggestedCredentials != null),
			UserName = userName,
			ErrorOnPreviousAuthentication = ((documentServiceException != null) ? ((Exception)(object)documentServiceException).Message : string.Empty)
		};
	}

	private string FindAuthenticationEmail()
	{
		using NavRecord navRecord = new NavRecord(session, 2000000120);
		navRecord.ALSetCurrentKey(1);
		navRecord.ALSetFilter(1, "%1", new NavGuid(session.User.Id));
		navRecord.ALFind(DataError.TrapError, "-");
		return navRecord.GetFieldValueSafe(11, NavType.Text).ToString();
	}

	private static bool TryAuthorizeCredentials(IDocumentServiceHandler documentServiceHandler, ICredentials suggestedCredentials, out ICredentials credentials, out DocumentServiceException documentServiceException)
	{
		//IL_001d: Expected Obj, but got Unknown
		credentials = null;
		documentServiceException = null;
		if (suggestedCredentials == null)
		{
			return false;
		}
		documentServiceHandler.Credentials = suggestedCredentials;
		try
		{
			credentials = documentServiceHandler.AuthenticateCredentials();
		}
		catch (DocumentServiceException ex)
		{
			DocumentServiceException ex2 = ex;
			documentServiceException = ex2;
			return false;
		}
		return credentials != null;
	}

	private bool TryObtainingCredentials(UserNamePasswordRequestOptions requestOptions, out ICredentials credentials)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected Obj, but got Unknown
		UserNamePasswordCredentials userNamePasswordCredentials = session.ClientCallback.RequestCredentials(requestOptions);
		if (userNamePasswordCredentials != null)
		{
			credentials = (ICredentials)new DocumentServiceCredentialsUserPassword
			{
				UserName = userNamePasswordCredentials.UserName,
				SecurePassword = userNamePasswordCredentials.SecurePassword
			};
			return true;
		}
		credentials = null;
		return false;
	}

	protected override void Dispose(bool disposing)
	{
	}
}
