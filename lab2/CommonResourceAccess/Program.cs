using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonResourceAccess
{
    class Program
    {
        private static int WRITERS_COUNT = 30;
        private static int READERS_COUNT = 20;
        private static int MESSAGES_PER_WRITER_COUNT = 10;

        static void Main(string[] args)
        {
            Test(new LockKit());
            Test(new AutoResetEventKit());
            Test(new SemaphoreKit());
            Test(new SemaphoreSlimKit());
            Test(new InterlockedKit());

            Console.ReadKey();
        }

        private static void Test(Kit kit)
        {
            Console.WriteLine(kit.GetType());
            Buffer buffer = new Buffer();
            List<List<string>> sources = new List<List<string>>();
            List<Thread> writers = new List<Thread>();
            for (int i = 0; i < WRITERS_COUNT; i++)
            {
                string[] array = new string[MESSAGES_PER_WRITER_COUNT];
                for (int j = 0; j < MESSAGES_PER_WRITER_COUNT; j++)
                {
                    array[j] = i + "." + j;
                }
                sources.Add(new List<string>(array));
                Queue<string> source = new Queue<string>(array);
                writers.Add(new Thread(() =>
                    kit.Writer(buffer, source)));
            }
            List<List<string>> results = new List<List<string>>();
            List<Thread> readers = new List<Thread>();
            for (int i = 0; i < READERS_COUNT; i++)
            {
                List<string> result = new List<string>();
                results.Add(result);
                readers.Add(new Thread(() =>
               kit.Reader(buffer, result)));
            }
            writers.ForEach(t => t.Start());
            readers.ForEach(t => t.Start());

            writers.ForEach(t => t.Join());
            buffer.Complete();
            kit.ReleaseReader();
            readers.ForEach(t => t.Join());

            results.SelectMany(s => s).ToList().ForEach(s => Console.Write(s + ", "));
            Console.WriteLine();

            HashSet<string> sourcesSet = new HashSet<string>(
                sources.SelectMany(s => s).ToList());
            HashSet<string> resultsSet = new HashSet<string>(
                results.SelectMany(s => s).ToList());
            Console.WriteLine("Sources and results are equal: {0}",
                IsSetsEqual(sourcesSet, resultsSet));
        }

        private static bool IsSetsEqual(HashSet<string> left, HashSet<string> right)
        {
            return left.Count == right.Count && left.Count == left.Intersect(right).Count();
        }
    }
}
