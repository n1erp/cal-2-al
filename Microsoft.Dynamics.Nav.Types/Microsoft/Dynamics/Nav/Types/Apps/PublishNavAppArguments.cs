using System;
using System.IO;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types.Apps;

[MessageContract(WrapperNamespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public sealed class PublishNavAppArguments : IDisposable
{
	[MessageHeader(MustUnderstand = true)]
	private bool skipSigninVerification;

	[MessageHeader(MustUnderstand = true)]
	private NavAppPackageType packageType;

	[MessageHeader(MustUnderstand = true)]
	private string tenantId = string.Empty;

	[MessageHeader(MustUnderstand = false)]
	private NavAppScope scope;

	[MessageBodyMember(Order = 1)]
	private Stream packageStream;

	public bool SkipSigningVerification
	{
		get
		{
			return skipSigninVerification;
		}
		set
		{
			skipSigninVerification = value;
		}
	}

	public NavAppPackageType PackageType
	{
		get
		{
			return packageType;
		}
		set
		{
			packageType = value;
		}
	}

	public string TenantId
	{
		get
		{
			return tenantId;
		}
		set
		{
			tenantId = value;
		}
	}

	public NavAppScope Scope
	{
		get
		{
			return scope;
		}
		set
		{
			scope = value;
		}
	}

	public Stream PackageStream
	{
		get
		{
			return packageStream;
		}
		set
		{
			packageStream = value;
		}
	}

	public Stream TakeStreamOwnership()
	{
		Stream stream = MemoryStreamManager.GetStream();
		PackageStream.CopyTo(stream);
		PackageStream = null;
		stream.Position = 0L;
		return stream;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		if (PackageStream != null)
		{
			PackageStream.Dispose();
			PackageStream = null;
		}
	}
}
