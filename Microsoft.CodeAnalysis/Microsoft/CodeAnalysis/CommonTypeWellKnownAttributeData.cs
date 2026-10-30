using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis;

internal class CommonTypeWellKnownAttributeData : WellKnownAttributeData, ISecurityAttributeTarget
{
	private bool _hasSpecialNameAttribute;

	private bool _hasSerializableAttribute;

	private bool _hasDefaultMemberAttribute;

	private bool _hasSuppressUnmanagedCodeSecurityAttribute;

	private SecurityWellKnownAttributeData _lazySecurityAttributeData;

	private bool _hasWindowsRuntimeImportAttribute;

	private string _guidString;

	private TypeLayout _layout;

	private CharSet _charSet;

	private bool _hasSecurityCriticalAttributes;

	private bool _hasExcludeFromCodeCoverageAttribute;

	public bool HasSpecialNameAttribute
	{
		get
		{
			return _hasSpecialNameAttribute;
		}
		set
		{
			_hasSpecialNameAttribute = value;
		}
	}

	public bool HasSerializableAttribute
	{
		get
		{
			return _hasSerializableAttribute;
		}
		set
		{
			_hasSerializableAttribute = value;
		}
	}

	public bool HasDefaultMemberAttribute
	{
		get
		{
			return _hasDefaultMemberAttribute;
		}
		set
		{
			_hasDefaultMemberAttribute = value;
		}
	}

	public bool HasSuppressUnmanagedCodeSecurityAttribute
	{
		get
		{
			return _hasSuppressUnmanagedCodeSecurityAttribute;
		}
		set
		{
			_hasSuppressUnmanagedCodeSecurityAttribute = value;
		}
	}

	internal bool HasDeclarativeSecurity
	{
		get
		{
			if (_lazySecurityAttributeData == null)
			{
				return HasSuppressUnmanagedCodeSecurityAttribute;
			}
			return true;
		}
	}

	public SecurityWellKnownAttributeData SecurityInformation => _lazySecurityAttributeData;

	public bool HasWindowsRuntimeImportAttribute
	{
		get
		{
			return _hasWindowsRuntimeImportAttribute;
		}
		set
		{
			_hasWindowsRuntimeImportAttribute = value;
		}
	}

	public string GuidString
	{
		get
		{
			return _guidString;
		}
		set
		{
			_guidString = value;
		}
	}

	public bool HasStructLayoutAttribute => _charSet != 0;

	public TypeLayout Layout => _layout;

	public CharSet MarshallingCharSet => _charSet;

	public bool HasSecurityCriticalAttributes
	{
		get
		{
			return _hasSecurityCriticalAttributes;
		}
		set
		{
			_hasSecurityCriticalAttributes = value;
		}
	}

	public bool HasExcludeFromCodeCoverageAttribute
	{
		get
		{
			return _hasExcludeFromCodeCoverageAttribute;
		}
		set
		{
			_hasExcludeFromCodeCoverageAttribute = value;
		}
	}

	SecurityWellKnownAttributeData ISecurityAttributeTarget.GetOrCreateData()
	{
		if (_lazySecurityAttributeData == null)
		{
			_lazySecurityAttributeData = new SecurityWellKnownAttributeData();
		}
		return _lazySecurityAttributeData;
	}

	public void SetStructLayout(TypeLayout layout, CharSet charSet)
	{
		_layout = layout;
		_charSet = charSet;
	}
}
