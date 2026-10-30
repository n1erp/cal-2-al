using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SourceAttributeSymbol : AttributeSymbol
{
	private BoundAttribute lazyAttribute;

	private ImmutableArray<AttributeArgumentSymbol> lazyAttributeArguments;

	private SymbolCompletionState state;

	private bool searchedForEventObjects;

	private MethodSymbol eventMethod;

	private IApplicationObjectTypeSymbol eventApplicationObject;

	private readonly object eventMethodLock = new object();

	internal MemberAttributeSyntax Syntax { get; }

	public override Symbol ContainingSymbol { get; }

	internal override AttributeTypeInfo AttributeInfo { get; }

	public override string Name { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(Syntax.Name.Parent.GetReference());

	internal override BoundAttribute Attribute
	{
		get
		{
			LazyGetAttribute();
			return lazyAttribute;
		}
	}

	internal override ImmutableArray<AttributeArgumentSymbol> Arguments
	{
		get
		{
			if (lazyAttributeArguments == null)
			{
				ArrayBuilder<AttributeArgumentSymbol> instance = ArrayBuilder<AttributeArgumentSymbol>.GetInstance();
				MemberAttributeSyntax syntax = Syntax;
				if (syntax != null && (syntax.ArgumentList?.Arguments).HasValue)
				{
					foreach (BoundExpression argument in Attribute.Arguments)
					{
						instance.Add(new SourceAttributeArgumentSymbol(this, argument));
					}
				}
				lazyAttributeArguments = instance.ToImmutableAndFree();
			}
			return lazyAttributeArguments;
		}
	}

	public static SourceAttributeSymbol Create(Symbol owner, MemberAttributeSyntax syntax)
	{
		DebugAssertHelper.Assert(syntax.Name != null);
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax));
		return new SourceAttributeSymbol(owner, syntax, locations);
	}

	private SourceAttributeSymbol(Symbol containingSymbol, MemberAttributeSyntax syntax, ImmutableArray<Location> locations)
	{
		ContainingSymbol = containingSymbol;
		Syntax = syntax;
		Name = syntax.Name.Identifier.ValueText;
		Locations = locations;
		AttributeInfo = (AttributeTypeInfo)(syntax.GetAttributeTypeAnnotation()?.Data);
	}

	internal MethodSymbol GetEventMethod()
	{
		LazyGetEventMethod();
		return eventMethod;
	}

	internal IApplicationObjectTypeSymbol GetEventApplicationObject()
	{
		LazyGetEventMethod();
		return eventApplicationObject;
	}

	private void LazyGetEventMethod()
	{
		if (AttributeInfo.Category != AttributeCategory.EventSubscriber || searchedForEventObjects || Attribute.HasAnyErrors)
		{
			return;
		}
		lock (eventMethodLock)
		{
			eventMethod = FindEventMethod();
			searchedForEventObjects = true;
		}
	}

	private MethodSymbol FindEventMethod()
	{
		BoundOptionAccess boundOptionAccess = (BoundOptionAccess)Attribute.Arguments[0];
		BoundExpression boundExpression = Attribute.Arguments[1];
		BoundExpression boundExpression2 = Attribute.Arguments[2];
		SymbolKind objectKind = (SymbolKind)Enum.Parse(typeof(SymbolKind), boundOptionAccess.OptionSymbol.Name, ignoreCase: true);
		int objectId = ((boundExpression.Kind == BoundKind.ApplicationObjectAccess) ? ((BoundApplicationObjectAccess)boundExpression).ApplicationObjectTypeSymbol.Id : boundExpression.ConstantValue.Int32Value);
		string stringValue = boundExpression2.ConstantValue.StringValue;
		return FindEvent(DeclaringCompilation, objectKind, objectId, stringValue, boundExpression.Syntax.GetLocation());
	}

	private MethodSymbol FindEvent(Compilation compilation, SymbolKind objectKind, int objectId, string eventName, Location applicationIdLocation)
	{
		IApplicationObjectTypeSymbol applicationObjectTypeSymbol = (eventApplicationObject = GetApplicationObjectTypeSymbolById(objectKind, objectId, DeclaringCompilation.DeclarationDiagnostics, applicationIdLocation));
		if (applicationObjectTypeSymbol == null)
		{
			return null;
		}
		foreach (ISymbol member in applicationObjectTypeSymbol.GetMembers(eventName))
		{
			if (member.Kind == SymbolKind.Method)
			{
				return (MethodSymbol)member;
			}
		}
		foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in compilation.GetApplicationObjectExtensionTypeSymbolsAcrossModules(applicationObjectTypeSymbol))
		{
			foreach (ISymbol member2 in applicationObjectExtensionTypeSymbolsAcrossModule.GetMembers(eventName))
			{
				if (member2.Kind == SymbolKind.Method)
				{
					return (MethodSymbol)member2;
				}
			}
		}
		return null;
	}

	private IApplicationObjectTypeSymbol GetApplicationObjectTypeSymbolById(SymbolKind kind, int id, DiagnosticBag diagnostics, Location errorLocation)
	{
		ImmutableArray<IApplicationObjectTypeSymbol> applicationObjectTypeSymbolsByIdAcrossModules = DeclaringCompilation.GetApplicationObjectTypeSymbolsByIdAcrossModules(kind, id);
		if (applicationObjectTypeSymbolsByIdAcrossModules.Length == 0)
		{
			diagnostics.Add(ErrorCode.ERR_MissingApplicationObject, errorLocation, kind.ToString(), id.ToString(CultureInfo.InvariantCulture));
		}
		else
		{
			if (applicationObjectTypeSymbolsByIdAcrossModules.Length == 1)
			{
				return applicationObjectTypeSymbolsByIdAcrossModules[0];
			}
			diagnostics.Add(ErrorCode.ERR_AmbigReference, errorLocation, id.ToString(CultureInfo.InvariantCulture));
		}
		return null;
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Parameters:
			case CompletionPart.ReturnType:
				LazyGetAttribute();
				break;
			case CompletionPart.Variables:
				CheckAttribute();
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)98275);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private void CheckAttribute()
	{
		if (state.NotePartComplete(CompletionPart.Variables))
		{
			CompilationTarget target = DeclaringCompilation.Options.Target;
			if (!this.CanCompileWithTarget(target, ContainingSymbol.GetTypeSymbol()))
			{
				DeclaringCompilation.DeclarationDiagnostics.Add(ErrorCode.ERR_SymbolCannotBeUsedInThisContext, Syntax.Location, Name, target.ToString());
			}
			AttributeInfo?.Validator?.Validate(this, DeclaringCompilation.DeclarationDiagnostics);
		}
	}

	private void LazyGetAttribute()
	{
		if (state.HasComplete(CompletionPart.ReturnType))
		{
			return;
		}
		lock (Syntax.Name.Parent)
		{
			if (state.NotePartComplete(CompletionPart.Parameters))
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				try
				{
					Binder binder = DeclaringCompilation.GetBinderFactory(Syntax.SyntaxTree).GetBinder(Syntax);
					lazyAttribute = binder.BindAttribute(Syntax, ContainingSymbol, instance);
					binder.ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(instance, this, Syntax);
					AddDeclarationDiagnostics(instance);
					return;
				}
				finally
				{
					instance.Free();
					state.NotePartComplete(CompletionPart.ReturnType);
				}
			}
		}
	}
}
