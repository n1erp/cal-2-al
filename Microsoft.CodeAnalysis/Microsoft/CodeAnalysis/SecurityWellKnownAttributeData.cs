using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis;

internal sealed class SecurityWellKnownAttributeData
{
	private byte[] _lazySecurityActions;

	private string[] _lazyPathsForPermissionSetFixup;

	public void SetSecurityAttribute(int attributeIndex, DeclarativeSecurityAction action, int totalSourceAttributes)
	{
		if (_lazySecurityActions == null)
		{
			Interlocked.CompareExchange(ref _lazySecurityActions, new byte[totalSourceAttributes], null);
		}
		_lazySecurityActions[attributeIndex] = (byte)action;
	}

	public void SetPathForPermissionSetAttributeFixup(int attributeIndex, string resolvedFilePath, int totalSourceAttributes)
	{
		if (_lazyPathsForPermissionSetFixup == null)
		{
			Interlocked.CompareExchange(ref _lazyPathsForPermissionSetFixup, new string[totalSourceAttributes], null);
		}
		_lazyPathsForPermissionSetFixup[attributeIndex] = resolvedFilePath;
	}

	public IEnumerable<SecurityAttribute> GetSecurityAttributes<T>(ImmutableArray<T> customAttributes) where T : ICustomAttribute
	{
		if (_lazySecurityActions == null)
		{
			yield break;
		}
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (_lazySecurityActions[i] != 0)
			{
				DeclarativeSecurityAction action = (DeclarativeSecurityAction)_lazySecurityActions[i];
				ICustomAttribute customAttribute = customAttributes[i];
				string[] lazyPathsForPermissionSetFixup = _lazyPathsForPermissionSetFixup;
				if (((lazyPathsForPermissionSetFixup != null) ? lazyPathsForPermissionSetFixup[i] : null) != null)
				{
					customAttribute = new PermissionSetAttributeWithFileReference(customAttribute, _lazyPathsForPermissionSetFixup[i]);
				}
				yield return new SecurityAttribute(action, customAttribute);
			}
		}
	}
}
