using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Licenser;

public static class HtmlCertificateProvider
{
    public static string? CreateHtmlReport(this LicenseInFileModel licenseInFileModel)
    {
        var licenseInFile = licenseInFileModel.ToLicenseInFile();

        var templateFileName = AppDomain.CurrentDomain.BaseDirectory + @"Resources\LicenseCertificateTemplate.html";
        if (!File.Exists(templateFileName))
            return null;
        var content = File.ReadAllText(templateFileName);

        var cssFile = "file:///" + AppDomain.CurrentDomain.BaseDirectory + @"Resources\Styles.css";
        content = content.Replace("Styles.css", cssFile);
        var headerPng = "file:///" + AppDomain.CurrentDomain.BaseDirectory + @"Resources\header.png";
        content = content.Replace("header.png", headerPng);

        var dict = DefineConstants(licenseInFile);
        foreach (var pair in dict)
            content = content.Replace($"@{pair.Key}@", pair.Value);

        content = InsertLicenseParameters(content, licenseInFile);
        if (licenseInFile.IsMachineKeyRequired)
            content = InsertPasswordPage(content, licenseInFileModel);
        return content;
    }

    private static string InsertLicenseParameters(string content, LicenseInFile licenseInFile)
    {
        var index = content.IndexOf("<div class='parameters'>", StringComparison.Ordinal) + 24;
        if (!licenseInFile.IsIncremental)
            content = InsertOneParameter(content, ref index, Resources.SID_License_type,
                licenseInFile.IsMachineKeyRequired
                    ? Resources.SID_With_user_s_account_to_workstation_linking
                    : Resources.SID_Standart);
        content = InsertOneParameter(content, ref index, Resources.SID_License_key_type,
            licenseInFile.IsIncremental ? Resources.SID_Incremental : Resources.SID_Basic);
        content = InsertOneParameter(content, ref index, Resources.SID_License_owner, licenseInFile.Owner);
        if (licenseInFile.RtuCount.Value != 0)
            content = InsertOneParameter(content, ref index, Resources.SID_Remote_testing_unit_count, licenseInFile.RtuCount.ToString());
        if (licenseInFile.ClientStationCount.Value != 0)
            content = InsertOneParameter(content, ref index, Resources.SID_Client_stations, licenseInFile.ClientStationCount.ToString());
        if (licenseInFile.WebClientCount.Value != 0)
            content = InsertOneParameter(content, ref index, Resources.SID_Web_clients, licenseInFile.WebClientCount.ToString());
        if (licenseInFile.SuperClientStationCount.Value != 0)
            content = InsertOneParameter(content, ref index, Resources.SID_SuperClients, licenseInFile.SuperClientStationCount.ToString());

        content = InsertOneParameter(content, ref index, Resources.SID_Creation_date, licenseInFile.CreationDate.ToString("d"));

        return content;
    }

    private static string InsertOneParameter(string content, ref int index, string a, string b)
    {
        var line = $"<p>{a}: {b}</p>";
        content = content.Insert(index, line);
        index += line.Length;
        return content;
    }

    private static string InsertPasswordPage(string content, LicenseInFileModel licenseInFileModel)
    {
        var index = content.IndexOf("<div class='pwdPage'>", StringComparison.Ordinal) + 21;


        var backPng = "file:///" + AppDomain.CurrentDomain.BaseDirectory + @"Resources\PasswordBackground.png";
        var line = $@"<img src='{backPng}'/>";
        content = content.Insert(index, line);
        index += line.Length;

        content = content.Insert(index, $"<p class='pwd'>{licenseInFileModel.SecurityAdminPassword}</p>");
        return content;
    }

    private static Dictionary<string, string> DefineConstants(LicenseInFile licenseInFile)
    {
        var result = new Dictionary<string, string>();
        result.Add("IitTitle", Resources.SID_JS_Institute_of_Information_Technologies);
        result.Add("IitAddress", Resources.SID_Iit_address);
        result.Add("OFMSS", Resources.SID_Optical_fiber_monitoring_system_software);

        result.Add("LicenseNumber", Resources.SID_License_number_);
        result.Add("LicenseKey", licenseInFile.Lk());

        /*
        if (!licenseInFile.IsIncremental)
        {
            result.Add("LicenseType", Resources.SID_License_type);
            result.Add("LicenseTypeValue", licenseInFile.IsMachineKeyRequired
                ? Resources.SID_With_user_s_account_to_workstation_linking : Resources.SID_Standart);
        }
        result.Add("LicenseKeyType", Resources.SID_License_key_type);
        result.Add("LicenseKeyTypeValue", licenseInFile.IsIncremental ? Resources.SID_Incremental : Resources.SID_Basic);

        result.Add("LicenseOwner", Resources.SID_License_owner);
        result.Add("LicenseOwnerValue", licenseInFile.Owner);

        result.Add("RTUCount", Resources.SID_Remote_testing_unit_count);
        result.Add("RTUCountValue", licenseInFile.RtuCount.ToString());

        result.Add("ClientStationCount", Resources.SID_Client_stations);
        result.Add("ClientStationCountValue", licenseInFile.ClientStationCount.ToString());

        result.Add("WebClientCount", Resources.SID_Web_clients);
        result.Add("WebClientCountValue", licenseInFile.WebClientCount.ToString());

        result.Add("SuperClientCount", Resources.SID_SuperClients);
        result.Add("SuperClientCountValue", licenseInFile.SuperClientStationCount.ToString());

        result.Add("CreationDate", Resources.SID_Creation_date);
        result.Add("CreationDateValue", licenseInFile.CreationDate.ToString("d"));
        */
         
        var bytes = Cryptography.Encode(licenseInFile);
        var code = ByteArrayToString(bytes!);
        result.Add("DigitalKey", code);

        result.Add("Signature", "     Директор     __________________________     Слесарчик М.В.");

        return result;
    }

    private static string ByteArrayToString(byte[] ba)
    {
        var hex = new StringBuilder(ba.Length * 3);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2} ", b);
        return hex.ToString().ToUpper();
    }

}