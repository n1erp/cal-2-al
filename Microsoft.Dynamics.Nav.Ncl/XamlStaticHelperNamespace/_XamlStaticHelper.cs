using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Xaml;

namespace XamlStaticHelperNamespace;

[GeneratedCode("XamlBuildTask", "15.0.0.0")]
internal class _XamlStaticHelper
{
	private static WeakReference schemaContextField;

	private static IList<Assembly> assemblyListField;

	internal static XamlSchemaContext SchemaContext
	{
		get
		{
			XamlSchemaContext xamlSchemaContext;
			if (schemaContextField != null)
			{
				xamlSchemaContext = (XamlSchemaContext)schemaContextField.Target;
				if (xamlSchemaContext != null)
				{
					return xamlSchemaContext;
				}
			}
			xamlSchemaContext = ((AssemblyList.Count <= 0) ? new XamlSchemaContext() : new XamlSchemaContext(AssemblyList));
			schemaContextField = new WeakReference(xamlSchemaContext);
			return xamlSchemaContext;
		}
	}

	internal static IList<Assembly> AssemblyList
	{
		get
		{
			if (assemblyListField == null)
			{
				assemblyListField = LoadAssemblies();
			}
			return assemblyListField;
		}
	}

	private static IList<Assembly> LoadAssemblies()
	{
		IList<Assembly> list = new List<Assembly>();
		list.Add(Load("Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("System.Collections, Version=4.0.11.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.Data.DataSetExtensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.DirectoryServices.AccountManagement, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.DirectoryServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.IdentityModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.IO.Compression, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.IO.Compression.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.IO, Version=4.1.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Management, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Net.Http, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Net.Http.WebRequest, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Resources.ResourceManager, Version=4.0.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Runtime.Caching, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Runtime, Version=4.1.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Runtime.InteropServices, Version=4.1.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Runtime.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.Security.Cryptography.Primitives, Version=4.0.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.ServiceModel.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.ServiceProcess, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Text.Encoding, Version=4.0.11.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Threading.Tasks, Version=4.0.11.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
		list.Add(Load("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("DocumentFormat.OpenXml, Version=2.5.5631.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Hyak.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.ApplicationInsights, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Azure.Common, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Azure.Common.NetFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Azure.KeyVault, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.CodeAnalysis.CSharp, Version=2.9.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.CodeAnalysis, Version=2.9.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Data.Edm, Version=5.8.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Data.OData, Version=5.8.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Data.Services.Client, Version=5.8.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Diagnostics.Tracing.EventSource, Version=1.0.26.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("Microsoft.Dynamics.Nav.Apps, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.CodeAnalysis, Version=3.0.5.4756, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Common.Exchange, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.DocumentService, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.DocumentService.Types, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.ExtensionProxySvc.Client, Version=1.1.19037.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.ExtensionProxySvc.Types, Version=1.1.19037.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.ExternalData, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Language, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.LicensingService.Client, Version=1.1.19193.5, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.LicensingService.Model, Version=1.1.19193.5, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Model, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Model.Parser, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Model.Tools, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.OpenXml, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Replication.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Replication.DataContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Types, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Watson, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Xrm, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Dynamics.Nav.Xrm.Proxy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Exchange.WebServices, Version=15.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Graph.Core, Version=1.14.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.Graph, Version=1.14.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.IdentityModel.Tokens, Version=5.2.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.IO.RecyclableMemoryStream, Version=1.2.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"));
		list.Add(Load("Microsoft.ReportViewer.WinForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"));
		list.Add(Load("Microsoft.SqlServer.Types, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"));
		list.Add(Load("Microsoft.Threading.Tasks, Version=1.0.12.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("Microsoft.Threading.Tasks.Extensions, Version=1.0.12.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("Microsoft.WindowsAzure.Storage, Version=7.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("Mono.Cecil, Version=0.10.0.0, Culture=neutral, PublicKeyToken=50cebf1cceb9d05e"));
		list.Add(Load("Newtonsoft.Json, Version=10.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed"));
		list.Add(Load("System.Collections.Immutable, Version=1.2.3.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.IdentityModel.Tokens.Jwt, Version=5.2.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Load("System.Net.Http.Extensions, Version=2.2.22.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Net.Http.Primitives, Version=4.2.22.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Reflection.Metadata, Version=1.4.3.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		list.Add(Load("System.Spatial, Version=5.8.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
		list.Add(Assembly.GetExecutingAssembly());
		return list;
	}

	private static Assembly Load(string assemblyNameVal)
	{
		AssemblyName assemblyName = new AssemblyName(assemblyNameVal);
		byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
		try
		{
			return Assembly.Load(assemblyName.FullName);
		}
		catch (Exception)
		{
			AssemblyName assemblyName2 = new AssemblyName(assemblyName.Name);
			if (publicKeyToken != null)
			{
				assemblyName2.SetPublicKeyToken(publicKeyToken);
			}
			return Assembly.Load(assemblyName2);
		}
	}
}
