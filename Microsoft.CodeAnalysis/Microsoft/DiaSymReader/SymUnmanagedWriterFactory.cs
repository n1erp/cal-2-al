using System;

namespace Microsoft.DiaSymReader;

internal static class SymUnmanagedWriterFactory
{
	public static SymUnmanagedWriter CreateWriter(ISymWriterMetadataProvider metadataProvider, SymUnmanagedWriterCreationOptions options = SymUnmanagedWriterCreationOptions.Default)
	{
		if (metadataProvider == null)
		{
			throw new ArgumentNullException("metadataProvider");
		}
		object obj = SymUnmanagedFactory.CreateObject(createReader: false, (options & SymUnmanagedWriterCreationOptions.UseAlternativeLoadPath) != 0, (options & SymUnmanagedWriterCreationOptions.UseComRegistry) != 0, out var moduleName, out var loadException);
		if (obj == null)
		{
			if (loadException is DllNotFoundException)
			{
				throw loadException;
			}
			throw new DllNotFoundException(loadException.Message, loadException);
		}
		if (!(obj is ISymUnmanagedWriter5 symUnmanagedWriter))
		{
			throw new SymUnmanagedWriterException(new NotSupportedException(), moduleName);
		}
		object emitter = new SymWriterMetadataAdapter(metadataProvider);
		ComMemoryStream comMemoryStream = new ComMemoryStream();
		try
		{
			if ((options & SymUnmanagedWriterCreationOptions.Deterministic) != 0)
			{
				if (!(obj is ISymUnmanagedWriter8 symUnmanagedWriter2))
				{
					throw new NotSupportedException();
				}
				symUnmanagedWriter2.InitializeDeterministic(emitter, comMemoryStream);
			}
			else
			{
				symUnmanagedWriter.Initialize(emitter, "filename.pdb", comMemoryStream, fullBuild: true);
			}
		}
		catch (Exception innerException)
		{
			throw new SymUnmanagedWriterException(innerException, moduleName);
		}
		return new SymUnmanagedWriterImpl(comMemoryStream, symUnmanagedWriter, moduleName);
	}
}
