namespace Cal2AL.Diagnostics;

/// <summary>Static description of one kind of diagnostic.</summary>
public sealed record DiagnosticDescriptor(
    string Code,
    string Title,
    DiagnosticSeverity DefaultSeverity,
    DiagnosticCategory Category,
    string? DefaultResolution = null)
{
    public Diagnostic Create(
        string message,
        DiagnosticLocation? location = null,
        string? construct = null,
        string? reason = null,
        string? resolution = null,
        string? originalSource = null,
        DiagnosticSeverity? severity = null) => new()
        {
            Code = Code,
            Severity = severity ?? DefaultSeverity,
            Category = Category,
            Message = message,
            Location = location ?? DiagnosticLocation.None,
            Construct = construct,
            Reason = reason,
            SuggestedResolution = resolution ?? DefaultResolution,
            OriginalSource = originalSource,
        };
}

/// <summary>
/// The catalogue of every diagnostic Cal2AL can produce. Codes are stable: CAL0xxx input and parsing,
/// CAL1xxx object structure, CAL2xxx properties, CAL3xxx code, CAL4xxx references, CAL5xxx delta files,
/// CAL9xxx output.
/// </summary>
public static class DiagnosticDescriptors
{
    // ---- Input -------------------------------------------------------------------------------------
    public static readonly DiagnosticDescriptor FileReadError = new(
        "CAL0001", "Input file could not be read", DiagnosticSeverity.Error, DiagnosticCategory.Input,
        "Check that the file exists, is not locked and that you have read permission.");

    public static readonly DiagnosticDescriptor EncodingFallback = new(
        "CAL0002", "File decoded with fallback code page", DiagnosticSeverity.Info, DiagnosticCategory.Input,
        "If special characters look wrong in the output, rerun with --encoding set to the code page C/SIDE used for the export (often 850 or 1252).");

    public static readonly DiagnosticDescriptor NoInputFiles = new(
        "CAL0003", "No input files found", DiagnosticSeverity.Warning, DiagnosticCategory.Input,
        "Point --source at a folder that contains .txt or .delta exports.");

    public static readonly DiagnosticDescriptor NotAnObjectFile = new(
        "CAL0004", "File contains no C/AL objects", DiagnosticSeverity.Warning, DiagnosticCategory.Input,
        "The file does not start with an OBJECT declaration. Make sure it is a C/SIDE text export.");

    // ---- Parse -------------------------------------------------------------------------------------
    public static readonly DiagnosticDescriptor TxtSyntaxError = new(
        "CAL0101", "Object could not be parsed", DiagnosticSeverity.Error, DiagnosticCategory.Parse,
        "Fix the TXT file at the reported position, or re-export the object from C/SIDE. The original text of the object was saved as a migration artifact.");

    public static readonly DiagnosticDescriptor UnexpectedText = new(
        "CAL0102", "Unexpected text outside an object", DiagnosticSeverity.Warning, DiagnosticCategory.Parse,
        "Remove the text or check whether an object declaration is damaged.");

    public static readonly DiagnosticDescriptor CodeSyntaxError = new(
        "CAL0110", "C/AL code could not be parsed", DiagnosticSeverity.Error, DiagnosticCategory.Parse,
        "The code was emitted unchanged except for comment conversion. Review it; it may contain a syntax error that C/SIDE tolerated or a construct Cal2AL does not know.");

    // ---- Structure ---------------------------------------------------------------------------------
    public static readonly DiagnosticDescriptor UnsupportedObjectType = new(
        "CAL1001", "Object type has no AL equivalent", DiagnosticSeverity.Error, DiagnosticCategory.Structure,
        "Rebuild the functionality manually. The original object text was saved as a migration artifact.");

    public static readonly DiagnosticDescriptor TriggerWithoutAlEquivalent = new(
        "CAL1002", "Trigger has no AL equivalent", DiagnosticSeverity.Error, DiagnosticCategory.Structure,
        "The trigger body was preserved as a local procedure with the same code. Call it from an appropriate AL trigger or event subscriber, or remove it if the behaviour is no longer needed.");

    public static readonly DiagnosticDescriptor NameMadeUnique = new(
        "CAL1003", "Element name changed to be unique", DiagnosticSeverity.Info, DiagnosticCategory.Structure,
        "AL requires unique control, action and data item names. Code that referenced the old name through CurrPage must be updated.");

    public static readonly DiagnosticDescriptor NameGenerated = new(
        "CAL1004", "Name generated for unnamed element", DiagnosticSeverity.Info, DiagnosticCategory.Structure);

    public static readonly DiagnosticDescriptor DisabledElementKept = new(
        "CAL1005", "Disabled element kept with Enabled = false", DiagnosticSeverity.Info, DiagnosticCategory.Structure);

    public static readonly DiagnosticDescriptor LegacyReportSections = new(
        "CAL1007", "Classic report sections are not supported", DiagnosticSeverity.Error, DiagnosticCategory.Structure,
        "Transform the report to an RDLC report in NAV 2009 R2 or later before converting.");

    public static readonly DiagnosticDescriptor UnresolvedEventOwner = new(
        "CAL1008", "Event trigger owner could not be resolved", DiagnosticSeverity.Error, DiagnosticCategory.Structure,
        "The trigger body was preserved as a local procedure. Attach it to the right usercontrol or WithEvents variable manually.");

    public static readonly DiagnosticDescriptor EnumGenerated = new(
        "CAL1009", "Enum generated from extensible option field", DiagnosticSeverity.Info, DiagnosticCategory.Structure);

    public static readonly DiagnosticDescriptor MissingRequiredPart = new(
        "CAL1010", "Required element missing", DiagnosticSeverity.Error, DiagnosticCategory.Structure,
        "Complete the element manually.");

    public static readonly DiagnosticDescriptor ConverterFailed = new(
        "CAL1012", "Object conversion failed unexpectedly", DiagnosticSeverity.Error, DiagnosticCategory.Structure,
        "This is a defect in Cal2AL. The original object text was saved as a migration artifact; please report the error with the object.");

    public static readonly DiagnosticDescriptor NameChanged = new(
        "CAL1013", "Name changed to a valid AL identifier", DiagnosticSeverity.Warning, DiagnosticCategory.Structure,
        "Update code, pages and reports in other apps that refer to this element by name.");

    public static readonly DiagnosticDescriptor ObjectMetadataKept = new(
        "CAL1011", "C/AL object metadata kept as a header comment", DiagnosticSeverity.Info, DiagnosticCategory.Structure);

    // ---- Properties --------------------------------------------------------------------------------
    public static readonly DiagnosticDescriptor PropertyNotSupported = new(
        "CAL2001", "Property has no AL equivalent", DiagnosticSeverity.Warning, DiagnosticCategory.Property,
        "The property was not emitted. Its original value is recorded in this diagnostic; re-create the behaviour manually if it matters.");

    public static readonly DiagnosticDescriptor PropertyUnmapped = new(
        "CAL2002", "Property has no known mapping", DiagnosticSeverity.Warning, DiagnosticCategory.Property,
        "The property was emitted with its C/AL name and a best-effort value. Check that AL accepts it.");

    public static readonly DiagnosticDescriptor PropertyValueNotParsed = new(
        "CAL2005", "Property value could not be parsed", DiagnosticSeverity.Warning, DiagnosticCategory.Property,
        "The value was emitted unchanged. Rewrite it in AL syntax if the compiler rejects it.");

    public static readonly DiagnosticDescriptor TranslationsMoved = new(
        "CAL2006", "Multi-language values moved to translation files", DiagnosticSeverity.Info, DiagnosticCategory.Property);

    public static readonly DiagnosticDescriptor PropertyInert = new(
        "CAL2007", "Property had no effect and was not emitted", DiagnosticSeverity.Info, DiagnosticCategory.Property,
        "The property depends on another property that is not set, so it had no effect in C/AL either; AL rejects it in this position.");

    public static readonly DiagnosticDescriptor PropertyValueAdjusted = new(
        "CAL2008", "Property value adjusted for AL", DiagnosticSeverity.Info, DiagnosticCategory.Property);

    // ---- Code --------------------------------------------------------------------------------------
    public static readonly DiagnosticDescriptor UnsupportedFunction = new(
        "CAL3001", "Function is not available in AL", DiagnosticSeverity.Error, DiagnosticCategory.Code);

    public static readonly DiagnosticDescriptor ObsoleteFunction = new(
        "CAL3002", "Function is obsolete or behaves differently in AL", DiagnosticSeverity.Warning, DiagnosticCategory.Code);

    public static readonly DiagnosticDescriptor OnPremOnly = new(
        "CAL3003", "Construct is only available on-premises", DiagnosticSeverity.Warning, DiagnosticCategory.Code,
        "Set \"target\": \"OnPrem\" in app.json, or replace the construct with a cloud-compatible alternative.");

    public static readonly DiagnosticDescriptor WithStatement = new(
        "CAL3004", "WITH statement", DiagnosticSeverity.Info, DiagnosticCategory.Code,
        "WITH is deprecated in AL (warning AL0606 from runtime 6). Qualify member access explicitly when you modernise the code.");

    public static readonly DiagnosticDescriptor AutomationNotSupported = new(
        "CAL3005", "Automation and OCX are not supported in AL", DiagnosticSeverity.Error, DiagnosticCategory.Code,
        "Replace the COM component with .NET interop (on-premises), a control add-in, or a web service. The declaration and all code using it were kept so the compiler shows every place to change.");

    public static readonly DiagnosticDescriptor ReservedIdentifierQuoted = new(
        "CAL3007", "Identifier quoted because it is reserved in AL", DiagnosticSeverity.Info, DiagnosticCategory.Code);

    public static readonly DiagnosticDescriptor PreservationCheckFailed = new(
        "CAL3009", "Converted code differs from the original beyond the intended rewrites", DiagnosticSeverity.Error, DiagnosticCategory.Code,
        "This indicates a defect in Cal2AL. Compare the member with the original and report the case.");

    public static readonly DiagnosticDescriptor FormReference = new(
        "CAL3010", "Reference to a classic form", DiagnosticSeverity.Error, DiagnosticCategory.Code,
        "Forms do not exist in AL. Replace the form with the corresponding page (FORM.RUN → PAGE.RUN, FORM:: → PAGE::).");

    public static readonly DiagnosticDescriptor DotNetVariable = new(
        "CAL3011", ".NET interop requires an on-premises target", DiagnosticSeverity.Warning, DiagnosticCategory.Code,
        "Keep \"target\": \"OnPrem\" in app.json and make sure the assembly is available to the AL compiler (assembly probing paths), or replace the .NET type.");

    public static readonly DiagnosticDescriptor UnknownAttribute = new(
        "CAL3012", "Unknown procedure attribute", DiagnosticSeverity.Warning, DiagnosticCategory.Code,
        "The attribute was emitted unchanged. Check that AL supports it.");

    public static readonly DiagnosticDescriptor AttributeDropped = new(
        "CAL3013", "Attribute has no AL equivalent", DiagnosticSeverity.Info, DiagnosticCategory.Code);

    public static readonly DiagnosticDescriptor CommentAdjusted = new(
        "CAL3014", "Comment text adjusted", DiagnosticSeverity.Info, DiagnosticCategory.Code);

    public static readonly DiagnosticDescriptor ImplicitWithInPage = new(
        "CAL3015", "Code relies on implicit record scope", DiagnosticSeverity.Info, DiagnosticCategory.Code);

    public static readonly DiagnosticDescriptor DateConstant = new(
        "CAL3017", "Date constant rewritten", DiagnosticSeverity.Info, DiagnosticCategory.Code,
        "C/AL date constants are MMDDYY[YY]D, AL date constants are yyyymmddD. If the C/AL code was written for day-first constants, rerun with --cal-date-order dmy.");

    public static readonly DiagnosticDescriptor UnsupportedDataType = new(
        "CAL3016", "Data type is not available in AL", DiagnosticSeverity.Error, DiagnosticCategory.Code);

    // ---- References --------------------------------------------------------------------------------
    public static readonly DiagnosticDescriptor UnresolvedObjectReference = new(
        "CAL4001", "Object reference kept as ID", DiagnosticSeverity.Info, DiagnosticCategory.Reference,
        "The referenced object is not part of the converted set, so its name is unknown. AL accepts the numeric ID; replace it with the name for readability.");

    // ---- Delta files -------------------------------------------------------------------------------
    public static readonly DiagnosticDescriptor DeltaCodeModification = new(
        "CAL5001", "Delta modifies existing code", DiagnosticSeverity.Error, DiagnosticCategory.Delta,
        "AL extensions cannot change code in the base object. Move the change into an event subscriber or a new procedure. The original and modified code were saved in the migration artifact.");

    public static readonly DiagnosticDescriptor DeltaUnsupportedChange = new(
        "CAL5002", "Delta change cannot be expressed in an extension", DiagnosticSeverity.Error, DiagnosticCategory.Delta,
        "Re-create the change manually. The original change block was saved in the migration artifact.");

    public static readonly DiagnosticDescriptor DeltaTargetNotSupported = new(
        "CAL5003", "Delta target cannot be extended", DiagnosticSeverity.Error, DiagnosticCategory.Delta,
        "Only tables and pages can be extended in AL. Re-implement the change with events or new objects.");

    public static readonly DiagnosticDescriptor DeltaCodeExtended = new(
        "CAL5004", "Delta code change converted to an extension trigger", DiagnosticSeverity.Info, DiagnosticCategory.Delta,
        "The added lines run in an extension trigger instead of inside the base trigger. Check that running them before or after the complete base trigger gives the same result.");

    public static readonly DiagnosticDescriptor DeltaAnchorUnverified = new(
        "CAL5005", "Extension anchor could not be verified", DiagnosticSeverity.Warning, DiagnosticCategory.Delta,
        "The base object is not in the converted set, so the anchor name was derived from the delta. Check it against the base object's AL names.");

    // ---- Output ------------------------------------------------------------------------------------
    public static readonly DiagnosticDescriptor OutputWriteError = new(
        "CAL9001", "Output file could not be written", DiagnosticSeverity.Error, DiagnosticCategory.Output);

    public static readonly DiagnosticDescriptor FileNameCollision = new(
        "CAL9002", "Output file name made unique", DiagnosticSeverity.Info, DiagnosticCategory.Output);

    public static readonly DiagnosticDescriptor ArtifactWritten = new(
        "CAL9003", "Migration artifact written", DiagnosticSeverity.Info, DiagnosticCategory.Output);

    /// <summary>All descriptors, for documentation and the <c>inspect --diagnostics</c> listing.</summary>
    public static IReadOnlyList<DiagnosticDescriptor> All { get; } = typeof(DiagnosticDescriptors)
        .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        .Where(f => f.FieldType == typeof(DiagnosticDescriptor))
        .Select(f => (DiagnosticDescriptor)f.GetValue(null)!)
        .OrderBy(d => d.Code, StringComparer.Ordinal)
        .ToArray();
}
