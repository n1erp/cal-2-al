using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SourceDotNetAssemblySymbol : DotNetAssemblySymbol
{
	private SymbolCompletionState state;

	private ImmutableArray<PropertySymbol> lazyProperties;

	private DotNetAssemblyInfo lazyAssembly;

	private ImmutableArray<DotNetTypeDeclarationSymbol> lazyTypeDeclarations;

	private ImmutableArray<Symbol> lazyMembers;

	public override string Name { get; }

	public override ImmutableArray<PropertySymbol> Properties
	{
		get
		{
			LazyGetProperties();
			return lazyProperties;
		}
	}

	internal override DotNetAssemblyInfo AssemblyInfo
	{
		get
		{
			LazyGetAssembly();
			return lazyAssembly;
		}
	}

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	internal override bool RequiresCompletion { get; } = true;

	internal override ImmutableArray<DotNetTypeDeclarationSymbol> TypeDeclarations
	{
		get
		{
			LazyGetMembers();
			return lazyTypeDeclarations;
		}
	}

	private DotNetAssemblySyntax Syntax { get; }

	private SourceDotNetAssemblySymbol(SourceDotNetPackageTypeSymbol containingSymbol, DotNetAssemblySyntax syntax, string assemblyName, ImmutableArray<Location> locations)
		: base(containingSymbol)
	{
		Syntax = syntax;
		Name = assemblyName;
		Locations = locations;
		DeclaringSyntaxReferences = ImmutableArray.Create(syntax.GetReference());
	}

	public static SourceDotNetAssemblySymbol Create(SourceDotNetPackageTypeSymbol containingSymbol, DotNetAssemblySyntax syntax)
	{
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.AssemblyName));
		string assemblyName = syntax.AssemblyName.Unquoted();
		return new SourceDotNetAssemblySymbol(containingSymbol, syntax, assemblyName, locations);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		instance.AddRange(Properties);
		instance.AddRange(TypeDeclarations);
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
			case CompletionPart.Variables:
				ForceCompleteProperties(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Variables));
				break;
			case CompletionPart.ObjectsCompleted:
				LazyGetAssembly();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.ObjectsCompleted));
				break;
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
				state.NotePartComplete((CompletionPart)98063);
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
				ArrayBuilder<DotNetTypeDeclarationSymbol> instance = ArrayBuilder<DotNetTypeDeclarationSymbol>.GetInstance();
				try
				{
					foreach (DotNetTypeDeclarationSyntax typeDeclaration in Syntax.TypeDeclarations)
					{
						instance.Add(SourceDotNetTypeDeclarationSymbol.Create(this, typeDeclaration));
					}
					lazyTypeDeclarations = instance.ToImmutable();
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
			foreach (SourceDotNetTypeDeclarationSymbol lazyTypeDeclaration in lazyTypeDeclarations)
			{
				cancellationToken.ThrowIfCancellationRequested();
				lazyTypeDeclaration.ForceComplete(locationOpt, cancellationToken);
			}
			state.NotePartComplete(CompletionPart.MembersCompleted);
		}
	}

	private void LazyGetAssembly()
	{
		if (state.HasComplete(CompletionPart.ObjectsCompleted))
		{
			return;
		}
		lock (Syntax)
		{
			if (state.HasComplete(CompletionPart.ObjectsCompleted))
			{
				return;
			}
			LazySetSymbol((DiagnosticBag diagnostics) =>
			{
				if (!string.IsNullOrWhiteSpace(Name))
				{
					string stringPropertyValue = GetStringPropertyValue(PropertyKind.Version);
					string stringPropertyValue2 = GetStringPropertyValue(PropertyKind.Culture);
					string stringPropertyValue3 = GetStringPropertyValue(PropertyKind.PublicKeyToken);
					string assemblyName = DotNetUtilities.GetAssemblyName(Name, stringPropertyValue, stringPropertyValue2, stringPropertyValue3);
					lazyAssembly = ContainingModule.DotNetResolver.LoadAssembly(assemblyName);
					if (lazyAssembly == null)
					{
						string text = ContainingModule.DotNetResolver.AssemblyLocator.ProbingPaths.Join(", ");
						diagnostics.Add(ErrorCode.ERR_DotNetAssemblyCouldNotBeFound, Syntax.AssemblyName.Location, assemblyName, text);
					}
				}
			}, CompletionPart.ObjectsCompleted, ref state);
		}
	}

	private string GetStringPropertyValue(PropertyKind kind)
	{
		return this.GetBoundPropertyValue<BoundLiteralPropertyValue>(kind)?.Value.StringValue;
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
