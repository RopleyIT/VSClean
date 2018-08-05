﻿using System;
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
            if (ProgressNotify != null)
                ProgressNotify(message, fraction);
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

        public async static void SourceBackup(string folderPath, bool excludeVC)
        {
            CopiedFolders = 0;
            SkippedFolders = 0;
            CopiedFiles = 0;
            SkippedFiles = 0;

            if (string.IsNullOrEmpty(folderPath))
            {
                Notify("Null or empty folder path", 0.0);
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Notify(string.Format("Folder {0} does not exist", folderPath), 0.0);
                return;
            }

            string fullSrcPath = Path.GetFullPath(folderPath);
            string zipPath = Path.Combine(fullSrcPath, Path.GetFileNameWithoutExtension(folderPath) + ".zip");
            if (File.Exists(zipPath))
            {
                Notify("Deleting existing ZIP: " + zipPath, 0.005);
                File.Delete(zipPath);
            }

            // Make a copy of the source folder so that the copy can be zipped

            string tmpFolder = GetTempPath();
            await Task.Run(() => RecursiveCopy(fullSrcPath, tmpFolder, 0.01, 0.99, excludeVC));
            Notify("Creating ZIP: " + zipPath, 0.99);
            ZipFile.CreateFromDirectory(tmpFolder, zipPath, CompressionLevel.Optimal, false);
            Notify("Deleting temporary folder", 0.995);
            Directory.Delete(tmpFolder, true);
            Notify("Copied: " + CopiedFiles + " (" + CopiedFolders + " folders), Skipped: "
                + SkippedFiles + " (" + SkippedFolders + " folders)", 0.0);
        }

        public async static void SourceClean(string folderPath)
        {
            CopiedFolders = 0;
            SkippedFolders = 0;
            CopiedFiles = 0;
            SkippedFiles = 0;

            if (string.IsNullOrEmpty(folderPath))
            {
                Notify("Null or empty folder path", 0.0);
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Notify(string.Format("Folder {0} does not exist", folderPath), 0.0);
                return;
            }

            string fullSrcPath = Path.GetFullPath(folderPath);

            // Make a copy of the source folder so that the copy can be zipped

            await Task.Run(() => RecursiveClean(fullSrcPath, 0.01, 0.99));
            Notify("Kept: " + CopiedFiles + " (" + CopiedFolders + " folders), Deleted: "
                + SkippedFiles + " (" + SkippedFolders + " folders)", 0.0);
        }

        private static string GetTempPath()
        {
            string tmpPath = "C:\\_vsctmp";
            if (Directory.Exists(tmpPath))
                Directory.Delete(tmpPath, true);
            Directory.CreateDirectory(tmpPath);
            return tmpPath;
        }

        private static string[] vcFolders = new string[]
        {
            "$tf", ".git"
        };

        private static string[] removableFolders = new string[]
        {
            "bin", "obj", "TestResults", "debug", "debugpublic",
            "release", "releases", "x64", "x86", "build", "bld",
            ".vs", "_upgradereport_files", "backup*", "packages"
        };

        private static string[] extensions = new string[]
        {
            "suo", "user", "userosscache", "sln.docstates",
            "userprefs", "pdb", "vsp", "vspx", "vspscc",
            "vssscc", "vsmdi", "psess", //"mdf", "ldf",
            //"gitattributes", "gitignore"
        };

        private static void RecursiveCopy(string root, string target, double min, double max, bool excludeVC)
        {
            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

            string[] childFolders = Directory.GetDirectories(root);
            double stepSize = (max - min) / (childFolders.Length + 1);
            double stepBase = min;

            IEnumerable<string> filterFolders = removableFolders;
            if (excludeVC)
                filterFolders = filterFolders.Concat(vcFolders);

            foreach (string folder in childFolders)
            {
                if (!filterFolders.Any(f => folder.EndsWith("\\" + f, StringComparison.CurrentCultureIgnoreCase)))
                {
                    CopiedFolders++;
                    RecursiveCopy(folder, Path.Combine(target, Path.GetFileName(folder)), stepBase, stepBase + stepSize, excludeVC);
                }
                else
                    SkippedFolders++;
                stepBase += stepSize;
            }

            Notify("Copying folder: " + root, min);

            string[] files = Directory.GetFiles(root);
            if (files.Length > 0)
                stepSize /= files.Length;
            foreach (string file in files)
            {
                if (!extensions.Any(e => file.EndsWith("." + e, StringComparison.CurrentCultureIgnoreCase)))
                {
                    string targetFile = Path.Combine(target, Path.GetFileName(file));
                    Notify("Copying: " + Path.GetFileName(file), stepBase);
                    File.Copy(file, targetFile);
                    File.SetAttributes(targetFile, FileAttributes.Normal);
                    CopiedFiles++;
                }
                else
                {
                    Notify("Skipping: " + Path.GetFileName(file), stepBase);
                    SkippedFiles++;
                }
                stepBase += stepSize;
            }
        }

        private static void RecursiveClean(string root, double min, double max)
        {
            string[] childFolders = Directory.GetDirectories(root);
            double stepSize = (max - min) / (childFolders.Length + 1);
            double stepBase = min;
            foreach (string folder in childFolders)
            {
                if (!removableFolders.Any(f => folder.EndsWith("\\" + f, StringComparison.CurrentCultureIgnoreCase)))
                {
                    RecursiveClean(folder, stepBase, stepBase + stepSize);
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

            Notify("Cleaning folder: " + root, stepBase);

            string[] files = Directory.GetFiles(root);
            if (files.Length > 0)
                stepSize /= files.Length;
            foreach (string file in files)
            {
                if (!extensions.Any(e => file.EndsWith("." + e, StringComparison.CurrentCultureIgnoreCase)))
                {
                    Notify("Keeping: " + Path.GetFileName(file), stepBase);
                    CopiedFiles++;
                }
                else
                {
                    Notify("Deleting: " + Path.GetFileName(file), stepBase);
                    File.SetAttributes(file, FileAttributes.Normal);
                    SkippedFiles++;
                    File.Delete(file);
                }
                stepBase += stepSize;
            }
        }
    }
}
