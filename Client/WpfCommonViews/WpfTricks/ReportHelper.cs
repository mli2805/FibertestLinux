using System;
using System.IO;
using Fibertest.Utils.Setup;
using iText.Html2pdf;

namespace Fibertest.WpfCommonViews;

public static class ReportHelper
{
    public static string SaveHtml(this string htmlString, string filename,
        bool inTempFolder = true, bool withTime = true)
    {
        var temp = inTempFolder ? FileOperations.GetMainFolder() + @"\temp\" : "";
        var timestamp = withTime ? $@"-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}" : "";
        var htmlFileName = $@"{temp}{filename}{timestamp}.html";

        File.WriteAllText(htmlFileName, htmlString);
        return htmlFileName;
    }

    public static string SaveHtmlAsPdf(this string htmlString, string filename, 
        bool inTempFolder = true, bool withTime = true)
    {
        var temp = inTempFolder ? FileOperations.GetMainFolder() + @"\temp\" : "";
        var timestamp = withTime ? $@"-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}" : "";
        var pdfFileName = $@"{temp}{filename}{timestamp}.pdf";

        using var stream = new MemoryStream();
        HtmlConverter.ConvertToPdf(htmlString, stream, new ConverterProperties());
        File.WriteAllBytes(pdfFileName, stream.ToArray());
        return pdfFileName;
    }
}