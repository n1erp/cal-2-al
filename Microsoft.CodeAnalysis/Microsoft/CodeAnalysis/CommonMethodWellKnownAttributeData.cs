using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal class CommonMethodWellKnownAttributeData : WellKnownAttributeData, ISecurityAttributeTarget
{
	private readonly bool _preserveSigFirstWriteWins;

	private DllImportData _platformInvokeInfo;

	private bool _dllImportPreserveSig;

	private int _dllImportIndex;

	private int _methodImplIndex;

	private MethodImplAttributes _attributes;

	private int _preserveSigIndex;

	private bool _hasSpecialNameAttribute;

	private bool _hasDynamicSecurityMethodAttribute;

	private bool _hasSuppressUnmanagedCodeSecurityAttribute;

	private SecurityWellKnownAttributeData _lazySecurityAttributeData;

	private bool _hasExcludeFromCodeCoverageAttribute;

	public DllImportData DllImportPlatformInvokeData => _platformInvokeInfo;

	public MethodImplAttributes MethodImplAttributes
	{
		get
		{
			MethodImplAttributes methodImplAttributes = _attributes;
			if (_dllImportPreserveSig || _preserveSigIndex >= 0)
			{
				methodImplAttributes |= MethodImplAttributes.PreserveSig;
			}
			if (_dllImportIndex >= 0 && !_dllImportPreserveSig)
			{
				if (_preserveSigFirstWriteWins)
				{
					if ((_preserveSigIndex == -1 || _dllImportIndex < _preserveSigIndex) && (_methodImplIndex == -1 || (_attributes & MethodImplAttributes.PreserveSig) == 0 || _dllImportIndex < _methodImplIndex))
					{
						methodImplAttributes &= (MethodImplAttributes)(-129);
					}
				}
				else if (_dllImportIndex > _preserveSigIndex && (_dllImportIndex > _methodImplIndex || (_attributes & MethodImplAttributes.PreserveSig) == 0))
				{
					methodImplAttributes &= (MethodImplAttributes)(-129);
				}
			}
			return methodImplAttributes;
		}
	}

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

	public bool HasDynamicSecurityMethodAttribute
	{
		get
		{
			return _hasDynamicSecurityMethodAttribute;
		}
		set
		{
			_hasDynamicSecurityMethodAttribute = value;
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

	public CommonMethodWellKnownAttributeData(bool preserveSigFirstWriteWins)
	{
		_preserveSigFirstWriteWins = preserveSigFirstWriteWins;
		_dllImportIndex = (_methodImplIndex = (_preserveSigIndex = -1));
	}

	public CommonMethodWellKnownAttributeData()
		: this(preserveSigFirstWriteWins: false)
	{
	}

	public void SetPreserveSignature(int attributeIndex)
	{
		_preserveSigIndex = attributeIndex;
	}

	public void SetMethodImplementation(int attributeIndex, MethodImplAttributes attributes)
	{
		_attributes = attributes;
		_methodImplIndex = attributeIndex;
	}

	public void SetDllImport(int attributeIndex, string moduleName, string entryPointName, MethodImportAttributes flags, bool preserveSig)
	{
		_platformInvokeInfo = new DllImportData(moduleName, entryPointName, flags);
		_dllImportIndex = attributeIndex;
		_dllImportPreserveSig = preserveSig;
	}

	SecurityWellKnownAttributeData ISecurityAttributeTarget.GetOrCreateData()
	{
		if (_lazySecurityAttributeData == null)
		{
			_lazySecurityAttributeData = new SecurityWellKnownAttributeData();
		}
		return _lazySecurityAttributeData;
	}
}
