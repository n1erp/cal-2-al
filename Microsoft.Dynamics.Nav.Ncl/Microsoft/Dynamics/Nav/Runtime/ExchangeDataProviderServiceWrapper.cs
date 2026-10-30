using System;
using System.IO;
using System.Net;
using Microsoft.Dynamics.Nav.Common.Exchange;
using Microsoft.Dynamics.Nav.Common.Exchange.Booking;
using Microsoft.Exchange.WebServices.Data;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ExchangeDataProviderServiceWrapper : IExchangeServiceWrapper
{
	public NavExchangeServiceBase exchangeServiceBase { get; private set; }

	string IExchangeServiceWrapper.LastError { get; }

	ExchangeCredentials IExchangeServiceWrapper.Credentials
	{
		get
		{
			return exchangeServiceBase.Credentials;
		}
		set
		{
			exchangeServiceBase.Credentials = value;
		}
	}

	string IExchangeServiceWrapper.ExchangeServiceUrl
	{
		get
		{
			Uri exchangeServiceUrl = exchangeServiceBase.ExchangeServiceUrl;
			if (!(exchangeServiceUrl == null))
			{
				return exchangeServiceUrl.ToString();
			}
			return string.Empty;
		}
		set
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				exchangeServiceBase.ExchangeServiceUrl = new Uri(value);
			}
			catch (UriFormatException ex)
			{
				throw new NavEwsException(ex.Message, (Exception)ex);
			}
		}
	}

	public ExchangeDataProviderServiceWrapper(ExchangeVersion version = (ExchangeVersion)1, string targetMailbox = null)
		: this(ExchangeCredentials.op_Implicit(CredentialCache.DefaultNetworkCredentials), version, targetMailbox)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
	}

	public ExchangeDataProviderServiceWrapper(ExchangeCredentials credentials, ExchangeVersion version = (ExchangeVersion)1, string targetMailbox = null)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		exchangeServiceBase = new NavExchangeServiceBase(version, credentials, targetMailbox);
	}

	public void SetNetworkCredential(ExchangeCredentials credentials)
	{
		exchangeServiceBase.Credentials = credentials;
	}

	bool IExchangeServiceWrapper.AutodiscoverServiceUrl(string autodiscoverEmailAddress)
	{
		exchangeServiceBase.AutodiscoverServiceUrl(autodiscoverEmailAddress);
		return true;
	}

	FolderInfoEnumeratorBase IExchangeServiceWrapper.GetPublicFolders(FolderInfoBase parent, int pageSize)
	{
		return exchangeServiceBase.GetPublicFolders(parent, pageSize);
	}

	IExchangeFolder IExchangeServiceWrapper.GetFolder(WellKnownFolderName knownFolder, string folderName)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return exchangeServiceBase.GetFolder(knownFolder, folderName);
	}

	void IExchangeServiceWrapper.SaveItem(IExchangeDataItem item)
	{
		((IExternalDataItem)item).Save();
	}

	void IExchangeServiceWrapper.SaveItem(IExchangeDataItem item, WellKnownFolderName folderName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		item.Save(folderName);
	}

	void IExchangeServiceWrapper.SaveItem(IExchangeDataItem item, string folderId)
	{
		item.Save(folderId);
	}

	void IExchangeServiceWrapper.UpdateItem(IExchangeDataItem item)
	{
		((IExternalDataItem)item).Update();
	}

	void IExchangeServiceWrapper.DeleteItem(IExchangeDataItem item)
	{
		((IExternalDataItem)item).Delete();
	}

	IContact IExchangeServiceWrapper.NewContact()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected Obj, but got Unknown
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		return (IContact)new ContactWrapper(new Contact(exchangeServiceBase.Service));
	}

	IBookingService IExchangeServiceWrapper.NewBookingService()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected Obj, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected Obj, but got Unknown
		ExchangeServiceBinding val = new ExchangeServiceBinding(exchangeServiceBase.Service);
		EmailAddressType val2 = new EmailAddressType
		{
			EmailAddress = exchangeServiceBase.TargetMailbox.Address
		};
		return (IBookingService)new BookingServiceWrapper(val, val2, (Item)null, (BookingServiceType)null);
	}

	bool IExchangeServiceWrapper.EmailMessageExists(string messageId)
	{
		return exchangeServiceBase.EmailMessageExists(messageId);
	}

	bool IExchangeServiceWrapper.FolderExists(string folderId)
	{
		return exchangeServiceBase.FolderExists(folderId);
	}

	public bool ValidateCredentials()
	{
		return exchangeServiceBase.ValidateCredentials();
	}

	public void SetImpersonatedIdentity(string userId)
	{
		exchangeServiceBase.SetImpersonatedIdentity(userId);
	}

	public void InstallApp(Stream manifest)
	{
		exchangeServiceBase.InstallApp(manifest);
	}

	public void SaveEmlMessageToInbox(string message)
	{
		throw new NotImplementedException();
	}

	public void SaveHtmlMessageToInbox(string subject, string bodyHtml, string senderAddress, string senderName, string recipientName)
	{
		throw new NotImplementedException();
	}

	public IExchangeDataItem GetEmailWithAttachments(string itemId)
	{
		throw new NotImplementedException();
	}

	public string GetExchangeUserTimeZone()
	{
		return exchangeServiceBase.GetExchangeUserTimeZone();
	}
}
