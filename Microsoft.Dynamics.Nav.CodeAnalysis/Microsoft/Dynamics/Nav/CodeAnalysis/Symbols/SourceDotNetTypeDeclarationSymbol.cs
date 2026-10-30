using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceDotNetTypeDeclarationSymbol : DotNetTypeDeclarationSymbol
{
	private readonly SourceDotNetAssemblySymbol containingSymbol;

	private SymbolCompletionState state;

	private ImmutableArray<PropertySymbol> lazyProperties;

	private DotNetTypeSymbol lazyDotNetClassTypeSymbol;

	public override Symbol ContainingSymbol => containingSymbol;

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public override ImmutableArray<PropertySymbol> Properties
	{
		get
		{
			LazyGetProperties();
			return lazyProperties;
		}
	}

	public override DotNetTypeSymbol DotNetType
	{
		get
		{
			LazyGetType();
			return lazyDotNetClassTypeSymbol;
		}
	}

	internal override bool RequiresCompletion { get; } = true;

	private DotNetTypeDeclarationSyntax Syntax { get; }

	private SourceDotNetTypeDeclarationSymbol(SourceDotNetAssemblySymbol containingSymbol, DotNetTypeDeclarationSyntax syntax, string typeName, string typeAlias, ImmutableArray<Location> locations)
		: base(containingSymbol, typeName, typeAlias)
	{
		this.containingSymbol = containingSymbol;
		Syntax = syntax;
		Locations = locations;
		DeclaringSyntaxReferences = ImmutableArray.Create(syntax.GetReference());
	}

	public static SourceDotNetTypeDeclarationSymbol Create(SourceDotNetAssemblySymbol containingSymbol, DotNetTypeDeclarationSyntax syntax)
	{
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.TypeName));
		string typeName = syntax.TypeName.Unquoted();
		string typeAlias = syntax.GetTypeAlias();
		return new SourceDotNetTypeDeclarationSymbol(containingSymbol, syntax, typeName, typeAlias, locations);
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.ReturnType:
				LazyGetType();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.ReturnType));
				break;
			case CompletionPart.Variables:
				ForceCompleteProperties(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Variables));
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)98279);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private void LazyGetType()
	{
		if (state.HasComplete(CompletionPart.ReturnType))
		{
			return;
		}
		lock (Syntax)
		{
			if (state.HasComplete(CompletionPart.ReturnType))
			{
				return;
			}
			LazySetSymbol((DiagnosticBag diagnostics) =>
			{
				DotNetAssemblyInfo assemblyInfo = containingSymbol.AssemblyInfo;
				if (assemblyInfo != null)
				{
					string text = Syntax.TypeName.Unquoted();
					if (text != null)
					{
						DotNetTypeInfo dotNetTypeInfo = ContainingModule.DotNetResolver.LoadType(assemblyInfo, text);
						if (dotNetTypeInfo == null)
						{
							diagnostics.Add(ErrorCode.ERR_DotNetTypeCouldNotBeFound, Syntax.TypeName.Location, text, assemblyInfo.FullName);
						}
						else
						{
							lazyDotNetClassTypeSymbol = (DotNetTypeSymbol)ContainingModule.DotNetResolver.MapDeclaredType(this, dotNetTypeInfo);
						}
					}
				}
			}, CompletionPart.ReturnType, ref state);
		}
	}

	private void ForceCompleteProperties(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetProperties();
		foreach (PropertySymbol lazyProperty in lazyProperties)
		{
			cancellationToken.ThrowIfCancellationRequested();
			lazyProperty.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.Variables);
	}

	private void LazyGetProperties()
	{
		if (!(lazyProperties != null))
		{
			ImmutableArray<PropertySymbol> value = Syntax.PropertyList?.Properties.CreatePropertySymbols(this) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyProperties, value, default);
		}
	}
}
