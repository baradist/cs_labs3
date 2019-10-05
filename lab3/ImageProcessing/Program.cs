using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageProcessing
{
    class Program
    {
        enum Mode
        {
            DEFAULT,
            DEFAULT_PARALLEL,
            BALANCED,
            STATIC,
            STATIC_FIXED_BLOCK
        }
        private static bool Verbose = true;

        static void Main(string[] args)
        {
            string path = @"..\..\images\";
            Directory.CreateDirectory(path + @"\out");

            foreach (Mode item in Enum.GetValues(typeof(Mode)))
            {
                ProcessImages(path, 10, item);
                ProcessImages(path, 25, item);
                ProcessImages(path, 50, item);
            }
            Console.WriteLine("Press any key");
            Console.ReadKey();
        }

        private static void ProcessImages(string path, int count, Mode mode)
        {
            int processedCount = 0;
            var ImageFiles = Directory.GetFiles(path)
                                .Select(filename => new FileInfo(filename))
                                .Take(count)
                                .ToArray();
            count = ImageFiles.Count();
            List<TaskDetails> bag = new List<TaskDetails>();

            CancellationTokenSource cts = new CancellationTokenSource();
            Thread thr = new Thread(() =>
            {
                while (count < processedCount && !cts.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                        cts.Cancel();
                        break;
                    }
                }
            });
            Stopwatch sw = Stopwatch.StartNew();
            double ms1 = sw.Elapsed.TotalSeconds;
            thr.Start();

            try
            {
                ParallelOptions Options = new ParallelOptions()
                {
                    CancellationToken = cts.Token,
                };
                switch (mode)
                {
                    case Mode.DEFAULT:
                        Options.MaxDegreeOfParallelism = 1;
                        Parallel.ForEach(ImageFiles, Options, (fi) => ImageWork.Processing(fi, bag, processedCount));
                        break;
                    case Mode.DEFAULT_PARALLEL:
                        Options.MaxDegreeOfParallelism = 4;
                        Parallel.ForEach(ImageFiles, Options, (fi) => ImageWork.Processing(fi, bag, processedCount));
                        break;
                    case Mode.BALANCED:
                        Parallel.ForEach(Partitioner.Create(ImageFiles, true), Options,
                            (fi) => ImageWork.Processing(fi, bag, processedCount));
                        break;
                    case Mode.STATIC:
                        Parallel.ForEach(Partitioner.Create(0, count), Options,
                            range => {
                                for (int i = range.Item1; i < range.Item2; i++)
                                {
                                    ImageWork.Processing(ImageFiles[i], bag, processedCount);
                                }
                            });
                        break;
                    case Mode.STATIC_FIXED_BLOCK:
                        Parallel.ForEach(Partitioner.Create(0, count, count / 4), Options,
                            range => {
                                for (int i = range.Item1; i < range.Item2; i++)
                                {
                                    ImageWork.Processing(ImageFiles[i], bag, processedCount);
                                }
                            });
                        break;
                }

            }
            catch (OperationCanceledException o)
            {
                //..
            }
            Console.WriteLine("Mode: {0}, count: {1}", mode, count);
            Console.WriteLine("Main Thread Id: {0}", Thread.CurrentThread.ManagedThreadId);

            if (Verbose)
            {
                Console.WriteLine("thr\ttask\tidx\tms1\tms2");
                for (int i = 0; i < bag.Count; i++)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}",
                        bag[i].thr, bag[i].task, bag[i].idx, bag[i].ms1, bag[i].ms2);
                }
            }
            double ms2 = sw.Elapsed.TotalSeconds;
            Console.WriteLine("Time spent: {0}", ms2 - ms1);
        }
    }
}
