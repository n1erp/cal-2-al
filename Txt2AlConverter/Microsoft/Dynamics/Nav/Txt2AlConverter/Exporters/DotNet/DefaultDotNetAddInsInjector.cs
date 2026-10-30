using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.DotNet;

public class DefaultDotNetAddInsInjector : BaseDotNetAddInsInjector
{
	public override DotNetPackageDefinition InjectAddIns(DotNetPackageDefinition package)
	{
		var array = new[]
		{
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.CodeViewer",
				TypeName = "Microsoft.Dynamics.Nav.Client.CodeViewer.CodeViewerControl",
				AddInName = "Microsoft.Dynamics.Nav.Client.CodeViewer"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.BusinessChart",
				TypeName = "Microsoft.Dynamics.Nav.Client.BusinessChart.BusinessChartAddIn",
				AddInName = "Microsoft.Dynamics.Nav.Client.BusinessChart"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.DynamicsOnlineConnect",
				TypeName = "Microsoft.Dynamics.Nav.Client.DynamicsOnlineConnect.IDynamicsOnlineConnectControlDefinition",
				AddInName = "Microsoft.Dynamics.Nav.Client.DynamicsOnlineConnect"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.FlowIntegration",
				TypeName = "Microsoft.Dynamics.Nav.Client.FlowIntegration.IFlowIntegration",
				AddInName = "Microsoft.Dynamics.Nav.Client.FlowIntegration"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.OAuthIntegration",
				TypeName = "Microsoft.Dynamics.Nav.Client.OAuthIntegration.OAuthIntegration",
				AddInName = "Microsoft.Dynamics.Nav.Client.OAuthIntegration"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.PageReady",
				TypeName = "Microsoft.Dynamics.Nav.Client.PageReady.IPageReady",
				AddInName = "Microsoft.Dynamics.Nav.Client.PageReady"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.PingPong",
				TypeName = "Microsoft.Dynamics.Nav.Client.PingPong.PingPongAddIn",
				AddInName = "Microsoft.Dynamics.Nav.Client.PingPong"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.PowerBIManagement",
				TypeName = "Microsoft.Dynamics.Nav.Client.PowerBIManagement.PowerBIManagement",
				AddInName = "Microsoft.Dynamics.Nav.Client.PowerBIManagement"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.RoleCenterSelector",
				TypeName = "Microsoft.Dynamics.Nav.Client.RoleCenterSelector.IRoleCenterSelector",
				AddInName = "Microsoft.Dynamics.Nav.Client.RoleCenterSelector"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.SocialListening",
				TypeName = "Microsoft.Dynamics.Nav.Client.SocialListening.ISocialListening",
				AddInName = "Microsoft.Dynamics.Nav.Client.SocialListening"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.TimelineVisualization",
				TypeName = "Microsoft.Dynamics.Nav.Client.TimelineVisualization.InteractiveTimelineVisualizationAddIn",
				AddInName = "Microsoft.Dynamics.Nav.Client.TimelineVisualization"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.VideoPlayer",
				TypeName = "Microsoft.Dynamics.Nav.Client.VideoPlayer.IVideoPlayer",
				AddInName = "Microsoft.Dynamics.Nav.Client.VideoPlayer"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.WebPageViewer",
				TypeName = "Microsoft.Dynamics.Nav.Client.WebPageViewer.IWebPageViewer",
				AddInName = "Microsoft.Dynamics.Nav.Client.WebPageViewer"
			},
			new
			{
				AssemblyName = "Microsoft.Dynamics.Nav.Client.WelcomeWizard",
				TypeName = "Microsoft.Dynamics.Nav.Client.WelcomeWizard.IWelcomeWizard",
				AddInName = "Microsoft.Dynamics.Nav.Client.WelcomeWizard"
			}
		};
		foreach (var anon in array)
		{
			AddAddIn(package, anon.AssemblyName, anon.TypeName, anon.AddInName);
		}
		return package;
	}
}
