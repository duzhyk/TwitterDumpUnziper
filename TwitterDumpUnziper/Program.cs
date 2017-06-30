using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.BZip2;
using System.IO;
using System.Threading;

namespace TwitterDumpUnziper
{
    class Program
    {
        private static string mainDirectoryPath;
        private static bool flag = true;
        private static int countOfThreads = -1;
        private static string[] filesPaths;
        private static FileInfo[] fi;

        private static Decompressor decompressor;

        static void Main(string[] args)
        {
            while (flag)
            {
                Console.WriteLine("Enter Twitter stream folder path (like this - C:\\twitter-stream-2011-09-27):");
                mainDirectoryPath = Console.ReadLine();
                try
                {
                    filesPaths = Directory.GetFiles(mainDirectoryPath, "*.bz2", SearchOption.AllDirectories);
                    if (filesPaths.Length != 0)
                    {
                        Console.WriteLine("{0} files found.", filesPaths.Length);
                        flag = false;
                    }
                    else
                    {
                        Console.WriteLine("Files not found. Try one more time.");
                    }
                }
                catch
                {
                    Console.WriteLine("Folder not found! Try one more time.");
                }
            }
            while (countOfThreads <= 0 || countOfThreads > 100)
            {
                Console.WriteLine("Enter count of threads (1-100):");
                try
                {
                    countOfThreads = Convert.ToInt32(Console.ReadLine());
                    if (countOfThreads <= 0 || countOfThreads > 100)
                        Console.WriteLine("Try one more time.");
                }
                catch
                {
                    Console.WriteLine("Try one more time.");
                }
            }
            
            fi = new FileInfo[filesPaths.Length];
            for (int i = 0; i < filesPaths.Length; i++)
            {
                fi[i] = new FileInfo(filesPaths[i]);
            }
 
            decompressor = new Decompressor(fi);
            if (countOfThreads == 1)
                decompressor.SinglethreadDecompress();
            else
                decompressor.MultithreadDecompress(countOfThreads);
            Console.ReadKey();
        }
    }
}
