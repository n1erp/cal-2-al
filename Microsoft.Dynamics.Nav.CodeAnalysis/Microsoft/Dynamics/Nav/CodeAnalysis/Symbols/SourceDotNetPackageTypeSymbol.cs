using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceDotNetPackageTypeSymbol : DotNetPackageTypeSymbol
{
	private SymbolCompletionState state;

	private ImmutableArray<DotNetAssemblySymbol> lazyAssemblies;

	private ImmutableArray<Symbol> lazyMembers;

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public override ImmutableArray<PropertySymbol> Properties { get; } = ImmutableArray<PropertySymbol>.Empty;

	internal override bool RequiresCompletion { get; } = true;

	internal override ImmutableArray<DotNetAssemblySymbol> Assemblies
	{
		get
		{
			LazyGetMembers();
			return lazyAssemblies;
		}
	}

	private DotNetPackageSyntax Syntax { get; }

	public SourceDotNetPackageTypeSymbol(ModuleSymbol containingSymbol, DotNetPackageSyntax syntax)
		: base(containingSymbol)
	{
		Syntax = syntax;
		Locations = ImmutableArray.Create(syntax.Location);
		DeclaringSyntaxReferences = ImmutableArray.Create(syntax.GetReference());
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		instance.AddRange(Properties);
		instance.AddRange(Assemblies);
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, instance.ToImmutableAndFree(), default);
		return lazyMembers;
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
			case CompletionPart.Members:
				LazyGetMembers();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Members));
				break;
			case CompletionPart.MembersCompleted:
				ForceCompleteMembers(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.MembersCompleted));
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)98111);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private void LazyGetMembers()
	{
		if (state.HasComplete(CompletionPart.Members))
		{
			return;
		}
		lock (Syntax)
		{
			if (state.HasComplete(CompletionPart.Members))
			{
				return;
			}
			LazySetSymbol((DiagnosticBag d) =>
			{
				ArrayBuilder<DotNetAssemblySymbol> instance = ArrayBuilder<DotNetAssemblySymbol>.GetInstance();
				try
				{
					foreach (DotNetAssemblySyntax assembly in Syntax.Assemblies)
					{
						instance.Add(SourceDotNetAssemblySymbol.Create(this, assembly));
					}
					lazyAssemblies = instance.ToImmutable();
				}
				finally
				{
					instance.Free();
				}
			}, CompletionPart.Members, ref state);
		}
	}

	private void ForceCompleteMembers(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetMembers();
		if (!state.HasComplete(CompletionPart.MembersCompleted))
		{
			foreach (DotNetAssemblySymbol lazyAssembly in lazyAssemblies)
			{
				cancellationToken.ThrowIfCancellationRequested();
				lazyAssembly.ForceComplete(locationOpt, cancellationToken);
			}
			state.NotePartComplete(CompletionPart.MembersCompleted);
		}
	}
}
