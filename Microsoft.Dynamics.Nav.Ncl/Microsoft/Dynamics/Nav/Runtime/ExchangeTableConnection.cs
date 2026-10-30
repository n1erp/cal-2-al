using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Common.Exchange;
using Microsoft.Dynamics.Nav.Common.Exchange.Properties;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Exchange.WebServices.Data;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ExchangeTableConnection : TableConnection
{
	private IExchangeServiceWrapper service;

	private readonly NavSession session;

	private string exchangeUserName;

	private string exchangeUri;

	private string exchangeMailbox;

	private const string DefaultFolderId = "Dynamics NAV";

	public string FolderId { get; set; }

	internal override TableConnectionType ConnectionType => TableConnectionType.Exchange;

	internal override bool SupportsTransactions => false;

	private ExchangeTableConnection(ITreeObject parent, NavSession session, string name, string connectionString)
		: base(parent, name)
	{
		if (string.IsNullOrEmpty(name))
		{
			name = Guid.NewGuid().ToString();
		}
		ReadConnectionString(connectionString);
		if (string.IsNullOrEmpty(exchangeUri))
		{
			bool flag;
			try
			{
				flag = service.AutodiscoverServiceUrl(exchangeUserName);
			}
			catch (Exception innerException)
			{
				throw new NavExchangeException(string.Format(CultureInfo.CurrentCulture, Resources.autoDiscoverFailedException, exchangeUserName), innerException);
			}
			if (!flag)
			{
				throw new NavExchangeException(string.Format(CultureInfo.CurrentCulture, Resources.autoDiscoverFailedException, exchangeUserName));
			}
		}
		else
		{
			service.ExchangeServiceUrl = exchangeUri;
		}
		this.session = session;
	}

	private void ReadConnectionString(string connectionString)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected Obj, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected Obj, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected Obj, but got Unknown
		Func<string, string> func = (string field) =>
		{
			string pattern = "{" + field + "}=([^;]*)?";
			Match match = Regex.Match(connectionString, pattern, RegexOptions.IgnoreCase);
			return (!match.Success) ? string.Empty : match.Groups[1].Value;
		};
		string text = func("userName");
		string text2 = func("password");
		string text3 = func("token");
		string text4 = func("folderId");
		string text5 = func("mailbox");
		string text6 = func("uri");
		exchangeUserName = text;
		exchangeMailbox = text5;
		exchangeUri = text6;
		if (exchangeUserName == "@@TEST@@")
		{
			service = (IExchangeServiceWrapper)new MockExchangeServiceWrapper();
		}
		else if (!string.IsNullOrWhiteSpace(exchangeMailbox))
		{
			service = (IExchangeServiceWrapper)(object)new ExchangeDataProviderServiceWrapper((ExchangeVersion)5, exchangeMailbox);
		}
		else
		{
			service = (IExchangeServiceWrapper)(object)new ExchangeDataProviderServiceWrapper((ExchangeVersion)5);
		}
		if (string.IsNullOrWhiteSpace(text3))
		{
			service.Credentials = (ExchangeCredentials)new WebCredentials(text, text2);
		}
		else
		{
			service.Credentials = (ExchangeCredentials)new OAuthCredentials(text3);
		}
		FolderId = ((string.IsNullOrWhiteSpace(text4) && string.IsNullOrWhiteSpace(exchangeMailbox)) ? "Dynamics NAV" : text4);
	}

	internal DataAccess CreateDataAccess(NCLMetaTable table, GlobalFilters globalFilters)
	{
		//IL_002c: Expected Obj, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		CheckDisposed();
		try
		{
			return new DataAccess(session, new ExchangeDataProvider(session, table), globalFilters, DataAccessTableVersionTokens.CreateForTempTable(), VirtualAndTempTransactionalDataCache.Instance);
		}
		catch (NavEwsException ex)
		{
			NavEwsException ex2 = ex;
			throw new NavNCLEwsException(string.Format(CultureInfo.InvariantCulture, "Unable to create data provider for table {0} with external name {1}.", table.TableName, table.ExternalName), ex2);
		}
	}

	internal static ExchangeTableConnection Create(ITreeObject parent, NavSession session, string name, string connectionString)
	{
		return new ExchangeTableConnection(parent, session, name, connectionString);
	}

	internal IExchangeServiceWrapper GetService()
	{
		CheckDisposed();
		return service ?? null;
	}
}
