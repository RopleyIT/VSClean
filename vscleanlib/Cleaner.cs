using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace vscleanlib
{
    public static class Cleaner
    {
        public static Action<string, double> ProgressNotify
        {
            get;
            set;
        }

        private static void Notify(string message, double fraction)
        {
            ProgressNotify?.Invoke(message, fraction);
        }

        public static long CopiedFolders
        {
            get;
            private set;
        }

        public static long SkippedFolders
        {
            get;
            private set;
        }

        public static int CopiedFiles
        {
            get;
            private set;
        }

        public static int SkippedFiles
        {
            get;
            private set;
        }

        private static string prepareSources(string folderPath)
        {
            CopiedFolders = 0;
            SkippedFolders = 0;
            CopiedFiles = 0;
            SkippedFiles = 0;

            if (string.IsNullOrEmpty(folderPath))
            {
                Notify("Null or empty folder path", 0.0);
                return null;
            }

            if (!Directory.Exists(folderPath))
            {
                Notify(string.Format("Folder {0} does not exist", folderPath), 0.0);
                return null;
            }

            string fullSrcPath = Path.GetFullPath(folderPath);

            // Load up the alternative filter script

            string scriptPath = Path.Combine(fullSrcPath, ".vsclean");
            if (File.Exists(scriptPath))
            {
                using (StreamReader scriptReader = new StreamReader(scriptPath))
                    gitIgnoreScript = scriptReader.ReadToEnd();
                Notify("Using filter script from " + scriptPath, 0.006);
            }
            return fullSrcPath;
        }

        public static async Task SourceBackup(string folderPath, bool excludeVC, string zipPath = null)
        {
            string fullSrcPath = prepareSources(folderPath);
            if(string.IsNullOrEmpty(zipPath))
                zipPath = Path.Combine(fullSrcPath, Path.GetFileNameWithoutExtension(folderPath) + ".zip");
            if (File.Exists(zipPath))
            {
                Notify("Deleting existing ZIP: " + zipPath, 0.005);
                File.Delete(zipPath);
            }

            // Make a copy of the source folder so that the copy can be zipped

            string tmpFolder = GetTempPath();
            pathFilterParser = new PathFilterParser
                ((excludeVC ? "/$tf/\r\n/.git/\r\n" : "") + gitIgnoreScript);
            pathFilterParser.RootFolder = fullSrcPath;
            await RecursiveCopyAsync(fullSrcPath, fullSrcPath, tmpFolder, 0.01, 0.99);
            Notify("Creating ZIP: " + zipPath, 0.99);
            ZipFile.CreateFromDirectory(tmpFolder, zipPath, CompressionLevel.Optimal, false);
            Notify("Deleting temporary folder", 0.995);
            Directory.Delete(tmpFolder, true);
            Notify("Copied: " + CopiedFiles + " (" + CopiedFolders + " folders), Skipped: "
                + SkippedFiles + " (" + SkippedFolders + " folders)", 1.0);
        }

        public static async Task SourceClean(string folderPath)
        {
            string fullSrcPath = prepareSources(folderPath);
            pathFilterParser = new PathFilterParser(gitIgnoreScript);
            pathFilterParser.RootFolder = fullSrcPath;
            await RecursiveCleanAsync(fullSrcPath, fullSrcPath, 0.01, 0.99);
            Notify("Kept: " + CopiedFiles + " (" + CopiedFolders + " folders), Deleted: "
                + SkippedFiles + " (" + SkippedFolders + " folders)", 1.0);
        }

        private static PathFilterParser pathFilterParser;

        private static string GetTempPath()
        {
            string tmpPath = "C:\\_vsctmp";
            if (Directory.Exists(tmpPath))
                Directory.Delete(tmpPath, true);
            Directory.CreateDirectory(tmpPath);
            return tmpPath;
        }

        private static string gitIgnoreScript =
        @"
            **/bin/
            **/obj/
            **/TestResults/
            **/debug/
            **/debugpublic/
            **/release/
            **/releases/
            **/x64/
            **/x86/
            **/build/
            **/bld/
            **/.vs/
            **/_upgradereport_files/
            **/backup*/
            **/packages/
            **/node_modules/
            *.suo
            *.user
            *.userosscache
            *.sln.docstates
            *.userprefs
            *.pdb
            *.vsp
            *.vspx
            *.vspscc
            *.vssscc
            *.vsmdi
            *.psess
        ";

        private static async Task RecursiveCopyAsync(string originalRoot, string root, string target, double min, double max)
        {
            await Task.Run(() => RecursiveCopy(originalRoot, root, target, min, max));
        }

        private static void RecursiveCopy(string originalRoot, string root, string target, double min, double max)
        {
            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

            string[] childFolders = Directory.GetDirectories(root);
            double stepSize = (max - min) / (childFolders.Length + 1);
            double stepBase = min;

            foreach (string folder in childFolders)
            {
                if (!pathFilterParser.DeniesDirectory(folder))
                {
                    CopiedFolders++;
                    RecursiveCopy(originalRoot, folder, Path.Combine(target, Path.GetFileName(folder)), stepBase, stepBase + stepSize);
                }
                else
                    SkippedFolders++;
                stepBase += stepSize;
            }

            Notify("In folder: " + root, min);

            string[] files = Directory.GetFiles(root);
            if (files.Length > 0)
                stepSize /= files.Length;
            foreach (string file in files)
            {
                if (pathFilterParser.Accepts(file, false))
                {
                    string targetFile = Path.Combine(target, Path.GetFileName(file));
                    Notify("    Copying: " + Path.GetFileName(file), stepBase);
                    File.Copy(file, targetFile);
                    File.SetAttributes(targetFile, FileAttributes.Normal);
                    CopiedFiles++;
                }
                else
                {
                    Notify("    Skipping: " + Path.GetFileName(file), stepBase);
                    SkippedFiles++;
                }
                stepBase += stepSize;
            }
        }

        private static async Task RecursiveCleanAsync(string originalRoot, string root, double min, double max)
        {
            await Task.Run(() => RecursiveClean(originalRoot, root, min, max));
        }

        private static void RecursiveClean(string originalRoot, string root, double min, double max)
        {
            if (!root.StartsWith(originalRoot))
                throw new ArgumentException("Root not under originalRoot");

            string[] childFolders = Directory.GetDirectories(root);
            double stepSize = (max - min) / (childFolders.Length + 1);
            double stepBase = min;
            foreach (string folder in childFolders)
            {
                if(!pathFilterParser.DeniesDirectory(folder))
                {
                    RecursiveClean(originalRoot, folder, stepBase, stepBase + stepSize);
                    CopiedFolders++;
                }
                else
                {
                    Notify("Deleting folder: " + folder, stepBase);
                    Directory.Delete(folder, true);
                    SkippedFolders++;
                }
                stepBase += stepSize;
            }

            Notify("In folder: " + root, stepBase);

            string[] files = Directory.GetFiles(root);
            if (files.Length > 0)
                stepSize /= files.Length;
            foreach (string file in files)
            {
                if(pathFilterParser.Accepts(file, false))
                {
                    Notify("    Keeping: " + Path.GetFileName(file), stepBase);
                    CopiedFiles++;
                }
                else
                {
                    Notify("    Deleting: " + Path.GetFileName(file), stepBase);
                    File.SetAttributes(file, FileAttributes.Normal);
                    SkippedFiles++;
                    File.Delete(file);
                }
                stepBase += stepSize;
            }
        }
    }
}
