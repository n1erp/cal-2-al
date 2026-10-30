using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.CodeAnalysis;

public sealed class DesktopAssemblyIdentityComparer : AssemblyIdentityComparer
{
	private sealed class FrameworkAssemblyDictionary : Dictionary<string, FrameworkAssemblyDictionary.Value>
	{
		public struct Value(ImmutableArray<byte> publicKeyToken, AssemblyVersion version)
		{
			public readonly ImmutableArray<byte> PublicKeyToken = publicKeyToken;

			public readonly AssemblyVersion Version = version;
		}

		public FrameworkAssemblyDictionary()
			: base((IEqualityComparer<string>?)AssemblyIdentityComparer.SimpleNameComparer)
		{
		}

		public void Add(string name, ImmutableArray<byte> publicKeyToken, AssemblyVersion version)
		{
			Add(name, new Value(publicKeyToken, version));
		}
	}

	private sealed class FrameworkRetargetingDictionary : Dictionary<FrameworkRetargetingDictionary.Key, List<FrameworkRetargetingDictionary.Value>>
	{
		public struct Key(string name, ImmutableArray<byte> publicKeyToken) : IEquatable<Key>
		{
			public readonly string Name = name;

			public readonly ImmutableArray<byte> PublicKeyToken = publicKeyToken;

			public bool Equals(Key other)
			{
				if (AssemblyIdentityComparer.SimpleNameComparer.Equals(Name, other.Name))
				{
					return PublicKeyToken.SequenceEqual(other.PublicKeyToken);
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is Key)
				{
					return Equals((Key)obj);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return AssemblyIdentityComparer.SimpleNameComparer.GetHashCode(Name) ^ PublicKeyToken[0];
			}
		}

		public struct Value(AssemblyVersion versionLow, AssemblyVersion versionHigh, string newName, ImmutableArray<byte> newPublicKeyToken, AssemblyVersion newVersion, bool isPortable)
		{
			public readonly AssemblyVersion VersionLow = versionLow;

			public readonly AssemblyVersion VersionHigh = versionHigh;

			public readonly string NewName = newName;

			public readonly ImmutableArray<byte> NewPublicKeyToken = newPublicKeyToken;

			public readonly AssemblyVersion NewVersion = newVersion;

			public readonly bool IsPortable = isPortable;
		}

		public void Add(string name, ImmutableArray<byte> publicKeyToken, AssemblyVersion versionLow, object versionHighNull, string newName, ImmutableArray<byte> newPublicKeyToken, AssemblyVersion newVersion)
		{
			Key key = new Key(name, publicKeyToken);
			if (!TryGetValue(key, out var value))
			{
				Add(key, value = new List<Value>());
			}
			value.Add(new Value(versionLow, default, newName, newPublicKeyToken, newVersion, isPortable: false));
		}

		public void Add(string name, ImmutableArray<byte> publicKeyToken, AssemblyVersion versionLow, AssemblyVersion versionHigh, string newName, ImmutableArray<byte> newPublicKeyToken, AssemblyVersion newVersion, bool isPortable)
		{
			Key key = new Key(name, publicKeyToken);
			if (!TryGetValue(key, out var value))
			{
				Add(key, value = new List<Value>());
			}
			value.Add(new Value(versionLow, versionHigh, newName, newPublicKeyToken, newVersion, isPortable));
		}

		public bool TryGetValue(AssemblyIdentity identity, out Value value)
		{
			if (!TryGetValue(new Key(identity.Name, identity.PublicKeyToken), out var value2))
			{
				value = default;
				return false;
			}
			for (int i = 0; i < value2.Count; i++)
			{
				value = value2[i];
				AssemblyVersion assemblyVersion = (AssemblyVersion)identity.Version;
				if (value.VersionHigh.Major == 0)
				{
					if (assemblyVersion == value.VersionLow)
					{
						return true;
					}
				}
				else if (assemblyVersion >= value.VersionLow && assemblyVersion <= value.VersionHigh)
				{
					return true;
				}
			}
			value = default;
			return false;
		}
	}

	internal readonly AssemblyPortabilityPolicy policy;

	private static readonly ImmutableArray<byte> s_NETCF_PUBLIC_KEY_TOKEN_1 = ImmutableArray.Create(new byte[8] { 28, 158, 37, 150, 134, 249, 33, 224 });

	private static readonly ImmutableArray<byte> s_NETCF_PUBLIC_KEY_TOKEN_2 = ImmutableArray.Create(new byte[8] { 95, 213, 124, 84, 58, 156, 2, 71 });

	private static readonly ImmutableArray<byte> s_NETCF_PUBLIC_KEY_TOKEN_3 = ImmutableArray.Create(new byte[8] { 150, 157, 184, 5, 61, 51, 34, 172 });

	private static readonly ImmutableArray<byte> s_SQL_PUBLIC_KEY_TOKEN = ImmutableArray.Create(new byte[8] { 137, 132, 93, 205, 128, 128, 204, 145 });

	private static readonly ImmutableArray<byte> s_SQL_MOBILE_PUBLIC_KEY_TOKEN = ImmutableArray.Create(new byte[8] { 59, 226, 53, 223, 28, 141, 42, 211 });

	private static readonly ImmutableArray<byte> s_ECMA_PUBLICKEY_STR_L = ImmutableArray.Create(new byte[8] { 183, 122, 92, 86, 25, 52, 224, 137 });

	private static readonly ImmutableArray<byte> s_SHAREDLIB_PUBLICKEY_STR_L = ImmutableArray.Create(new byte[8] { 49, 191, 56, 86, 173, 54, 78, 53 });

	private static readonly ImmutableArray<byte> s_MICROSOFT_PUBLICKEY_STR_L = ImmutableArray.Create(new byte[8] { 176, 63, 95, 127, 17, 213, 10, 58 });

	private static readonly ImmutableArray<byte> s_SILVERLIGHT_PLATFORM_PUBLICKEY_STR_L = ImmutableArray.Create(new byte[8] { 124, 236, 133, 215, 190, 167, 121, 142 });

	private static readonly ImmutableArray<byte> s_SILVERLIGHT_PUBLICKEY_STR_L = ImmutableArray.Create(new byte[8] { 49, 191, 56, 86, 173, 54, 78, 53 });

	private static readonly ImmutableArray<byte> s_RIA_SERVICES_KEY_TOKEN = ImmutableArray.Create(new byte[8] { 221, 208, 218, 77, 62, 103, 130, 23 });

	private static readonly AssemblyVersion s_VER_VS_COMPATIBILITY_ASSEMBLYVERSION_STR_L = new AssemblyVersion(8, 0, 0, 0);

	private static readonly AssemblyVersion s_VER_VS_ASSEMBLYVERSION_STR_L = new AssemblyVersion(10, 0, 0, 0);

	private static readonly AssemblyVersion s_VER_SQL_ASSEMBLYVERSION_STR_L = new AssemblyVersion(9, 0, 242, 0);

	private static readonly AssemblyVersion s_VER_LINQ_ASSEMBLYVERSION_STR_L = new AssemblyVersion(3, 0, 0, 0);

	private static readonly AssemblyVersion s_VER_LINQ_ASSEMBLYVERSION_STR_2_L = new AssemblyVersion(3, 5, 0, 0);

	private static readonly AssemblyVersion s_VER_SQL_ORCAS_ASSEMBLYVERSION_STR_L = new AssemblyVersion(3, 5, 0, 0);

	private static readonly AssemblyVersion s_VER_ASSEMBLYVERSION_STR_L = new AssemblyVersion(4, 0, 0, 0);

	private static readonly AssemblyVersion s_VER_VC_STLCLR_ASSEMBLYVERSION_STR_L = new AssemblyVersion(2, 0, 0, 0);

	private const string NULL = null;

	private const bool TRUE = true;

	private static readonly FrameworkRetargetingDictionary s_arRetargetPolicy = new FrameworkRetargetingDictionary
	{
		{
			"System",
			s_ECMA_PUBLICKEY_STR_L,
			new AssemblyVersion(1, 0, 0, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Xml",
			s_ECMA_PUBLICKEY_STR_L,
			new AssemblyVersion(1, 0, 0, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System",
			s_NETCF_PUBLIC_KEY_TOKEN_1,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Drawing",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Web.Services",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Windows.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Xml",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Drawing",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Web.Services",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Windows.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Xml",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"Microsoft.VisualBasic",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(7, 0, 5000, 0),
			null,
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_VS_ASSEMBLYVERSION_STR_L
		},
		{
			"System",
			s_NETCF_PUBLIC_KEY_TOKEN_1,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Drawing",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Web.Services",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Windows.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Xml",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Drawing",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Web.Services",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Windows.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Xml",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"Microsoft.VisualBasic",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(7, 0, 5500, 0),
			null,
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_VS_ASSEMBLYVERSION_STR_L
		},
		{
			"Microsoft.WindowsCE.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_1,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"Microsoft.WindowsCE.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_1,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"Microsoft.WindowsCE.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"Microsoft.WindowsCE.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_2,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"Microsoft.WindowsCE.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			null,
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"Microsoft.WindowsCE.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			null,
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.SqlClient",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			"System.Data",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.SqlClient",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			"System.Data",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.Common",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			"System.Data",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.Common",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			"System.Data",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Windows.Forms.DataGrid",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5000, 0),
			null,
			"System.Windows.Forms",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Windows.Forms.DataGrid",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(1, 0, 5500, 0),
			null,
			"System.Windows.Forms",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(2, 0, 0, 0),
			new AssemblyVersion(2, 0, 10, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Xml",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(2, 0, 0, 0),
			new AssemblyVersion(2, 0, 10, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Drawing",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(2, 0, 0, 0),
			new AssemblyVersion(2, 0, 10, 0),
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Web.Services",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(2, 0, 0, 0),
			new AssemblyVersion(2, 0, 10, 0),
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Windows.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(2, 0, 0, 0),
			new AssemblyVersion(2, 0, 10, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(2, 0, 0, 0),
			new AssemblyVersion(2, 0, 10, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Messaging",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(2, 0, 0, 0),
			new AssemblyVersion(2, 0, 10, 0),
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.SqlClient",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(2, 0, 0, 0),
			new AssemblyVersion(2, 0, 10, 0),
			"System.Data",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.Common",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(2, 0, 0, 0),
			new AssemblyVersion(2, 0, 10, 0),
			"System.Data",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Windows.Forms.DataGrid",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(2, 0, 0, 0),
			new AssemblyVersion(2, 0, 10, 0),
			"System.Windows.Forms",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"Microsoft.VisualBasic",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(8, 0, 0, 0),
			new AssemblyVersion(8, 0, 10, 0),
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_VS_ASSEMBLYVERSION_STR_L
		},
		{
			"System",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Xml",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Drawing",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Web.Services",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Windows.Forms",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Messaging",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.SqlClient",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			"System.Data",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Windows.Forms.DataGrid",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			"System.Windows.Forms",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"Microsoft.VisualBasic",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(8, 1, 0, 0),
			new AssemblyVersion(8, 1, 5, 0),
			"Microsoft.VisualBasic",
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_VS_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.SqlClient",
			s_SQL_MOBILE_PUBLIC_KEY_TOKEN,
			new AssemblyVersion(3, 5, 0, 0),
			null,
			"System.Data",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.SqlServerCe",
			s_SQL_MOBILE_PUBLIC_KEY_TOKEN,
			new AssemblyVersion(3, 5, 0, 0),
			null,
			null,
			s_SQL_PUBLIC_KEY_TOKEN,
			s_VER_SQL_ORCAS_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.SqlServerCe",
			s_SQL_MOBILE_PUBLIC_KEY_TOKEN,
			new AssemblyVersion(3, 5, 1, 0),
			new AssemblyVersion(3, 5, 200, 999),
			null,
			s_SQL_PUBLIC_KEY_TOKEN,
			s_VER_SQL_ORCAS_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.SqlClient",
			s_SQL_MOBILE_PUBLIC_KEY_TOKEN,
			new AssemblyVersion(3, 0, 3600, 0),
			null,
			"System.Data",
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Data.SqlServerCe",
			s_SQL_MOBILE_PUBLIC_KEY_TOKEN,
			new AssemblyVersion(3, 0, 3600, 0),
			null,
			null,
			s_SQL_PUBLIC_KEY_TOKEN,
			s_VER_SQL_ASSEMBLYVERSION_STR_L
		},
		{
			"system.xml.linq",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_LINQ_ASSEMBLYVERSION_STR_2_L
		},
		{
			"system.data.DataSetExtensions",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_LINQ_ASSEMBLYVERSION_STR_2_L
		},
		{
			"System.Core",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_LINQ_ASSEMBLYVERSION_STR_2_L
		},
		{
			"System.ServiceModel",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_LINQ_ASSEMBLYVERSION_STR_L
		},
		{
			"System.Runtime.Serialization",
			s_NETCF_PUBLIC_KEY_TOKEN_3,
			new AssemblyVersion(3, 5, 0, 0),
			new AssemblyVersion(3, 9, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_LINQ_ASSEMBLYVERSION_STR_L
		},
		{
			"mscorlib",
			s_SILVERLIGHT_PLATFORM_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System",
			s_SILVERLIGHT_PLATFORM_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.ComponentModel.Composition",
			s_SILVERLIGHT_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.ComponentModel.DataAnnotations",
			s_RIA_SERVICES_KEY_TOKEN,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_SHAREDLIB_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.Core",
			s_SILVERLIGHT_PLATFORM_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.Net",
			s_SILVERLIGHT_PLATFORM_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.Numerics",
			s_SILVERLIGHT_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"Microsoft.CSharp",
			s_SILVERLIGHT_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.Runtime.Serialization",
			s_SILVERLIGHT_PLATFORM_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.ServiceModel",
			s_SILVERLIGHT_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.ServiceModel.Web",
			s_SILVERLIGHT_PLATFORM_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_SHAREDLIB_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.Xml",
			s_SILVERLIGHT_PLATFORM_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.Xml.Linq",
			s_SILVERLIGHT_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.Xml.Serialization",
			s_SILVERLIGHT_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_ECMA_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		},
		{
			"System.Windows",
			s_SILVERLIGHT_PLATFORM_PUBLICKEY_STR_L,
			new AssemblyVersion(2, 0, 5, 0),
			new AssemblyVersion(99, 0, 0, 0),
			null,
			s_MICROSOFT_PUBLICKEY_STR_L,
			s_VER_ASSEMBLYVERSION_STR_L,
			true
		}
	};

	private static readonly FrameworkAssemblyDictionary s_arFxPolicy = new FrameworkAssemblyDictionary
	{
		{ "Accessibility", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "CustomMarshalers", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "ISymWrapper", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.JScript", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_VS_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.VisualBasic", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_VS_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.VisualBasic.Compatibility", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_VS_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.VisualBasic.Compatibility.Data", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_VS_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.VisualC", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_VS_ASSEMBLYVERSION_STR_L },
		{ "mscorlib", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Configuration", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Configuration.Install", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Data", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Data.OracleClient", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Data.SqlXml", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Deployment", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Design", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.DirectoryServices", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.DirectoryServices.Protocols", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Drawing", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Drawing.Design", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.EnterpriseServices", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Management", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Messaging", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.Remoting", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.Serialization.Formatters.Soap", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Security", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceProcess", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Transactions", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.Mobile", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.RegularExpressions", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.Services", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Windows.Forms", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Xml", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "AspNetMMCExt", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "sysglobl", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Build.Engine", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Build.Framework", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationCFFRasterizer", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationCore", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework.Aero", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework.Classic", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework.Luna", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework.Royale", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationUI", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "ReachFramework", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Printing", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Speech", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "UIAutomationClient", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "UIAutomationClientsideProviders", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "UIAutomationProvider", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "UIAutomationTypes", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "WindowsBase", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "WindowsFormsIntegration", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "SMDiagnostics", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.IdentityModel", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.IdentityModel.Selectors", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.IO.Log", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.Serialization", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Install", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.WasHosting", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Workflow.Activities", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Workflow.ComponentModel", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Workflow.Runtime", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Transactions.Bridge", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Transactions.Bridge.Dtc", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.AddIn", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.AddIn.Contract", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ComponentModel.Composition", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Core", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Data.DataSetExtensions", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Data.Linq", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Xml.Linq", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.DirectoryServices.AccountManagement", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Management.Instrumentation", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Net", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Web", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.Extensions", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.Extensions.Design", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Windows.Presentation", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.WorkflowServices", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ComponentModel.DataAnnotations", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Data.Entity", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Data.Entity.Design", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Data.Services", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Data.Services.Client", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Data.Services.Design", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.Abstractions", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.DynamicData", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.DynamicData.Design", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.Entity", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.Entity.Design", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.Routing", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Build", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.CSharp", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Dynamic", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Numerics", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Xaml", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Workflow.Compiler", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Activities.Build", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Build.Conversion.v4.0", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Build.Tasks.v4.0", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Build.Utilities.v4.0", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Internal.Tasks.Dataflow", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.VisualBasic.Activities.Compiler", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_VS_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.VisualC.STLCLR", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_VC_STLCLR_ASSEMBLYVERSION_STR_L },
		{ "Microsoft.Windows.ApplicationServer.Applications", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationBuildTasks", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework.Aero2", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework.AeroLite", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework-SystemCore", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework-SystemData", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework-SystemDrawing", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework-SystemXml", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "PresentationFramework-SystemXmlLinq", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Activities", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Activities.Core.Presentation", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Activities.DurableInstancing", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Activities.Presentation", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ComponentModel.Composition.Registration", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Device", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.IdentityModel.Services", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.IO.Compression", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.IO.Compression.FileSystem", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Net.Http", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Net.Http.WebRequest", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Reflection.Context", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.Caching", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.DurableInstancing", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.WindowsRuntime", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.WindowsRuntime.UI.Xaml", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Activation", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Activities", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Channels", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Discovery", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Internals", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Routing", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.ServiceMoniker40", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.ApplicationServices", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.DataVisualization", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Web.DataVisualization.Design", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Windows.Controls.Ribbon", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Windows.Forms.DataVisualization", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Windows.Forms.DataVisualization.Design", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Windows.Input.Manipulations", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Xaml.Hosting", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "XamlBuildTask", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "XsdBuildTask", s_SHAREDLIB_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Numerics.Vectors", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Collections", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Collections.Concurrent", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ComponentModel", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ComponentModel.Annotations", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ComponentModel.EventBasedAsync", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Diagnostics.Contracts", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Diagnostics.Debug", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Diagnostics.Tools", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Diagnostics.Tracing", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Dynamic.Runtime", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Globalization", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.IO", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Linq", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Linq.Expressions", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Linq.Parallel", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Linq.Queryable", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Net.Http.Rtc", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Net.NetworkInformation", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Net.Primitives", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Net.Requests", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ObjectModel", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Reflection", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Reflection.Emit", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Reflection.Emit.ILGeneration", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Reflection.Emit.Lightweight", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Reflection.Extensions", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Reflection.Primitives", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Resources.ResourceManager", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.Extensions", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.Handles", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.InteropServices", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.InteropServices.WindowsRuntime", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.Numerics", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.Serialization.Json", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.Serialization.Primitives", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Runtime.Serialization.Xml", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Security.Principal", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Duplex", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Http", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.NetTcp", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Primitives", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.ServiceModel.Security", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Text.Encoding", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Text.Encoding.Extensions", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Text.RegularExpressions", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Threading", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Threading.Tasks", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Threading.Tasks.Parallel", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Threading.Timer", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Xml.ReaderWriter", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Xml.XDocument", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Xml.XmlSerializer", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Windows", s_MICROSOFT_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L },
		{ "System.Xml.Serialization", s_ECMA_PUBLICKEY_STR_L, s_VER_ASSEMBLYVERSION_STR_L }
	};

	public new static DesktopAssemblyIdentityComparer Default { get; } = new DesktopAssemblyIdentityComparer(default);

	internal AssemblyPortabilityPolicy PortabilityPolicy => policy;

	internal DesktopAssemblyIdentityComparer(AssemblyPortabilityPolicy policy)
	{
		this.policy = policy;
	}

	public static DesktopAssemblyIdentityComparer LoadFromXml(Stream input)
	{
		return new DesktopAssemblyIdentityComparer(AssemblyPortabilityPolicy.LoadFromXml(input));
	}

	internal override bool ApplyUnificationPolicies(ref AssemblyIdentity reference, ref AssemblyIdentity definition, AssemblyIdentityParts referenceParts, out bool isDefinitionFxAssembly)
	{
		if (reference.ContentType == AssemblyContentType.Default && AssemblyIdentityComparer.SimpleNameComparer.Equals(reference.Name, definition.Name) && AssemblyIdentityComparer.SimpleNameComparer.Equals(reference.Name, "mscorlib"))
		{
			isDefinitionFxAssembly = true;
			reference = definition;
			return true;
		}
		if (!reference.IsRetargetable && definition.IsRetargetable)
		{
			isDefinitionFxAssembly = false;
			return false;
		}
		reference = Port(reference);
		definition = Port(definition);
		if (reference.IsRetargetable && !definition.IsRetargetable)
		{
			if (!AssemblyIdentity.IsFullName(referenceParts))
			{
				isDefinitionFxAssembly = false;
				return false;
			}
			if (!IsOptionallyRetargetableAssembly(reference) || !AssemblyIdentity.KeysEqual(reference, definition))
			{
				reference = Retarget(reference);
			}
		}
		if (reference.IsRetargetable && definition.IsRetargetable)
		{
			isDefinitionFxAssembly = IsRetargetableAssembly(definition);
		}
		else
		{
			isDefinitionFxAssembly = IsFrameworkAssembly(definition);
		}
		return true;
	}

	private static bool IsFrameworkAssembly(AssemblyIdentity identity)
	{
		if (identity.ContentType != AssemblyContentType.Default)
		{
			return false;
		}
		if (!s_arFxPolicy.TryGetValue(identity.Name, out var value) || !value.PublicKeyToken.SequenceEqual(identity.PublicKeyToken))
		{
			return false;
		}
		int num = (identity.Version.Major << 16) | identity.Version.Minor;
		uint num2 = (uint)((value.Version.Major << 16) | value.Version.Minor);
		return (uint)num <= num2;
	}

	private static bool IsRetargetableAssembly(AssemblyIdentity identity)
	{
		IsRetargetableAssembly(identity, out var retargetable, out var _);
		return retargetable;
	}

	private static bool IsOptionallyRetargetableAssembly(AssemblyIdentity identity)
	{
		if (!identity.IsRetargetable)
		{
			return false;
		}
		IsRetargetableAssembly(identity, out var retargetable, out var portable);
		return retargetable & portable;
	}

	private static bool IsTriviallyNonRetargetable(AssemblyIdentity identity)
	{
		if (identity.CultureName.Length == 0 && identity.ContentType == AssemblyContentType.Default)
		{
			return !identity.IsStrongName;
		}
		return true;
	}

	private static void IsRetargetableAssembly(AssemblyIdentity identity, out bool retargetable, out bool portable)
	{
		retargetable = (portable = false);
		if (!IsTriviallyNonRetargetable(identity))
		{
			retargetable = s_arRetargetPolicy.TryGetValue(identity, out var value);
			portable = value.IsPortable;
		}
	}

	private static AssemblyIdentity Retarget(AssemblyIdentity identity)
	{
		if (IsTriviallyNonRetargetable(identity))
		{
			return identity;
		}
		if (s_arRetargetPolicy.TryGetValue(identity, out var value))
		{
			return new AssemblyIdentity(value.NewName ?? identity.Name, (Version)value.NewVersion, identity.CultureName, value.NewPublicKeyToken, hasPublicKey: false, identity.IsRetargetable);
		}
		return identity;
	}

	private AssemblyIdentity Port(AssemblyIdentity identity)
	{
		if (identity.IsRetargetable || !identity.IsStrongName || identity.ContentType != AssemblyContentType.Default)
		{
			return identity;
		}
		Version version = null;
		ImmutableArray<byte> publicKeyOrToken = default;
		AssemblyVersion assemblyVersion = (AssemblyVersion)identity.Version;
		if (assemblyVersion >= new AssemblyVersion(2, 0, 0, 0) && assemblyVersion <= new AssemblyVersion(5, 9, 0, 0))
		{
			if (identity.PublicKeyToken.SequenceEqual(s_SILVERLIGHT_PLATFORM_PUBLICKEY_STR_L))
			{
				if (!policy.SuppressSilverlightPlatformAssembliesPortability && (AssemblyIdentityComparer.SimpleNameComparer.Equals(identity.Name, "System") || AssemblyIdentityComparer.SimpleNameComparer.Equals(identity.Name, "System.Core")))
				{
					version = (Version)s_VER_ASSEMBLYVERSION_STR_L;
					publicKeyOrToken = s_ECMA_PUBLICKEY_STR_L;
				}
			}
			else if (identity.PublicKeyToken.SequenceEqual(s_SILVERLIGHT_PUBLICKEY_STR_L) && !policy.SuppressSilverlightLibraryAssembliesPortability)
			{
				if (AssemblyIdentityComparer.SimpleNameComparer.Equals(identity.Name, "Microsoft.VisualBasic"))
				{
					version = new Version(10, 0, 0, 0);
					publicKeyOrToken = s_MICROSOFT_PUBLICKEY_STR_L;
				}
				if (AssemblyIdentityComparer.SimpleNameComparer.Equals(identity.Name, "System.ComponentModel.Composition"))
				{
					version = (Version)s_VER_ASSEMBLYVERSION_STR_L;
					publicKeyOrToken = s_ECMA_PUBLICKEY_STR_L;
				}
			}
		}
		if (version == null)
		{
			return identity;
		}
		return new AssemblyIdentity(identity.Name, version, identity.CultureName, publicKeyOrToken, hasPublicKey: false, identity.IsRetargetable);
	}
}
