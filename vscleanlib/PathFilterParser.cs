using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace vscleanlib
{
    /// <summary>
    /// Create a parser from a file that uses the gitignore
    /// syntax. The parser can be used to decide whether
    /// files/folder paths are to be included or excluded
    /// from a file backup, folder cleanup, or filtered copy.
    /// </summary>

    public class PathFilterParser
    {
        List<PathFilter> filters = new List<PathFilter>();

        /// <summary>
        /// Create a gitignore parser from a set of
        /// glob strings in gitignore file format
        /// </summary>
        /// <param name="globs">The multiline string
        /// containing the filter expressions</param>

        public PathFilterParser(string globs)
            : this(new StringReader(globs))
        {}

        /// <summary>
        /// Create a gitignore parser from a set of
        /// glob strings in gitignore file format
        /// </summary>
        /// <param name="reader">The open text
        /// stream to the file containing the
        /// filter expressions</param>

        public PathFilterParser(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Ignore blank lines or comments

                line = line.Trim();
                if (string.IsNullOrEmpty(line) || line[0] == '#')
                    continue;

                filters.Add(new PathFilter(line));
            }

            RootFolder = string.Empty;
        }

        /// <summary>
        /// The root folder to which all paths in
        /// this parser are expressed relatively
        /// </summary>

        public string RootFolder { get; set; }
        
        /// <summary>
        /// For a give file path, determine if it is still
        /// included despite the gitignore expressions
        /// </summary>
        /// <param name="path">The file path, relative to
        /// the folder being searched by the application</param>
        /// <returns>True if the path is not to be excluded</returns>

        public bool Accepts(string path, bool directory)
        {
            path = removeRootFolderFromPath(path);
            if (path.StartsWith("/") || path.StartsWith("\\"))
                path = path.Substring(1);
            bool accepted = true;
            foreach(var f in filters)
            {
                if ((directory || !f.DirectoryOnly) && f.Pattern.IsMatch(path))
                    accepted = f.Negative;
            }
            return accepted;
        }

        /// <summary>
        /// For a give file path, determine if it is not
        /// included because of the gitignore expressions
        /// </summary>
        /// <param name="path">The file path</param>
        /// <returns>True if the path is to be excluded</returns>

        public bool Denies(string path, bool directory)
        {
            return !Accepts(path, directory);
        }

        /// <summary>
        /// Return true if there exists an exact pattern match that
        /// explicitly denies this path if it is a folder not a file.
        /// The pattern must end with a '/' for this to be true. This
        /// is used when we want to make the PathFilterParser behave
        /// like Git's own gitignore logic, where excluded folders
        /// cannot reinclude child files or folders.
        /// </summary>
        /// <param name="path">The path being inspected</param>
        /// <returns>True if the path is a denied folder</returns>

        public bool DeniesDirectory(string path)
        {
            path = removeRootFolderFromPath(path);
            if (path.StartsWith("/") || path.StartsWith("\\"))
                path = path.Substring(1);
            return filters.Any(f => 
                !f.Negative && 
                f.DirectoryOnly && 
                f.Pattern.IsMatch(path));
        }

        private string removeRootFolderFromPath(string path)
        {
            // Remove that part of the path which lies above
            // the root folder. If RootFolder is set to empty,
            // then we assume 'path' is relative.

            if (!string.IsNullOrEmpty(RootFolder))
            {
                if (!path.StartsWith(RootFolder, StringComparison.CurrentCultureIgnoreCase))
                    throw new ArgumentException("Path outside of root folder");
                path = path.Substring(RootFolder.Length);
            }
            return path;
        }
    }
}
