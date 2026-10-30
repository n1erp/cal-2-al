using System;
using System.Security.Cryptography;

namespace Microsoft.Dynamics.Nav.Types;

public static class CryptographyHelper
{
	public static string GenerateSaltedPasswordHash(string passwordHash, Guid salt)
	{
		byte[] array = salt.ToByteArray();
		return Convert.ToBase64String(new Rfc2898DeriveBytes(passwordHash, array, salt.Equals(Guid.Empty) ? 1 : 1000).GetBytes(array.Length));
	}

	public static string GeneratePasswordHash(string password)
	{
		return GenerateSaltedPasswordHash(password, Guid.Empty);
	}

	public static string GenerateSaltedPasswordHashFromPassword(string password, Guid salt)
	{
		return GenerateSaltedPasswordHash(GeneratePasswordHash(password), salt);
	}
}
