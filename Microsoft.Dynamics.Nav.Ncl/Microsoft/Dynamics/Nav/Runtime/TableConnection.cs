using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class TableConnection : TreeObject
{
	internal static readonly string DefaultTableConnectionName = string.Empty;

	internal string Name { get; private set; }

	internal abstract TableConnectionType ConnectionType { get; }

	internal virtual string ConnectionTypeName => ConnectionType.ToString();

	internal abstract bool SupportsTransactions { get; }

	internal TableConnection(ITreeObject parent, string name)
		: base(parent)
	{
		Name = name;
	}

	protected internal bool RequestCredentials(out UserNamePasswordCredentials credentials)
	{
		if (!Tree.Session.ClientCallback.IsCallbackAllowed)
		{
			throw new NavNotSupportedException(Lang.RequestCredentialsIsNotSupportedError);
		}
		UserNamePasswordRequestOptions requestOptions = new UserNamePasswordRequestOptions
		{
			DialogCaption = Lang.TableConnectionGetCredentialsCaption,
			Description = string.Format(CultureInfo.CurrentCulture, Lang.TableConnectionGetCredentialsDescription, ConnectionTypeName)
		};
		credentials = Tree.Session.ClientCallback.RequestCredentials(requestOptions);
		return credentials != null;
	}

	protected void CheckDisposed()
	{
		if (IsDisposed)
		{
			throw new NavNclObjectDisposedException(string.Format(CultureInfo.CurrentCulture, Lang.TableConnectionIsDisposed, Name, ConnectionType));
		}
	}
}
