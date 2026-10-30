using System;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SourceDotNetEventTriggerSymbol : SourceMethodOrTriggerSymbol
{
	private readonly ObjectTypeSymbol containingObject;

	private readonly EventTriggerDeclarationSyntax triggerDeclarationSyntax;

	private VariableSymbol lazyEventPublisher;

	private DotNetEventSymbol lazyEventSymbol;

	public override bool IsLocal { get; } = true;

	public override MethodKind MethodKind { get; } = MethodKind.EventTrigger;

	public override Accessibility DeclaredAccessibility { get; }

	public override string Name { get; }

	public override string MetadataName { get; }

	public string PublisherName { get; }

	public string EventName { get; }

	public VariableSymbol Publisher
	{
		get
		{
			if (lazyEventPublisher != null)
			{
				return lazyEventPublisher;
			}
			VariableSymbol value = containingObject.GlobalVariables.FirstOrDefault((VariableSymbol x) => string.Equals(x.Name, PublisherName, StringComparison.Ordinal));
			Interlocked.Exchange(ref lazyEventPublisher, value);
			return lazyEventPublisher;
		}
	}

	public DotNetEventSymbol Event
	{
		get
		{
			if (lazyEventSymbol != null)
			{
				return lazyEventSymbol;
			}
			VariableSymbol publisher = Publisher;
			if ((object)publisher == null || publisher.Type?.NavTypeKind != NavTypeKind.DotNet)
			{
				return null;
			}
			DotNetEventSymbol value = (DotNetEventSymbol)((DotNetTypeSymbol)Publisher.Type).Events.FirstOrDefault((EventSymbol x) => string.Equals(x.Name, EventName, StringComparison.Ordinal));
			Interlocked.Exchange(ref lazyEventSymbol, value);
			return lazyEventSymbol;
		}
	}

	private SourceDotNetEventTriggerSymbol(ObjectTypeSymbol containingSymbol, EventTriggerDeclarationSyntax methodOrTriggerDeclarationSyntax)
		: base(containingSymbol, methodOrTriggerDeclarationSyntax)
	{
		triggerDeclarationSyntax = methodOrTriggerDeclarationSyntax;
		containingObject = containingSymbol;
		PublisherName = methodOrTriggerDeclarationSyntax.Publisher.Unquoted();
		EventName = methodOrTriggerDeclarationSyntax.Name.Unquoted();
		MetadataName = (Name = methodOrTriggerDeclarationSyntax.GetName()).MangleUnquotedIdentifierName();
	}

	public static SourceDotNetEventTriggerSymbol Create(ObjectTypeSymbol containingSymbol, EventTriggerDeclarationSyntax eventTriggerDeclarationSyntax)
	{
		return new SourceDotNetEventTriggerSymbol(containingSymbol, eventTriggerDeclarationSyntax);
	}

	protected override void PerformAdditionalMethodChecks(DiagnosticBag diagnostics)
	{
		if (string.IsNullOrEmpty(PublisherName) || string.IsNullOrEmpty(EventName))
		{
			return;
		}
		VariableSymbol publisher = Publisher;
		if ((object)publisher == null || publisher.Type?.NavTypeKind != NavTypeKind.DotNet)
		{
			diagnostics.Add(ErrorCode.ERR_InvalidDotNetEventPublisher, triggerDeclarationSyntax.Publisher.Location, PublisherName);
			return;
		}
		if (!Publisher.HasWithEventsAttribute())
		{
			diagnostics.Add(ErrorCode.ERR_InvalidDotNetEventPublisher, triggerDeclarationSyntax.Publisher.Location, PublisherName);
			return;
		}
		EventSymbol eventSymbol = Event;
		if (eventSymbol == null)
		{
			diagnostics.Add(ErrorCode.ERR_UnknownDotNetEvent, triggerDeclarationSyntax.Publisher.Location, PublisherName, EventName);
		}
		else
		{
			eventSymbol.CheckSubscriberParameters(this, diagnostics, enforceExactVarMatch: true);
		}
	}
}
