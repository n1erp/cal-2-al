using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppClrTypeRetriever : INavAppClrTypeRetriever
{
	private const int AssemblyCacheVersion = 1;

	private readonly CSharpCompiler compiler = new CSharpCompiler();

	private readonly INavFilesystemContentCache filesystemCache;

	private readonly INavAppMetadataRetriever metadataRetriever;

	private readonly bool enableDebugging;

	public NavAppClrTypeRetriever(string assemblyFolder, INavAppMetadataRetriever metadataRetriever, bool enableDebugging)
	{
		string text = (enableDebugging ? Path.Combine(assemblyFolder, "debug") : Path.Combine(assemblyFolder, "release"));
		string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
		string text2 = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", fileVersion, 1);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000793", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "CLR type retriever -- Path:{0}; Version:{1}; Dbg:{2}", text, text2, enableDebugging);
		NavFilesystemContentCache navFilesystemContentCache = new NavFilesystemContentCache(text, text2);
		navFilesystemContentCache.Initialize();
		filesystemCache = navFilesystemContentCache;
		this.metadataRetriever = metadataRetriever;
		this.enableDebugging = enableDebugging;
	}

	public Type GetClrType(PackageId packageId, ApplicationObjectId objectId)
	{
		NavAppRuntimeMetadata metadata = metadataRetriever.GetMetadata(packageId);
		object obj;
		if (metadata == null)
		{
			obj = null;
		}
		else
		{
			NavAppObjectMetadataSummary objectSummary = metadata.GetObjectSummary(objectId);
			obj = ((objectSummary != null) ? objectSummary.UserCodeHash : null);
		}
		Sha256Hash sha256Hash = (Sha256Hash)obj;
		if (sha256Hash == null)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "Cannot find object -- Package:{0}; Type:{1}; Id:{2}", packageId, objectId.ObjectType, objectId.ObjectNumber);
			Exception exception = new InvalidOperationException(message);
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000794", Category.Extensions, exception);
		}
		NavFilesystemContentCacheKey cacheKey = new NavFilesystemContentCacheKey(sha256Hash, "dll");
		NavFilesystemContentCacheKey cacheKey2 = new NavFilesystemContentCacheKey(sha256Hash, "pdb");
		byte[] content = filesystemCache.GetContent(cacheKey);
		byte[] content2 = filesystemCache.GetContent(cacheKey2);
		if (content != null && (!enableDebugging || content2 != null))
		{
			Assembly assembly = Assembly.Load(content, content2);
			return ExtractClrTypeFromAssembly(assembly, packageId, objectId);
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		string filePath = metadataRetriever.GetObjectMetadataBlob(objectId, packageId, (NavAppObjectMetadataBlobType)1).FilePath;
		string assemblyName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", ObjectTypeExtensions.ToUniqueShortString(objectId.ObjectType), objectId.ObjectNumber, sha256Hash.HexString);
		NavAppCSharpCompilerResult navAppCSharpCompilerResult = compiler.CompileCSharpFiles(assemblyName, new string[1] { filePath }, enableDebugging);
		if (navAppCSharpCompilerResult.Errors != null)
		{
			Exception exception2 = new InvalidOperationException(navAppCSharpCompilerResult.Errors);
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000777", Category.Extensions, exception2);
		}
		Assembly assembly2 = Assembly.Load(navAppCSharpCompilerResult.AssemblyContent, navAppCSharpCompilerResult.PdbContent);
		filesystemCache.PutContent(cacheKey, navAppCSharpCompilerResult.AssemblyContent);
		if (navAppCSharpCompilerResult.PdbContent != null)
		{
			filesystemCache.PutContent(cacheKey2, navAppCSharpCompilerResult.PdbContent);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000778", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Assembly compiled -- Package:{0}; Type:{1}; Id:{2}; Hash:{3}; Ms:{4}", ((WrappedValue<Guid>)(object)packageId).Value, objectId.ObjectType, objectId.ObjectNumber, sha256Hash.HexString, stopwatch.ElapsedMilliseconds);
		return ExtractClrTypeFromAssembly(assembly2, packageId, objectId);
	}

	public void PurgeUnreferencedArtifacts(IEnumerable<NavAppRuntimeMetadata> allPublished)
	{
		HashSet<Sha256Hash> referencedHashes = (from m in allPublished
			from s in m.ObjectMetadataSummaries
			select s.UserCodeHash).ToHashSet();
		filesystemCache.PurgeUnreferencedContent(referencedHashes);
	}

	private static Type ExtractClrTypeFromAssembly(Assembly assembly, PackageId packageId, ApplicationObjectId objectId)
	{
		string text = "Microsoft.Dynamics.Nav.BusinessApplication." + NCLCodeLoader.ClassName(objectId.ObjectType, objectId.ObjectNumber);
		Type type = assembly.GetType(text, throwOnError: false, ignoreCase: true);
		if (type == null)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "CLR type load failed -- Assembly:{0}; Type:{1}; Package:{2}", assembly.GetName().Name, text, ((WrappedValue<Guid>)(object)packageId).Value);
			InvalidOperationException exception = new InvalidOperationException(message);
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00006XN", Category.Extensions, exception);
		}
		return type;
	}
}
