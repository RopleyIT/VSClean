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
            string usage =
                "Usage: vsclean rootFolderName [-z zipFilePath [-v]] [-d]\r\n" +
                "        -v  Include version control folders in ZIP\r\n" +
                "        -z  Output ZIP file path\r\n" +
                "        -d  Detailed output, otherwise silent\r\n" +
                "Either cleans unnecessary files from folder, or\r\n" +
                "zips all necessary files to a ZIP file if -z present.";
            try
            {
                if (args.Length < 1 || args.Length > 5)
                    throw new ArgumentException("Invalid number of arguments");

                string rootFolder = Path.GetFullPath(args[0]);
                if (!Directory.Exists(rootFolder))
                    throw new ArgumentException("Invalid folder name");
                if(args[args.Length - 1] == "-d")
                    Cleaner.ProgressNotify = Notifier;
                if (args.Length == 1)
                    Cleaner.SourceClean(rootFolder).GetAwaiter().GetResult();
                else if (args.Length >= 3 && args[1] == "-z")
                {
                    if (args.Length == 4 && args[3] == "-v")
                        Cleaner.SourceBackup(rootFolder, false, args[2]).GetAwaiter().GetResult();
                    else
                        Cleaner.SourceBackup(rootFolder, true, args[2]).GetAwaiter().GetResult();
                }
                else
                    throw new ArgumentException("Invalid arguments");
            }
            catch(Exception x)
            {
                Console.WriteLine("*** " + x.Message);
                Console.WriteLine(usage);
            }
        }

        public static void Notifier(string msg, double progress)
        {
            Console.WriteLine("{0:N2}%:  {1}", progress * 100, msg);
        }
    }
}
