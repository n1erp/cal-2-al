using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

public class ServerInstanceRsaEncryptionProvider : RsaEncryptionProviderBase
{
	private static ISystemEncryptionProvider instance;

	private const string DefaultKeyFileName = "{0}.key";

	private static readonly object SyncObject = new object();

	public static ISystemEncryptionProvider Instance
	{
		get
		{
			lock (SyncObject)
			{
				if (instance == null)
				{
					instance = new ServerInstanceRsaEncryptionProvider(ServerUserSettings.CommandLineServerInstance ?? "DynamicsNAV90");
				}
			}
			return instance;
		}
	}

	protected override string StoredEncryptionFileName
	{
		get
		{
			string text = CreateEncryptionFileName();
			if (!KeyFileExists(text))
			{
				return string.Empty;
			}
			return text;
		}
		set
		{
		}
	}

	public ServerInstanceRsaEncryptionProvider(string serverInstance)
	{
		Id = serverInstance;
	}

	public static void DisposeAndClearInstance()
	{
		lock (SyncObject)
		{
			if (instance != null)
			{
				ISystemEncryptionProvider systemEncryptionProvider = instance;
				instance = null;
				systemEncryptionProvider.Dispose();
			}
		}
	}

	protected override string CreateEncryptionFileName()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}.key", Id);
	}
}
