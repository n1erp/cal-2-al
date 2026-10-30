using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class PropertyTypeInfo
{
	private class EqualityComparerImpl : IEqualityComparer<PropertyTypeInfo>
	{
		public bool Equals(PropertyTypeInfo x, PropertyTypeInfo y)
		{
			return x.Kind == y.Kind;
		}

		public int GetHashCode(PropertyTypeInfo obj)
		{
			return obj.Kind.GetHashCode();
		}
	}

	internal static IEqualityComparer<PropertyTypeInfo> EqualityComparer = new EqualityComparerImpl();

	public PropertyKind Kind { get; }

	public string Name { get; }

	public string TypeName { get; }

	public string ParserName { get; }

	public string MetadataName { get; }

	public string DefaultValue { get; }

	public bool AlwaysOutputValue => EmitMetadata == MetadataEmissionKind.Always;

	public bool IsObsolete { get; }

	public bool PrependObjectName { get; }

	private MetadataEmissionKind EmitMetadata { get; }

	public ModificationKind Modification { get; }

	public string SyntaxOwner { get; }

	internal ImmutableList<DependentProperty> DependentProperties { get; }

	internal ImmutableList<DependentProperty> DependentParentProperties { get; }

	internal ImmutableList<DependentProperty> DependentDeclaringApplicationObjectProperties { get; }

	internal ParseFunc ParseFunc { get; }

	public bool CanGenerateMetadata
	{
		get
		{
			if (!IsObsolete)
			{
				return EmitMetadata != MetadataEmissionKind.Never;
			}
			return false;
		}
	}

	public SymbolCompilationScope Scope { get; }

	internal bool EmitAsAttribute { get; }

	internal Func<string, string, PropertyValueSyntax, DiagnosticBag, bool> ValidatorFunc { get; }

	internal Func<IEnumerable<string>> RecommendationFunc { get; }

	internal string ValueKind { get; }

	internal VersionCompatibility Compatibility { get; }

	internal PropertyTypeInfo(PropertyKind kind, string name, string typeName, string parserName, ParseFunc parseFunc, IList<string> dependentProperties = null, IList<string> dependentParentProperties = null, IList<string> dependentDeclaringApplicationObjectProperties = null, string defaultValue = null, bool isObsolete = false, bool generateMetadata = true, ModificationKind modification = ModificationKind.None, Func<string, string, PropertyValueSyntax, DiagnosticBag, bool> validator = null, string metadataName = null, Func<IEnumerable<string>> recommendationFunc = null, string compilationScope = null, bool emitAsAttribute = true, string valueKind = null, bool prependObjectName = false, string syntaxOwner = null, VersionCompatibility compatibility = null)
	{
		Kind = kind;
		Name = name;
		TypeName = typeName;
		ParserName = parserName;
		ParseFunc = parseFunc;
		DefaultValue = defaultValue;
		EmitMetadata = GetMetadataEmissionKind(generateMetadata, defaultValue != null);
		IsObsolete = isObsolete;
		PrependObjectName = prependObjectName;
		Modification = modification;
		DependentProperties = dependentProperties?.Select((string d) => new DependentProperty(d)).ToImmutableList() ?? ImmutableList<DependentProperty>.Empty;
		DependentParentProperties = dependentParentProperties?.Select((string d) => new DependentProperty(d)).ToImmutableList() ?? ImmutableList<DependentProperty>.Empty;
		DependentDeclaringApplicationObjectProperties = dependentDeclaringApplicationObjectProperties?.Select((string d) => new DependentProperty(d)).ToImmutableList() ?? ImmutableList<DependentProperty>.Empty;
		ValidatorFunc = validator;
		RecommendationFunc = recommendationFunc;
		MetadataName = metadataName ?? name;
		if (string.IsNullOrEmpty(compilationScope) || !Enum.TryParse<SymbolCompilationScope>(compilationScope, out var result))
		{
			Scope = SymbolCompilationScope.Personalization;
		}
		else
		{
			Scope = result;
		}
		EmitAsAttribute = emitAsAttribute;
		ValueKind = valueKind;
		SyntaxOwner = syntaxOwner;
		Compatibility = compatibility;
	}

	private static MetadataEmissionKind GetMetadataEmissionKind(bool generateMetadata, bool hasDefaultValue)
	{
		if (!generateMetadata)
		{
			return MetadataEmissionKind.Never;
		}
		if (hasDefaultValue)
		{
			return MetadataEmissionKind.Always;
		}
		return MetadataEmissionKind.WhenSpecified;
	}

	internal InternalPropertyValueSyntax ParseValue(ObjectParser parser, ref InternalSyntaxToken equalsToken)
	{
		return ParseFunc(parser, this, ref equalsToken);
	}

	internal virtual string GetTypeDescription()
	{
		switch (ParserName)
		{
		case "ClientSideBooleanExpression":
		case "TextExpression":
			return "Expression";
		case "Constant":
			return "Value";
		case "CommaSeparated":
			return "List Of " + ValueKind;
		case "QualifiedObjectReference":
			return "Application Object";
		default:
			if (ValueKind != null)
			{
				return ValueKind;
			}
			switch (TypeName)
			{
			case "Int32":
				return NavTypeKind.Integer.ToString();
			case "Boolean":
			case "String":
				return TypeName;
			default:
				return ParserName ?? string.Empty;
			}
		}
	}

	public bool IsStringScriptProperty()
	{
		PropertyKind kind = Kind;
		if ((uint)(kind - 224) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal bool IsStringLinkProperty()
	{
		PropertyKind kind = Kind;
		if (kind == PropertyKind.HelpLink)
		{
			return true;
		}
		return false;
	}

	internal bool IsStringAlphaNumericProperty()
	{
		PropertyKind kind = Kind;
		if ((uint)(kind - 96) <= 3u)
		{
			return true;
		}
		return false;
	}
}
