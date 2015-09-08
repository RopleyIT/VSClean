using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using vscleanlib;

namespace vsclean
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Usage: vsclean [-v] rootFolderName\r\n\t-x\tInclude version control folders");
                return;
            }

            if (string.Compare(args[0], "-v", true) == 0 && args.Length > 1)
                Cleaner.SourceBackup(args[1], false);
            else
                Cleaner.SourceBackup(args[0], true);
        }
    }
}
