using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal class CommonAssemblyWellKnownAttributeData<TNamedTypeSymbol> : WellKnownAttributeData, ISecurityAttributeTarget
{
	private string _assemblySignatureKeyAttributeSetting;

	private ThreeState _assemblyDelaySignAttributeSetting;

	private string _assemblyKeyFileAttributeSetting = WellKnownAttributeData.StringMissingValue;

	private string _assemblyKeyContainerAttributeSetting = WellKnownAttributeData.StringMissingValue;

	private Version _assemblyVersionAttributeSetting;

	private string _assemblyFileVersionAttributeSetting;

	private string _assemblyTitleAttributeSetting;

	private string _assemblyDescriptionAttributeSetting;

	private string _assemblyCultureAttributeSetting;

	private string _assemblyCompanyAttributeSetting;

	private string _assemblyProductAttributeSetting;

	private string _assemblyInformationalVersionAttributeSetting;

	private string _assemblyCopyrightAttributeSetting;

	private string _assemblyTrademarkAttributeSetting;

	private AssemblyFlags _assemblyFlagsAttributeSetting;

	private AssemblyHashAlgorithm? _assemblyAlgorithmIdAttributeSetting;

	private bool _hasCompilationRelaxationsAttribute;

	private bool _hasReferenceAssemblyAttribute;

	private bool? _runtimeCompatibilityWrapNonExceptionThrows;

	internal const bool WrapNonExceptionThrowsDefault = true;

	private bool _hasDebuggableAttribute;

	private SecurityWellKnownAttributeData _lazySecurityAttributeData;

	private HashSet<TNamedTypeSymbol> _forwardedTypes;

	public string AssemblySignatureKeyAttributeSetting
	{
		get
		{
			return _assemblySignatureKeyAttributeSetting;
		}
		set
		{
			_assemblySignatureKeyAttributeSetting = value;
		}
	}

	public ThreeState AssemblyDelaySignAttributeSetting
	{
		get
		{
			return _assemblyDelaySignAttributeSetting;
		}
		set
		{
			_assemblyDelaySignAttributeSetting = value;
		}
	}

	public string AssemblyKeyFileAttributeSetting
	{
		get
		{
			return _assemblyKeyFileAttributeSetting;
		}
		set
		{
			_assemblyKeyFileAttributeSetting = value;
		}
	}

	public string AssemblyKeyContainerAttributeSetting
	{
		get
		{
			return _assemblyKeyContainerAttributeSetting;
		}
		set
		{
			_assemblyKeyContainerAttributeSetting = value;
		}
	}

	public Version AssemblyVersionAttributeSetting
	{
		get
		{
			return _assemblyVersionAttributeSetting;
		}
		set
		{
			_assemblyVersionAttributeSetting = value;
		}
	}

	public string AssemblyFileVersionAttributeSetting
	{
		get
		{
			return _assemblyFileVersionAttributeSetting;
		}
		set
		{
			_assemblyFileVersionAttributeSetting = value;
		}
	}

	public string AssemblyTitleAttributeSetting
	{
		get
		{
			return _assemblyTitleAttributeSetting;
		}
		set
		{
			_assemblyTitleAttributeSetting = value;
		}
	}

	public string AssemblyDescriptionAttributeSetting
	{
		get
		{
			return _assemblyDescriptionAttributeSetting;
		}
		set
		{
			_assemblyDescriptionAttributeSetting = value;
		}
	}

	public string AssemblyCultureAttributeSetting
	{
		get
		{
			return _assemblyCultureAttributeSetting;
		}
		set
		{
			_assemblyCultureAttributeSetting = value;
		}
	}

	public string AssemblyCompanyAttributeSetting
	{
		get
		{
			return _assemblyCompanyAttributeSetting;
		}
		set
		{
			_assemblyCompanyAttributeSetting = value;
		}
	}

	public string AssemblyProductAttributeSetting
	{
		get
		{
			return _assemblyProductAttributeSetting;
		}
		set
		{
			_assemblyProductAttributeSetting = value;
		}
	}

	public string AssemblyInformationalVersionAttributeSetting
	{
		get
		{
			return _assemblyInformationalVersionAttributeSetting;
		}
		set
		{
			_assemblyInformationalVersionAttributeSetting = value;
		}
	}

	public string AssemblyCopyrightAttributeSetting
	{
		get
		{
			return _assemblyCopyrightAttributeSetting;
		}
		set
		{
			_assemblyCopyrightAttributeSetting = value;
		}
	}

	public string AssemblyTrademarkAttributeSetting
	{
		get
		{
			return _assemblyTrademarkAttributeSetting;
		}
		set
		{
			_assemblyTrademarkAttributeSetting = value;
		}
	}

	public AssemblyFlags AssemblyFlagsAttributeSetting
	{
		get
		{
			return _assemblyFlagsAttributeSetting;
		}
		set
		{
			_assemblyFlagsAttributeSetting = value;
		}
	}

	public AssemblyHashAlgorithm? AssemblyAlgorithmIdAttributeSetting
	{
		get
		{
			return _assemblyAlgorithmIdAttributeSetting;
		}
		set
		{
			_assemblyAlgorithmIdAttributeSetting = value;
		}
	}

	public bool HasCompilationRelaxationsAttribute
	{
		get
		{
			return _hasCompilationRelaxationsAttribute;
		}
		set
		{
			_hasCompilationRelaxationsAttribute = value;
		}
	}

	public bool HasReferenceAssemblyAttribute
	{
		get
		{
			return _hasReferenceAssemblyAttribute;
		}
		set
		{
			_hasReferenceAssemblyAttribute = value;
		}
	}

	public bool HasRuntimeCompatibilityAttribute => _runtimeCompatibilityWrapNonExceptionThrows.HasValue;

	public bool RuntimeCompatibilityWrapNonExceptionThrows
	{
		get
		{
			return _runtimeCompatibilityWrapNonExceptionThrows ?? true;
		}
		set
		{
			_runtimeCompatibilityWrapNonExceptionThrows = value;
		}
	}

	public bool HasDebuggableAttribute
	{
		get
		{
			return _hasDebuggableAttribute;
		}
		set
		{
			_hasDebuggableAttribute = value;
		}
	}

	public SecurityWellKnownAttributeData SecurityInformation => _lazySecurityAttributeData;

	public HashSet<TNamedTypeSymbol> ForwardedTypes
	{
		get
		{
			return _forwardedTypes;
		}
		set
		{
			_forwardedTypes = value;
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
}
