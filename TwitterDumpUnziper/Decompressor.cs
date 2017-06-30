using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.BZip2;

namespace TwitterDumpUnziper
{
    interface IDecompressor
    {
        void SinglethreadDecompress();
        void MultithreadDecompress(int countOfThreads);
    }

    class Decompressor : IDecompressor
    {
        private Thread[] threads;
        private WaitHandle[] events;
        private FileInfo[] files;
        private object locker;
        private int countOfIterations = 0;

        public Decompressor(FileInfo[] files)
        {
            this.files = files;
            locker = new object();
        }

        // create directorys before decompressing
        private void CreateDirectorys()
        {
            Console.Write("Creating directorys...   ");
            foreach (FileInfo file in files)
            {
                Directory.CreateDirectory(file.FullName.Replace(".bz2", ""));
            }
            Console.WriteLine("Done!");
        }

        private void Decompress(object o)
        {
            Box box = (Box)o;
            string inputPath;
            string outputPath;
            int lenght = files.Length;

            for (int i = 0; i < box.files.Length; i++)
            {
                inputPath = box.files[i].FullName;
                outputPath = box.files[i].FullName.Replace(".bz2", "") + "\\" + box.files[i].Name.Replace(".bz2", "");
                
                using (FileStream inputFileStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream outputFileStream = new FileStream(outputPath, FileMode.Create))
                    {
                        BZip2.Decompress(inputFileStream, outputFileStream, true);
                    }
                }

                lock (locker)
                {
                    countOfIterations++;
                    Console.SetCursorPosition(0, Console.CursorTop - 2);
                    Console.WriteLine(box.files[i].FullName);
                    Console.WriteLine("{0} / {1} decompressed...", countOfIterations, lenght);
                }
            }
            if (box.auto != null)
                box.auto.Set();
        }

        public void SinglethreadDecompress()
        {
            DateTime dt = DateTime.Now;

            CreateDirectorys();
            Box box = new Box() {
                auto = null,
                files = this.files
            };
                
            Console.WriteLine("Single-trhead decompressing...");
            Console.WriteLine();
            Console.WriteLine();
            Decompress(box);
            Console.WriteLine("Decompressed in {0:0.00} seconds", (DateTime.Now - dt).TotalSeconds);
        }

        public void MultithreadDecompress(int countOfThreads)
        {
            threads = new Thread[countOfThreads];
            events = new WaitHandle[countOfThreads];
            FileInfo[][] parts = new FileInfo[countOfThreads][];
            Box[] boxes = new Box[countOfThreads];

            CreateDirectorys();

            // create arrays of files for each thread
            Console.Write("Split array of files...   ");
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = new FileInfo[0];
            }
            
            int j = 0;
            for (int i = 0; i < files.Length; i++)
            {
                Array.Resize(ref parts[j], parts[j].Length + 1);
                parts[j][parts[j].Length - 1] = files[i];
                j = j == countOfThreads - 1 ? 0 : j + 1;
            }
            Console.WriteLine("Done!");
            Console.WriteLine("Multithread decompressing ({0} threads)...", countOfThreads);
            Console.WriteLine();
            Console.WriteLine();
            DateTime dt = DateTime.Now;
            for (int i = 0; i < countOfThreads; i++)
            {
                events[i] = new AutoResetEvent(false);
                boxes[i] = new Box() {
                    auto = (AutoResetEvent)events[i],
                    files = parts[i]
                };
                threads[i] = new Thread(Decompress);
                threads[i].Start(boxes[i]);
            }

            WaitHandle.WaitAll(events);
            Console.WriteLine("Decompressed in {0:0.00} seconds", (DateTime.Now - dt).TotalSeconds);
        }
    }
}