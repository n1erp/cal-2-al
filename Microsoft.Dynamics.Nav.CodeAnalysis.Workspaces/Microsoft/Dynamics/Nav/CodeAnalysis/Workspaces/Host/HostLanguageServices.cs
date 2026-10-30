using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.SignatureHelp;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public class HostLanguageServices : AbstractHostLanguageServices
{
	private ImmutableDictionary<Type, ILanguageService> serviceMap = ImmutableDictionary<Type, ILanguageService>.Empty;

	public HostWorkspaceServices WorkspaceServices { get; }

	internal virtual ICompilationFactoryService CompilationFactory => GetService<ICompilationFactoryService>();

	internal HostLanguageServices(HostWorkspaceServices workspaceServices)
	{
		WorkspaceServices = workspaceServices;
	}

	public override TLanguageService GetService<TLanguageService>()
	{
		if (TryGetService(typeof(TLanguageService), out var service))
		{
			return (TLanguageService)service;
		}
		return default;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	private bool TryGetService(Type serviceType, out ILanguageService service)
	{
		if (!serviceMap.TryGetValue(serviceType, out service))
		{
			service = ImmutableInterlocked.GetOrAdd<Type, ILanguageService>(ref serviceMap, serviceType, (Type svctype) =>
			{
				if (svctype == typeof(ICompilationFactoryService))
				{
					return new CompilationFactoryService();
				}
				if (svctype == typeof(ISyntaxTreeFactoryService))
				{
					return new SyntaxTreeFactoryService(this);
				}
				if (svctype == typeof(IClassificationService))
				{
					return new ClassificationService();
				}
				if (svctype == typeof(IRecommendationService))
				{
					return new RecommendationService();
				}
				if (svctype == typeof(CompletionService))
				{
					return new ALCompletionService(WorkspaceServices.Workspace);
				}
				if (svctype == typeof(CodeActionService))
				{
					return CodeActionService.Create();
				}
				if (svctype == typeof(IReferenceLoaderFactoryService))
				{
					return new ReferenceLoaderFactoryService();
				}
				if (svctype == typeof(IDotNetResolverFactoryService))
				{
					return new DotNetResolverFactoryService();
				}
				if (serviceType == typeof(ISymbolDisplayService))
				{
					return new SymbolDisplayService();
				}
				if (serviceType == typeof(IFindReferencesService))
				{
					return new FindReferencesService();
				}
				if (serviceType == typeof(ISignatureHelpService))
				{
					return new SignatureHelpService();
				}
				if (serviceType == typeof(ISymbolDeclarationService))
				{
					return new SymbolDeclarationService();
				}
				if (serviceType == typeof(ISyntaxFactsService))
				{
					return SyntaxFactsService.Instance;
				}
				if (serviceType == typeof(ISemanticFactsService))
				{
					return SemanticFactsService.Instance;
				}
				if (serviceType == typeof(ISyntaxFormattingService))
				{
					return SyntaxFormattingService.Instance;
				}
				return (serviceType == typeof(IFormattingService)) ? FormattingService.Instance : null;
			});
		}
		return service != null;
	}

	public TLanguageService GetRequiredService<TLanguageService>() where TLanguageService : ILanguageService
	{
		return GetService<TLanguageService>() ?? throw new InvalidOperationException(WorkspacesResources.WorkspaceServicesUnavailable);
	}
}
