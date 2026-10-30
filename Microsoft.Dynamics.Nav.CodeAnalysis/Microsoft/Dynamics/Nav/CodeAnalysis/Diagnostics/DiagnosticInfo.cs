using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal class DiagnosticInfo : IFormattable
{
	private static ImmutableDictionary<int, DiagnosticDescriptor> errorCodeToDescriptorMap = ImmutableDictionary<int, DiagnosticDescriptor>.Empty;

	private static readonly ImmutableArray<string> CompilerErrorCustomTags = ImmutableArray.Create("Compiler", "Telemetry", "NotConfigurable");

	private static readonly ImmutableArray<string> CompilerNonErrorCustomTags = ImmutableArray.Create("Compiler", "Telemetry");

	public int Code { get; }

	public DiagnosticDescriptor Descriptor => GetOrCreateDescriptor(Code, DefaultSeverity, MessageProvider);

	public DiagnosticSeverity Severity { get; }

	public DiagnosticSeverity DefaultSeverity { get; }

	public int WarningLevel
	{
		get
		{
			if (Severity != DefaultSeverity)
			{
				return Diagnostic.GetDefaultWarningLevel(Severity);
			}
			return MessageProvider.GetWarningLevel(Code);
		}
	}

	public bool IsWarningAsError
	{
		get
		{
			if (DefaultSeverity == DiagnosticSeverity.Warning)
			{
				return Severity == DiagnosticSeverity.Error;
			}
			return false;
		}
	}

	public string Category => MessageProvider.GetCategory(Code);

	internal ImmutableArray<string> CustomTags => GetCustomTags(DefaultSeverity);

	public virtual IReadOnlyList<Location> AdditionalLocations => SpecializedCollections.EmptyReadOnlyList<Location>();

	public string MessageIdentifier => MessageProvider.GetIdForErrorCode(Code);

	internal object[] Arguments { get; }

	internal CommonMessageProvider MessageProvider { get; }

	internal DiagnosticInfo(CommonMessageProvider messageProvider, int errorCode)
	{
		MessageProvider = messageProvider;
		Code = errorCode;
		DefaultSeverity = messageProvider.GetSeverity(errorCode);
		Severity = DefaultSeverity;
	}

	internal DiagnosticInfo(CommonMessageProvider messageProvider, int errorCode, params object[] arguments)
		: this(messageProvider, errorCode)
	{
		Arguments = arguments;
	}

	private DiagnosticInfo(DiagnosticInfo original, DiagnosticSeverity overridenSeverity)
	{
		MessageProvider = original.MessageProvider;
		Code = original.Code;
		DefaultSeverity = original.DefaultSeverity;
		Arguments = original.Arguments;
		Severity = overridenSeverity;
	}

	internal static DiagnosticDescriptor GetDescriptor(int errorCode, CommonMessageProvider messageProvider)
	{
		DiagnosticSeverity severity = messageProvider.GetSeverity(errorCode);
		return GetOrCreateDescriptor(errorCode, severity, messageProvider);
	}

	private static DiagnosticDescriptor GetOrCreateDescriptor(int errorCode, DiagnosticSeverity defaultSeverity, CommonMessageProvider messageProvider)
	{
		return ImmutableInterlocked.GetOrAdd(ref errorCodeToDescriptorMap, errorCode, (int code) => CreateDescriptor(code, defaultSeverity, messageProvider));
	}

	private static DiagnosticDescriptor CreateDescriptor(int errorCode, DiagnosticSeverity defaultSeverity, CommonMessageProvider messageProvider)
	{
		string idForErrorCode = messageProvider.GetIdForErrorCode(errorCode);
		LocalizableString title = messageProvider.GetTitle(errorCode);
		LocalizableString description = messageProvider.GetDescription(errorCode);
		LocalizableString messageFormat = messageProvider.GetMessageFormat(errorCode);
		string helpLink = messageProvider.GetHelpLink(errorCode);
		string category = messageProvider.GetCategory(errorCode);
		ImmutableArray<string> customTags = GetCustomTags(defaultSeverity);
		return new DiagnosticDescriptor(idForErrorCode, title, messageFormat, category, defaultSeverity, isEnabledByDefault: true, description, helpLink, customTags);
	}

	[Conditional("DEBUG")]
	internal static void AssertArgumentsAreValid(object[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			DebugAssertHelper.Assert(args[i] != null);
		}
	}

	internal DiagnosticInfo(CommonMessageProvider messageProvider, bool isWarningAsError, int errorCode, params object[] arguments)
		: this(messageProvider, errorCode, arguments)
	{
		DebugAssertHelper.Assert(!isWarningAsError || DefaultSeverity == DiagnosticSeverity.Warning);
		if (isWarningAsError)
		{
			Severity = DiagnosticSeverity.Error;
		}
	}

	internal DiagnosticInfo GetInstanceWithSeverity(DiagnosticSeverity severity)
	{
		return new DiagnosticInfo(this, severity);
	}

	private static ImmutableArray<string> GetCustomTags(DiagnosticSeverity defaultSeverity)
	{
		if (defaultSeverity != DiagnosticSeverity.Error)
		{
			return CompilerNonErrorCustomTags;
		}
		return CompilerErrorCustomTags;
	}

	internal bool IsNotConfigurable()
	{
		return DefaultSeverity == DiagnosticSeverity.Error;
	}

	public virtual string GetMessage(IFormatProvider formatProvider = null)
	{
		string text = MessageProvider.LoadMessage(Code, formatProvider as CultureInfo);
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		if (Arguments == null || Arguments.Length == 0)
		{
			return text;
		}
		return string.Format(formatProvider, text, GetArgumentsToUse(formatProvider));
	}

	protected object[] GetArgumentsToUse(IFormatProvider formatProvider)
	{
		object[] array = null;
		for (int i = 0; i < Arguments.Length; i++)
		{
			if (Arguments[i] is DiagnosticInfo diagnosticInfo)
			{
				array = InitializeArgumentListIfNeeded(array);
				array[i] = diagnosticInfo.GetMessage(formatProvider);
			}
		}
		return array ?? Arguments;
	}

	private object[] InitializeArgumentListIfNeeded(object[] argumentsToUse)
	{
		if (argumentsToUse != null)
		{
			return argumentsToUse;
		}
		object[] array = new object[Arguments.Length];
		Array.Copy(Arguments, array, array.Length);
		return array;
	}

	public override string ToString()
	{
		return ToString(null);
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return ((IFormattable)this).ToString((string)null, formatProvider);
	}

	string IFormattable.ToString(string format, IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "{0}: {1}", MessageProvider.GetMessagePrefix(MessageIdentifier, Severity, IsWarningAsError, formatProvider as CultureInfo), GetMessage(formatProvider));
	}

	public override int GetHashCode()
	{
		int num = Code;
		if (Arguments != null)
		{
			for (int i = 0; i < Arguments.Length; i++)
			{
				num = Hash.Combine(Arguments[i], num);
			}
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		DiagnosticInfo diagnosticInfo = obj as DiagnosticInfo;
		bool result = false;
		if (diagnosticInfo != null && diagnosticInfo.Code == Code && GetType() == obj.GetType())
		{
			if (Arguments == null && diagnosticInfo.Arguments == null)
			{
				result = true;
			}
			else if (Arguments != null && diagnosticInfo.Arguments != null && Arguments.Length == diagnosticInfo.Arguments.Length)
			{
				result = true;
				for (int i = 0; i < Arguments.Length; i++)
				{
					if (Arguments[i] != diagnosticInfo.Arguments[i])
					{
						result = false;
						break;
					}
				}
			}
		}
		return result;
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		return Code switch
		{
			-1 => "Unresolved DiagnosticInfo", 
			-2 => "Void DiagnosticInfo", 
			_ => ToString(), 
		};
	}

	internal virtual DiagnosticInfo GetResolvedInfo()
	{
		throw new NotSupportedException();
	}
}
