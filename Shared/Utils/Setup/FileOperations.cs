using System.ComponentModel;

namespace Fibertest.Utils.Setup;

public static class FileOperations
{
    public static string GetParentFolder(string path, int depth = 1)
    {
        for (int i = 0; i < depth; i++)
        {
            var index = path.Substring(0, path.Length - 1).LastIndexOf(@"\", StringComparison.CurrentCulture);
            if (index == -1) return string.Empty;
            path = path.Substring(0, index);
        }
        return path;
    }

    public static bool DirectoryCopyWithDecorations(string sourceDirName, string destDirName,
        BackgroundWorker worker)
    {
        var currentDomain = AppDomain.CurrentDomain.BaseDirectory;
        var fullSourcePath = Path.Combine(currentDomain, sourceDirName);
        try
        {
            return DirectoryCopyRecursively(fullSourcePath, destDirName, worker);
        }
        catch (Exception e)
        {
            worker.ReportProgress((int)BwReturnProgressCode.CopyFilesError, e.Message);
            return false;
        }
    }

    private static bool DirectoryCopyRecursively(string sourceDirName, string destDirName, BackgroundWorker worker)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
        if (!dir.Exists)
        {
            worker.ReportProgress((int)BwReturnProgressCode.ErrorSourceFolderNotFound, sourceDirName);
            return false;
        }

        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
            Directory.CreateDirectory(destDirName);

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            var ss = file.CopyTo(temppath, true);
            worker.ReportProgress(1, ss.Name);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        foreach (DirectoryInfo subdir in dirs)
        {
            string temppath = Path.Combine(destDirName, subdir.Name);
            DirectoryCopyRecursively(subdir.FullName, temppath, worker);
        }

        return true;
    }

    public static bool DirectoryRemove(string dirName, BackgroundWorker worker)
    {
        try
        {

            DirectoryInfo dir = new DirectoryInfo(dirName);
            if (dir.Exists)
                dir.Delete(true);
        }
        catch (Exception e)
        {
            worker.ReportProgress((int)BwReturnProgressCode.ErrorSourceFolderNotFound, e.Message);
            return false;
        }
        return true;

    }

    private static readonly List<string> Models = new List<string>() { "673", "680", "810" };
    public static bool CleanAntiGhost(string fullRtuManagerPath, bool isThereEtcDefaultFolder, BackgroundWorker worker)
    {
        foreach (var model in Models)
        {
            var filename = Path.Combine(fullRtuManagerPath, $@"Etc\param{model}.ini");
            if (!CleanAntiGhostInOneFile(filename, worker)) 
                return false;

            if (!isThereEtcDefaultFolder) 
                continue;

            var filename2 = Path.Combine(fullRtuManagerPath, $@"Etc_default\param{model}.ini");
            if (!CleanAntiGhostInOneFile(filename2, worker)) 
                return false;
        }

        return true;
    }

    private static bool CleanAntiGhostInOneFile(string filename, BackgroundWorker worker)
    {
        try
        {
            var content = File.ReadAllLines(filename);
            var newContent = content.Select(line => line == "aGost=1" ? "aGost=0" : line).ToList();
            File.WriteAllLines(filename, newContent);
            return true;
        }
        catch (Exception e)
        {
            worker.ReportProgress((int)BwReturnProgressCode.AntiGhostSettingFailed, e.Message);
            return false;
        }
    }

}