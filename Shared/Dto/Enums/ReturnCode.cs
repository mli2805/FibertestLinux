namespace Fibertest.Dto;

public enum ReturnCode
{
    Error = 0x0,
    Ok = 1,

    ClientCleanedAsDead = 10,
    InvalidDto = 11,
    NotImplementedYet = 12,

    RtuInitializationError = 1000,
    RtuInitializedSuccessfully = 1001,
    OtdrInitializationCannotLoadDll = 1002,
    OtdrInitializationCannotInitializeDll = 1003,
    FailedToConnectOtdr = 1004,
    OtauInitializationError = 1005,
    CharonComPortError = 1006,
    OtdrInitializationFailed = 1007, // Veex
    RtuUnauthorizedAccess = 1008,

    RtuDoesNotSupportBop = 1012,
    RtuTooBigPortNumber = 1013,

    RtuIsBusy = 1100,
    RtuInitializationInProgress = 1101,
    RtuAutoBaseMeasurementInProgress = 1102,
    RtuBaseRefAssignmentError = 1106,
    RtuMonitoringSettingsApplyError = 1107,
    RtuAttachOtauError = 1108,
    RtuDetachOtauError = 1109,
    RtuToggleToPortError = 1110,
    RtuToggleToBopPortError = 1111,
    InvalidValueOfLmax = 1121,
    SnrIs0 = 1122,

    MeasurementError = 1500,
    MeasurementEndedNormally = 1501,
    MeasurementPreparationError = 1502,
    MeasurementInterrupted = 1503,
    MeasurementTimeoutExpired,

    TcpConnectionError = 2000,
    C2DGrpcOperationError = 2004,
    D2RGrpcOperationError = 2014,
    C2RGrpcOperationError  = 2024,
    R2DGrpcOperationError = 2034,
    ToClientGrpcOperationError = 2035,
        
    FailedDeserializeJson = 2101,
    UnAuthorizedAccess = 2111, // NoSuchClientStation
    RtuNotFound = 2121,
    RtuNotAvailable = 2122,

    DbError = 3000,
    DbInitializedSuccessfully = 3001,
    DbIsNotInitializedError = 3002,
    DbCannotConvertThisReSendToAssign = 3003,
    DbEntityToUpdateNotFound = 3004,

    BaseRefAssignedSuccessfully = 4001,
    MonitoringSettingsAppliedSuccessfully = 4002,
    OtauAttachedSuccessfully = 4003,
    OtauDetachedSuccessfully = 4004,

    ClientRegisteredSuccessfully = 9000,
    NoSuchUserOrWrongPassword = 9001,
    ThisUserRegisteredFromAnotherDevice = 9002,
    ClientsCountExceeded = 9005,
    ClientsCountLicenseExpired = 9006,
    WebClientsCountExceeded,
    WebClientsCountLicenseExpired,
    SuperClientsCountExceeded,
    SuperClientsCountLicenseExpired,
    UserHasNoRightsToStartClient,
    UserHasNoRightsToStartSuperClient,
    UserHasNoRightsToStartWebClient,
    VersionsDoNotMatch,
    WrongMachineKey = 9051,
    WrongSecurityAdminPassword = 9052,
    EmptyMachineKey,
    SaveUsersMachineKey,
    NoLicenseHasBeenAppliedYet = 9099,
    NoSuchVeexTest,

    BaseRefAssignmentFailed = 9401,
    BaseRefAssignmentParamNotAcceptable = 9402,
    BaseRefAssignmentNoThresholds = 9403,
    BaseRefAssignmentLandmarkCountWrong = 9404,
    BaseRefAssignmentEdgeLandmarksWrong = 9405,

    FirstLicenseKeyMustNotBeIncremental = 9501,
    LicenseCouldNotBeAppliedRepeatedly,
    LicenseCouldNotBeApplied,

    MeasurementClientStartedSuccessfully = 9601,
    FetchMeasurementFromRtu4000Failed = 9632,
}