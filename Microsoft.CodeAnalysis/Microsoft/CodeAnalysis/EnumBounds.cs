namespace Microsoft.CodeAnalysis;

internal static class EnumBounds
{
	internal static bool IsValid(this OptimizationLevel value)
	{
		if (value >= OptimizationLevel.Debug)
		{
			return value <= OptimizationLevel.Release;
		}
		return false;
	}

	internal static bool IsValid(this Platform value)
	{
		if (value >= Platform.AnyCpu)
		{
			return value <= Platform.Arm64;
		}
		return false;
	}

	internal static bool Requires64Bit(this Platform value)
	{
		if (value != Platform.X64 && value != Platform.Itanium)
		{
			return value == Platform.Arm64;
		}
		return true;
	}

	internal static bool Requires32Bit(this Platform value)
	{
		return value == Platform.X86;
	}

	internal static bool IsValid(this MetadataImportOptions value)
	{
		if ((int)value >= 0)
		{
			return (int)value <= 2;
		}
		return false;
	}

	internal static bool IsValid(this MetadataImageKind kind)
	{
		if ((int)kind >= 0)
		{
			return (int)kind <= 1;
		}
		return false;
	}

	internal static bool IsValid(this OutputKind value)
	{
		if (value >= OutputKind.ConsoleApplication)
		{
			return value <= OutputKind.WindowsRuntimeApplication;
		}
		return false;
	}

	internal static string GetDefaultExtension(this OutputKind kind)
	{
		switch (kind)
		{
		case OutputKind.ConsoleApplication:
		case OutputKind.WindowsApplication:
		case OutputKind.WindowsRuntimeApplication:
			return ".exe";
		case OutputKind.DynamicallyLinkedLibrary:
			return ".dll";
		case OutputKind.NetModule:
			return ".netmodule";
		case OutputKind.WindowsRuntimeMetadata:
			return ".winmdobj";
		default:
			return ".dll";
		}
	}

	internal static bool IsApplication(this OutputKind kind)
	{
		switch (kind)
		{
		case OutputKind.ConsoleApplication:
		case OutputKind.WindowsApplication:
		case OutputKind.WindowsRuntimeApplication:
			return true;
		case OutputKind.DynamicallyLinkedLibrary:
		case OutputKind.NetModule:
		case OutputKind.WindowsRuntimeMetadata:
			return false;
		default:
			return false;
		}
	}

	internal static bool IsNetModule(this OutputKind kind)
	{
		return kind == OutputKind.NetModule;
	}

	internal static bool IsWindowsRuntime(this OutputKind kind)
	{
		return kind == OutputKind.WindowsRuntimeMetadata;
	}

	internal static bool IsValid(this SymbolDisplayPartKind value)
	{
		if (value < SymbolDisplayPartKind.AliasName || value > SymbolDisplayPartKind.RangeVariableName)
		{
			if (value >= (SymbolDisplayPartKind)28)
			{
				return value <= (SymbolDisplayPartKind)29;
			}
			return false;
		}
		return true;
	}
}
