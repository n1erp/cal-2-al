namespace Microsoft.Dynamics.Nav.Runtime;

internal class CrmErrorCodes
{
	public const int CustomImageAttributeOnlyAllowedOnCustomEntity = -2147187407;

	public const int SqlEncryptionSymmetricKeyCannotOpenBecauseWrongPassword = -2147187408;

	public const int SqlEncryptionSymmetricKeyDoesNotExistOrNoPermission = -2147187409;

	public const int SqlEncryptionSymmetricKeyPasswordDoesNotExistInConfigDB = -2147187410;

	public const int SqlEncryptionSymmetricKeySourceDoesNotExistInConfigDB = -2147187411;

	public const int CannotExecuteRequestBecauseHttpsIsRequired = -2147187412;

	public const int SqlEncryptionRestoreEncryptionKeyCannotDecryptExistingData = -2147187413;

	public const int SqlEncryptionSetEncryptionKeyIsAlreadyRunningCannotRunItInParallel = -2147187414;

	public const int SqlEncryptionChangeEncryptionKeyExceededQuotaForTheInterval = -2147187415;

	public const int SqlEncryptionEncryptionKeyValidationError = -2147187416;

	public const int SqlEncryptionIsInactiveCannotChangeEncryptionKey = -2147187417;

	public const int SqlEncryptionDeleteEncryptionKeyError = -2147187418;

	public const int SqlEncryptionIsActiveCannotRestoreEncryptionKey = -2147187419;

	public const int SqlEncryptionKeyCannotDecryptExistingData = -2147187420;

	public const int SqlEncryptionEncryptionDecryptionTestError = -2147187421;

	public const int SqlEncryptionDeleteSymmetricKeyError = -2147187422;

	public const int SqlEncryptionCreateSymmetricKeyError = -2147187423;

	public const int SqlEncryptionSymmetricKeyDoesNotExist = -2147187424;

	public const int SqlEncryptionDeleteCertificateError = -2147187425;

	public const int SqlEncryptionCreateCertificateError = -2147187426;

	public const int SqlEncryptionCertificateDoesNotExist = -2147187427;

	public const int SqlEncryptionDeleteDatabaseMasterKeyError = -2147187428;

	public const int SqlEncryptionCreateDatabaseMasterKeyError = -2147187429;

	public const int SqlEncryptionCannotOpenSymmetricKeyBecauseDatabaseMasterKeyDoesNotExistOrIsNotOpened = -2147187430;

	public const int SqlEncryptionDatabaseMasterKeyDoesNotExist = -2147187431;

	public const int SqlEncryption = -2147187432;

	public const int ErrorsInSlaWorkflowActivation = -2147187403;

	public const int ManifestParsingFailure = -2147187404;

	public const int InvalidManifestFilePath = -2147187405;

	public const int OnPremiseRestoreOrganizationManifestFailed = -2147187406;

	public const int InvalidAuth = -2147187434;

	public const int CannotUpdateOrgDBOrgSettingWhenOffline = -2147187435;

	public const int InvalidOrgDBOrgSetting = -2147187436;

	public const int UnknownInvalidTransformationParameterGeneric = -2147187437;

	public const int InvalidTransformationParameterOutsideRangeGeneric = -2147187438;

	public const int InvalidTransformationParameterEmptyCollection = -2147187439;

	public const int InvalidTransformationParameterOutsideRange = -2147187440;

	public const int InvalidTransformationParameterZeroToRange = -2147187447;

	public const int InvalidTransformationParameterString = -2147187448;

	public const int InvalidTransformationParametersGeneric = -2147187449;

	public const int InsufficientTransformationParameters = -2147187450;

	public const int MaximumNumberHandlersExceeded = -2147187451;

	public const int ErrorInUnzipAlternate = -2147187453;

	public const int IncorrectSingleFileMultipleEntityMap = -2147187454;

	public const int ActivityEntityCannotBeActivityParty = -2147187455;

	public const int TargetAttributeInvalidForIgnore = -2147187456;

	public const int MaxUnzipFolderSizeExceeded = -2147187559;

	public const int InvalidMultipleMapping = -2147187560;

	public const int ErrorInStoringImportFile = -2147187561;

	public const int UnzipTimeout = -2147187562;

	public const int UnsupportedZipFileForImport = -2147187563;

	public const int UnzipProcessCountLimitReached = -2147187564;

	public const int AttachmentNotFound = -2147187565;

	public const int TooManyPicklistValues = -2147187566;

	public const int VeryLargeFileInZipImport = -2147187567;

	public const int InvalidAttachmentsFolder = -2147187568;

	public const int ZipInsideZip = -2147187575;

	public const int InvalidZipFileFormat = -2147187576;

	public const int EmptyFileForImport = -2147187577;

	public const int EmptyFilesInZip = -2147187578;

	public const int ZipFileHasMixOfCsvAndXmlFiles = -2147187579;

	public const int DuplicateFileNamesInZip = -2147187580;

	public const int ErrorInUnzip = -2147187581;

	public const int InvalidZipFileForImport = -2147187582;

	public const int InvalidLookupMapNode = -2147187583;

	public const int ImportMailMergeTemplateEntityMissingError = -2147187584;

	public const int CannotUpdateOpportunityCurrency = -2147187591;

	public const int ParentRecordAlreadyExists = -2147187592;

	public const int MissingWebToLeadRedirect = -2147187593;

	public const int InvalidWebToLeadRedirect = -2147187594;

	public const int TemplateNotAllowedForInternetMarketing = -2147187595;

	public const int CopyNotAllowedForInternetMarketing = -2147187596;

	public const int MissingOrInvalidRedirectId = -2147187597;

	public const int ImportNotComplete = -2147187598;

	public const int UIDataMissingInWorkflow = -2147187599;

	public const int RefEntityRelationshipRoleRequired = -2147187600;

	public const int ImportTemplateLanguageIgnored = -2147187590;

	public const int ImportTemplatePersonalIgnored = -2147187589;

	public const int ImportComponentDeletedIgnored = -2147187588;

	public const int RelationshipRoleNodeNumberInvalid = -2147187607;

	public const int AssociationRoleOrdinalInvalid = -2147187608;

	public const int RelationshipRoleMismatch = -2147187609;

	public const int ImportMapInUse = -2147187611;

	public const int PreviousOperationNotComplete = -2147187612;

	public const int TransformationResumeNotSupported = -2147187613;

	public const int CannotDisableDuplicateDetection = -2147187614;

	public const int TargetEntityNotMapped = -2147187616;

	public const int BulkDeleteChildFailure = -2147187623;

	public const int CannotRemoveNonListMember = -2147187624;

	public const int JobNameIsEmptyOrNull = -2147187625;

	public const int ImportMailMergeTemplateError = -2147187626;

	public const int ErrorsInWorkflowDefinition = -2147187627;

	public const int DistributeNoListAssociated = -2147187628;

	public const int DistributeListAssociatedVary = -2147187629;

	public const int OfflineFilterParentDownloaded = -2147187631;

	public const int OfflineFilterNestedDateTimeOR = -2147187632;

	public const int DuplicateOfflineFilter = -2147187639;

	public const int CannotAssignAddressBookFilters = -2147187640;

	public const int CannotCreateAddressBookFilters = -2147187641;

	public const int CannotGrantAccessToAddressBookFilters = -2147187642;

	public const int CannotModifyAccessToAddressBookFilters = -2147187643;

	public const int CannotRevokeAccessToAddressBookFilters = -2147187644;

	public const int DuplicateMapName = -2147187645;

	public const int InvalidWordXmlFile = -2147187647;

	public const int FileNotFound = -2147187648;

	public const int MultipleFilesFound = -2147187655;

	public const int InvalidAttributeMapping = -2147187656;

	public const int FileReadError = -2147187657;

	public const int ViewForDuplicateDetectionNotDefined = -2147186632;

	public const int FileInUse = -2147186633;

	public const int NoPublishedDuplicateDetectionRules = -2147187658;

	public const int NoEntitiesForBulkDelete = -2147187646;

	public const int BulkDeleteRecordDeletionFailure = -2147187659;

	public const int RuleAlreadyPublishing = -2147187660;

	public const int RuleNotFound = -2147187661;

	public const int CannotDeleteSystemEmailTemplate = -2147187662;

	public const int EntityDupCheckNotSupportedSystemWide = -2147187663;

	public const int DuplicateDetectionNotSupportedOnAttributeType = -2147187664;

	public const int MaxMatchCodeLengthExceeded = -2147187671;

	public const int CannotDeleteUpdateInUseRule = -2147187672;

	public const int ImportMappingsInvalidIdSpecified = -2147187673;

	public const int NotAWellFormedXml = -2147187674;

	public const int NoncompliantXml = -2147187675;

	public const int DuplicateDetectionTemplateNotFound = -2147187676;

	public const int RulesInInconsistentStateFound = -2147187677;

	public const int BulkDetectInvalidEmailRecipient = -2147187678;

	public const int CannotEnableDuplicateDetection = -2147187679;

	public const int CannotDeleteInUseEntity = -2147187680;

	public const int StringAttributeIndexError = -2147167598;

	public const int CannotChangeAttributeRequiredLevel = -2147167597;

	public const int MaximumNumberOfAttributesForEntityReached = -2147187686;

	public const int CannotPublishMoreRules = -2147187687;

	public const int CannotDeleteInUseAttribute = -2147187688;

	public const int CannotDeleteInUseOptionSet = -2147187689;

	public const int InvalidEntityName = -2147187690;

	public const int InvalidOperatorCode = -2147187691;

	public const int CannotPublishEmptyRule = -2147187692;

	public const int CannotPublishInactiveRule = -2147187693;

	public const int DuplicateCheckNotEnabled = -2147187694;

	public const int DuplicateCheckNotSupportedOnEntity = -2147187696;

	public const int InvalidStateCodeStatusCode = -2147187704;

	public const int SyncToMsdeFailure = -2147187705;

	public const int FormDoesNotExist = -2147187706;

	public const int AccessDenied = -2147187707;

	public const int CannotDeleteOptionSet = -2147187708;

	public const int InvalidOptionSetOperation = -2147187709;

	public const int OptionValuePrefixOutOfRange = -2147187710;

	public const int CheckPrivilegeGroupForUserOnPremiseError = -2147187711;

	public const int CheckPrivilegeGroupForUserOnSplaError = -2147187712;

	public const int unManagedIdsAccessDenied = -2147187962;

	public const int EntityIsIntersect = -2147187953;

	public const int CannotDeleteTeamOwningRecords = -2147187954;

	public const int CannotRemoveMembersFromDefaultTeam = -2147187956;

	public const int CannotAddMembersToDefaultTeam = -2147187957;

	public const int CannotUpdateNameDefaultTeam = -2147187958;

	public const int CannotSetParentDefaultTeam = -2147187960;

	public const int CannotDeleteDefaultTeam = -2147187961;

	public const int TeamNameTooLong = -2147187963;

	public const int CannotAssignRolesOrProfilesToAccessTeam = -2147187919;

	public const int TooManyEntitiesEnabledForAutoCreatedAccessTeams = -2147187918;

	public const int TooManyTeamTemplatesForEntityAccessTeams = -2147187917;

	public const int EntityNotEnabledForAutoCreatedAccessTeams = -2147187916;

	public const int InvalidAccessMaskForTeamTemplate = -2147187915;

	public const int CannotChangeTeamTypeDueToRoleOrProfile = -2147187914;

	public const int CannotChangeTeamTypeDueToOwnership = -2147187913;

	public const int CannotDisableAutoCreateAccessTeams = -2147187912;

	public const int CannotShareSystemManagedTeam = -2147187911;

	public const int CannotAssignToAccessTeam = -2147187904;

	public const int DuplicateSalesTeamMember = -2147187903;

	public const int TargetUserInsufficientPrivileges = -2147187902;

	public const int InvalidDomainName = -2147188715;

	public const int InvalidUserName = -2147188587;

	public const int BulkMailServiceNotAccessible = -2147187964;

	public const int RSMoveItemError = -2147187920;

	public const int ReportParentChildNotCustomizable = -2147187921;

	public const int ConvertFetchDataSetError = -2147187922;

	public const int ConvertReportToCrmError = -2147187923;

	public const int ReportViewerError = -2147187924;

	public const int RSGetItemTypeError = -2147187925;

	public const int RSSetPropertiesError = -2147187926;

	public const int RSReportParameterTypeMismatchError = -2147187927;

	public const int RSUpdateReportExecutionSnapshotError = -2147187928;

	public const int RSSetReportHistoryLimitError = -2147187929;

	public const int RSSetReportHistoryOptionsError = -2147187930;

	public const int RSSetExecutionOptionsError = -2147187931;

	public const int RSSetReportParametersError = -2147187932;

	public const int RSGetReportParametersError = -2147187933;

	public const int RSSetItemDataSourcesError = -2147187934;

	public const int RSGetItemDataSourcesError = -2147187935;

	public const int RSCreateBatchError = -2147187936;

	public const int RSListReportHistoryError = -2147187937;

	public const int RSGetReportHistoryLimitError = -2147187938;

	public const int RSExecuteBatchError = -2147187939;

	public const int RSCancelBatchError = -2147187940;

	public const int RSListExtensionsError = -2147187941;

	public const int RSGetDataSourceContentsError = -2147187942;

	public const int RSSetDataSourceContentsError = -2147187943;

	public const int RSFindItemsError = -2147187944;

	public const int RSDeleteItemError = -2147187945;

	public const int ReportSecurityError = -2147187946;

	public const int ReportMissingReportSourceError = -2147187947;

	public const int ReportMissingParameterError = -2147187948;

	public const int ReportMissingEndpointError = -2147187949;

	public const int ReportMissingDataSourceError = -2147187950;

	public const int ReportMissingDataSourceCredentialsError = -2147187951;

	public const int ReportLocalProcessingError = -2147187952;

	public const int ReportServerSP2HotFixNotApplied = -2147187959;

	public const int DataSourceProhibited = -2147187955;

	public const int ReportServerVersionLow = -2147187965;

	public const int ReportServerNoPrivilege = -2147187966;

	public const int ReportServerInvalidUrl = -2147187967;

	public const int ReportServerUnknownException = -2147187968;

	public const int ReportNotAvailable = -2147188071;

	public const int ErrorUploadingReport = -2147188072;

	public const int ReportFileTooBig = -2147188073;

	public const int ReportFileZeroLength = -2147188074;

	public const int ReportTypeBlocked = -2147188075;

	public const int ReportUploadDisabled = -2147188076;

	public const int BothConnectionSidesAreNeeded = -2147188200;

	public const int CannotConnectToSelf = -2147188201;

	public const int UnrelatedConnectionRoles = -2147188202;

	public const int ConnectionRoleNotValidForObjectType = -2147188203;

	public const int ConnectionCannotBeEnabledOnThisEntity = -2147188204;

	public const int ConnectionNotSupported = -2147188205;

	public const int ConnectionObjectsMissing = -2147188208;

	public const int ConnectionInvalidStartEndDate = -2147188215;

	public const int ConnectionExists = -2147188216;

	public const int DecoupleUserOwnedEntity = -2147188217;

	public const int DecoupleChildEntity = -2147188218;

	public const int ExistingParentalRelationship = -2147188219;

	public const int InvalidCascadeLinkType = -2147188220;

	public const int InvalidDeleteModification = -2147188221;

	public const int CustomerOpportunityRoleExists = -2147188222;

	public const int CustomerRelationshipExists = -2147188223;

	public const int MultipleRelationshipsNotSupported = -2147188224;

	public const int ImportDuplicateEntity = -2147188468;

	public const int CascadeProxyEmptyCallerId = -2147188469;

	public const int CascadeProxyInvalidPrincipalType = -2147188470;

	public const int CascadeProxyInvalidNativeDAPtr = -2147188471;

	public const int CascadeFailToCreateNativeDAWrapper = -2147188472;

	public const int CascadeReparentOnNonUserOwned = -2147188473;

	public const int CascadeMergeInvalidSpecialColumn = -2147188474;

	public const int CascadeRemoveLinkOnNonNullable = -2147188476;

	public const int CascadeDeleteNotAllowDelete = -2147188477;

	public const int CascadeInvalidLinkType = -2147188478;

	public const int IsvExtensionsPrivilegeNotPresent = -2147188695;

	public const int RelationshipNameLengthExceedsLimit = -2147188694;

	public const int ImportEmailTemplateErrorMissingFile = -2147188693;

	public const int CascadeInvalidExtraConditionValue = -2147188479;

	public const int ImportWorkflowNameConflictError = -2147188697;

	public const int ImportWorkflowPublishedError = -2147188696;

	public const int ImportWorkflowEntityDependencyError = -2147188701;

	public const int ImportWorkflowAttributeDependencyError = -2147188702;

	public const int ImportWorkflowError = -2147188703;

	public const int ImportGenericEntitiesError = -2147188704;

	public const int ImportRolePermissionError = -2147188712;

	public const int ImportRoleError = -2147188713;

	public const int ImportOrgSettingsError = -2147188711;

	public const int InvalidSharePointSiteCollectionUrl = -2147188654;

	public const int InvalidSiteRelativeUrlFormat = -2147188653;

	public const int InvalidRelativeUrlFormat = -2147188652;

	public const int InvalidAbsoluteUrlFormat = -2147188651;

	public const int InvalidUrlConsecutiveSlashes = -2147188650;

	public const int SharePointRecordWithDuplicateUrl = -2147188649;

	public const int SharePointAbsoluteAndRelativeUrlEmpty = -2147188407;

	public const int ImportOptionSetsError = -2147188688;

	public const int ImportRibbonsError = -2147188687;

	public const int ImportReportsError = -2147188686;

	public const int ImportSolutionError = -2147188685;

	public const int ImportDependencySolutionError = -2147188684;

	public const int ExportSolutionError = -2147188683;

	public const int ExportManagedSolutionError = -2147188682;

	public const int ExportMissingSolutionError = -2147188681;

	public const int ImportSolutionManagedError = -2147188680;

	public const int ImportOptionSetAttributeError = -2147188679;

	public const int ImportSolutionManagedToUnmanagedMismatch = -2147188672;

	public const int ImportSolutionUnmanagedToManagedMismatch = -2147188671;

	public const int ImportSolutionIsvConfigWarning = -2147188670;

	public const int ImportSolutionSiteMapWarning = -2147188669;

	public const int ImportSolutionOrganizationSettingsWarning = -2147188668;

	public const int ImportExportDeprecatedError = -2147188667;

	public const int ImportSystemSolutionError = -2147188666;

	public const int ImportTranslationMissingSolutionError = -2147188665;

	public const int ExportDefaultAsPackagedError = -2147188664;

	public const int ImportDefaultAsPackageError = -2147188663;

	public const int ImportCustomizationsBadZipFileError = -2147188640;

	public const int ImportTranslationsBadZipFileError = -2147188639;

	public const int ImportAttributeNameError = -2147188638;

	public const int ImportFieldSecurityProfileIsSecuredMissingError = -2147188637;

	public const int ImportFieldSecurityProfileAttributesMissingError = -2147188636;

	public const int ImportFileSignatureInvalid = -2147188635;

	public const int ImportSolutionPackageNotValid = -2147188634;

	public const int ImportSolutionPackageNeedsUpgrade = -2147188633;

	public const int ImportSolutionPackageInvalidSolutionPackageVersion = -2147188632;

	public const int ImportSolutionPackageMinimumVersionNeeded = 1;

	public const int ImportSolutionPackageRequiresOptInAvailable = -2147188631;

	public const int ImportSolutionPackageRequiresOptInNotAvailable = -2147188630;

	public const int ImportSdkMessagesError = -2147188714;

	public const int ImportEmailTemplatePersonalError = -2147188716;

	public const int ImportNonWellFormedFileError = -2147188717;

	public const int ImportPluginTypesError = -2147188718;

	public const int ImportSiteMapError = -2147188719;

	public const int ImportMappingsMissingEntityMapError = -2147188720;

	public const int ImportMappingsSystemMapError = -2147188721;

	public const int ImportIsvConfigError = -2147188722;

	public const int ImportArticleTemplateError = -2147188723;

	public const int ImportEmailTemplateError = -2147188724;

	public const int ImportContractTemplateError = -2147188725;

	public const int ImportRelationshipRoleMapsError = -2147188726;

	public const int ImportRelationshipRolesError = -2147188727;

	public const int ImportRelationshipRolesPrivilegeError = -2147188689;

	public const int ImportEntityNameMismatchError = -2147188728;

	public const int ImportFormXmlError = -2147188729;

	public const int ImportFieldXmlError = -2147188730;

	public const int ImportSavedQueryExistingError = -2147188731;

	public const int ImportSavedQueryOtcMismatchError = -2147188732;

	public const int ImportEntityCustomResourcesNewStringError = -2147188733;

	public const int ImportEntityCustomResourcesError = -2147188734;

	public const int ImportEntityIconError = -2147188735;

	public const int ImportSavedQueryDeletedError = -2147188709;

	public const int ImportEntitySystemUserOnPremiseMismatchError = -2147188700;

	public const int ImportEntitySystemUserLiveMismatchError = -2147188699;

	public const int ImportLanguagesIgnoredError = -2147188698;

	public const int ImportInvalidFileError = -2147188736;

	public const int ImportXsdValidationError = -2147188710;

	public const int ImportInvalidXmlError = -2147188692;

	public const int ImportWrongPublisherError = -2147188708;

	public const int ImportMissingDependenciesError = -2147188707;

	public const int ImportGenericError = -2147188706;

	public const int ImportMissingComponent = -2147188705;

	public const int ImportMissingRootComponentEntry = -2147188678;

	public const int UnmanagedComponentParentsManagedComponent = -2147188677;

	public const int FailedToGetNetworkServiceName = -2147192573;

	public const int CustomParentingSystemNotSupported = -2147192574;

	public const int InvalidFormatParameters = -2147192575;

	public const int DuplicatePrimaryNameAttribute = -2147192802;

	public const int ConfigurationPageNotValidForSolution = -2147192803;

	public const int SolutionConfigurationPageMustBeHtmlWebResource = -2147192804;

	public const int InvalidSolutionConfigurationPage = -2147192805;

	public const int InvalidLanguageForSolution = -2147192807;

	public const int CannotHaveDuplicateYomi = -2147192808;

	public const int SavedQueryIsNotCustomizable = -2147192809;

	public const int CannotDeleteChildAttribute = -2147192810;

	public const int EntityHasNoStateCode = -2147192811;

	public const int NoAttributesForEntityCreate = -2147192812;

	public const int DuplicateAttributeSchemaName = -2147192813;

	public const int DuplicateDisplayCollectionName = -2147192814;

	public const int DuplicateDisplayName = -2147192815;

	public const int DuplicateName = -2147192816;

	public const int InvalidRelationshipType = -2147192817;

	public const int InvalidPrimaryFieldType = -2147192818;

	public const int InvalidOwnershipTypeMask = -2147192819;

	public const int InvalidDisplayName = -2147192820;

	public const int InvalidSchemaName = -2147192821;

	public const int RelationshipIsNotCustomRelationship = -2147192822;

	public const int AttributeIsNotCustomAttribute = -2147192823;

	public const int EntityIsNotCustomizable = -2147192824;

	public const int MultipleParentsNotSupported = -2147192825;

	public const int CannotCreateActivityRelationship = -2147192826;

	public const int CyclicalRelationship = -2147192828;

	public const int InvalidRelationshipDescription = -2147192829;

	public const int CannotDeletePrimaryUIAttribute = -2147192830;

	public const int RowGuidIsNotValidName = -2147192831;

	public const int FailedToScheduleActivity = -2147192832;

	public const int CannotDeleteLastEmailAttribute = -2147192833;

	public const int SystemAttributeMap = -2147196411;

	public const int UpdateAttributeMap = -2147196412;

	public const int InvalidAttributeMap = -2147196413;

	public const int SystemEntityMap = -2147196414;

	public const int UpdateEntityMap = -2147196415;

	public const int NonMappableEntity = -2147196416;

	public const int unManagedidsCalloutException = -2147197179;

	public const int unManagedidscalloutinvalidevent = -2147197180;

	public const int unManagedidscalloutinvalidconfig = -2147197181;

	public const int unManagedidscalloutisvstop = -2147197182;

	public const int unManagedidscalloutisvabort = -2147197183;

	public const int unManagedidscalloutisvexception = -2147197184;

	public const int unManagedidscustomentityambiguousrelationship = -2147198707;

	public const int unManagedidscustomentitynorelationship = -2147198708;

	public const int unManagedidscustomentityparentchildidentical = -2147198709;

	public const int unManagedidscustomentityinvalidparent = -2147198710;

	public const int unManagedidscustomentityinvalidchild = -2147198711;

	public const int unManagedidscustomentitywouldcreateloop = -2147198712;

	public const int unManagedidscustomentityexistingloop = -2147198713;

	public const int unManagedidscustomentitystackunderflow = -2147198714;

	public const int unManagedidscustomentitystackoverflow = -2147198715;

	public const int unManagedidscustomentitytlsfailure = -2147198716;

	public const int unManagedidscustomentityinvalidownership = -2147198717;

	public const int unManagedidscustomentitynotinitialized = -2147198718;

	public const int unManagedidscustomentityalreadyinitialized = -2147198719;

	public const int unManagedidscustomentitynameviolation = -2147198720;

	public const int unManagedidscascadeunexpectederror = -2147199485;

	public const int unManagedidscascadeemptylinkerror = -2147199486;

	public const int unManagedidscascadeundefinedrelationerror = -2147199487;

	public const int unManagedidscascadeinconsistencyerror = -2147199488;

	public const int MergeLossOfParentingWarning = -2147200233;

	public const int MergeDifferentlyParentedWarning = -2147200234;

	public const int MergeEntitiesIdenticalError = -2147200251;

	public const int MergeEntityNotActiveError = -2147200252;

	public const int unManagedidsmergedifferentbizorgerror = -2147200253;

	public const int MergeActiveQuoteError = -2147200254;

	public const int MergeSecurityError = -2147200255;

	public const int MergeCyclicalParentingError = -2147200256;

	public const int unManagedidscalendarruledoesnotexist = -2147200768;

	public const int unManagedidscalendarinvalidcalendar = -2147201792;

	public const int AttachmentInvalidFileName = -2147202552;

	public const int unManagedidsattachmentcannottruncatetempfile = -2147202553;

	public const int unManagedidsattachmentcannotunmaptempfile = -2147202554;

	public const int unManagedidsattachmentcannotcreatetempfile = -2147202555;

	public const int unManagedidsattachmentisempty = -2147202556;

	public const int unManagedidsattachmentcannotreadtempfile = -2147202557;

	public const int unManagedidsattachmentinvalidfilesize = -2147202558;

	public const int unManagedidsattachmentcannotgetfilesize = -2147202559;

	public const int unManagedidsattachmentcannotopentempfile = -2147202560;

	public const int unManagedidscustomizationtransformationnotsupported = -2147203328;

	public const int ContractDetailDiscountAmountAndPercent = -2147204076;

	public const int ContractDetailDiscountAmount = -2147204077;

	public const int ContractDetailDiscountPercent = -2147204078;

	public const int IncidentIsAlreadyClosedOrCancelled = -2147204079;

	public const int unManagedidsincidentparentaccountandparentcontactnotpresent = -2147204080;

	public const int unManagedidsincidentparentaccountandparentcontactpresent = -2147204081;

	public const int IncidentCannotCancel = -2147204082;

	public const int IncidentInvalidContractLineStateForCreate = -2147204083;

	public const int IncidentNullSpentTimeOrBilled = -2147204084;

	public const int IncidentInvalidAllotmentType = -2147204085;

	public const int unManagedidsincidentcannotclose = -2147204086;

	public const int IncidentMissingActivityRegardingObject = -2147204087;

	public const int unManagedidsincidentmissingactivityobjecttype = -2147204088;

	public const int unManagedidsincidentnullactivitytypecode = -2147204089;

	public const int unManagedidsincidentinvalidactivitytypecode = -2147204090;

	public const int unManagedidsincidentassociatedactivitycorrupted = -2147204091;

	public const int unManagedidsincidentinvalidstate = -2147204092;

	public const int IncidentContractDoesNotHaveAllotments = -2147204093;

	public const int unManagedidsincidentcontractdetaildoesnotmatchcontract = -2147204094;

	public const int IncidentMissingContractDetail = -2147204095;

	public const int IncidentInvalidContractStateForCreate = -2147204096;

	public const int InvalidPrimaryContactBasedOnAccount = -2147157916;

	public const int InvalidPrimaryContactBasedOnContact = -2147157915;

	public const int InvalidEntitlementForSelectedCustomerOrProduct = -2147157914;

	public const int InvalidEntitlementContacts = -2147204601;

	public const int EntitlementAlreadyInCanceledState = -2147204600;

	public const int DisabledCRMGoingOffline = -2147204608;

	public const int DisabledCRMGoingOnline = -2147204607;

	public const int DisabledCRMAddinLoadFailure = -2147204606;

	public const int DisabledCRMClientVersionLower = -2147204605;

	public const int DisabledCRMClientVersionHigher = -2147204604;

	public const int DisabledCRMPostOfflineUpgrade = -2147204603;

	public const int DisabledCRMOnlineCrmNotAvailable = -2147204602;

	public const int GoOfflineMetadataVersionsMismatch = -2147204576;

	public const int GoOfflineGetBCPFileException = -2147204575;

	public const int GoOfflineDbSizeLimit = -2147204574;

	public const int GoOfflineServerFailedGenerateBCPFile = -2147204573;

	public const int GoOfflineBCPFileSize = -2147204572;

	public const int GoOfflineFailedMoveData = -2147204571;

	public const int GoOfflineFailedPrepareMsde = -2147204570;

	public const int GoOfflineFailedReloadMetadataCache = -2147204569;

	public const int DoNotTrackItem = -2147204568;

	public const int GoOfflineFileWasDeleted = -2147204567;

	public const int GoOfflineEmptyFileForDelete = -2147204560;

	public const int OrganizationUIDeprecated = -2147204775;

	public const int unManagedidssqltimeouterror = -2147204783;

	public const int unManagedidssqlerror = -2147204784;

	public const int unManagedidsrcsyncinvalidfiltererror = -2147204851;

	public const int unManagedidsrcsyncnotprimary = -2147204847;

	public const int unManagedidsrcsyncnoprimary = -2147204846;

	public const int unManagedidsrcsyncnoclient = -2147204845;

	public const int unManagedidsrcsyncmethodnone = -2147204844;

	public const int unManagedidsrcsyncfilternoaccess = -2147204849;

	public const int InvalidOfflineOperation = -2147204850;

	public const int unManagedidsrcsyncsqlgenericerror = -2147204848;

	public const int unManagedidsrcsyncsqlpausederror = -2147204852;

	public const int unManagedidsrcsyncsqlstoppederror = -2147204853;

	public const int unManagedidsrcsyncsubscriptionowner = -2147204854;

	public const int unManagedidsrcsyncinvalidsubscription = -2147204855;

	public const int unManagedidsrcsyncsoapparseerror = -2147204856;

	public const int unManagedidsrcsyncsoapreaderror = -2147204857;

	public const int unManagedidsrcsyncsoapfaulterror = -2147204858;

	public const int unManagedidsrcsyncsoapservererror = -2147204859;

	public const int unManagedidsrcsyncsoapsendfailed = -2147204860;

	public const int unManagedidsrcsyncsoapconnfailed = -2147204861;

	public const int unManagedidsrcsyncsoapgenfailed = -2147204862;

	public const int unManagedidsrcsyncmsxmlfailed = -2147204863;

	public const int unManagedidsrcsyncinvalidsynctime = -2147204864;

	public const int AttachmentBlocked = -2147205623;

	public const int unManagedidsarticletemplateisnotactive = -2147205625;

	public const int unManagedidsfulltextoperationfailed = -2147205626;

	public const int unManagedidsarticletemplatecontainsarticles = -2147205627;

	public const int unManagedidsqueueorganizationidnotmatch = -2147205628;

	public const int unManagedidsqueuemissingbusinessunitid = -2147205629;

	public const int SubjectDoesNotExist = -2147205630;

	public const int SubjectLoopBeingCreated = -2147205631;

	public const int SubjectLoopExists = -2147205632;

	public const int InvalidSubmitFromUnapprovedArticle = -2147185153;

	public const int InvalidUnpublishFromUnapprovedArticle = -2147185154;

	public const int InvalidApproveFromDraftArticle = -2147185155;

	public const int InvalidUnpublishFromDraftArticle = -2147185156;

	public const int InvalidApproveFromPublishedArticle = -2147185157;

	public const int InvalidSubmitFromPublishedArticle = -2147185158;

	public const int QuoteReviseExistingActiveQuote = -2147185408;

	public const int BaseCurrencyNotDeletable = -2147185409;

	public const int CannotDeleteBaseMoneyCalculationAttribute = -2147185410;

	public const int InvalidExchangeRate = -2147185411;

	public const int InvalidCurrency = -2147185412;

	public const int CurrencyCannotBeNullDueToNonNullMoneyFields = -2147185413;

	public const int CannotUpdateProductCurrency = -2147185414;

	public const int InvalidPriceLevelCurrencyForPricingMethod = -2147185415;

	public const int DiscountTypeAndPriceLevelCurrencyNotEqual = -2147185416;

	public const int CurrencyRequiredForDiscountTypeAmount = -2147185417;

	public const int RecordAndPricelistCurrencyNotEqual = -2147185418;

	public const int ExchangeRateOfBaseCurrencyNotUpdatable = -2147185419;

	public const int BaseCurrencyCannotBeDeactivated = -2147185420;

	public const int DuplicateIsoCurrencyCode = -2147185421;

	public const int InvalidIsoCurrencyCode = -2147185422;

	public const int PercentageDiscountCannotHaveCurrency = -2147185423;

	public const int RecordAndOpportunityCurrencyNotEqual = -2147185425;

	public const int QuoteAndSalesOrderCurrencyNotEqual = -2147185426;

	public const int SalesOrderAndInvoiceCurrencyNotEqual = -2147185427;

	public const int BaseCurrencyOverflow = -2147185428;

	public const int BaseCurrencyUnderflow = -2147185429;

	public const int CurrencyNotEqual = -2147185430;

	public const int UnitNoName = -2147206362;

	public const int unManagedidsinvoicecloseapideprecated = -2147206363;

	public const int ProductDoesNotExist = -2147206364;

	public const int ProductKitLoopBeingCreated = -2147206365;

	public const int ProductKitLoopExists = -2147206366;

	public const int DiscountPercent = -2147206367;

	public const int DiscountAmount = -2147206368;

	public const int DiscountAmountAndPercent = -2147206369;

	public const int EntityIsUnlocked = -2147206370;

	public const int EntityIsLocked = -2147206371;

	public const int BaseUnitDoesNotExist = -2147206372;

	public const int UnitDoesNotExist = -2147206373;

	public const int UnitLoopBeingCreated = -2147206374;

	public const int UnitLoopExists = -2147206375;

	public const int QuantityReadonly = -2147206376;

	public const int BaseUnitNotNull = -2147206377;

	public const int UnitNotInSchedule = -2147206378;

	public const int MissingOpportunityId = -2147206379;

	public const int ProductInvalidUnit = -2147206380;

	public const int ProductMissingUomSheduleId = -2147206381;

	public const int MissingPriceLevelId = -2147206382;

	public const int MissingProductId = -2147206383;

	public const int InvalidPricePerUnit = -2147206384;

	public const int PriceLevelNameExists = -2147206385;

	public const int PriceLevelNoName = -2147206386;

	public const int MissingUomId = -2147206387;

	public const int ProductInvalidPriceLevelPercentage = -2147206388;

	public const int InvalidBaseUnit = -2147206389;

	public const int MissingUomScheduleId = -2147206390;

	public const int ParentReadOnly = -2147206391;

	public const int DuplicateProductPriceLevel = -2147206392;

	public const int ProductInvalidQuantityDecimal = -2147206393;

	public const int ProductProductNumberExists = -2147206394;

	public const int ProductNoProductNumber = -2147206395;

	public const int unManagedidscannotdeactivatepricelevel = -2147206396;

	public const int BaseUnitNotDeletable = -2147206397;

	public const int DiscountRangeOverlap = -2147206398;

	public const int LowQuantityGreaterThanHighQuantity = -2147206399;

	public const int LowQuantityLessThanZero = -2147206400;

	public const int InvalidSubstituteProduct = -2147206401;

	public const int InvalidKitProduct = -2147206402;

	public const int InvalidKit = -2147206403;

	public const int InvalidQuantityDecimalCode = -2147206404;

	public const int CannotSpecifyBothProductAndProductDesc = -2147206405;

	public const int CannotSpecifyBothUomAndProductDesc = -2147206406;

	public const int unManagedidsstatedoesnotexist = -2147206407;

	public const int FiscalSettingsAlreadyUpdated = -2147207159;

	public const int unManagedidssalespeopleinvalidfiscalcalendartype = -2147207160;

	public const int unManagedidssalespeopleinvalidfiscalperiodindex = -2147207161;

	public const int SalesPeopleManagerNotAllowed = -2147207163;

	public const int unManagedidssalespeopleinvalidterritoryobjecttype = -2147207164;

	public const int SalesPeopleDuplicateCalendarNotAllowed = -2147207165;

	public const int unManagedidssalespeopleduplicatecalendarfound = -2147207166;

	public const int SalesPeopleEmptyEffectiveDate = -2147207167;

	public const int SalesPeopleEmptySalesPerson = -2147207168;

	public const int InvalidNumberGroupFormat = -2147207424;

	public const int BaseUomNameNotSpecified = -2147207152;

	public const int InvalidActivityPartyAddress = -2147207912;

	public const int FaxNoSupport = -2147207913;

	public const int FaxNoData = -2147207914;

	public const int InvalidPartyMapping = -2147207915;

	public const int InvalidActivityXml = -2147207916;

	public const int ActivityInvalidObjectTypeCode = -2147207917;

	public const int ActivityInvalidSessionToken = -2147207918;

	public const int FaxServiceNotRunning = -2147207919;

	public const int FaxSendBlocked = -2147207920;

	public const int NoDialNumber = -2147207921;

	public const int TooManyRecipients = -2147207922;

	public const int MissingRecipient = -2147207923;

	public const int unManagedidsactivitynotroutable = -2147207925;

	public const int unManagedidsactivitydurationdoesnotmatch = -2147207926;

	public const int unManagedidsactivityinvalidduration = -2147207927;

	public const int unManagedidsactivityinvalidtimeformat = -2147207928;

	public const int unManagedidsactivityinvalidregardingobject = -2147207929;

	public const int ActivityPartyObjectTypeNotAllowed = -2147207930;

	public const int unManagedidsactivityinvalidpartyobjecttype = -2147207931;

	public const int unManagedidsactivitypartyobjectidortypemissing = -2147207932;

	public const int unManagedidsactivityinvalidobjecttype = -2147207933;

	public const int unManagedidsactivityobjectidortypemissing = -2147207934;

	public const int unManagedidsactivityinvalidtype = -2147207935;

	public const int unManagedidsactivityinvalidstate = -2147207936;

	public const int ContractInvalidDatesForRenew = -2147208680;

	public const int unManagedidscontractinvalidstartdateforrenewedcontract = -2147208681;

	public const int unManagedidscontracttemplateabbreviationexists = -2147208682;

	public const int ContractInvalidPrice = -2147208683;

	public const int unManagedidscontractinvalidtotalallotments = -2147208684;

	public const int ContractInvalidContract = -2147208685;

	public const int unManagedidscontractinvalidowner = -2147208686;

	public const int ContractInvalidContractTemplate = -2147208687;

	public const int ContractInvalidBillToCustomer = -2147208688;

	public const int ContractInvalidBillToAddress = -2147208689;

	public const int ContractInvalidServiceAddress = -2147208690;

	public const int ContractInvalidCustomer = -2147208691;

	public const int ContractNoLineItems = -2147208692;

	public const int ContractTemplateNoAbbreviation = -2147208693;

	public const int unManagedidscontractopencasesexist = -2147208694;

	public const int unManagedidscontractlineitemdoesnotexist = -2147208696;

	public const int unManagedidscontractdoesnotexist = -2147208697;

	public const int ContractTemplateDoesNotExist = -2147208698;

	public const int ContractInvalidAllotmentTypeCode = -2147208699;

	public const int ContractLineInvalidState = -2147208700;

	public const int ContractInvalidState = -2147208701;

	public const int ContractInvalidStartEndDate = -2147208702;

	public const int unManagedidscontractaccountmissing = -2147208703;

	public const int unManagedidscontractunexpected = -2147208704;

	public const int unManagedidsevalerrorformatlookupparameter = -2147210164;

	public const int unManagedidsevalerrorformattimezonecodeparameter = -2147210165;

	public const int unManagedidsevalerrorformatdecimalparameter = -2147210166;

	public const int unManagedidsevalerrorformatintegerparameter = -2147210167;

	public const int unManagedidsevalerrorobjecttype = -2147210168;

	public const int unManagedidsevalerrorqueueidparameter = -2147210169;

	public const int unManagedidsevalerrorformatpicklistparameter = -2147210170;

	public const int unManagedidsevalerrorformatbooleanparameter = -2147210171;

	public const int unManagedidsevalerrorformatdatetimeparameter = -2147210172;

	public const int unManagedidsevalerrorisnulllistparameter = -2147210173;

	public const int unManagedidsevalerrorinlistparameter = -2147210174;

	public const int unManagedidsevalerrorsetactivityparty = -2147210175;

	public const int unManagedidsevalerrorremovefromactivityparty = -2147210176;

	public const int unManagedidsevalerrorappendtoactivityparty = -2147210177;

	public const int unManagedidsevaltimererrorcalculatescheduletime = -2147210178;

	public const int unManagedidsevaltimerinvalidparameternumber = -2147210179;

	public const int unManagedidsevalcreateshouldhave2parameters = -2147210180;

	public const int unManagedidsevalerrorcreate = -2147210181;

	public const int unManagedidsevalerrorcontainparameter = -2147210182;

	public const int unManagedidsevalerrorendwithparameter = -2147210183;

	public const int unManagedidsevalerrorbeginwithparameter = -2147210184;

	public const int unManagedidsevalerrorstrlenparameter = -2147210185;

	public const int unManagedidsevalerrorsubstrparameter = -2147210186;

	public const int unManagedidsevalerrorinvalidrecipient = -2147210187;

	public const int unManagedidsevalerrorinparameter = -2147210188;

	public const int unManagedidsevalerrorbetweenparameter = -2147210189;

	public const int unManagedidsevalerrorneqparameter = -2147210190;

	public const int unManagedidsevalerroreqparameter = -2147210191;

	public const int unManagedidsevalerrorleqparameter = -2147210192;

	public const int unManagedidsevalerrorltparameter = -2147210193;

	public const int unManagedidsevalerrorgeqparameter = -2147210194;

	public const int unManagedidsevalerrorgtparameter = -2147210195;

	public const int unManagedidsevalerrorabsparameter = -2147210196;

	public const int unManagedidsevalerrorinvalidparameter = -2147210197;

	public const int unManagedidsevalgenericerror = -2147210198;

	public const int unManagedidsevalerrorincidentqueue = -2147210199;

	public const int unManagedidsevalerrorhalt = -2147210200;

	public const int unManagedidsevalerrorexec = -2147210201;

	public const int unManagedidsevalerrorposturl = -2147210202;

	public const int unManagedidsevalerrorsetstate = -2147210203;

	public const int unManagedidsevalerrorroute = -2147210204;

	public const int unManagedidsevalerrorupdate = -2147210205;

	public const int unManagedidsevalerrorassign = -2147210206;

	public const int unManagedidsevalerroremailtemplate = -2147210207;

	public const int unManagedidsevalerrorsendemail = -2147210208;

	public const int unManagedidsevalerrorunhandleincident = -2147210209;

	public const int unManagedidsevalerrorhandleincident = -2147210210;

	public const int unManagedidsevalerrorcreateincident = -2147210211;

	public const int unManagedidsevalerrornoteattachment = -2147210212;

	public const int unManagedidsevalerrorcreatenote = -2147210213;

	public const int unManagedidsevalerrorunhandleactivity = -2147210214;

	public const int unManagedidsevalerrorhandleactivity = -2147210215;

	public const int unManagedidsevalerroractivityattachment = -2147210216;

	public const int unManagedidsevalerrorcreateactivity = -2147210217;

	public const int unManagedidsevalerrordividedbyzero = -2147210218;

	public const int unManagedidsevalerrormodulusparameter = -2147210219;

	public const int unManagedidsevalerrormodulusparameters = -2147210220;

	public const int unManagedidsevalerrordivisionparameter = -2147210221;

	public const int unManagedidsevalerrordivisionparameters = -2147210222;

	public const int unManagedidsevalerrormultiplicationparameter = -2147210223;

	public const int unManagedidsevalerrorsubtractionparameter = -2147210224;

	public const int unManagedidsevalerroraddparameter = -2147210225;

	public const int unManagedidsevalmissselectquery = -2147210226;

	public const int unManagedidsevalchangetypeerror = -2147210227;

	public const int unManagedidsevalallcompleted = -2147210228;

	public const int unManagedidsevalmetabaseattributenotmatchquery = -2147210229;

	public const int unManagedidsevalmetabaseentitynotmatchquery = -2147210230;

	public const int unManagedidsevalpropertyisnull = -2147210231;

	public const int unManagedidsevalmetabaseattributenotfound = -2147210232;

	public const int unManagedidsevalmetabaseentitycompoundkeys = -2147210233;

	public const int unManagedidsevalpropertynotfound = -2147210234;

	public const int unManagedidsevalobjectnotfound = -2147210235;

	public const int unManagedidsevalcompleted = -2147210236;

	public const int unManagedidsevalaborted = -2147210237;

	public const int unManagedidsevalallaborted = -2147210238;

	public const int unManagedidsevalassignshouldhave4parameters = -2147210239;

	public const int unManagedidsevalupdateshouldhave3parameters = -2147210240;

	public const int unManagedidscpdecryptfailed = -2147211005;

	public const int unManagedidscpencryptfailed = -2147211006;

	public const int unManagedidscpbadpassword = -2147211007;

	public const int unManagedidscpuserdoesnotexist = -2147211008;

	public const int unManagedidsdataaccessunexpected = -2147212544;

	public const int unManagedidspropbagattributealreadyset = -2147213249;

	public const int unManagedidspropbagattributenotnullable = -2147213250;

	public const int unManagedidsrspropbagdbinfoalreadyset = -2147213251;

	public const int unManagedidsrspropbagdbinfonotset = -2147213252;

	public const int unManagedidspropbagcolloutofrange = -2147213282;

	public const int unManagedidspropbagnullproperty = -2147213310;

	public const int unManagedidspropbagnointerface = -2147213311;

	public const int unManagedMissingObjectType = -2147213309;

	public const int unManagedObjectTypeUnexpected = -2147213308;

	public const int BusinessUnitCannotBeDisabled = -2147213991;

	public const int BusinessUnitIsNotDisabledAndCannotBeDeleted = -2147213984;

	public const int BusinessUnitHasChildAndCannotBeDeleted = -2147213983;

	public const int BusinessUnitDefaultTeamOwnsRecords = -2147213982;

	public const int RootBusinessUnitCannotBeDisabled = -2147213981;

	public const int unManagedidspropbagpropertynotfound = -2147213312;

	public const int ReadOnlyUserNotSupported = -2147214016;

	public const int SupportUserCannotBeCreateNorUpdated = -2147214015;

	public const int ApplicationUserCannotBeUpdated = -2147214008;

	public const int ApplicationNotRegisteredWithDeployment = -2147214007;

	public const int InvalidOAuthToken = -2147214000;

	public const int ExpiredOAuthToken = -2147213998;

	public const int CannotAssignRolesToSupportUser = -2147213999;

	public const int CannotMakeSelfReadOnlyUser = -2147214023;

	public const int CannotMakeReadOnlyUser = -2147214024;

	public const int unManagedidsbizmgmtcantchangeorgname = -2147214026;

	public const int MultipleOrganizationsNotAllowed = -2147214027;

	public const int UserSettingsInvalidAdvancedFindStartupMode = -2147214028;

	public const int CannotModifySpecialUser = -2147214029;

	public const int unManagedidsbizmgmtcannotaddlocaluser = -2147214030;

	public const int CannotModifySysAdmin = -2147214031;

	public const int CannotModifySupportUser = -2147214013;

	public const int CannotAssignSupportUser = -2147214012;

	public const int CannotRemoveFromSupportUser = -2147214011;

	public const int CannotCreateFromSupportUser = -2147214010;

	public const int CannotUpdateSupportUser = -2147214009;

	public const int CannotRemoveFromSysAdmin = -2147214032;

	public const int CannotDisableSysAdmin = -2147214033;

	public const int CannotDeleteSysAdmin = -2147214034;

	public const int CannotDeleteSupportUser = -2147214014;

	public const int CannotDeleteSystemCustomizer = -2147214006;

	public const int CannotCreateSyncUserObjectMissing = -2147214005;

	public const int CannotUpdateSyncUserIsLicensedField = -2147214004;

	public const int CannotCreateSyncUserIsLicensedField = -2147214003;

	public const int CannotUpdateSyncUserIsSyncWithDirectoryField = -2147214002;

	public const int unManagedidsbizmgmtcannotreadaccountcontrol = -2147214035;

	public const int UserAlreadyExists = -2147214036;

	public const int unManagedidsbizmgmtusersettingsnotcreated = -2147214037;

	public const int ObjectNotFoundInAD = -2147214038;

	public const int GenericActiveDirectoryError = -2147214025;

	public const int unManagedidsbizmgmtnoparentbusiness = -2147214039;

	public const int ParentUserDoesNotExist = -2147214041;

	public const int ChildUserDoesNotExist = -2147214042;

	public const int UserLoopBeingCreated = -2147214043;

	public const int UserLoopExists = -2147214044;

	public const int ParentBusinessDoesNotExist = -2147214045;

	public const int ChildBusinessDoesNotExist = -2147214046;

	public const int BusinessManagementLoopBeingCreated = -2147214047;

	public const int BusinessManagementLoopExists = -2147214048;

	public const int BusinessManagementInvalidUserId = -2147214049;

	public const int unManagedidsbizmgmtuserdoesnothaveparent = -2147214050;

	public const int unManagedidsbizmgmtcannotenableprovision = -2147214051;

	public const int unManagedidsbizmgmtcannotenablebusiness = -2147214052;

	public const int unManagedidsbizmgmtcannotdisableprovision = -2147214053;

	public const int unManagedidsbizmgmtcannotdisablebusiness = -2147214054;

	public const int unManagedidsbizmgmtcannotdeleteprovision = -2147214055;

	public const int unManagedidsbizmgmtcannotdeletebusiness = -2147214056;

	public const int unManagedidsbizmgmtcannotremovepartnershipdefaultuser = -2147214057;

	public const int unManagedidsbizmgmtpartnershipnotinpendingstatus = -2147214058;

	public const int unManagedidsbizmgmtdefaultusernotinpartnerbusiness = -2147214059;

	public const int unManagedidsbizmgmtcallernotinpartnerbusiness = -2147214060;

	public const int unManagedidsbizmgmtdefaultusernotinprimarybusiness = -2147214061;

	public const int unManagedidsbizmgmtcallernotinprimarybusiness = -2147214062;

	public const int unManagedidsbizmgmtpartnershipalreadyexists = -2147214063;

	public const int unManagedidsbizmgmtprimarysameaspartner = -2147214064;

	public const int unManagedidsbizmgmtmisspartnerbusiness = -2147214065;

	public const int unManagedidsbizmgmtmissprimarybusiness = -2147214066;

	public const int InvalidAccessModeTransition = -2147213978;

	public const int MissingTeamName = -2147214069;

	public const int TeamAdministratorMissedPrivilege = -2147214070;

	public const int CannotDisableTenantAdmin = -2147213979;

	public const int CannotRemoveTenantAdminFromSysAdminRole = -2147213980;

	public const int UserNotInParentHierarchy = -2147214073;

	public const int unManagedidsbizmgmtusercannotbeownparent = -2147214074;

	public const int unManagedidsbizmgmtcannotmovedefaultuser = -2147214075;

	public const int unManagedidsbizmgmtbusinessparentdiffmerchant = -2147214076;

	public const int unManagedidsbizmgmtdefaultusernotinbusiness = -2147214077;

	public const int unManagedidsbizmgmtmissparentbusiness = -2147214078;

	public const int unManagedidsbizmgmtmissuserdomainname = -2147214079;

	public const int unManagedidsbizmgmtmissbusinessname = -2147214080;

	public const int unManagedidsxmlinvalidread = -2147214840;

	public const int unManagedidsxmlinvalidfield = -2147214841;

	public const int unManagedidsxmlinvalidentityattributes = -2147214842;

	public const int unManagedidsxmlunexpected = -2147214843;

	public const int unManagedidsxmlparseerror = -2147214844;

	public const int unManagedidsxmlinvalidcollectionname = -2147214845;

	public const int unManagedidsxmlinvalidupdate = -2147214846;

	public const int unManagedidsxmlinvalidcreate = -2147214847;

	public const int unManagedidsxmlinvalidentityname = -2147214848;

	public const int unManagedidsnotesnoattachment = -2147215612;

	public const int unManagedidsnotesloopbeingcreated = -2147215613;

	public const int unManagedidsnotesloopexists = -2147215614;

	public const int unManagedidsnotesalreadyattached = -2147215615;

	public const int unManagedidsnotesnotedoesnotexist = -2147215616;

	public const int DuplicatedPrivilege = -2147216369;

	public const int MemberHasAlreadyBeenContacted = -2147216370;

	public const int TeamInWrongBusiness = -2147216371;

	public const int unManagedidsrolesdeletenonparentrole = -2147216372;

	public const int InvalidPrivilegeDepth = -2147216373;

	public const int unManagedidsrolesinvalidrolename = -2147216374;

	public const int UserInWrongBusiness = -2147216375;

	public const int unManagedidsrolesmissprivid = -2147216376;

	public const int unManagedidsrolesmissrolename = -2147216377;

	public const int unManagedidsrolesmissbusinessid = -2147216378;

	public const int unManagedidsrolesmissroleid = -2147216379;

	public const int unManagedidsrolesinvalidtemplateid = -2147216380;

	public const int RoleAlreadyExists = -2147216381;

	public const int unManagedidsrolesroledoesnotexist = -2147216382;

	public const int unManagedidsrolesinvalidroleid = -2147216383;

	public const int unManagedidsrolesinvalidroledata = -2147216384;

	public const int QueryBuilderSerializationInvalidIsQuickFindFilter = -2147217096;

	public const int QueryBuilderAttributeCannotBeGroupByAndAggregate = -2147217097;

	public const int SqlArithmeticOverflowError = -2147217098;

	public const int QueryBuilderInvalidDateGrouping = -2147217099;

	public const int QueryBuilderAliasRequiredForAggregateOrderBy = -2147217100;

	public const int QueryBuilderAttributeRequiredForNonAggregateOrderBy = -2147217101;

	public const int QueryBuilderAliasNotAllowedForNonAggregateOrderBy = -2147217102;

	public const int QueryBuilderAttributeNotAllowedForAggregateOrderBy = -2147217103;

	public const int QueryBuilderDuplicateAlias = -2147217104;

	public const int QueryBuilderInvalidAggregateAttribute = -2147217105;

	public const int QueryBuilderDeserializeInvalidGroupBy = -2147217106;

	public const int QueryBuilderNoAttrsDistinctConflict = -2147217108;

	public const int QueryBuilderInvalidPagingCookie = -2147217110;

	public const int QueryBuilderPagingOrderBy = -2147217111;

	public const int QueryBuilderEntitiesDontMatch = -2147217112;

	public const int QueryBuilderLinkNodeForOrderNotFound = -2147217114;

	public const int QueryBuilderDeserializeNoDocElemXml = -2147217115;

	public const int QueryBuilderDeserializeEmptyXml = -2147217116;

	public const int QueryBuilderElementNotFound = -2147217117;

	public const int QueryBuilderInvalidFilterType = -2147217118;

	public const int QueryBuilderInvalidJoinOperator = -2147217119;

	public const int QueryBuilderInvalidConditionOperator = -2147217120;

	public const int QueryBuilderInvalidOrderType = -2147217121;

	public const int QueryBuilderAttributeNotFound = -2147217122;

	public const int QueryBuilderDeserializeInvalidUtcOffset = -2147217123;

	public const int QueryBuilderDeserializeInvalidNode = -2147217124;

	public const int QueryBuilderDeserializeInvalidGetMinActiveRowVersion = -2147217125;

	public const int QueryBuilderDeserializeInvalidAggregate = -2147217126;

	public const int QueryBuilderDeserializeInvalidDescending = -2147217127;

	public const int QueryBuilderDeserializeInvalidNoLock = -2147217128;

	public const int QueryBuilderDeserializeInvalidLinkType = -2147217129;

	public const int QueryBuilderDeserializeInvalidMapping = -2147217130;

	public const int QueryBuilderDeserializeInvalidDistinct = -2147217131;

	public const int QueryBuilderSerialzeLinkTopCriteria = -2147217132;

	public const int QueryBuilderColumnSetVersionMissing = -2147217133;

	public const int QueryBuilderInvalidColumnSetVersion = -2147217134;

	public const int QueryBuilderAttributePairMismatch = -2147217135;

	public const int QueryBuilderByAttributeNonEmpty = -2147217136;

	public const int QueryBuilderByAttributeMismatch = -2147217137;

	public const int QueryBuilderMultipleIntersectEntities = -2147217138;

	public const int QueryBuilderReportView_Does_Not_Exist = -2147217139;

	public const int QueryBuilderValue_GreaterThanZero = -2147217140;

	public const int QueryBuilderNoAlias = -2147217141;

	public const int QueryBuilderAlias_Does_Not_Exist = -2147217142;

	public const int QueryBuilderInvalid_Alias = -2147217143;

	public const int QueryBuilderInvalid_Value = -2147217144;

	public const int QueryBuilderAttribute_With_Aggregate = -2147217145;

	public const int QueryBuilderBad_Condition = -2147217146;

	public const int QueryBuilderNoAttribute = -2147217149;

	public const int QueryBuilderNoEntity = -2147217150;

	public const int QueryBuilderUnexpected = -2147217151;

	public const int QueryBuilderInvalidUpdate = -2147217152;

	public const int QueryBuilderInvalidLogicalOperator = -2147217154;

	public const int unManagedidsmetadatanorelationship = -2147217918;

	public const int MetadataNoMapping = -2147217919;

	public const int MetadataNotSerializable = -2147217917;

	public const int unManagedidsmetadatanoentity = -2147217920;

	public const int unManagedidscommunicationsnosenderaddress = -2147218680;

	public const int unManagedidscommunicationstemplateinvalidtemplate = -2147218681;

	public const int unManagedidscommunicationsnoparticipationmask = -2147218682;

	public const int unManagedidscommunicationsnorecipients = -2147218683;

	public const int EmailRecipientNotSpecified = -2147218684;

	public const int unManagedidscommunicationsnosender = -2147218686;

	public const int unManagedidscommunicationsbadsender = -2147218687;

	public const int unManagedidscommunicationsnopartyaddress = -2147218688;

	public const int unManagedidsjournalingmissingincidentid = -2147219447;

	public const int unManagedidsjournalingmissingcontactid = -2147219448;

	public const int unManagedidsjournalingmissingopportunityid = -2147219449;

	public const int unManagedidsjournalingmissingaccountid = -2147219450;

	public const int unManagedidsjournalingmissingleadid = -2147219451;

	public const int unManagedidsjournalingmissingeventtype = -2147219452;

	public const int unManagedidsjournalinginvalideventtype = -2147219453;

	public const int unManagedidsjournalingmissingeventdirection = -2147219454;

	public const int unManagedidsjournalingunsupportedobjecttype = -2147219455;

	public const int SdkEntityDoesNotSupportMessage = -2147219456;

	public const int OpportunityAlreadyInOpenState = -2147220198;

	public const int LeadAlreadyInClosedState = -2147220199;

	public const int LeadAlreadyInOpenState = -2147220200;

	public const int CustomerIsInactive = -2147220201;

	public const int OpportunityCannotBeClosed = -2147220202;

	public const int OpportunityIsAlreadyClosed = -2147220203;

	public const int unManagedidscustomeraddresstypeinvalid = -2147220204;

	public const int unManagedidsleadnotassignedtocaller = -2147220205;

	public const int unManagedidscontacthaschildopportunities = -2147220206;

	public const int unManagedidsaccounthaschildopportunities = -2147220207;

	public const int unManagedidsleadoneaccount = -2147220208;

	public const int unManagedidsopportunityorphan = -2147220209;

	public const int unManagedidsopportunityoneaccount = -2147220210;

	public const int unManagedidsleadusercannotreject = -2147220211;

	public const int unManagedidsleadnotassigned = -2147220212;

	public const int unManagedidsleadnoparent = -2147220213;

	public const int ContactLoopBeingCreated = -2147220214;

	public const int ContactLoopExists = -2147220215;

	public const int PresentParentAccountAndParentContact = -2147220216;

	public const int AccountLoopBeingCreated = -2147220217;

	public const int AccountLoopExists = -2147220218;

	public const int unManagedidsopportunitymissingparent = -2147220219;

	public const int unManagedidsopportunityinvalidparent = -2147220220;

	public const int ContactDoesNotExist = -2147220221;

	public const int AccountDoesNotExist = -2147220222;

	public const int unManagedidsleaddoesnotexist = -2147220223;

	public const int unManagedidsopportunitydoesnotexist = -2147220224;

	public const int ReportDoesNotExist = -2147220327;

	public const int ReportLoopBeingCreated = -2147220328;

	public const int ReportLoopExists = -2147220329;

	public const int ParentReportLinksToSameNameChild = -2147220330;

	public const int DuplicateReportVisibility = -2147220331;

	public const int ReportRenderError = -2147220332;

	public const int SubReportDoesNotExist = -2147220333;

	public const int SrsDataConnectorNotInstalled = -2147220334;

	public const int InvalidCustomReportingWizardXml = -2147220335;

	public const int UpdateNonCustomReportFromTemplate = -2147220336;

	public const int SnapshotReportNotReady = -2147220343;

	public const int ExistingExternalReport = -2147220344;

	public const int ParentReportNotSupported = -2147220345;

	public const int ParentReportDoesNotReferenceChild = -2147220346;

	public const int MultipleParentReportsFound = -2147220347;

	public const int ReportingServicesReportExpected = -2147220348;

	public const int InvalidTransformationParameter = -2147220599;

	public const int ReflexiveEntityParentOrChildDoesNotExist = -2147220600;

	public const int EntityLoopBeingCreated = -2147220601;

	public const int EntityLoopExists = -2147220602;

	public const int UnsupportedProcessCode = -2147220603;

	public const int NoOutputTransformationParameterMappingFound = -2147220604;

	public const int RequiredColumnsNotFoundInImportFile = -2147220605;

	public const int InvalidTransformationParameterMapping = -2147220606;

	public const int UnmappedTransformationOutputDataFound = -2147220607;

	public const int InvalidTransformationParameterDataType = -2147220608;

	public const int ArrayMappingFoundForSingletonParameter = -2147220609;

	public const int SingletonMappingFoundForArrayParameter = -2147220610;

	public const int IncompleteTransformationParameterMappingsFound = -2147220611;

	public const int InvalidTransformationParameterMappings = -2147220612;

	public const int GenericTransformationInvocationError = -2147220613;

	public const int InvalidTransformationType = -2147220614;

	public const int UnableToLoadTransformationType = -2147220615;

	public const int UnableToLoadTransformationAssembly = -2147220616;

	public const int InvalidColumnMapping = -2147220617;

	public const int CannotModifyOldDataFromImport = -2147220618;

	public const int ImportFileTooLargeToUpload = -2147220619;

	public const int InvalidImportFileContent = -2147220620;

	public const int EmptyRecord = -2147220621;

	public const int LongParseRow = -2147220622;

	public const int ParseMustBeCalledBeforeTransform = -2147220623;

	public const int HeaderValueDoesNotMatchAttributeDisplayLabel = -2147220624;

	public const int InvalidTargetEntity = -2147220631;

	public const int NoHeaderColumnFound = -2147220632;

	public const int ParsingMetadataNotFound = -2147220633;

	public const int EmptyHeaderRow = -2147220634;

	public const int EmptyContent = -2147220635;

	public const int InvalidIsFirstRowHeaderForUseSystemMap = -2147220636;

	public const int InvalidGuid = -2147220637;

	public const int GuidNotPresent = -2147220638;

	public const int OwnerValueNotMapped = -2147220639;

	public const int PicklistValueNotMapped = -2147220640;

	public const int ErrorInDelete = -2147220646;

	public const int ErrorIncreate = -2147220647;

	public const int ErrorInUpdate = -2147220648;

	public const int ErrorInSetState = -2147220649;

	public const int InvalidDataFormat = -2147220650;

	public const int InvalidFormatForDataDelimiter = -2147220651;

	public const int CRMUserDoesNotExist = -2147220652;

	public const int LookupNotFound = -2147220653;

	public const int DuplicateLookupFound = -2147220654;

	public const int InvalidImportFileData = -2147220655;

	public const int InvalidXmlSSContent = -2147220656;

	public const int InvalidImportFileParseData = -2147220663;

	public const int InvalidValueForFileType = -2147220664;

	public const int EmptyImportFileRow = -2147220665;

	public const int ErrorInParseRow = -2147220666;

	public const int DataColumnsNumberMismatch = -2147220667;

	public const int InvalidHeaderColumn = -2147220668;

	public const int OwnerMappingExistsWithSourceSystemUserName = -2147220669;

	public const int PickListMappingExistsWithSourceValue = -2147220670;

	public const int InvalidValueForDataDelimiter = -2147220671;

	public const int InvalidValueForFieldDelimiter = -2147220672;

	public const int PickListMappingExistsForTargetValue = -2147220673;

	public const int MappingExistsForTargetAttribute = -2147220674;

	public const int SourceEntityMappedToMultipleTargets = -2147220675;

	public const int AttributeNotOfTypePicklist = -2147220676;

	public const int AttributeNotOfTypeReference = -2147220592;

	public const int TargetEntityNotFound = -2147220591;

	public const int TargetAttributeNotFound = -2147220590;

	public const int PicklistValueNotFound = -2147220589;

	public const int TargetAttributeInvalidForMap = -2147220588;

	public const int TargetEntityInvalidForMap = -2147220587;

	public const int InvalidFileBadCharacters = -2147220586;

	public const int ErrorsInImportFiles = -2147220662;

	public const int InvalidOperationWhenListIsNotActive = -2147220678;

	public const int InvalidOperationWhenPartyIsNotActive = -2147220677;

	public const int AsyncOperationSuspendedOrLocked = -2147220679;

	public const int DuplicateHeaderColumn = -2147220680;

	public const int EmptyHeaderColumn = -2147220681;

	public const int InvalidColumnNumber = -2147220682;

	public const int TransformMustBeCalledBeforeImport = -2147220683;

	public const int OperationCanBeCalledOnlyOnce = -2147220684;

	public const int DuplicateRecordsFound = -2147220685;

	public const int CampaignActivityClosed = -2147220687;

	public const int UnexpectedErrorInMailMerge = -2147220688;

	public const int UserCancelledMailMerge = -2147220689;

	public const int FilteredDuetoMissingEmailAddress = -2147220690;

	public const int CannotDeleteAsBackgroundOperationInProgress = -2147220693;

	public const int FilteredDuetoInactiveState = -2147220694;

	public const int MissingBOWFRules = -2147220695;

	public const int CannotSpecifyOwnerForActivityPropagation = -2147220697;

	public const int CampaignActivityAlreadyPropagated = -2147220698;

	public const int FilteredDuetoAntiSpam = -2147220699;

	public const int TemplateTypeNotSupportedForUnsubscribeAcknowledgement = -2147220700;

	public const int ErrorInImportConfig = -2147220701;

	public const int ImportConfigNotSpecified = -2147220702;

	public const int InvalidActivityType = -2147220703;

	public const int UnsupportedParameter = -2147220704;

	public const int MissingParameter = -2147220705;

	public const int CannotSpecifyCommunicationAttributeOnActivityForPropagation = -2147220706;

	public const int CannotSpecifyRecipientForActivityPropagation = -2147220707;

	public const int CannotSpecifyAttendeeForAppointmentPropagation = -2147220708;

	public const int CannotSpecifySenderForActivityPropagation = -2147220709;

	public const int CannotSpecifyOrganizerForAppointmentPropagation = -2147220710;

	public const int InvalidRegardingObjectTypeCode = -2147220711;

	public const int UnspecifiedActivityXmlForCampaignActivityPropagate = -2147220712;

	public const int MoneySizeExceeded = -2147220713;

	public const int ExtraPartyInformation = -2147220714;

	public const int NotSupported = -2147220715;

	public const int InvalidOperationForClosedOrCancelledCampaignActivity = -2147220716;

	public const int InvalidEmailTemplate = -2147220717;

	public const int CannotCreateResponseForTemplate = -2147220718;

	public const int CannotPropagateCamapaignActivityForTemplate = -2147220719;

	public const int InvalidChannelForCampaignActivityPropagate = -2147220720;

	public const int InvalidActivityTypeForCampaignActivityPropagate = -2147220721;

	public const int ObjectNotRelatedToCampaign = -2147220722;

	public const int CannotRelateObjectTypeToCampaignActivity = -2147220723;

	public const int CannotUpdateCampaignForCampaignResponse = -2147220724;

	public const int CannotUpdateCampaignForCampaignActivity = -2147220725;

	public const int CampaignNotSpecifiedForCampaignResponse = -2147220726;

	public const int CampaignNotSpecifiedForCampaignActivity = -2147220727;

	public const int CannotRelateObjectTypeToCampaign = -2147220729;

	public const int CannotCopyIncompatibleListType = -2147220730;

	public const int InvalidActivityTypeForList = -2147220731;

	public const int CannotAssociateInactiveItemToCampaign = -2147220732;

	public const int InvalidFetchXml = -2147220733;

	public const int InvalidOperationWhenListLocked = -2147220734;

	public const int UnsupportedListMemberType = -2147220735;

	public const int CannotMoveDisabledUser = -2147220889;

	public const int InvalidPrimaryKey = -2147220890;

	public const int IsvAborted = -2147220891;

	public const int CannotAssignOutlookFilters = -2147220892;

	public const int CannotCreateOutlookFilters = -2147220893;

	public const int CannotGrantAccessToOutlookFilters = -2147220888;

	public const int CannotModifyAccessToOutlookFilters = -2147220887;

	public const int CannotRevokeAccessToOutlookFilters = -2147220880;

	public const int CannotGrantAccessToOfflineFilters = -2147220879;

	public const int CannotModifyAccessToOfflineFilters = -2147220878;

	public const int CannotRevokeAccessToOfflineFilters = -2147220877;

	public const int DuplicateOutlookAppointment = -2147220876;

	public const int AppointmentScheduleNotSet = -2147220875;

	public const int PrivilegeCreateIsDisabledForOrganization = -2147220874;

	public const int UnauthorizedAccess = -2147220873;

	public const int InvalidCharactersInField = -2147220872;

	public const int CannotChangeStateOfNonpublicView = -2147220871;

	public const int CannotDeactivateDefaultView = -2147220870;

	public const int CannotSetInactiveViewAsDefault = -2147220869;

	public const int CannotExceedFilterLimit = -2147220868;

	public const int CannotHaveMultipleDefaultFilterTemplates = -2147220867;

	public const int CrmConstraintParsingError = -2147220894;

	public const int CrmConstraintEvaluationError = -2147220895;

	public const int CrmExpressionEvaluationError = -2147220896;

	public const int CrmExpressionParametersParsingError = -2147220897;

	public const int CrmExpressionBodyParsingError = -2147220898;

	public const int CrmExpressionParsingError = -2147220899;

	public const int CrmMalformedExpressionError = -2147220900;

	public const int CalloutException = -2147220901;

	public const int DateTimeFormatFailed = -2147220902;

	public const int NumberFormatFailed = -2147220903;

	public const int InvalidRestore = -2147220904;

	public const int InvalidCaller = -2147220905;

	public const int CrmSecurityError = -2147220906;

	public const int TransactionAborted = -2147220907;

	public const int CannotBindToSession = -2147220908;

	public const int SessionTokenUnavailable = -2147220909;

	public const int TransactionNotCommited = -2147220910;

	public const int TransactionNotStarted = -2147220911;

	public const int MultipleChildPicklist = -2147220912;

	public const int InvalidSingletonResults = -2147220913;

	public const int FailedToLoadAssembly = -2147220914;

	public const int CrmQueryExpressionNotInitialized = -2147220915;

	public const int InvalidRegistryKey = -2147220916;

	public const int InvalidPriv = -2147220917;

	public const int MetadataNotFound = -2147220918;

	public const int InvalidEntityClassException = -2147220919;

	public const int InvalidXmlEntityNameException = -2147220920;

	public const int InvalidXmlCollectionNameException = -2147220921;

	public const int InvalidRecurrenceRule = -2147220922;

	public const int CrmImpersonationError = -2147220923;

	public const int ServiceInstantiationFailed = -2147220924;

	public const int EntityInstantiationFailed = -2147220925;

	public const int FormTransitionError = -2147220926;

	public const int UserTimeConvertException = -2147220927;

	public const int UserTimeZoneException = -2147220928;

	public const int InvalidConnectionString = -2147220929;

	public const int OpenCrmDBConnection = -2147220930;

	public const int UnpopulatedPrimaryKey = -2147220931;

	public const int InvalidVersion = -2147220932;

	public const int InvalidOperation = -2147220933;

	public const int InvalidMetadata = -2147220934;

	public const int InvalidDateTime = -2147220935;

	public const int unManagedidscannotdefaultprivateview = -2147220936;

	public const int DuplicateRecord = -2147220937;

	public const int unManagedidsnorelationship = -2147220938;

	public const int MissingQueryType = -2147220939;

	public const int InvalidRollupType = -2147220940;

	public const int InvalidState = -2147220941;

	public const int unManagedidsviewisnotsharable = -2147220942;

	public const int PrincipalPrivilegeDenied = -2147220943;

	public const int CannotUpdateObjectBecauseItIsInactive = -2147220944;

	public const int CannotDeleteCannedView = -2147220945;

	public const int CannotUpdateBecauseItIsReadOnly = -2147220946;

	public const int CaseAlreadyResolved = -2147220273;

	public const int InvalidCustomer = -2147220947;

	public const int unManagedidsdataoutofrange = -2147220948;

	public const int unManagedidsownernotenabled = -2147220949;

	public const int BusinessManagementObjectAlreadyExists = -2147220950;

	public const int InvalidOwnerID = -2147220951;

	public const int CannotDeleteAsItIsReadOnly = -2147220952;

	public const int CannotDeleteDueToAssociation = -2147220953;

	public const int unManagedidsanonymousenabled = -2147220954;

	public const int unManagedidsusernotenabled = -2147220955;

	public const int BusinessNotEnabled = -2147220692;

	public const int CannotAssignToDisabledBusiness = -2147220691;

	public const int IsvUnExpected = -2147220956;

	public const int OnlyOwnerCanRevoke = -2147220957;

	public const int unManagedidsoutofmemory = -2147220958;

	public const int unManagedidscannotassigntobusiness = -2147220959;

	public const int PrivilegeDenied = -2147220960;

	public const int InvalidObjectTypes = -2147220961;

	public const int unManagedidscannotgrantorrevokeaccesstobusiness = -2147220962;

	public const int unManagedidsinvaliduseridorbusinessidorusersbusinessinvalid = -2147220963;

	public const int unManagedidspresentuseridandteamid = -2147220964;

	public const int MissingUserId = -2147220965;

	public const int MissingBusinessId = -2147220966;

	public const int NotImplemented = -2147220967;

	public const int InvalidPointer = -2147220968;

	public const int ObjectDoesNotExist = -2147220969;

	public const int UnExpected = -2147220970;

	public const int MissingOwner = -2147220971;

	public const int CannotShareWithOwner = -2147220972;

	public const int unManagedidsinvalidvisibilitymodificationaccess = -2147220973;

	public const int unManagedidsinvalidowninguser = -2147220974;

	public const int unManagedidsinvalidassociation = -2147220975;

	public const int InvalidAssigneeId = -2147220976;

	public const int unManagedidsfailureinittoken = -2147220977;

	public const int unManagedidsinvalidvisibility = -2147220978;

	public const int InvalidAccessRights = -2147220979;

	public const int InvalidSharee = -2147220980;

	public const int unManagedidsinvaliditemid = -2147220981;

	public const int unManagedidsinvalidorgid = -2147220982;

	public const int unManagedidsinvalidbusinessid = -2147220983;

	public const int unManagedidsinvalidteamid = -2147220984;

	public const int unManagedidsinvaliduserid = -2147220985;

	public const int InvalidParentId = -2147220986;

	public const int InvalidParent = -2147220987;

	public const int InvalidUserAuth = -2147220988;

	public const int InvalidArgument = -2147220989;

	public const int EmptyXml = -2147220990;

	public const int InvalidXml = -2147220991;

	public const int RequiredFieldMissing = -2147220992;

	public const int SearchTextLenExceeded = -2147220993;

	public const int CannotAssignOfflineFilters = -2147220225;

	public const int ArticleIsPublished = -2147220226;

	public const int InvalidArticleTemplateState = -2147220227;

	public const int InvalidArticleStateTransition = -2147220228;

	public const int InvalidArticleState = -2147220229;

	public const int NullKBArticleTemplateId = -2147220230;

	public const int NullArticleTemplateStructureXml = -2147220231;

	public const int NullArticleTemplateFormatXml = -2147220232;

	public const int NullArticleXml = -2147220233;

	public const int InvalidContractDetailId = -2147220234;

	public const int InvalidTotalPrice = -2147220235;

	public const int InvalidTotalDiscount = -2147220236;

	public const int InvalidNetPrice = -2147220237;

	public const int InvalidAllotmentsRemaining = -2147220238;

	public const int InvalidAllotmentsUsed = -2147220239;

	public const int InvalidAllotmentsTotal = -2147220240;

	public const int InvalidAllotmentsCalc = -2147220241;

	public const int CannotRouteToSameQueue = -2147220197;

	public const int CannotAddSingleQueueEnabledEntityToQueue = -2147220196;

	public const int CannotUpdateDeactivatedQueueItem = -2147220195;

	public const int CannotCreateQueueItemInactiveObject = -2147220194;

	public const int InsufficientPrivilegeToQueueOwner = -2147220192;

	public const int NoPrivilegeToWorker = -2147220191;

	public const int CannotAddQueueItemsToInactiveQueue = -2147220190;

	public const int EmailAlreadyExistsInDestinationQueue = -2147220189;

	public const int CouldNotFindQueueItemInQueue = -2147220188;

	public const int MultipleQueueItemsFound = -2147220187;

	public const int ActiveQueueItemAlreadyExists = -2147220186;

	public const int CannotRouteInactiveQueueItem = -2147220185;

	public const int QueueIdNotPresent = -2147220184;

	public const int QueueItemNotPresent = -2147220183;

	public const int CannotUpdatePrivateOrWIPQueue = -2147220242;

	public const int CannotFindUserQueue = -2147220244;

	public const int CannotFindObjectInQueue = -2147220245;

	public const int CannotRouteToQueue = -2147220246;

	public const int RouteTypeUnsupported = -2147220247;

	public const int UserIdOrQueueNotSet = -2147220248;

	public const int RoutingNotAllowed = -2147220249;

	public const int CannotUpdateMetricOnChildGoal = -2147202816;

	public const int CannotUpdateGoalPeriodInfoChildGoal = -2147202815;

	public const int CannotUpdateMetricOnGoalWithChildren = -2147202814;

	public const int FiscalPeriodGoalMissingInfo = -2147202813;

	public const int CustomPeriodGoalHavingExtraInfo = -2147202812;

	public const int ParentChildMetricIdDiffers = -2147202811;

	public const int ParentChildPeriodAttributesDiffer = -2147202810;

	public const int CustomPeriodGoalMissingInfo = -2147202809;

	public const int GoalMissingPeriodTypeInfo = -2147202808;

	public const int ParticipatingQueryEntityMismatch = -2147202807;

	public const int CannotUpdateGoalPeriodInfoClosedGoal = -2147202800;

	public const int CannotUpdateRollupFields = -2147202799;

	public const int CannotDeleteMetricWithGoals = -2147203072;

	public const int CannotUpdateRollupAttributeWithClosedGoals = -2147203071;

	public const int MetricNameAlreadyExists = -2147203070;

	public const int CannotUpdateMetricWithGoals = -2147203069;

	public const int CannotCreateUpdateSourceAttribute = -2147203068;

	public const int InvalidDateAttribute = -2147203067;

	public const int InvalidSourceEntityAttribute = -2147203066;

	public const int GoalAttributeAlreadyMapped = -2147203065;

	public const int InvalidSourceAttributeType = -2147203064;

	public const int MaxLimitForRollupAttribute = -2147203062;

	public const int InvalidGoalAttribute = -2147203061;

	public const int CannotUpdateParentAndDependents = -2147203060;

	public const int UserDoesNotHaveSendAsAllowed = -2147203059;

	public const int CannotUpdateQuoteCurrency = -2147203058;

	public const int UserDoesNotHaveSendAsForQueue = -2147203057;

	public const int InvalidSourceStateValue = -2147203056;

	public const int InvalidSourceStatusValue = -2147203055;

	public const int InvalidEntityForDateAttribute = -2147203054;

	public const int InvalidEntityForRollup = -2147203053;

	public const int InvalidFiscalPeriod = -2147203052;

	public const int unManagedchildentityisnotchild = -2147220250;

	public const int unManagedmissingparententity = -2147220251;

	public const int unManagedunablegetexecutioncontext = -2147220252;

	public const int unManagedpendingtrxexists = -2147220253;

	public const int unManagedinvalidtrxcountforcommit = -2147220254;

	public const int unManagedinvalidtrxcountforrollback = -2147220255;

	public const int unManagedunableswitchusercontext = -2147220256;

	public const int unManagedmissingdataaccess = -2147220257;

	public const int unManagedinvalidcharacterdataforaggregate = -2147220258;

	public const int unManagedtrxinterophandlerset = -2147220259;

	public const int unManagedinvalidbinaryfield = -2147220260;

	public const int unManagedinvaludidispatchfield = -2147220261;

	public const int unManagedinvaliddbdatefield = -2147220262;

	public const int unManagedinvalddbtimefield = -2147220263;

	public const int unManagedinvalidfieldtype = -2147220264;

	public const int unManagedinvalidstreamfield = -2147220265;

	public const int unManagedinvalidparametertypeforparameterizedquery = -2147220266;

	public const int unManagedinvaliddynamicparameteraccessor = -2147220267;

	public const int unManagedunablegetsessiontokennotrx = -2147220268;

	public const int unManagedunablegetsessiontoken = -2147220269;

	public const int unManagedinvalidsecurityprincipal = -2147220270;

	public const int unManagedmissingpreviousownertype = -2147220272;

	public const int unManagedinvalidprivilegeid = -2147220274;

	public const int unManagedinvalidprivilegeusergroup = -2147220275;

	public const int unManagedunexpectedpropertytype = -2147220276;

	public const int unManagedmissingaddressentity = -2147220277;

	public const int unManagederroraddingfiltertoqueryplan = -2147220278;

	public const int unManagedmissingreferencesfromrelationship = -2147220279;

	public const int unManagedmissingreferencingattribute = -2147220280;

	public const int unManagedinvalidoperator = -2147220281;

	public const int unManagedunabletoaccessqueryplanfilter = -2147220282;

	public const int unManagedmissingattributefortag = -2147220283;

	public const int unManagederrorprocessingfilternodes = -2147220284;

	public const int unManagedunabletolocateconditionfilter = -2147220285;

	public const int unManagedinvalidpagevalue = -2147220286;

	public const int unManagedinvalidcountvalue = -2147220287;

	public const int unManagedinvalidversionvalue = -2147220288;

	public const int unManagedinvalidvaluettagoutsideconditiontag = -2147220289;

	public const int unManagedinvalidorganizationid = -2147220290;

	public const int unManagedinvalidowninguser = -2147220291;

	public const int unManagedinvalidowningbusinessunitorbusinessunitid = -2147220292;

	public const int unManagedinvalidprivilegeedepth = -2147220293;

	public const int unManagedinvalidlinkobjects = -2147220294;

	public const int unManagedpartylistattributenotsupported = -2147220296;

	public const int unManagedinvalidargumentsforcondition = -2147220297;

	public const int unManagedunknownaggregateoperation = -2147220298;

	public const int unManagedmissingparentattributeonentity = -2147220299;

	public const int unManagedinvalidprocesschildofcondition = -2147220300;

	public const int unManagedunexpectedrimarykey = -2147220301;

	public const int unManagedmissinglinkentity = -2147220302;

	public const int unManagedinvalidprocessliternalcondition = -2147220303;

	public const int unManagedemptyprocessliteralcondition = -2147220304;

	public const int unManagedunusablevariantdata = -2147220305;

	public const int unManagedfieldnotvalidatedbyplatform = -2147220306;

	public const int unManagedmissingfilterattribute = -2147220307;

	public const int unManagedinvalidequalityoperand = -2147220308;

	public const int unManagedfilterindexoutofrange = -2147220309;

	public const int unManagedentityisnotintersect = -2147220310;

	public const int unManagedcihldofconditionforoffilefilters = -2147220311;

	public const int unManagedinvalidowningbusinessunit = -2147220312;

	public const int unManagedinvalidbusinessunitid = -2147220313;

	public const int unManagedmorethanonesortattribute = -2147220314;

	public const int unManagedunabletoaccessqueryplan = -2147220315;

	public const int unManagedparentattributenotfound = -2147220316;

	public const int unManagedinvalidtlsmananger = -2147220318;

	public const int unManagedinvalidescapedxml = -2147220319;

	public const int unManagedunabletoretrieveprivileges = -2147220320;

	public const int unManagedproxycreationfailed = -2147220321;

	public const int unManagedinvalidprincipal = -2147220322;

	public const int RestrictInheritedRole = -2147204782;

	public const int unManagedidsfetchbetweentext = -2147204781;

	public const int unManagedidscantdisable = -2147204780;

	public const int CascadeInvalidLinkTypeTransition = -2147204779;

	public const int InvalidOrgOwnedCascadeLinkType = -2147204778;

	public const int CallerCannotChangeOwnDomainName = -2147204767;

	public const int AsyncOperationInvalidStateChange = -2147204766;

	public const int AsyncOperationInvalidStateChangeUnexpected = -2147204765;

	public const int AsyncOperationMissingId = -2147204764;

	public const int AsyncOperationInvalidStateChangeToComplete = -2147204763;

	public const int AsyncOperationInvalidStateChangeToReady = -2147204762;

	public const int AsyncOperationInvalidStateChangeToSuspended = -2147204761;

	public const int AsyncOperationCannotUpdateNonrecurring = -2147204760;

	public const int AsyncOperationCannotUpdateRecurring = -2147204759;

	public const int AsyncOperationCannotDeleteUnlessCompleted = -2147204758;

	public const int SdkInvalidMessagePropertyName = -2147204757;

	public const int PluginAssemblyMustHavePublicKeyToken = -2147204756;

	public const int SdkMessageInvalidImageTypeRegistration = -2147204755;

	public const int SdkMessageDoesNotSupportPostImageRegistration = -2147204754;

	public const int CannotDeserializeRequest = -2147204753;

	public const int InvalidPluginRegistrationConfiguration = -2147204752;

	public const int SandboxClientPluginTimeout = -2147204751;

	public const int SandboxHostPluginTimeout = -2147204750;

	public const int SandboxWorkerPluginTimeout = -2147204749;

	public const int SandboxSdkListenerStartFailed = -2147204748;

	public const int ServiceBusPostFailed = -2147204747;

	public const int ServiceBusIssuerNotFound = -2147204746;

	public const int ServiceBusIssuerCertificateError = -2147204745;

	public const int ServiceBusExtendedTokenFailed = -2147204744;

	public const int ServiceBusPostPostponed = -2147204743;

	public const int ServiceBusPostDisabled = -2147204742;

	public const int SdkMessageNotSupportedOnServer = -2147204736;

	public const int SdkMessageNotSupportedOnClient = -2147204735;

	public const int SdkCorrelationTokenDepthTooHigh = -2147204734;

	public const int OnlyStepInPredefinedStagesCanBeModified = -2147204732;

	public const int OnlyStepInServerOnlyCanHaveSecureConfiguration = -2147204731;

	public const int OnlyStepOutsideTransactionCanCreateCrmService = -2147204730;

	public const int SdkCustomProcessingStepIsNotAllowed = -2147204729;

	public const int SdkEntityOfflineQueuePlaybackIsNotAllowed = -2147204728;

	public const int SdkMessageDoesNotSupportImageRegistration = -2147204727;

	public const int RequestLengthTooLarge = -2147204726;

	public const int SandboxWorkerNotAvailable = -2147204723;

	public const int SandboxHostNotAvailable = -2147204722;

	public const int PluginAssemblyContentSizeExceeded = -2147204721;

	public const int UnableToLoadPluginType = -2147204720;

	public const int UnableToLoadPluginAssembly = -2147204719;

	public const int InvalidPluginAssemblyContent = -2147204725;

	public const int InvalidPluginTypeImplementation = -2147204724;

	public const int InvalidPluginAssemblyVersion = -2147204741;

	public const int PluginTypeMustBeUnique = -2147204740;

	public const int InvalidAssemblySourceType = -2147204739;

	public const int InvalidAssemblyProcessorArchitecture = -2147204738;

	public const int CyclicReferencesNotSupported = -2147204737;

	public const int InvalidQuery = -2147204733;

	public const int InvalidEmailAddressFormat = -2147204718;

	public const int ContractInvalidDiscount = -2147204717;

	public const int InvalidLanguageCode = -2147204715;

	public const int ConfigNullPrimaryKey = -2147204714;

	public const int ConfigMissingDescription = -2147204713;

	public const int AttributeDoesNotSupportLocalizedLabels = -2147204712;

	public const int NoLanguageProvisioned = -2147204711;

	public const int CannotImportNullStringsForBaseLanguage = -2147204538;

	public const int CannotUpdateNonCustomizableString = -2147204537;

	public const int InvalidOrganizationId = -2147204536;

	public const int InvalidTranslationsFile = -2147204535;

	public const int MetadataRecordNotDeletable = -2147204528;

	public const int InvalidImportJobTemplateFile = -2147204527;

	public const int InvalidImportJobId = -2147204526;

	public const int MissingCrmAuthenticationToken = -2147204352;

	public const int IntegratedAuthenticationIsNotAllowed = -2147204351;

	public const int RequestIsNotAuthenticated = -2147204350;

	public const int AsyncOperationTypeIsNotRecognized = -2147204349;

	public const int FailedToDeserializeAsyncOperationData = -2147204348;

	public const int UserSettingsOverMaxPagingLimit = -2147204347;

	public const int AsyncNetworkError = -2147204346;

	public const int AsyncCommunicationError = -2147204345;

	public const int MissingCrmAuthenticationTokenOrganizationName = -2147204344;

	public const int SdkNotEnoughPrivilegeToSetCallerOriginToken = -2147204343;

	public const int OverRetrievalUpperLimitWithoutPagingCookie = -2147204342;

	public const int InvalidAllotmentsOverage = -2147204341;

	public const int TooManyConditionsInQuery = -2147204340;

	public const int TooManyLinkEntitiesInQuery = -2147204339;

	public const int TooManyConditionParametersInQuery = -2147204338;

	public const int InvalidOneToManyRelationshipForRelatedEntitiesQuery = -2147204337;

	public const int PicklistValueNotUnique = -2147204336;

	public const int UnableToLogOnUserFromUserNameAndPassword = -2147204335;

	public const int PicklistValueOutOfRange = -2147204326;

	public const int WrongNumberOfBooleanOptions = -2147204325;

	public const int BooleanOptionOutOfRange = -2147204324;

	public const int CannotAddNewBooleanValue = -2147204323;

	public const int CannotAddNewStateValue = -2147204322;

	public const int NoMoreCustomOptionValuesExist = -2147204321;

	public const int InsertOptionValueInvalidType = -2147204320;

	public const int NewStatusRequiresAssociatedState = -2147204319;

	public const int NewStatusHasInvalidState = -2147204318;

	public const int CannotDeleteEnumOptionsFromAttributeType = -2147204317;

	public const int OptionReorderArrayIncorrectLength = -2147204316;

	public const int ValueMissingInOptionOrderArray = -2147204315;

	public const int NavPaneOrderValueNotAllowed = -2147204313;

	public const int EntityRelationshipRoleCustomLabelsMissing = -2147204312;

	public const int NavPaneNotCustomizable = -2147204311;

	public const int EntityRelationshipSchemaNameRequired = -2147204310;

	public const int EntityRelationshipSchemaNameNotUnique = -2147204309;

	public const int CustomReflexiveRelationshipNotAllowedForEntity = -2147204308;

	public const int EntityCannotBeChildInCustomRelationship = -2147204307;

	public const int ReferencedEntityHasLogicalPrimaryNameField = -2147204306;

	public const int IntegerValueOutOfRange = -2147204305;

	public const int DecimalValueOutOfRange = -2147204304;

	public const int StringLengthTooLong = -2147204303;

	public const int EntityCannotParticipateInEntityAssociation = -2147204302;

	public const int DataMigrationManagerUnknownProblem = -2147204301;

	public const int ImportOperationChildFailure = -2147204300;

	public const int AttributeDeprecated = -2147204299;

	public const int DataMigrationManagerMandatoryUpdatesNotInstalled = -2147204298;

	public const int ReferencedEntityMustHaveLookupView = -2147204297;

	public const int ReferencingEntityMustHaveAssociationView = -2147204296;

	public const int CouldNotObtainLockOnResource = -2147204295;

	public const int SourceAttributeHeaderTooBig = -2147204288;

	public const int CannotDeleteDefaultStatusOption = -2147204287;

	public const int CannotFindDomainAccount = -2147204286;

	public const int CannotUpdateAppDefaultValueForStateAttribute = -2147204285;

	public const int CannotUpdateAppDefaultValueForStatusAttribute = -2147204284;

	public const int InvalidOptionSetSchemaName = -2147204283;

	public const int ReferencingEntityCannotBeSolutionAware = -2147204272;

	public const int ErrorInFieldWidthIncrease = -2147204271;

	public const int ExpiredVersionStamp = -2147204270;

	public const int AsyncOperationCannotCancel = -2147201280;

	public const int AsyncOperationCannotPause = -2147201279;

	public const int WorkflowCompileFailure = -2147201023;

	public const int UpdatePublishedWorkflowDefinition = -2147201022;

	public const int UpdateWorkflowActivation = -2147201021;

	public const int DeleteWorkflowActivation = -2147201020;

	public const int DeleteWorkflowActivationWorkflowDependency = -2147201019;

	public const int DeletePublishedWorkflowDefinitionWorkflowDependency = -2147201018;

	public const int UpdateWorkflowActivationWorkflowDependency = -2147201017;

	public const int UpdatePublishedWorkflowDefinitionWorkflowDependency = -2147201016;

	public const int CreateWorkflowActivationWorkflowDependency = -2147201015;

	public const int CreatePublishedWorkflowDefinitionWorkflowDependency = -2147201014;

	public const int WorkflowPublishedByNonOwner = -2147201013;

	public const int PublishedWorkflowOwnershipChange = -2147201012;

	public const int OnlyWorkflowDefinitionOrTemplateCanBePublished = -2147201011;

	public const int OnlyWorkflowDefinitionOrTemplateCanBeUnpublished = -2147201010;

	public const int DeleteWorkflowActiveDefinition = -2147201009;

	public const int WorkflowConditionIncorrectUnaryOperatorFormation = -2147201008;

	public const int WorkflowConditionIncorrectBinaryOperatorFormation = -2147201007;

	public const int WorkflowConditionOperatorNotSupported = -2147201006;

	public const int WorkflowConditionTypeNotSupport = -2147201005;

	public const int WorkflowValidationFailure = -2147201004;

	public const int PublishedWorkflowLimitForSkuReached = -2147201003;

	public const int NoPrivilegeToPublishWorkflow = -2147201002;

	public const int WorkflowSystemPaused = -2147201001;

	public const int WorkflowPublishNoActivationParameters = -2147201000;

	public const int CreateWorkflowDependencyForPublishedTemplate = -2147200999;

	public const int DeleteActiveWorkflowTemplateDependency = -2147200998;

	public const int UpdatePublishedWorkflowTemplate = -2147200997;

	public const int DeleteWorkflowActiveTemplate = -2147200996;

	public const int CustomActivityInvalid = -2147200995;

	public const int PrimaryEntityInvalid = -2147200994;

	public const int CannotDeserializeWorkflowInstance = -2147200993;

	public const int CannotDeserializeXamlWorkflow = -2147200992;

	public const int CannotDeleteCustomEntityUsedInWorkflow = -2147200980;

	public const int BulkMailOperationFailed = -2147200979;

	public const int WorkflowExpressionOperatorNotSupported = -2147200978;

	public const int ChildWorkflowNotFound = -2147200977;

	public const int CannotDeleteAttributeUsedInWorkflow = -2147200976;

	public const int CannotLocateRecordForWorkflowActivity = -2147200975;

	public const int PublishWorkflowWhileActingOnBehalfOfAnotherUserError = -2147200974;

	public const int CannotDisableInternetMarketingUser = -2147200973;

	public const int CannotSetWindowsLiveIdForInternetMarketingUser = -2147200972;

	public const int CannotChangeAccessModeForInternetMarketingUser = -2147200971;

	public const int CannotChangeInvitationStatusForInternetMarketingUser = -2147200970;

	public const int UIDataGenerationFailed = -2147200969;

	public const int WorkflowReferencesInvalidActivity = -2147200968;

	public const int PublishWorkflowWhileImpersonatingError = -2147200967;

	public const int ExchangeAutodiscoverError = -2147200966;

	public const int NonCrmUIWorkflowsNotSupported = -2147200960;

	public const int NotEnoughPrivilegesForXamlWorkflows = -2147200959;

	public const int WorkflowAutomaticallyDeactivated = -2147200958;

	public const int StepAutomaticallyDisabled = -2147200957;

	public const int NonCrmUIInteractiveWorkflowNotSupported = -2147200956;

	public const int WorkflowActivityNotSupported = -2147200955;

	public const int ExecuteNotOnDemandWorkflow = -2147200954;

	public const int ExecuteUnpublishedWorkflow = -2147200953;

	public const int ChildWorkflowParameterMismatch = -2147200952;

	public const int InvalidProcessStateData = -2147200951;

	public const int OutOfScopeSlug = -2147200944;

	public const int CustomWorkflowActivitiesNotSupported = -2147200943;

	public const int CrmSqlGovernorDatabaseRequestDenied = -2147180543;

	public const int InvalidAuthTicket = -2147180288;

	public const int ExpiredAuthTicket = -2147180287;

	public const int BadAuthTicket = -2147180286;

	public const int InsufficientAuthTicket = -2147180285;

	public const int OrganizationDisabled = -2147180284;

	public const int TamperedAuthTicket = -2147180283;

	public const int ExpiredKey = -2147180282;

	public const int ScaleGroupDisabled = -2147180281;

	public const int SupportLogOnExpired = -2147180280;

	public const int InvalidPartnerSolutionCustomizationProvider = -2147180279;

	public const int MultiplePartnerSecurityRoleWithSameInformation = -2147180278;

	public const int MultiplePartnerUserWithSameInformation = -2147180277;

	public const int MultipleRootBusinessUnit = -2147180276;

	public const int CannotDeletePartnerWithPartnerSolutions = -2147180275;

	public const int CannotDeletePartnerSolutionWithOrganizations = -2147180274;

	public const int CannotProvisionPartnerSolution = -2147180273;

	public const int CannotActOnBehalfOfAnotherUser = -2147180272;

	public const int SystemUserDisabled = -2147180270;

	public const int UserDoesNotHaveAdminOnlyModePermissions = -2147180269;

	public const int PluginDoesNotImplementCorrectInterface = -2147180032;

	public const int CannotCreatePluginInstance = -2147180031;

	public const int CrmLiveGenericError = -2147176448;

	public const int CrmLiveOrganizationProvisioningFailed = -2147176447;

	public const int CrmLiveMissingActiveDirectoryGroup = -2147176446;

	public const int CrmLiveInternalProvisioningError = -2147176445;

	public const int CrmLiveQueueItemDoesNotExist = -2147176444;

	public const int CrmLiveInvalidSetupParameter = -2147176443;

	public const int CrmLiveMultipleWitnessServersInScaleGroup = -2147176442;

	public const int CrmLiveMissingServerRolesInScaleGroup = -2147176441;

	public const int CrmLiveServerCannotHaveWitnessAndDataServerRoles = -2147176440;

	public const int IsNotLiveToSendInvitation = -2147176439;

	public const int MissingOrganizationFriendlyName = -2147176438;

	public const int MissingOrganizationUniqueName = -2147176437;

	public const int OfferingCategoryAndTokenNull = -2147176436;

	public const int OfferingIdNotSupported = -2147176435;

	public const int OrganizationTakenByYou = -2147176434;

	public const int OrganizationTakenBySomeoneElse = -2147176433;

	public const int InvalidTemplate = -2147176432;

	public const int InvalidUserQuota = -2147176431;

	public const int InvalidRole = -2147176430;

	public const int ErrorGeneratingInvitation = -2147176429;

	public const int CrmLiveOrganizationUpgradeFailed = -2147176428;

	public const int UnableToSendEmail = -2147176427;

	public const int InvalidEmail = -2147176426;

	public const int VersionMismatch = -2147176416;

	public const int MissingParameterToMethod = -2147176415;

	public const int InvalidValueForCountryCode = -2147176414;

	public const int InvalidValueForCurrency = -2147176413;

	public const int InvalidValueForLocale = -2147176412;

	public const int CrmLiveSupportOrganizationExistsInScaleGroup = -2147176411;

	public const int CrmLiveMonitoringOrganizationExistsInScaleGroup = -2147176410;

	public const int InvalidUserLicenseCount = -2147176409;

	public const int MissingColumn = -2147176408;

	public const int InvalidResourceType = -2147176407;

	public const int InvalidMinimumResourceLimit = -2147176406;

	public const int InvalidMaximumResourceLimit = -2147176405;

	public const int ConflictingProvisionTypes = -2147176404;

	public const int InvalidAmountProvided = -2147176403;

	public const int CrmLiveOrganizationDeleteFailed = -2147176402;

	public const int OnlyDisabledOrganizationCanBeDeleted = -2147176401;

	public const int CrmLiveOrganizationDetailsNotFound = -2147176400;

	public const int CrmLiveOrganizationFriendlyNameTooShort = -2147176399;

	public const int CrmLiveOrganizationFriendlyNameTooLong = -2147176398;

	public const int CrmLiveOrganizationUniqueNameTooShort = -2147176397;

	public const int CrmLiveOrganizationUniqueNameTooLong = -2147176396;

	public const int CrmLiveOrganizationUniqueNameInvalid = -2147176395;

	public const int CrmLiveOrganizationUniqueNameReserved = -2147176394;

	public const int ValueParsingError = -2147176393;

	public const int InvalidGranularityValue = -2147176392;

	public const int CrmLiveInvalidQueueItemSchedule = -2147176391;

	public const int CrmLiveQueueItemTimeInPast = -2147176384;

	public const int CrmLiveUnknownSku = -2147176383;

	public const int ExceedCustomEntityQuota = -2147176382;

	public const int ImportWillExceedCustomEntityQuota = -2147176381;

	public const int OrganizationMigrationUnderway = -2147176380;

	public const int CrmLiveInvoicingAccountIdMissing = -2147176379;

	public const int CrmLiveDuplicateWindowsLiveId = -2147176378;

	public const int CrmLiveDnsDomainNotFound = -2147176377;

	public const int CrmLiveDnsDomainAlreadyExists = -2147176376;

	public const int InvalidInteractiveUserQuota = -2147176375;

	public const int InvalidNonInteractiveUserQuota = -2147176368;

	public const int CrmLiveCannotFindExternalMessageProvider = -2147176367;

	public const int CrmLiveInvalidExternalMessageData = -2147176366;

	public const int CrmLiveOrganizationEnableFailed = -2147176365;

	public const int CrmLiveOrganizationDisableFailed = -2147176364;

	public const int CrmLiveAddOnUnexpectedError = -2147176363;

	public const int CrmLiveAddOnAddLicenseLimitReached = -2147176362;

	public const int CrmLiveAddOnAddStorageLimitReached = -2147176361;

	public const int CrmLiveAddOnRemoveStorageLimitReached = -2147176360;

	public const int CrmLiveAddOnOrgInNoUpdateMode = -2147176359;

	public const int CrmLiveUnknownCategory = -2147176358;

	public const int CrmLiveInvalidInvoicingAccountNumber = -2147176357;

	public const int CrmLiveAddOnDataChanged = -2147176356;

	public const int CrmLiveInvalidEmail = -2147176355;

	public const int CrmLiveInvalidPhone = -2147176354;

	public const int CrmLiveInvalidZipCode = -2147176353;

	public const int InvalidAmountFreeResourceLimit = -2147176352;

	public const int InvalidToken = -2147176351;

	public const int CrmLiveRegisterCustomCodeDisabled = -2147176350;

	public const int CrmLiveExecuteCustomCodeDisabled = -2147176349;

	public const int CrmLiveInvalidTaxId = -2147176348;

	public const int DatacenterNotAvailable = -2147176347;

	public const int ErrorConnectingToDiscoveryService = -2147176346;

	public const int OrgDoesNotExistInDiscoveryService = -2147176345;

	public const int ErrorConnectingToOrganizationService = -2147176344;

	public const int UserIsNotSystemAdminInOrganization = -2147176343;

	public const int MobileServiceError = -2147176336;

	public const int LivePlatformGeneralEmailError = -2147109600;

	public const int LivePlatformEmailInvalidTo = -2147175135;

	public const int LivePlatformEmailInvalidFrom = -2147175134;

	public const int LivePlatformEmailInvalidSubject = -2147175133;

	public const int LivePlatformEmailInvalidBody = -2147175132;

	public const int BillingPartnerCertificate = -2147175120;

	public const int BillingNoSettingError = -2147175119;

	public const int BillingTestConnectionError = -2147175118;

	public const int BillingTestConnectionException = -2147175117;

	public const int BillingUserPuidNullError = -2147175116;

	public const int BillingUnmappedErrorCode = -2147175115;

	public const int BillingUnknownErrorCode = -2147175114;

	public const int BillingUnknownException = -2147175113;

	public const int BillingRetrieveKeyError = -2147175112;

	public const int BDK_E_ADDRESS_VALIDATION_FAILURE = -2147175104;

	public const int BDK_E_AGREEMENT_ALREADY_SIGNED = -2147175103;

	public const int BDK_E_AUTHORIZATION_FAILED = -2147175102;

	public const int BDK_E_AVS_FAILED = -2147175101;

	public const int BDK_E_BAD_CITYNAME_LENGTH = -2147175100;

	public const int BDK_E_BAD_STATECODE_LENGTH = -2147175099;

	public const int BDK_E_BAD_ZIPCODE_LENGTH = -2147175098;

	public const int BDK_E_BADXML = -2147175097;

	public const int BDK_E_BANNED_PAYMENT_INSTRUMENT = -2147175096;

	public const int BDK_E_BANNEDPERSON = -2147175095;

	public const int BDK_E_CANNOT_EXCEED_MAX_OWNERSHIP = -2147175094;

	public const int BDK_E_COUNTRY_CURRENCY_PI_MISMATCH = -2147175093;

	public const int BDK_E_CREDIT_CARD_EXPIRED = -2147175092;

	public const int BDK_E_DATE_EXPIRED = -2147175091;

	public const int BDK_E_ERROR_COUNTRYCODE_MISMATCH = -2147175090;

	public const int BDK_E_ERROR_COUNTRYCODE_REQUIRED = -2147175089;

	public const int BDK_E_EXTRA_REFERRAL_DATA = -2147175088;

	public const int BDK_E_GUID_EXISTS = -2147175087;

	public const int BDK_E_INVALID_ADDRESS_ID = -2147175086;

	public const int BDK_E_INVALID_BILLABLE_ACCOUNT_ID = -2147175085;

	public const int BDK_E_INVALID_BUF_SIZE = -2147175084;

	public const int BDK_E_INVALID_CATEGORY_NAME = -2147175083;

	public const int BDK_E_INVALID_COUNTRY_CODE = -2147175082;

	public const int BDK_E_INVALID_CURRENCY = -2147175081;

	public const int BDK_E_INVALID_CUSTOMER_TYPE = -2147175080;

	public const int BDK_E_INVALID_DATE = -2147175079;

	public const int BDK_E_INVALID_EMAIL_ADDRESS = -2147175078;

	public const int BDK_E_INVALID_FILTER = -2147175077;

	public const int BDK_E_INVALID_GUID = -2147175076;

	public const int BDK_E_INVALID_INPUT_TO_TAXWARE_OR_VERAZIP = -2147175075;

	public const int BDK_E_INVALID_LOCALE = -2147175074;

	public const int BDK_E_INVALID_OBJECT_ID = -2147175073;

	public const int BDK_E_INVALID_OFFERING_GUID = -2147175072;

	public const int BDK_E_INVALID_PAYMENT_INSTRUMENT_STATUS = -2147175071;

	public const int BDK_E_INVALID_PAYMENT_METHOD_ID = -2147175070;

	public const int BDK_E_INVALID_PHONE_TYPE = -2147175069;

	public const int BDK_E_INVALID_POLICY_ID = -2147175068;

	public const int BDK_E_INVALID_REFERRALDATA_XML = -2147175067;

	public const int BDK_E_INVALID_STATE_FOR_COUNTRY = -2147175066;

	public const int BDK_E_INVALID_SUBSCRIPTION_ID = -2147175065;

	public const int BDK_E_INVALID_TAX_EXEMPT_TYPE = -2147175064;

	public const int BDK_E_MEG_CONFLICT = -2147175063;

	public const int BDK_E_MULTIPLE_CITIES_FOUND = -2147175062;

	public const int BDK_E_MULTIPLE_COUNTIES_FOUND = -2147175061;

	public const int BDK_E_NON_ACTIVE_ACCOUNT = -2147175060;

	public const int BDK_E_NOPERMISSION = -2147175059;

	public const int BDK_E_OBJECT_ROLE_LIMIT_EXCEEDED = -2147175058;

	public const int BDK_E_OFFERING_ACCOUNT_CURRENCY_MISMATCH = -2147175057;

	public const int BDK_E_OFFERING_COUNTRY_ACCOUNT_MISMATCH = -2147175056;

	public const int BDK_E_OFFERING_NOT_PURCHASEABLE = -2147175055;

	public const int BDK_E_OFFERING_PAYMENT_INSTRUMENT_MISMATCH = -2147175054;

	public const int BDK_E_OFFERING_REQUIRES_PI = -2147175053;

	public const int BDK_E_PARTNERNOTINBILLING = -2147175052;

	public const int BDK_E_PAYMENT_PROVIDER_CONNECTION_FAILED = -2147175051;

	public const int BDK_E_PRIMARY_PHONE_REQUIRED = -2147175050;

	public const int BDK_E_POLICY_DEAL_COUNTRY_MISMATCH = -2147175049;

	public const int BDK_E_PUID_ROLE_LIMIT_EXCEEDED = -2147175048;

	public const int BDK_E_RATING_FAILURE = -2147175047;

	public const int BDK_E_REQUIRED_FIELD_MISSING = -2147175046;

	public const int BDK_E_STATE_CITY_INVALID = -2147175045;

	public const int BDK_E_STATE_INVALID = -2147175044;

	public const int BDK_E_STATE_ZIP_CITY_INVALID = -2147175043;

	public const int BDK_E_STATE_ZIP_CITY_INVALID2 = -2147175042;

	public const int BDK_E_STATE_ZIP_CITY_INVALID3 = -2147175041;

	public const int BDK_E_STATE_ZIP_CITY_INVALID4 = -2147175040;

	public const int BDK_E_STATE_ZIP_COVERS_MULTIPLE_CITIES = -2147175039;

	public const int BDK_E_STATE_ZIP_INVALID = -2147175038;

	public const int BDK_E_TAXID_EXPDATE = -2147175037;

	public const int BDK_E_TOKEN_BLACKLISTED = -2147175036;

	public const int BDK_E_TOKEN_EXPIRED = -2147175035;

	public const int BDK_E_TOKEN_NOT_VALID_FOR_OFFERING = -2147175034;

	public const int BDK_E_TOKEN_RANGE_BLACKLISTED = -2147175033;

	public const int BDK_E_TRANS_BALANCE_TO_PI_INVALID = -2147175032;

	public const int BDK_E_UNKNOWN_SERVER_FAILURE = -2147175031;

	public const int BDK_E_UNSUPPORTED_CHAR_EXIST = -2147175030;

	public const int BDK_E_VATID_DOESNOTHAVEEXPDATE = -2147175029;

	public const int BDK_E_ZIP_CITY_MISSING = -2147175028;

	public const int BDK_E_ZIP_INVALID = -2147175027;

	public const int BDK_E_ZIP_INVALID_FOR_ENTERED_STATE = -2147175026;

	public const int BDK_E_USAGE_COUNT_FOR_TOKEN_EXCEEDED = -2147175025;

	public const int MissingParameterToStoredProcedure = -2147172352;

	public const int SqlErrorInStoredProcedure = -2147172351;

	public const int StoredProcedureContext = -2147172350;

	public const int InvitingOrganizationNotFound = -2147167744;

	public const int InvitingUserNotInOrganization = -2147167743;

	public const int InvitedUserAlreadyExists = -2147167742;

	public const int InvitedUserIsOrganization = -2147167741;

	public const int InvitationNotFound = -2147167740;

	public const int InvitedUserAlreadyAdded = -2147167739;

	public const int InvitationWrongUserOrgRelation = -2147167738;

	public const int InvitationIsExpired = -2147167737;

	public const int InvitationIsAccepted = -2147167736;

	public const int InvitationIsRejected = -2147167735;

	public const int InvitationIsRevoked = -2147167734;

	public const int InvitedUserMultipleTimes = -2147167733;

	public const int InvitationStatusError = -2147167732;

	public const int InvalidInvitationToken = -2147167731;

	public const int InvalidInvitationLiveId = -2147167730;

	public const int InvitationSendToSelf = -2147167729;

	public const int InvitationCannotBeReset = -2147167728;

	public const int UserDataNotFound = -2147167727;

	public const int CannotInviteDisabledUser = -2147167726;

	public const int InvitationBillingAdminUnknown = -2147167725;

	public const int CannotResetSysAdminInvite = -2147167724;

	public const int CannotSendInviteToDuplicateWindowsLiveId = -2147167723;

	public const int UserInviteDisabled = -2147167722;

	public const int InvitationOrganizationNotEnabled = -2147167721;

	public const int ClientAuthSignedOut = -2147167711;

	public const int ClientAuthSyncIssue = -2147167709;

	public const int ClientAuthCanceled = -2147167708;

	public const int ClientAuthNoConnectivityOffline = -2147167707;

	public const int ClientAuthNoConnectivity = -2147167706;

	public const int ClientAuthOfflineInvalidCallerId = -2147167705;

	public const int ConfigDBObjectDoesNotExist = -2147167696;

	public const int ConfigDBDuplicateRecord = -2147167695;

	public const int ConfigDBCannotDeleteObjectDueState = -2147167694;

	public const int ConfigDBCascadeDeleteNotAllowDelete = -2147167693;

	public const int MoveBothToPrimary = -2147167692;

	public const int MoveBothToSecondary = -2147167691;

	public const int MoveOrganizationFailedNotDisabled = -2147167690;

	public const int ConfigDBCannotUpdateObjectDueState = -2147167689;

	public const int LiveAdminUnknownObject = -2147167688;

	public const int LiveAdminUnknownCommand = -2147167687;

	public const int OperationOrganizationNotFullyDisabled = -2147167686;

	public const int ConfigDBCannotDeleteDefaultOrganization = -2147167685;

	public const int LicenseNotEnoughToActivate = -2147209452;

	public const int UserNotAssignedRoles = -2147209463;

	public const int TeamNotAssignedRoles = -2147209462;

	public const int InvalidLicenseKey = -2147167680;

	public const int NoLicenseInConfigDB = -2147167679;

	public const int InvalidLicensePid = -2147167678;

	public const int InvalidLicensePidGenCannotLoad = -2147167677;

	public const int InvalidLicensePidGenOtherError = -2147167676;

	public const int InvalidLicenseCannotReadMpcFile = -2147167675;

	public const int InvalidLicenseMpcCode = -2147167674;

	public const int LicenseUpgradePathNotAllowed = -2147167673;

	public const int OrgsInaccessible = -2147167670;

	public const int UserNotAssignedLicense = -2147167669;

	public const int UserCannotEnableWithoutLicense = -2147167668;

	public const int LicenseConfigFileInvalid = -2147167664;

	public const int LicenseTrialExpired = -2147204772;

	public const int LicenseRegistrationExpired = -2147204771;

	public const int LicenseTampered = -2147204769;

	public const int NonInteractiveUserCannotAccessUI = -2147204768;

	public const int InvalidOrganizationUniqueName = -2147167663;

	public const int InvalidOrganizationFriendlyName = -2147167662;

	public const int OrganizationNotConfigured = -2147167661;

	public const int InvalidDeviceToConfigureOrganization = -2147167660;

	public const int InvalidBrowserToConfigureOrganization = -2147167659;

	public const int DeploymentServiceNotAllowSetToThisState = -2147167648;

	public const int DeploymentServiceNotAllowOperation = -2147167647;

	public const int DeploymentServiceCannotChangeStateForDeploymentService = -2147167646;

	public const int DeploymentServiceRequestValidationFailure = -2147167645;

	public const int DeploymentServiceOperationIdentifierNotFound = -2147167644;

	public const int DeploymentServiceCannotDeleteOperationInProgress = -2147167643;

	public const int ConfigureClaimsBeforeIfd = -2147167642;

	public const int EndUserNotificationTypeNotValidForEmail = -2147167599;

	public const int InvalidRecurrenceRuleForBulkDeleteAndDuplicateDetection = -2147167584;

	public const int InvalidRecurrenceInterval = -2147167583;

	public const int QueriesForDifferentEntities = -2147167568;

	public const int AggregateInnerQuery = -2147167567;

	public const int InvalidDataDescription = -2147164160;

	public const int NonPrimaryEntityDataDescriptionFound = -2147164159;

	public const int InvalidPresentationDescription = -2147164158;

	public const int SeriesMeasureCollectionMismatch = -2147164157;

	public const int YValuesPerPointMeasureMismatch = -2147164156;

	public const int ChartAreaCategoryMismatch = -2147164155;

	public const int MultipleSubcategoriesFound = -2147164154;

	public const int MultipleMeasuresFound = -2147164153;

	public const int MultipleChartAreasFound = -2147164152;

	public const int InvalidCategory = -2147164151;

	public const int InvalidMeasureCollection = -2147164150;

	public const int DuplicateAliasFound = -2147164149;

	public const int EntityNotEnabledForCharts = -2147164148;

	public const int InvalidPageResponse = -2147164147;

	public const int VisualizationRenderingError = -2147164146;

	public const int InvalidGroupByAlias = -2147164145;

	public const int MeasureDataTypeInvalid = -2147164144;

	public const int NoDataForVisualization = -2147164143;

	public const int VisualizationModuleNotFound = -2147164142;

	public const int ImportVisualizationDeletedError = -2147164141;

	public const int ImportVisualizationExistingError = -2147164140;

	public const int VisualizationOtcNotFoundError = -2147164139;

	public const int InvalidDundasPresentationDescription = -2147164138;

	public const int InvalidWebResourceForVisualization = -2147164137;

	public const int ChartTypeNotSupportedForComparisonChart = -2147164136;

	public const int InvalidFetchCollection = -2147164135;

	public const int CategoryDataTypeInvalid = -2147164134;

	public const int DuplicateGroupByFound = -2147164133;

	public const int MultipleMeasureCollectionsFound = -2147164132;

	public const int InvalidGroupByColumn = -2147164131;

	public const int InvalidFilterCriteriaForVisualization = -2147164130;

	public const int CountSpecifiedWithoutOrder = -2147164129;

	public const int NoPreviewForCustomWebResource = -2147164128;

	public const int ChartTypeNotSupportedForMultipleSeriesChart = -2147164127;

	public const int InsufficientColumnsInSubQuery = -2147164126;

	public const int AggregateQueryRecordLimitExceeded = -2147164125;

	public const int QuickFindQueryRecordLimitExceeded = -2147164124;

	public const int CannotAddOrActonBehalfAnotherUserPrivilege = -2147160765;

	public const int HipNoSettingError = -2147160764;

	public const int HipInvalidCertificate = -2147160763;

	public const int NoSettingError = -2147160762;

	public const int AppLockTimeout = -2147160761;

	public const int InvalidRecurrencePattern = -2147163904;

	public const int CreateRecurrenceRuleFailed = -2147163903;

	public const int PartialExpansionSettingLoadError = -2147163902;

	public const int InvalidCrmDateTime = -2147163901;

	public const int InvalidAppointmentInstance = -2147163900;

	public const int InvalidSeriesId = -2147163899;

	public const int AppointmentDeleted = -2147163898;

	public const int InvalidInstanceTypeCode = -2147163897;

	public const int OverlappingInstances = -2147163896;

	public const int InvalidSeriesIdOriginalStart = -2147163895;

	public const int ValidateNotSupported = -2147163894;

	public const int RecurringSeriesCompleted = -2147163893;

	public const int ExpansionRequestIsOutsideExpansionWindow = -2147163892;

	public const int InvalidInstanceEntityName = -2147163891;

	public const int BookFirstInstanceFailed = -2147163890;

	public const int InvalidSeriesStatus = -2147163889;

	public const int RecurrenceRuleUpdateFailure = -2147163888;

	public const int RecurrenceRuleDeleteFailure = -2147163887;

	public const int EntityNotRule = -2147163886;

	public const int RecurringSeriesMasterIsLocked = -2147163885;

	public const int UpdateRecurrenceRuleFailed = -2147163884;

	public const int InstanceOutsideEffectiveRange = -2147163883;

	public const int RecurrenceCalendarTypeNotSupported = -2147163882;

	public const int RecurrenceHasNoOccurrence = -2147163881;

	public const int RecurrenceStartDateTooSmall = -2147163880;

	public const int RecurrenceEndDateTooBig = -2147163879;

	public const int OccurrenceCrossingBoundary = -2147163872;

	public const int OccurrenceTimeSpanTooBig = -2147163871;

	public const int OccurrenceSkipsOverForward = -2147163870;

	public const int OccurrenceSkipsOverBackward = -2147163869;

	public const int InvalidDaysInFebruary = -2147163868;

	public const int InvalidOccurrenceNumber = -2147163867;

	public const int InvalidNumberOfPartitions = -2147163648;

	public const int InvalidElementFound = -2147163392;

	public const int MaximumControlsLimitExceeded = -2147163391;

	public const int UserViewsOrVisualizationsFound = -2147163390;

	public const int InvalidAttributeFound = -2147163389;

	public const int MultipleFormElementsFound = -2147163388;

	public const int NullDashboardName = -2147163387;

	public const int InvalidFormType = -2147163386;

	public const int InvalidControlClass = -2147163385;

	public const int ImportDashboardDeletedError = -2147163384;

	public const int PersonalReportFound = -2147163383;

	public const int ObjectAlreadyExists = -2147163382;

	public const int EntityTypeSpecifiedForDashboard = -2147163381;

	public const int UnrestrictedIFrameInUserDashboard = -2147163380;

	public const int MultipleLabelsInUserDashboard = -2147163379;

	public const int UnsupportedDashboardInEditor = -2147163378;

	public const int InvalidUrlProtocol = -2147163377;

	public const int CannotRemoveComponentFromDefaultSolution = -2147160064;

	public const int InvalidSolutionUniqueName = -2147160062;

	public const int CannotUndeleteLabel = -2147160061;

	public const int ErrorReactivatingComponentInstance = -2147160060;

	public const int CannotDeleteRestrictedSolution = -2147160059;

	public const int CannotDeleteRestrictedPublisher = -2147160058;

	public const int ImportRestrictedSolutionError = -2147160057;

	public const int CannotSetSolutionSystemAttributes = -2147160056;

	public const int CannotUpdateDefaultSolution = -2147160055;

	public const int CannotUpdateRestrictedSolution = -2147160054;

	public const int CannotAddWorkflowActivationToSolution = -2147160052;

	public const int CannotQueryBaseTableWithAggregates = -2147160051;

	public const int InvalidStateTransition = -2147160050;

	public const int CannotUpdateUnpublishedDeleteInstance = -2147160049;

	public const int UnsupportedComponentOperation = -2147160048;

	public const int InvalidCreateOnProtectedComponent = -2147160047;

	public const int InvalidUpdateOnProtectedComponent = -2147160046;

	public const int InvalidDeleteOnProtectedComponent = -2147160045;

	public const int InvalidPublishOnProtectedComponent = -2147160044;

	public const int CannotAddNonCustomizableComponent = -2147160043;

	public const int CannotOverwriteActiveComponent = -2147160042;

	public const int CannotUpdateRestrictedPublisher = -2147160041;

	public const int CannotAddSolutionComponentWithoutRoots = -2147160040;

	public const int ComponentDefinitionDoesNotExists = -2147160039;

	public const int DependencyAlreadyExists = -2147160038;

	public const int DependencyTableNotEmpty = -2147160037;

	public const int InvalidPublisherUniqueName = -2147160036;

	public const int CannotUninstallWithDependencies = -2147160035;

	public const int InvalidSolutionVersion = -2147160034;

	public const int CannotDeleteInUseComponent = -2147160033;

	public const int CannotUninstallReferencedProtectedSolution = -2147160032;

	public const int CannotRemoveComponentFromSolution = -2147160031;

	public const int RestrictedSolutionName = -2147160030;

	public const int SolutionUniqueNameViolation = -2147160029;

	public const int CannotUpdateManagedSolution = -2147160028;

	public const int DependencyTrackingClosed = -2147160027;

	public const int GenericManagedPropertyFailure = -2147160026;

	public const int CombinedManagedPropertyFailure = -2147160025;

	public const int ReportImportCategoryOptionNotFound = -2147160024;

	public const int RequiredChildReportHasOtherParent = -2147160023;

	public const int InvalidManagedPropertyException = -2147160016;

	public const int OnlyOwnerCanSetManagedProperties = -2147160015;

	public const int CannotDeleteMetadata = -2147160014;

	public const int CannotUpdateReadOnlyPublisher = -2147160013;

	public const int CannotSelectReadOnlyPublisher = -2147160012;

	public const int CannotRemoveComponentFromSystemSolution = -2147160011;

	public const int InvalidDependency = -2147160010;

	public const int InvalidDependencyFetchXml = -2147160009;

	public const int CannotModifyReportOutsideSolutionIfManaged = -2147160008;

	public const int DuplicateDetectionRulesWereUnpublished = -2147160007;

	public const int SharePointUnableToAddUserToGroup = -2147159823;

	public const int SharePointUnableToRemoveUserFromGroup = -2147159822;

	public const int SharePointSiteNotPresentInSharePoint = -2147159821;

	public const int SharePointUnableToRetrieveGroup = -2147159820;

	public const int SharePointUnableToAclSiteWithPrivilege = -2147159819;

	public const int SharePointUnableToAclSite = -2147159818;

	public const int SharePointUnableToCreateSiteGroup = -2147159817;

	public const int SharePointSiteCreationFailure = -2147159816;

	public const int SharePointTeamProvisionJobAlreadyExists = -2147159815;

	public const int SharePointRoleProvisionJobAlreadyExists = -2147159814;

	public const int SharePointSiteWideProvisioningJobFailed = -2147159813;

	public const int DataTypeMismatchForLinkedAttribute = -2147159812;

	public const int InvalidEntityForLinkedAttribute = -2147159811;

	public const int AlreadyLinkedToAnotherAttribute = -2147159810;

	public const int DocumentManagementDisabled = -2147159809;

	public const int DefaultSiteCollectionUrlChanged = -2147159808;

	public const int RibbonImportHidingBasicHomeTab = -2147159807;

	public const int RibbonImportInvalidPrivilegeName = -2147159806;

	public const int RibbonImportEntityNotSupported = -2147159805;

	public const int RibbonImportDependencyMissingEntity = -2147159804;

	public const int RibbonImportDependencyMissingRibbonElement = -2147159803;

	public const int RibbonImportDependencyMissingWebResource = -2147159802;

	public const int RibbonImportDependencyMissingRibbonControl = -2147159801;

	public const int RibbonImportModifyingTopLevelNode = -2147159800;

	public const int RibbonImportLocationAndIdDoNotMatch = -2147159799;

	public const int RibbonImportHidingJewel = -2147159798;

	public const int RibbonImportDuplicateElementId = -2147159797;

	public const int WebResourceInvalidType = -2147159791;

	public const int WebResourceEmptySilverlightVersion = -2147159790;

	public const int WebResourceInvalidSilverlightVersion = -2147159789;

	public const int WebResourceContentSizeExceeded = -2147159788;

	public const int WebResourceDuplicateName = -2147159787;

	public const int WebResourceEmptyName = -2147159786;

	public const int WebResourceNameInvalidCharacters = -2147159785;

	public const int WebResourceNameInvalidPrefix = -2147159784;

	public const int WebResourceNameInvalidFileExtension = -2147159783;

	public const int WebResourceImportMissingFile = -2147159782;

	public const int WebResourceImportError = -2147159781;

	public const int InvalidActivityOwnershipTypeMask = -2147159776;

	public const int ActivityCannotHaveRelatedActivities = -2147159775;

	public const int CustomActivityMustHaveOfflineAvailability = -2147159774;

	public const int ActivityMustHaveRelatedNotes = -2147159773;

	public const int CustomActivityCannotBeMailMergeEnabled = -2147159772;

	public const int InvalidCustomActivityType = -2147159771;

	public const int ActivityMetadataUpdate = -2147159770;

	public const int InvalidPrimaryFieldForActivity = -2147159769;

	public const int CannotDeleteNonLeafNode = -2147159552;

	public const int DuplicateUIStatementRootsFound = -2147159551;

	public const int ErrorUpdateStatementTextIsReferenced = -2147159550;

	public const int ErrorDeleteStatementTextIsReferenced = -2147159549;

	public const int ErrorScriptSessionCannotCreateForDraftScript = -2147159548;

	public const int ErrorScriptSessionCannotUpdateForDraftScript = -2147159547;

	public const int ErrorScriptLanguageNotInstalled = -2147159546;

	public const int ErrorScriptInitialStatementNotInScript = -2147159545;

	public const int ErrorScriptInitialStatementNotRoot = -2147159544;

	public const int ErrorScriptCannotDeletePublishedScript = -2147159543;

	public const int ErrorScriptPublishMissingInitialStatement = -2147159542;

	public const int ErrorScriptPublishMalformedScript = -2147159541;

	public const int ErrorScriptUnpublishActiveScript = -2147159540;

	public const int ErrorScriptSessionCannotSetStateForDraftScript = -2147159539;

	public const int ErrorScriptStatementResponseTypeOnlyForPrompt = -2147159538;

	public const int ErrorStatementOnlyForDraftScript = -2147159537;

	public const int ErrorStatementDeleteOnlyForDraftScript = -2147159536;

	public const int ErrorInvalidUIScriptImportFile = -2147159535;

	public const int ErrorScriptFileParse = -2147159534;

	public const int ErrorScriptCannotUpdatePublishedScript = -2147159533;

	public const int ErrorInvalidFileNameChars = -2147159532;

	public const int ErrorMimeTypeNullOrEmpty = -2147159531;

	public const int ErrorImportInvalidForPublishedScript = -2147159530;

	public const int UIScriptIdentifierDuplicate = -2147159529;

	public const int UIScriptIdentifierInvalid = -2147159528;

	public const int UIScriptIdentifierInvalidLength = -2147159527;

	public const int ErrorNoQueryData = -2147159520;

	public const int ErrorUIScriptPromptMissing = -2147159519;

	public const int SharePointUrlHostValidator = -2147159295;

	public const int SharePointCrmDomainValidator = -2147159294;

	public const int SharePointServerDiscoveryValidator = -2147159293;

	public const int SharePointServerVersionValidator = -2147159292;

	public const int SharePointSiteCollectionIsAccessibleValidator = -2147159291;

	public const int SharePointUrlIsRootWebValidator = -2147159290;

	public const int SharePointSitePermissionsValidator = -2147159289;

	public const int SharePointServerLanguageValidator = -2147159288;

	public const int SharePointCrmGridIsInstalledValidator = -2147159287;

	public const int SharePointErrorRetrieveAbsoluteUrl = -2147159280;

	public const int SharePointInvalidEntityForValidation = -2147159279;

	public const int DocumentManagementIsDisabled = -2147159278;

	public const int DocumentManagementNotEnabledNoPrimaryField = -2147159277;

	public const int SharePointErrorAbsoluteUrlClipped = -2147159276;

	public const int SiteMapXsdValidationError = -2147159039;

	public const int LinkedAttributeCannotBeSecured = -2147158784;

	public const int CannotSecureAttribute = -2147158783;

	public const int AttributePrivilegeCreateIsMissing = -2147158782;

	public const int AttributePermissionUpdateIsMissingDuringShare = -2147158781;

	public const int AttributePermissionReadIsMissing = -2147158780;

	public const int CannotRemoveSysAdminProfileFromSysAdminUser = -2147158779;

	public const int QueryContainedSecuredAttributeWithoutAccess = -2147158778;

	public const int AttributePermissionUpdateIsMissingDuringUpdate = -2147158777;

	public const int AttributeNotSecured = -2147158776;

	public const int AttributeSharingCreateShouldSetReadOrAndUpdateAccess = -2147158775;

	public const int AttributeSharingUpdateInvalid = -2147158774;

	public const int AttributeSharingCreateDuplicate = -2147158773;

	public const int AdminProfileCannotBeEditedOrDeleted = -2147158772;

	public const int AttributePrivilegeInvalidToUnsecure = -2147158771;

	public const int RequireValidImportMapForUpdate = -2147158528;

	public const int InvalidFormatForUpdateMode = -2147158527;

	public const int MaximumCountForUpdateModeExceeded = -2147158526;

	public const int RecordResolutionFailed = -2147158525;

	public const int InvalidOperationForDynamicList = -2147158271;

	public const int QueryNotValidForStaticList = -2147158270;

	public const int LockStatusNotValidForDynamicList = -2147158269;

	public const int CannotCopyStaticList = -2147158268;

	public const int CannotDeleteSystemForm = -2147158446;

	public const int CannotUpdateSystemEntityIcons = -2147158445;

	public const int FallbackFormDeletion = -2147158444;

	public const int SystemFormImportMissingRoles = -2147158443;

	public const int SystemFormCopyUnmatchedEntity = -2147158442;

	public const int SystemFormCopyUnmatchedFormType = -2147158441;

	public const int SystemFormCreateWithExistingLabel = -2147158440;

	public const int QuickFormNotCustomizableThroughSdk = -2147158439;

	public const int InvalidDeactivateFormType = -2147158432;

	public const int FallbackFormDeactivation = -2147158431;

	public const int RuntimeRibbonXmlValidation = -2147158415;

	public const int InitializeErrorNoReadOnSource = -2147158016;

	public const int NoRollupAttributesDefined = -2147158399;

	public const int GoalPercentageAchievedValueOutOfRange = -2147158398;

	public const int InvalidRollupQueryAttributeSet = -2147158397;

	public const int InvalidGoalManager = -2147158396;

	public const int InactiveRollupQuerySetOnGoal = -2147158395;

	public const int InactiveMetricSetOnGoal = -2147158394;

	public const int MetricEntityOrFieldDeleted = -2147158393;

	public const int ExceededNumberOfRecordsCanFollow = -2147158368;

	public const int EntityIsNotEnabledForFollowUser = -2147158367;

	public const int EntityIsNotEnabledForFollow = -2147158366;

	public const int CannotFollowInactiveEntity = -2147158365;

	public const int MustContainAtLeastACharInMention = -2147158364;

	public const int LanguageProvisioningSrsDataConnectorNotInstalled = -2147158256;

	public const int BidsInvalidConnectionString = -2147098624;

	public const int BidsInvalidUrl = -2147098623;

	public const int BidsServerConnectionFailed = -2147098622;

	public const int BidsAuthenticationError = -2147098621;

	public const int BidsNoOrganizationsFound = -2147098620;

	public const int BidsOrganizationNotFound = -2147098619;

	public const int BidsAuthenticationFailed = -2147098618;

	public const int TransactionNotSupported = -2147098617;

	public const int IndexOutOfRange = -2147098616;

	public const int InvalidAttribute = -2147098615;

	public const int MultiValueParameterFound = -2147098614;

	public const int QueryParameterNotUnique = -2147098613;

	public const int InvalidEntity = -2147098612;

	public const int UnsupportedAttributeType = -2147098611;

	public const int FetchDataSetQueryTimeout = -2147098610;

	public const int InvalidCommand = -2147098368;

	public const int InvalidDataXml = -2147098367;

	public const int InvalidLanguageForProcessConfiguration = -2147098366;

	public const int InvalidComplexControlId = -2147098365;

	public const int InvalidProcessControlEntity = -2147098364;

	public const int InvalidProcessControlAttribute = -2147098363;

	public const int BadRequest = -2147094272;

	public const int AccessTokenExpired = -2147094271;

	public const int Forbidden = -2147094270;

	public const int Throttling = -2147094269;

	public const int NetworkIssue = -2147094268;

	public const int CouldNotReadAccessToken = -2147094267;

	public const int NotVerifiedAdmin = -2147094266;

	public const int YammerAuthTimedOut = -2147094265;

	public const int NoYammerNetworksFound = -2147094264;

	public const int OAuthTokenNotFound = -2147094263;

	public const int CouldNotDecryptOAuthToken = -2147094256;

	public const int UserNeverLoggedIntoYammer = -2147094255;

	public const int StepNotSupportedForClientBusinessRule = -2147090432;

	public const int EventNotSupportedForBusinessRule = -2147090431;

	public const int CannotUpdateTriggerForPublishedRules = -2147090430;

	public const int EventTypeAndControlNameAreMismatched = -2147090429;

	public const int ExpressionNotSupportedForEditor = -2147090428;

	public const int EditorOnlySupportAndOperatorForLogicalConditions = -2147090427;

	public const int UnexpectedRightOperandCount = -2147090426;

	public const int RuleNotSupportedForEditor = -2147090425;

	public const int BusinessRuleEditorSupportsOnlyIfConditionBranch = -2147090424;

	public const int UnsupportedStepForBusinessRuleEditor = -2147090423;

	public const int UnsupportedAttributeForEditor = -2147090416;

	public const int ExpectingAtLeastOneBusinessRuleStep = -2147090415;

	public const int EntityLimitExceeded = -2147089920;

	public const int InvalidSearchEntity = -2147089919;

	public const int InvalidSearchEntities = -2147089918;

	public const int NoQuickFindFound = -2147089917;

	public const int InvalidSearchName = -2147089916;

	public const int EntityGroupNameOrEntityNamesMustBeProvided = -2147089915;

	public const int OnlyOneSearchParameterMustBeProvided = -2147089914;

	public const int WorkflowIdIsNull = -2147089408;

	public const int PrimaryEntityIsNull = -2147089407;

	public const int TypeNotSetToDefinition = -2147089406;

	public const int ScopeNotSetToGlobal = -2147089405;

	public const int CategoryNotSetToBusinessProcessFlow = -2147089404;

	public const int BusinessProcessFlowStepHasInvalidParent = -2147089403;

	public const int NullOrEmptyAttributeInXaml = -2147089402;

	public const int InvalidGuidInXaml = -2147089401;

	public const int NoLabelsAssociatedWithStep = -2147089400;

	public const int StepStepDoesNotHaveAnyControlStepAsItsChildren = -2147089399;

	public const int InvalidXmlForParameters = -2147089392;

	public const int ControlIdIsNotUnique = -2147089391;

	public const int InvalidAttributeInXaml = -2147089390;

	public const int AttributeCannotBeUpdated = -2147089389;

	public const int StepCountInXamlExceedsMaxAllowed = -2147089388;

	public const int StepDoesNotHaveAnyChildInXaml = -2147089386;

	public const int InvalidXaml = -2147089385;

	public const int ProcessNameIsNullOrEmpty = -2147089384;

	public const int LabelIdDoesNotMatchStepId = -2147089383;

	public const int EntityExceedsMaxActiveBusinessProcessFlows = -2147089376;

	public const int EntityIsNotBusinessProcessFlowEnabled = -2147089375;

	public const int CannotDeleteUserMailbox = -2147098112;

	public const int EmailServerProfileSslRequiredForOnline = -2147098111;

	public const int EmailServerProfileInvalidCredentialRetrievalForOnline = -2147098110;

	public const int EmailServerProfileInvalidCredentialRetrievalForExchange = -2147098109;

	public const int EmailServerProfileAutoDiscoverNotAllowed = -2147098108;

	public const int EmailServerProfileLocationNotRequired = -2147098107;

	public const int ForwardMailboxCannotAssociateWithUser = -2147098105;

	public const int MailboxCannotModifyEmailAddress = -2147098104;

	public const int MailboxCredentialNotSpecified = -2147098103;

	public const int EmailServerProfileInvalidServerLocation = -2147098102;

	public const int CannotAcceptEmail = -2147098101;

	public const int QueueMailboxUnexpectedDeliveryMethod = -2147098096;

	public const int ForwardMailboxEmailAddressRequired = -2147098095;

	public const int ForwardMailboxUnexpectedIncomingDeliveryMethod = -2147098094;

	public const int ForwardMailboxUnexpectedOutgoingDeliveryMethod = -2147098093;

	public const int InvalidCredentialTypeForNonExchangeIncomingConnection = -2147098092;

	public const int Pop3UnexpectedException = -2147098091;

	public const int OpenMailboxException = -2147098090;

	public const int InvalidMailbox = -2147098089;

	public const int InvalidEmailServerLocation = -2147098088;

	public const int InactiveMailbox = -2147098087;

	public const int UnapprovedMailbox = -2147098080;

	public const int InvalidEmailAddressInMailbox = -2147098079;

	public const int EmailServerProfileNotAssociated = -2147098078;

	public const int IncomingDeliveryIsForwardMailbox = -2147098077;

	public const int InvalidIncomingDeliveryExpectingEmailConnector = -2147098076;

	public const int OutgoingNotAllowedForForwardMailbox = -2147098075;

	public const int InvalidOutgoingDeliveryExpectingEmailConnector = -2147098074;

	public const int InaccessibleSmtpServer = -2147098073;

	public const int InactiveEmailServerProfile = -2147098072;

	public const int CannotUseUserCredentials = -2147098071;

	public const int CannotActivateMailboxForDisabledUserOrQueue = -2147098064;

	public const int ZeroEmailReceived = -2147098063;

	public const int NoTestEmailAccessPrivilege = -2147098062;

	public const int MailboxCannotDeleteEmails = -2147098061;

	public const int EmailServerProfileSslRequiredForOnPremise = -2147098060;

	public const int EmailServerProfileDelegateAccessNotAllowed = -2147098059;

	public const int EmailServerProfileImpersonationNotAllowed = -2147098058;

	public const int EmailMessageSizeExceeded = -2147098057;

	public const int OutgoingSettingsUpdateNotAllowed = -2147098056;

	public const int CertificateNotFound = -2147098055;

	public const int InvalidCertificate = -2147098054;

	public const int EmailServerProfileInvalidAuthenticationProtocol = -2147098053;

	public const int EmailServerProfileADBasedAuthenticationProtocolNotAllowed = -2147098052;

	public const int EmailServerProfileBasicAuthenticationProtocolNotAllowed = -2147098051;

	public const int IncomingServerLocationAndSslSetToNo = -2147098050;

	public const int OutgoingServerLocationAndSslSetToNo = -2147098049;

	public const int IncomingServerLocationAndSslSetToYes = -2147098048;

	public const int OutgoingServerLocationAndSslSetToYes = -2147098047;

	public const int UnsupportedEmailServer = -2147098046;

	public const int S2SAccessTokenCannotBeAcquired = -2147098045;

	public const int InvalidValueProcessEmailAfter = -2147098044;

	public const int InvalidS2SAuthentication = -2147098043;

	public const int RouterIsDisabled = -2147098042;

	public const int TraceMessageConstructionError = -2147157760;

	public const int TooManyBytesInInputStream = -2147157759;

	public const int EmailRouterFileTooLargeToProcess = -2147094479;

	public const int ErrorsInEmailRouterMigrationFiles = -2147094478;

	public const int InvalidMigrationFileContent = -2147094477;

	public const int ErrorMigrationProcessExcessOnServer = -2147094476;

	public const int EntityNotEnabledForThisDevice = -2147094016;

	public const int MobileClientLanguageNotSupported = -2147094015;

	public const int MobileClientVersionNotSupported = -2147094014;

	public const int RoleNotEnabledForTabletApp = -2147094013;

	public const int NoMinimumRequiredPrivilegesForTabletApp = -2147094001;

	public const int FilePickerErrorAttachmentTypeBlocked = -2147094012;

	public const int FilePickerErrorFileSizeBreached = -2147094011;

	public const int FilePickerErrorFileSizeCannotBeZero = -2147094010;

	public const int FilePickerErrorUnableToOpenFile = -2147094009;

	public const int GetPhotoFromGalleryFailed = -2147094008;

	public const int SaveDataFileErrorOutOfSpace = -2147094007;

	public const int OpenDocumentErrorCodeUnableToFindAnActivity = -2147094006;

	public const int OpenDocumentErrorCodeUnableToFindTheDataId = -2147094005;

	public const int OpenDocumentErrorCodeGeneric = -2147094004;

	public const int FilePickerErrorApplicationInSnapView = -2147094003;

	public const int MobileClientNotConfiguredForCurrentUser = -2147094002;

	public const int DataSourceInitializeFailedErrorCode = -2147094000;

	public const int DataSourceOfflineErrorCode = -2147093999;

	public const int PingFailureErrorCode = -2147093998;

	public const int RetrieveRecordOfflineErrorCode = -2147093997;

	public const int NotMobileEnabled = -2147093995;

	public const int EntitlementInvalidStartEndDate = -2147088896;

	public const int EntitlementInvalidState = -2147088895;

	public const int InvalidChannelOrigin = -2147088894;

	public const int EntitlementChannelInvalidState = -2147088893;

	public const int EntitlementInvalidTerms = -2147088892;

	public const int InvalidEntitlementChannelTerms = -2147088891;

	public const int InvalidEntitlementActivate = -2147088890;

	public const int InvalidEntitlementCancel = -2147088889;

	public const int InvalidEntitlementDeactivate = -2147088888;

	public const int InvalidEntitlementAssociationToCase = -2147088887;

	public const int InvalidEntitlementRenew = -2147088880;

	public const int InvalidEntitlementStateAssociateToCase = -2147088879;

	public const int EntitlementChannelWithoutEntitlementId = -2147088878;

	public const int EntitlementEditDraft = -2147088877;

	public const int EntitlementAlreadyInDraftState = -2147088876;

	public const int EntitlementAlreadyInactiveState = -2147088875;

	public const int EntitlementNotActiveInAssociationToCase = -2147088874;

	public const int ExpiredEntitlementActivate = -2147088873;

	public const int InvalidEntitlementExpire = -2147088872;

	public const int InvalidDeleteProcess = -2147088751;

	public const int EntitlementTotalTerms = -2147088871;

	public const int EntitlementTemplateTotalTerms = -2147088864;

	public const int SocialCareDisabledError = -2147088863;

	public const int EntitlementBlankTerms = -2147088862;

	public const int NoIncidentMergeHavingSameParent = -2147224496;

	public const int CancelActiveChildCaseFirst = -2147224495;

	public const int CloseActiveChildCaseFirst = -2147224494;

	public const int MultilevelParentChildRelationshipNotAllowed = -2147224493;

	public const int MaxChildCasesLimitExceeded = -2147224492;

	public const int ParentCaseNotAllowedAsAChildCase = -2147224491;

	public const int CannotCloseCase = -2147157898;

	public const int CannotMergeCase = -2147157897;

	public const int CaseStateChangeInvalid = 134242420;

	public const int CannotDeleteActiveRule = -2147157936;

	public const int CannotEditActiveRule = -2147157935;

	public const int RuleAlreadyInactiveState = -2147157934;

	public const int RuleAlreadyInDraftState = -2147157933;

	public const int RuleAlreadyExistsWithSameQueueAndChannel = -2147157884;

	public const int RoutingRuleActivateDeactivateByNonOwner = -2147157883;

	public const int ConvertRuleActivateDeactivateByNonOwner = -2147157882;

	public const int CannotDeleteQueueWithQueueItems = -2140991209;

	public const int CannotDeleteQueueWithRouteRules = -2139942632;

	public const int CannotRoutePrivateQueueItemNonmember = -2140991199;

	public const int CannotRouteToNonQueueMember = -2139942631;

	public const int StateTransitionActiveToResolve = -2147157932;

	public const int StateTransitionActiveToCanceled = -2147157931;

	public const int StateTransitionResolvedOrCanceledToActive = -2147157930;

	public const int StateTransitionActivateNewStatus = -2147157929;

	public const int StateTransitionDeactivateNewStatus = -2147157928;

	public const int CannotDeleteRelatedSla = -2147157927;

	public const int CannotEditActiveSla = -2147157920;

	public const int SlaAlreadyInactiveState = -2147157919;

	public const int SlaAlreadyInDraftState = -2147157918;

	public const int CannotChangeState = -2147157917;

	public const int ImportRoutingRuleError = -2147157913;

	public const int ImportSlaError = -2147157912;

	public const int ImportConvertRuleError = -2147157911;

	public const int CannotDeleteActiveSla = -2147157904;

	public const int ActiveSlaCannotEdit = -2147157903;

	public const int SlaActivateDeactivateByNonOwner = -2147157902;

	public const int PartialHolidayScheduleCreation = -2147157901;

	public const int ErrorNoActiveRoutingRuleExists = -2147157900;

	public const int SlaPermissionToPerformAction = -2147157899;

	public const int RoutingRulePublishedByNonOwner = -2147157896;

	public const int RoutingRulePublishedByOwner = -2147157887;

	public const int ConvertRuleInvalidAutoResponseSettings = -2147157895;

	public const int CannotDeleteActiveCaseCreationRule = -2147157888;

	public const int CannotAssignActiveRecord = -2147157886;

	public const int SPFileNotCheckedOutErrorCode = -2147088640;

	public const int SPUnauthorizedAccessErrorCode = -2147088639;

	public const int SPFileAlreadyCheckedOutErrorCode = -2147088638;

	public const int SPFileCheckedOutInvalidArgsErrorCode = -2147088637;

	public const int SPSharedLockOnFileErrorCode = -2147088636;

	public const int SPExclusiveLockOnFileErrorCode = -2147088635;

	public const int SPFileNotFoundErrorCode = -2147088634;

	public const int SPFileNotLockedErrorCode = -2147088633;

	public const int SPDuplicateValuesFoundErrorCode = -2147088632;

	public const int SPFileTooLargeOrInfectedErrorCode = -2147088631;

	public const int SPBadLockInFileCollectionErrorCode = -2147088630;

	public const int SPInvalidLookupValuesErrorCode = -2147088629;

	public const int SPNullFileUrlErrorCode = -2147088628;

	public const int SPFileContentNullErrorCode = -2147088627;

	public const int SPFileSizeMismatchErrorCode = -2147088626;

	public const int SPFileIsReadOnlyErrorCode = -2147088625;

	public const int SPModifiedOnServerErrorCode = -2147088624;

	public const int SPDataValidationFiledOnFieldErrorCode = -2147088623;

	public const int SPDataValidationFiledOnListErrorCode = -2147088622;

	public const int SPDataValidationFiledOnFieldAndListErrorCode = -2147088621;

	public const int SPThrottlingLimitExceededErrorCode = -2147088620;

	public const int SPOperationNotSupportedErrorCode = -2147088619;

	public const int SPInstanceOfRecurringEventErrorCode = -2147088618;

	public const int SPItemNotExistErrorCode = -2147088617;

	public const int SPInvalidSavedQueryErrorCode = -2147088616;

	public const int SPGenericErrorCode = -2147088615;

	public const int SPSiteNotFoundErrorCode = -2147088614;

	public const int SPFolderNotFoundErrorCode = -2147088613;

	public const int SPNoActiveDocumentLocationErrorCode = -2147088612;

	public const int SPIllegalFileTypeErrorCode = -2147088611;

	public const int SPInvalidFieldValueErrorCode = -2147088610;

	public const int SPIllegalCharactersInFileNameErrorCode = -2147088609;

	public const int SPCurrentDocumentLocationDisabledErrorCode = -2147088608;

	public const int SPCurrentFolderAlreadyExistErrorCode = -2147088607;

	public const int SPNullRegardingObjectErrorCode = -2147088605;

	public const int SPOperatorNotSupportedErrorCode = -2147088604;

	public const int SPRequiredColCheckInErrorCode = -2147088603;

	public const int SPFileIsCheckedOutByOtherUser = -2147088600;

	public const int SPFileNameModifiedErrorCode = -2147088599;
}
