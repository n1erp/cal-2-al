using Microsoft.Dynamics.Nav.Runtime.Apps;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaApplicationObjectLoader : INCLMetaApplicationObjectLoader
{
	public INCLObjectXmlMetadataLoader XmlMetadataLoader { get; }

	public INCLCodeLoader CodeLoader { get; }

	public NCLMetadata MetadataCache { get; }

	public INavAppClrTypeRetriever AppClrTypeRetriever { get; }

	private NCLMetaApplicationObjectLoader(NCLMetadata metadataCache, INCLObjectXmlMetadataLoader xmlMetadataLoader, INCLCodeLoader codeLoader, INavAppClrTypeRetriever appClrTypeRetriever)
	{
		MetadataCache = metadataCache;
		XmlMetadataLoader = xmlMetadataLoader;
		CodeLoader = codeLoader;
		AppClrTypeRetriever = appClrTypeRetriever;
	}

	public static NCLMetaApplicationObjectLoader Create(NCLMetadata metadataCache, INCLObjectXmlMetadataLoader xmlMetadataLoader, INCLCodeLoader codeLoader, INavAppClrTypeRetriever appClrTypeRetriever)
	{
		return new NCLMetaApplicationObjectLoader(metadataCache, xmlMetadataLoader, codeLoader, appClrTypeRetriever);
	}

	public static NCLMetaApplicationObjectLoader CreateFromNavGlobal()
	{
		return new NCLMetaApplicationObjectLoader(NavGlobal.NCLMetadata, NavGlobal.NCLObjectXmlMetadataLoader, NavGlobal.NCLCodeLoader, NavGlobal.NavAppClrTypeRetriever);
	}
}
