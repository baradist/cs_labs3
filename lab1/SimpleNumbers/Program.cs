using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleNumbers
{
    class Program
    {
        public enum Mode
        {
            SERIAL,
            PARALLEL_SPLIT_BY_NUMBERS,
            PARALLEL_SPLIT_BY_PRIMES,
            THREAD_POOL,
            USING_PARALLEL_FOR,
            WORK_POOLING,
            WORK_POOLING_WITH_LOCK
        }
        private static bool isDebug;

        private int n;
        private int[] basePrimes;
        private bool[] marks;
        private int sqrN;
        private int threadsCount;
        private Mode mode;

        public Program(Mode mode, int n) : this(mode, n, 1) { }

        public Program(Mode mode, int n, int threadCount)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if ((mode == Mode.PARALLEL_SPLIT_BY_NUMBERS || mode == Mode.PARALLEL_SPLIT_BY_PRIMES)
                && threadCount < 1)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.n = n;
            this.threadsCount = threadCount;
            marks = new bool[n];
            this.mode = mode;
        }

        static void Main(string[] args)
        {
            int N = 300_000_000;
            Program.isDebug = false;
            Program s = new Program(Mode.SERIAL, N);
            s.Process();

            Program p1 = new Program(Mode.PARALLEL_SPLIT_BY_PRIMES, N, 4);
            p1.Process();

            Program p2 = new Program(Mode.PARALLEL_SPLIT_BY_NUMBERS, N, 4);
            p2.Process();

            Program p3 = new Program(Mode.THREAD_POOL, N);
            p3.Process();

            Program pfe = new Program(Mode.USING_PARALLEL_FOR, N);
            pfe.Process();

            Program wp = new Program(Mode.WORK_POOLING, N, 10);
            wp.Process();

            Program wpl = new Program(Mode.WORK_POOLING_WITH_LOCK, N, 10);
            wpl.Process();

            Console.WriteLine(IsListsEqual(s.GetNumbers(), p1.GetNumbers()));
            Console.WriteLine(IsListsEqual(s.GetNumbers(), p2.GetNumbers()));
            Console.WriteLine(IsListsEqual(s.GetNumbers(), p3.GetNumbers()));
            Console.WriteLine(IsListsEqual(s.GetNumbers(), wp.GetNumbers()));
            Console.WriteLine(IsListsEqual(s.GetNumbers(), wpl.GetNumbers()));

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static bool IsListsEqual(List<int> l1, List<int> l2)
        {
            if(l1.Count != l2.Count)
            {
                return false;
            }
            for (int i = 0; i < l1.Count; i++)
            {
                if (l1[i] != l2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private List<int> GetNumbers()
        {
            List<int> result = new List<int>();
            for (int i = 1; i < marks.Length; i++)
            {
                if (!marks[i])
                {
                    result.Add(i);
                }
            }
            return result;
        }

        private void Process()
        {
            Console.WriteLine(mode);

            Stopwatch sw = Stopwatch.StartNew();

            FindPrimes();

            sw.Stop();
            Console.WriteLine("Base stage, milliseconds: " + sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();

            switch (mode)
            {
                case Mode.SERIAL:
                    Serial();
                    break;
                case Mode.PARALLEL_SPLIT_BY_PRIMES:
                    SplitByPrimes();
                    break;
                case Mode.PARALLEL_SPLIT_BY_NUMBERS:
                    SplitByNumbers();
                    break;
                case Mode.THREAD_POOL:
                    UsingThreadPool();
                    break;
                case Mode.USING_PARALLEL_FOR:
                    UsingParallelFor();
                    break;
                case Mode.WORK_POOLING:
                    WorkPooling();
                    break;
                case Mode.WORK_POOLING_WITH_LOCK:
                    WorkPoolingWithLock();
                    break;
            }
            sw.Stop();
            Console.WriteLine("Main stage, milliseconds: " + sw.ElapsedMilliseconds);
        }

        private void Serial()
        {
            MarkRange(0, basePrimes.Length, sqrN, n);
        }

        private void SplitByPrimes()
        {
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < threadsCount; i++)
            {
                int from = (i * basePrimes.Length / threadsCount);
                int to = (i + 1) * basePrimes.Length / threadsCount;
                threads.Add(new Thread(() => MarkRange(from, to, sqrN, n)));
            }
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
        }

        private void SplitByNumbers()
        {
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < threadsCount; i++)
            {
                int from = (i * (n - sqrN) / threadsCount) + sqrN;
                int to = ((i + 1) * (n - sqrN) / threadsCount) + sqrN;
                threads.Add(new Thread(() => MarkRange(0, basePrimes.Length, from, to))); 
            }
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
        }

        private void UsingThreadPool()
        {
            CountdownEvent cde = new CountdownEvent(basePrimes.Length);
            for (int i = 0; i < basePrimes.Length; i++)
            {
                ThreadPool.QueueUserWorkItem(Task, new object[] { i, cde });
            }
            cde.Wait();
        }

        private void Task(object threadContext)
        {
            int primeIndex = (int) ((object [])threadContext)[0];
            CountdownEvent cde = (CountdownEvent)((object[])threadContext)[1];

            MarkPrimeIndex(primeIndex);

            cde.Signal();
        }

        private void UsingParallelFor()
        {
            Parallel.For(0, basePrimes.Length, MarkPrimeIndex);
        }

        private void WorkPooling()
        {
            int pointer = -1;
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < threadsCount; i++)
            {
                threads.Add(new Thread(() =>
                {
                    while (true)
                    {
                        int next = Interlocked.Increment(ref pointer);
                        if (next >= basePrimes.Length)
                        {
                            break;
                        }
                        MarkPrimeIndex(next);
                    }
                }));
            }
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
        }

        private void WorkPoolingWithLock()
        {
            object sync_obj = "sync_obj";
            int pointer = -1;
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < threadsCount; i++)
            {
                threads.Add(new Thread(() =>
                {
                    while (true)
                    {
                        int next;
                        lock (sync_obj)
                        {
                            next = ++pointer;
                        }
                        if (next >= basePrimes.Length)
                        {
                            break;
                        }
                        MarkPrimeIndex(next);
                    }
                }));
            }
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
        }

        private void MarkPrimeIndex(int index)
        {
            MarkRange(index, index + 1, sqrN, n);
        }

        private void FindPrimes()
        {
            sqrN = 1 + (int)Math.Sqrt(n);

            MarkPrimesRange(2, sqrN);
            int count = 0;
            for (int i = 2; i < sqrN; i++)
            {
                if (!marks[i])
                {
                    count++;
                }
            }
            int index = 0;
            basePrimes = new int[count];
            for (int i = 2; i < sqrN; i++)
            {
                if (!marks[i])
                {
                    basePrimes[index] = i;
                    index++;
                }
            }
        }

        private void MarkPrimesRange(int from, int to)
        {
            for (int i = from + 1; i < to; i++)
            {
                for (int j = from; j < i; j++)
                {
                    if (i % j == 0)
                    {
                        marks[i] = true;
                        break;
                    }
                }

            }
        }

        private void MarkRange(int primesFrom, int primesTo, int from, int to)
        {
            if (isDebug)
            {
                Console.WriteLine("primesFrom=" + primesFrom + ", primesTo=" + primesTo
                + ", from=" + from + ", to=" + to);
            }

            for (int j = primesFrom; j < primesTo; j++)
            {
                for (int i = basePrimes[j] * (from / basePrimes[j]); i < to; i += basePrimes[j])
                {
                    marks[i] = true;
                }
            }
        }
    }
}
