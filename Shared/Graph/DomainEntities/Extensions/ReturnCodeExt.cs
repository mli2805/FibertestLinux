using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.Graph
{
    public static class ReturnCodeExt
    {
        public static string GetLocalizedWithOsInfo(this ReturnCode returnCode, string? exceptionMessage)
        {
            if (exceptionMessage != null)
                exceptionMessage = Environment.NewLine + Resources.SID_Additional_OS_info_ + Environment.NewLine + exceptionMessage;

            return returnCode.GetLocalizedString() + exceptionMessage;
        }

        public static string GetLocalizedString(this ReturnCode returnCode)
        {
            switch (returnCode)
            {
                case ReturnCode.Ok: return @"OK";
                case ReturnCode.Error:
                    return Resources.SID_Error_;

                // 1000
                case ReturnCode.RtuInitializationError:
                    return Resources.SID_RTU_initialization_error_;
                case ReturnCode.RtuInitializedSuccessfully:
                    return Resources.SID_RTU_initialized_successfully_;
                case ReturnCode.OtdrInitializationCannotLoadDll:
                    return Resources.SID_Cannot_find_dll_file_;
                case ReturnCode.OtdrInitializationCannotInitializeDll:
                    return Resources.SID_Cannot_initialize_dll_;
                case ReturnCode.OtauInitializationError:
                    return Resources.SID_Failed_to_initialize_optical_switch;
                case ReturnCode.OtdrInitializationFailed:
                    return Resources.SID_Failed_to_initialize_OTDR;
                case ReturnCode.RtuUnauthorizedAccess:
                    return Resources.SID_Unauthorized_RTU_access;
                case ReturnCode.RtuDoesNotSupportBop:
                    return Resources.SID_RTU_does_not_support_BOP;
                case ReturnCode.RtuTooBigPortNumber:
                    return Resources.SID_Too_big_port_number_for_BOP_attachment;

                case ReturnCode.RtuIsBusy:
                    return Resources.SID_RTU_is_busy;
                case ReturnCode.RtuInitializationInProgress:
                    return Resources.SID_RTU_initialization_in_progress;
                case ReturnCode.RtuAutoBaseMeasurementInProgress:
                    return Resources.SID_Auto_base_measurement_in_progress;

                case ReturnCode.RtuAttachOtauError:
                    return Resources.SID_Attach_OTAU_error_;
                case ReturnCode.RtuDetachOtauError:
                    return Resources.SID_Failed_to_detach_additional_otau_;
                case ReturnCode.RtuMonitoringSettingsApplyError:
                    return Resources.SID_Failed_to_apply_monitoring_settings_;

                case ReturnCode.RtuToggleToPortError:
                case ReturnCode.RtuToggleToBopPortError:
                    return Resources.SID_Failed_to_toggle_to_port;
                case ReturnCode.InvalidValueOfLmax:
                    return Resources.SID_Failed_to_automatically_determine_the_correct_measurement_parameters;
                case ReturnCode.SnrIs0:
                    return Resources.SID_No_fiber;
                case ReturnCode.MeasurementError:
                    return Resources.SID_Measurement_error;
                case ReturnCode.MeasurementEndedNormally:
                    return Resources.SID_Measurement_completed_successfully;
                case ReturnCode.MeasurementPreparationError:
                    return Resources.SID_Measurement_preparation_error;
                case ReturnCode.MeasurementInterrupted:
                    return Resources.SID_Measurement_interrupted;
                case ReturnCode.MeasurementTimeoutExpired:
                    return Resources.SID_Measurement_timeout_expired;


                // 2000
                //case ReturnCode.C2DWcfConnectionError:
                //    return Resources.SID_Cannot_establish_connection_with_DataCenter_;
                //case ReturnCode.C2DWcfOperationError:
                //    return Resources.SID_Error_during_Client_Datacenter_connection;
                //case ReturnCode.D2RWcfConnectionError:
                //    return Resources.SID_Cannot_establish_connection_with_RTU_;
                //case ReturnCode.D2RWcfOperationError:
                //    return Resources.SID_Error_during_Datacenter_Rtu_connection;
                //case ReturnCode.C2RWcfConnectionError:
                //    return Resources.SID_Cannot_establish_connection_with_RTU_;
                //case ReturnCode.C2RWcfOperationError:
                //    return Resources.SID_Error_during_Client_RTU_operation__;

                // 3000
                case ReturnCode.DbError:
                    return Resources.SID_Database_error_;

                // 4000
                case ReturnCode.BaseRefAssignedSuccessfully:
                    return Resources.SID_Base_ref_s__are_saved_successfully_;
                case ReturnCode.MonitoringSettingsAppliedSuccessfully:
                    return Resources.SID_Monitoring_settings_applied_successfully_;
                // 9000
                case ReturnCode.ClientRegisteredSuccessfully:
                    return @"OK";
                case ReturnCode.NoSuchUserOrWrongPassword:
                    return Resources.SID_No_such_user_or_wrong_password_;
                case ReturnCode.ThisUserRegisteredFromAnotherDevice:
                    return Resources.SID_User_with_the_same_name_is_registered_on_another_PC;
                case ReturnCode.ClientsCountExceeded:
                    return Resources.SID_Exceeded_the_number_of_clients_registered_simultaneously;
                case ReturnCode.ClientsCountLicenseExpired:
                    return Resources.SID_Clients_license_is_expired;
                case ReturnCode.WebClientsCountExceeded:
                    return Resources.SID_Exceeded_the_number_of_web_clients_registered_simultaneously;
                case ReturnCode.WebClientsCountLicenseExpired:
                    return Resources.SID_Web_clients_license_is_expired;
                case ReturnCode.SuperClientsCountExceeded:
                    return Resources.SID_Exceeded_the_number_of_super_clients_registered_simultaneously;
                case ReturnCode.SuperClientsCountLicenseExpired:
                    return Resources.SID_Super_clients_license_is_expired;
                case ReturnCode.UserHasNoRightsToStartClient:
                    return Resources.SID_This_user_has_no_right_to_start_client;
                case ReturnCode.UserHasNoRightsToStartSuperClient:
                    return Resources.SID_This_user_has_no_right_to_start_SuperClient;
                case ReturnCode.UserHasNoRightsToStartWebClient:
                    return Resources.SID_This_user_has_no_right_to_start_Web_Client;
                case ReturnCode.WrongMachineKey:
                    return Resources.SID_User_is_linked_to_another_workstation;
                case ReturnCode.WrongSecurityAdminPassword:
                    return Resources.SID_Wrong_security_admin_password;
                case ReturnCode.EmptyMachineKey:
                    return Resources.SID_User_has_no_linking_to_any_workstation;
                case ReturnCode.NoLicenseHasBeenAppliedYet:
                    return Resources.SID_No_license_has_been_applied_yet;
                //9401
                case ReturnCode.BaseRefAssignmentFailed:
                    return Resources.SID_Base_reference_assignment_failed;
                //9501
                case ReturnCode.FirstLicenseKeyMustNotBeIncremental:
                    return Resources.SID_First_license_key_must_not_be_Incremental;
                case ReturnCode.LicenseCouldNotBeApplied:
                    return Resources.SID_License_could_not_be_applied_;
                case ReturnCode.LicenseCouldNotBeAppliedRepeatedly:
                    return Resources.SID_License_could_not_be_applied_repeatedly_;

                //9601
                case ReturnCode.MeasurementClientStartedSuccessfully:
                    return @"Measurement(Client) started.";


                default: return Resources.SID_Unknown_return_code + @":  " + (int)returnCode;
            }
        }

        public static string RtuAutoBaseStyle(this ReturnCode code)
        {
            switch (code)
            {
                case ReturnCode.MeasurementClientStartedSuccessfully:
                    return @"Measurement started";
                case ReturnCode.MeasurementEndedNormally:
                    return Resources.SID_Measurement___successfully;
                case ReturnCode.BaseRefAssignedSuccessfully:
                    return Resources.SID_Assignment___successfully;

                case ReturnCode.RtuInitializationInProgress:
                case ReturnCode.RtuAutoBaseMeasurementInProgress:
                case ReturnCode.RtuToggleToPortError:
                case ReturnCode.RtuToggleToBopPortError:
                case ReturnCode.InvalidValueOfLmax:
                case ReturnCode.SnrIs0:
                case ReturnCode.MeasurementError:
                case ReturnCode.MeasurementTimeoutExpired:
                case ReturnCode.FetchMeasurementFromRtu4000Failed:

                //case ReturnCode.C2DWcfConnectionError:
                //case ReturnCode.C2DWcfOperationError:
                //case ReturnCode.D2RWcfConnectionError:
                //case ReturnCode.D2RWcfOperationError:
                //case ReturnCode.C2RWcfConnectionError:
                //case ReturnCode.C2RWcfOperationError:
                //    return Resources.SID_Measurement___failed;

                case ReturnCode.BaseRefAssignmentFailed:
                    return Resources.SID_Assignment___failed;
            }
            return Resources.SID_Unknown_code;
        }
    }
}