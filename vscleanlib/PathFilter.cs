using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vscleanlib
{
    public class PathFilter
    {
        public bool Negative
        {
            get; private set;
        }

        public bool DirectoryOnly
        {
            get;
            private set;
        }

        public Regex Pattern
        {
            get;
            private set;
        }

        public PathFilter(string glob)
        {
            // Remove white space from each end of the line. This
            // also strips trailing \r or \n characters.

            glob = glob.Trim();

            // Lines beginning with '#' are comments. Should not
            // get to construct a path filter with one of these

            if (string.IsNullOrEmpty(glob) || glob.StartsWith("#"))
                throw new ArgumentException
                    ("Attempt to make an empty path filter");

            // Lines beginning with '!' are includes rather
            // than excludes. This fact is captured from
            // the IsNegative method, so we just strip these
            // out here.

            if (glob.StartsWith("!"))
            {
                Negative = true;
                glob = glob.Substring(1);
            }
            else
                Negative = false;

            // If there are any directory separators in the
            // path, then a single leading '*' is relative
            // to the current folder only. Otherwise the path
            // can match a file in any sub directory, so prepend
            // the pattern with "**/" to indicate any number
            // of subdirectories.

            if (!glob.Contains("/"))
                glob = "**/" + glob;

            // A single leading directory separator is now
            // redundant, so remove it.

            if (glob.StartsWith("/"))
                glob = glob.Substring(1);

            // A trailing directory separator is redundant
            // assuming we have previously invoked the
            // DirectoryOnly method.

            if (glob.EndsWith("/"))
            {
                DirectoryOnly = true;
                glob = glob.Substring(0, glob.Length - 1);
            }
            else
                DirectoryOnly = false;

            // A pattern of the form abc/** should match
            // any trailing files or folders, but should
            // not cause the elimination of folder abc.

            if (glob.EndsWith("/**"))
                glob = glob + "/*";

            // Now convert this to a regular expression

            Pattern = new Regex(ToRegularExpression(glob));
        }

        static Regex inCharSet = new Regex(@"^\[([^!\]][^\]]+)\]");
        static Regex notInCharSet = new Regex(@"^\[!([^\]]+)\]");

        string ToRegularExpression(string glob)
        {
            StringBuilder sb = new StringBuilder("^");
            Match m;
            for (int i = 0; i < glob.Length; i++)
            {
                if (glob.IndexOf("**/", i) == i)
                {
                    sb.Append(@"(.+[\\/])?");
                    i += 2;
                }
                else if (glob[i] == '*')
                    sb.Append(@"[^\\/]*");
                else if (glob[i] == '?')
                    sb.Append(@"[^\\/]");
                else if (glob[i] == '/')
                    sb.Append(@"[\\/]");
                else if ((m = Regex.Match(glob.Substring(i), @"^\[!([^\]]+)\]")).Success)
                {
                    sb.Append("[^" + m.Groups[1].Value + "]");
                    i += m.Groups[0].Length - 1;
                }
                else if ((m = Regex.Match(glob.Substring(i), @"^\[([^!\]][^\]]+)\]")).Success)
                {
                    sb.Append("[" + m.Groups[1].Value + "]");
                    i += m.Groups[0].Length - 1;
                }
                else if ("-[]/{}()+?.\\^$|".Contains(glob[i]))
                    sb.Append("\\" + glob[i]);
                else
                    sb.Append(glob[i]);
            }
            sb.Append("$");
            return sb.ToString();
        }
    }
}
