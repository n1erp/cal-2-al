using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.IO.Txt;
using Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;
using Microsoft.Dynamics.Nav.Model.Tools.Language;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class ApplicationObjectPropertyTool : ReadWriteModelTool
{
	private readonly ApplicationObjectPropertyToolOptions options;

	internal ApplicationObjectPropertyTool(ILogger logger, ApplicationObjectPropertyToolOptions options)
		: base(logger, options)
	{
		this.options = options;
	}

	internal ApplicationObjectFileInfoCollection GetApplicationObjectProperty(string[] source, string sourceName = null)
	{
		return ReadObjects(source, sourceName);
	}

	internal ApplicationObjectFileInfoCollection SetApplicationObjectProperty(string[] source, string sourceName = null)
	{
		ApplicationObjectFileInfoCollection applicationObjectFileInfoCollection = ReadObjects(source, sourceName);
		string dateStr = null;
		string timeStr = null;
		if (options.DateTimeProperty != null)
		{
			if (options.DateTimeProperty.Length == 0)
			{
				dateStr = (timeStr = string.Empty);
			}
			else
			{
				DateTime dateTime = DateTime.Parse(options.DateTimeProperty, CultureInfo.CurrentCulture);
				dateStr = dateTime.ToString(ModelChangeOptions.GetDateTextFormatString(), CultureInfo.CurrentCulture);
				timeStr = dateTime.ToString(ModelChangeOptions.GetTimeTextFormatString(), CultureInfo.CurrentCulture);
			}
		}
		var output = from i in applicationObjectFileInfoCollection
			group i by i.FileName into i
			select new
			{
				FileName = i.Key,
				Objects = i.Select((ApplicationObjectFileInfo ao) =>
				{
					if (options.VersionListProperty != null)
					{
						ao.ApplicationObject.ObjectProperties.VersionList = options.VersionListProperty;
					}
					if (options.ModifiedProperty.HasValue)
					{
						ao.ApplicationObject.ObjectProperties.Modified = ((options.ModifiedProperty == SetModifiedPropertyAction.Yes) ? "Yes" : string.Empty);
					}
					if (dateStr != null)
					{
						ao.ApplicationObject.ObjectProperties.Date = dateStr;
					}
					if (timeStr != null)
					{
						ao.ApplicationObject.ObjectProperties.Time = timeStr;
					}
					return ao.ApplicationObject;
				})
			};
		Logger.InvokeParallelProcessing(NonLocalizable.WriteFiles, applicationObjectFileInfoCollection.Count(), (IProgressLogger progress) =>
		{
			Parallel.ForEach(output, ParallelOptions, o =>
			{
				if (progress.ShouldProcess(o.FileName))
				{
					using (FileStream fileStream = File.Open(o.FileName, FileMode.Create, FileAccess.Write))
					{
						TxtExportOptions txtExportOptions = options.GetTxtExportOptions();
						txtExportOptions.ProgressAction = progress.Increment;
						new TxtExporter(TxtFileModelInfo.Instance, txtExportOptions).ExportObjects(o.Objects, o.Objects.Any((ApplicationObject obj) => obj.ShouldEncode) ? ((Stream)new NeaStreamWriter(fileStream, leaveOpen: true)) : ((Stream)fileStream));
					}
				}
			});
		});
		return applicationObjectFileInfoCollection;
	}
}
