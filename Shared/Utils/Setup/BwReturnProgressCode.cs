using Fibertest.StringResources;

namespace Fibertest.Utils.Setup
{
    public enum BwReturnProgressCode
    {
        FilesAreCopiedSuccessfully = 10,
        CopyFilesError,
        ErrorSourceFolderNotFound,
        FilesAreBeingCopied,
        AntiGhostSettingFailed,

        FilesAreUnziped,
        FilesAreUnzipedSuccessfully,
        ErrorFilesUnzipped,

        ServiceIsBeingInstalled,
        CannotInstallService,
        ServiceInstalledSuccessfully,
        ServiceIsBeingUninstalled,
        CannotUninstallService,
        ServiceUninstalledSuccessfully,
        ServiceIsBeingStopped,
        CannotStopService,
        ServiceStoppedSuccessfully,

        ShortcutsAreCreatedSuccessfully,

        ClientSetupStarted,
        ClientSetupCompletedSuccessfully,
        DataCenterSetupStarted,
        DataCenterSetupCompletedSuccessfully,
        WebComponentsSetupStarted,
        WebComponentsSetupCompletedSuccessfully,
        RtuManagerSetupStarted,
        RtuManagerSetupCompletedSuccessfully,
        SuperClientSetupStarted,
        SuperClientSetupCompletedSuccessfully,
        UninstallSetupStarted,
        UninstallSetupCompletedSuccessfully,

        IisOperationError,
        SiteInstalledSuccessfully,
        SiteInstallationError,
        SiteUninstalledSuccessfully,
        SiteUninstallationError,

        UninstallStarted,
        DeletingFiles,
        CannotDeleteSpecifiedFolder,
        FilesAreDeletedSuccessfully,
        ShortcutsDeleted,
        RegistryCleaned,
        UninstallFinished,
    }

    public static class BwReturnProgressCodeExt
    {
        public static string GetLocalizedString(this BwReturnProgressCode code, string addition)
        {
            switch (code)
            {
                case BwReturnProgressCode.FilesAreCopiedSuccessfully:
                    return Resources.SID_Files_are_copied_successfully_;
                case BwReturnProgressCode.CopyFilesError:
                    return string.Format(Resources.SID_Copy_files_error___0_, addition);
                case BwReturnProgressCode.ErrorSourceFolderNotFound:
                    return string.Format(Resources.SID_Error__Source_folder__0__not_found_, addition);
                case BwReturnProgressCode.FilesAreBeingCopied:
                    return Resources.SID_Files_are_copied___;
                case BwReturnProgressCode.AntiGhostSettingFailed:
                    return string.Format(Resources.SID_Failed_to_set_AntiGhost_parameter___0_, addition);

                case BwReturnProgressCode.FilesAreUnziped:
                    return Resources.SID_Files_are_unzipped___;
                case BwReturnProgressCode.FilesAreUnzipedSuccessfully:
                    return Resources.SID_Files_are_unzipped_successfully_;
                case BwReturnProgressCode.ErrorFilesUnzipped:
                    return string.Format(Resources.SID_Unzip_files_error___0_, addition);


                case BwReturnProgressCode.ServiceIsBeingInstalled:
                    return string.Format(Resources.SID__0__service_is_being_installed___, addition);
                case BwReturnProgressCode.CannotInstallService:
                    return string.Format(Resources.SID_Cannot_install_service__0_, addition);
                case BwReturnProgressCode.ServiceInstalledSuccessfully:
                    return string.Format(Resources.SID__0__service_installed_successfully, addition);
                case BwReturnProgressCode.ServiceIsBeingUninstalled:
                    return string.Format(Resources.SID__0__service_is_being_uninstalled___, addition);
                case BwReturnProgressCode.CannotUninstallService:
                    return string.Format(Resources.SID_Cannot_uninstall_service__0_, addition);
                case BwReturnProgressCode.ServiceUninstalledSuccessfully:
                    return string.Format(Resources.SID_Service__0__uninstalled_successfully_, addition);
                case BwReturnProgressCode.ServiceIsBeingStopped:
                    return string.Format(Resources.SID__0__service_is_being_stopped___, addition);
                case BwReturnProgressCode.CannotStopService:
                    return string.Format(Resources.SID_Cannot_stop_service__0_, addition);
                case BwReturnProgressCode.ServiceStoppedSuccessfully:
                    return string.Format(Resources.SID_Service__0__stopped_successfully_, addition);

                case BwReturnProgressCode.ShortcutsAreCreatedSuccessfully:
                    return Resources.SID_Shortcuts_are_created_successfully_;

                case BwReturnProgressCode.ClientSetupStarted:
                    return Resources.SID_Client_setup_started_;
                case BwReturnProgressCode.ClientSetupCompletedSuccessfully:
                    return Resources.SID_Client_setup_completed_successfully_;
                case BwReturnProgressCode.DataCenterSetupStarted:
                    return Resources.SID_Data_Center_setup_started_;
                case BwReturnProgressCode.DataCenterSetupCompletedSuccessfully:
                    return Resources.SID_Data_Center_setup_completed_successfully_;

                case BwReturnProgressCode.WebComponentsSetupStarted:
                    return Resources.SID_Web_components_setup_started___;
                case BwReturnProgressCode.WebComponentsSetupCompletedSuccessfully:
                    return Resources.SID_Web_components_setup_completed_successfully_;

                case BwReturnProgressCode.RtuManagerSetupStarted:
                    return Resources.SID_RTU_Manager_setup_started_;
                case BwReturnProgressCode.RtuManagerSetupCompletedSuccessfully:
                    return Resources.SID_RTU_Manager_setup_completed_successfully_;
                case BwReturnProgressCode.UninstallSetupStarted:
                    return Resources.SID_Uninstall_setup_started_;
                case BwReturnProgressCode.UninstallSetupCompletedSuccessfully:
                    return Resources.SID_Uninstall_setup_completed_successfully_;

                case BwReturnProgressCode.IisOperationError:
                    return string.Format(Resources.SID_IIS_operation_error___0_, addition);
                case BwReturnProgressCode.SiteInstalledSuccessfully:
                    return string.Format(Resources.SID_Site__0__installed_successfully, addition);
                case BwReturnProgressCode.SiteInstallationError:
                    return string.Format(Resources.SID_Site_installation_error___0_, addition);
                case BwReturnProgressCode.SiteUninstalledSuccessfully:
                    return string.Format(Resources.SID_Site__0__uninstalled_successfully, addition);
                case BwReturnProgressCode.SiteUninstallationError:
                    return string.Format(Resources.SID_Site_uninstallation_error___0_, addition);

                case BwReturnProgressCode.UninstallStarted:
                    return Resources.SID_Uninstall_started___;
                case BwReturnProgressCode.DeletingFiles:
                    return Resources.SID_Deleting_files___;
                case BwReturnProgressCode.CannotDeleteSpecifiedFolder:
                    return Resources.SID_Cannot_delete_specified_folder_;
                case BwReturnProgressCode.FilesAreDeletedSuccessfully:
                    return Resources.SID_Files_are_deleted_successfully_;
                case BwReturnProgressCode.ShortcutsDeleted:
                    return Resources.SID_Shortcuts_deleted_;
                case BwReturnProgressCode.RegistryCleaned:
                    return Resources.SID_Registry_cleaned_;
                case BwReturnProgressCode.UninstallFinished:
                    return Resources.SID_Uninstall_finished_;
            }

            return "";
        }
    }
}
