using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceControlAddInTypeSymbol : ControlAddInTypeSymbol
{
	private static readonly ConcurrentDictionary<string, string> PublicKeyTokenCache = new ConcurrentDictionary<string, string>();

	public override string Name { get; }

	public override string MetadataName { get; }

	public override string PublicKeyToken => PublicKeyTokenCache.GetOrAdd(ContainingModule.Name, CalculatePublicKeyToken);

	internal override bool RequiresCompletion => true;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<Location> Locations => Members.Locations;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	internal ControlAddInObjectMembers Members { get; }

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	internal SourceControlAddInTypeSymbol(ModuleSymbol containingSymbol, ControlAddInSyntax declaration)
		: base(containingSymbol)
	{
		Members = new ControlAddInObjectMembers(this, declaration);
		Name = Members.Name;
		MetadataName = Members.Name.MangleUnquotedIdentifierName();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return Members.GetMembers();
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return Members.GetMembers(name);
	}

	public ImmutableArray<string> GetAllScripts()
	{
		return GetConstantListValue(PropertyKind.Scripts);
	}

	public ImmutableArray<string> GetAllStyleSheets()
	{
		return GetConstantListValue(PropertyKind.StyleSheets);
	}

	public ImmutableArray<string> GetLocalScripts()
	{
		return GetAllScripts().Where((string x) =>
		{
			Uri uri = UriUtilities.ParseUri(x);
			return (object)uri != null && !uri.IsAbsoluteUri;
		}).ToImmutableArray();
	}

	public ImmutableArray<string> GetExternalScripts()
	{
		return (from x in GetAllScripts()
			where UriUtilities.ParseUri(x)?.IsAbsoluteUri ?? false
			select x).ToImmutableArray();
	}

	public ImmutableArray<string> GetLocalStyleSheets()
	{
		return GetAllStyleSheets().Where((string x) =>
		{
			Uri uri = UriUtilities.ParseUri(x);
			return (object)uri != null && !uri.IsAbsoluteUri;
		}).ToImmutableArray();
	}

	public ImmutableArray<string> GetExternalStyleSheets()
	{
		return (from x in GetAllStyleSheets()
			where UriUtilities.ParseUri(x)?.IsAbsoluteUri ?? false
			select x).ToImmutableArray();
	}

	public ImmutableArray<string> GetAllImages()
	{
		return GetConstantListValue(PropertyKind.Images);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		Members.ForceComplete(locationOpt, cancellationToken);
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return Members.HasComplete(part);
	}

	private ImmutableArray<string> GetConstantListValue(PropertyKind propertyKind)
	{
		if (!(GetProperty(propertyKind)?.Property.PropertyValue is BoundConstantValueListPropertyValue { Values: var values }))
		{
			return ImmutableArray<string>.Empty;
		}
		return values.Select((ConstantValue x) => x.StringValue).ToImmutableArray();
	}

	private static string CalculatePublicKeyToken(string moduleName)
	{
		using SHA256 sHA = SHA256.Create();
		byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(moduleName));
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		for (int i = 0; i < 8; i++)
		{
			instance.Builder.Append(array[i].ToString("x2", CultureInfo.InvariantCulture));
		}
		return instance.ToStringAndFree();
	}
}
