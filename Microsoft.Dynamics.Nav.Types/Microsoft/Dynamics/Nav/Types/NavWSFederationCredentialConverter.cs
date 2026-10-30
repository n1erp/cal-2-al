using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Microsoft.Dynamics.Nav.Types;

public class NavWSFederationCredentialConverter
{
	public const string WSFederationAuthenticationPrefix = " WSFED:";

	private readonly string username;

	private readonly string authenticationKey;

	private readonly string rawToken;

	private readonly string passwordField;

	private readonly string federationOrigin;

	private readonly string tokenId;

	private readonly AuthorizationCodeContext authorizationCodeContext;

	private readonly string tokenType;

	public string PasswordField => passwordField;

	public string UserName => username;

	public string AuthenticationKey => authenticationKey;

	public string RawToken => rawToken;

	public AuthorizationCodeContext AuthorizationCodeContext => authorizationCodeContext;

	public string WSFederationOrigin => TransformTokenOrigin(federationOrigin);

	public string TokenId => tokenId;

	public string TokenType => tokenType;

	public NavWSFederationCredentialConverter(string userName, string federationOrigin, string authenticationKey, string rawToken, string tokenType, AuthorizationCodeContext authorizationCodeContext)
	{
		if (string.IsNullOrWhiteSpace(rawToken))
		{
			throw new ArgumentNullException("rawToken");
		}
		username = userName ?? string.Empty;
		this.federationOrigin = federationOrigin ?? string.Empty;
		this.authenticationKey = authenticationKey ?? string.Empty;
		this.rawToken = rawToken;
		tokenId = Guid.NewGuid().ToString("d");
		this.tokenType = tokenType;
		this.authorizationCodeContext = authorizationCodeContext ?? AuthorizationCodeContext.Empty;
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8);
		binaryWriter.Write(username);
		binaryWriter.Write(this.authenticationKey);
		binaryWriter.Write(this.rawToken);
		binaryWriter.Write(this.federationOrigin);
		binaryWriter.Write(tokenId);
		binaryWriter.Write(this.tokenType);
		new BinaryFormatter().Serialize(memoryStream, this.authorizationCodeContext);
		passwordField = Convert.ToBase64String(memoryStream.ToArray());
	}

	public NavWSFederationCredentialConverter(string passwordField)
	{
		this.passwordField = passwordField;
		using MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(passwordField));
		using BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);
		username = binaryReader.ReadString();
		authenticationKey = binaryReader.ReadString();
		rawToken = binaryReader.ReadString();
		federationOrigin = binaryReader.ReadString();
		tokenId = binaryReader.ReadString();
		tokenType = binaryReader.ReadString();
		BinaryFormatter binaryFormatter = new BinaryFormatter
		{
			Binder = new NavSerializationBinder(typeof(AuthorizationCodeContext))
		};
		authorizationCodeContext = binaryFormatter.Deserialize(memoryStream) as AuthorizationCodeContext;
	}

	public static string TransformTokenOrigin(string origin)
	{
		if (origin.Contains("login.windows.net"))
		{
			return origin.Replace("login.windows.net", "sts.windows.net");
		}
		return origin;
	}
}
