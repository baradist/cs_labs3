using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelShellSort
{
    class Program
    {
        static int BLOCKS_COUNT = 8; // must be x^2
        static int THREADS_COUNT = 1;
        static void Main(string[] args)
        {
            //List<int> list = new List<int>(new int[] { 0, 14, 9, 17, 20, 5, 3, 8, 19, 4, 16, 23, 30, 29, 11, 1, 21 });
            Random random = new Random();
            List<int> list = Enumerable.Range(0, 1_000_000)
                .Select(n => random.Next())
                .ToList();

            //printList(list, "Initial list");
            Stopwatch sw = Stopwatch.StartNew();

            Sort(list);

            sw.Stop();
            //printList(list, "Result");

            List<int> reference = new List<int>(list);

            reference.Sort();

            Console.WriteLine("List size= {0}, THREADS_COUNT={1}", list.Count, THREADS_COUNT);
            Console.WriteLine("List is sorted correctly: {0}", IsListsEqual(list, reference));

            Console.WriteLine("Time spent: " + sw.ElapsedMilliseconds);
            Console.ReadKey();
        }

        private static bool IsListsEqual(List<int> left, List<int> right)
        {
            if(left.Count != right.Count)
            {
                return false;
            }
            for (int i = 0; i < left.Count; i++)
            {
                if(left[i] != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        static void Sort(List<int> list)
        {
            int[] indexes = GetIndexes(list.Count, BLOCKS_COUNT);

            LocalBlockSort(list, indexes);

            ParallelMergeSplit(list, indexes);

            PolishByOddEvenSort(list, indexes);
        }

        private static int[] GetIndexes(int length, int blocksAmount)
        {
            int[] indexes = new int[blocksAmount + 1];

            SetIndexes(indexes, 0, length, 0, blocksAmount);
            indexes[blocksAmount] = length;
            return indexes;
        }

        private static void SetIndexes(int[] indexes, int left, int right, 
            int leftBlock, int rightBlock)
        {
            if(rightBlock - leftBlock == 1)
            {
                indexes[leftBlock] = left;
                return;
            }
            int middle = (right - left) / 2 + left;
            int middleBlock = (rightBlock - leftBlock) / 2 + leftBlock;
            SetIndexes(indexes, left, middle, leftBlock, middleBlock);
            SetIndexes(indexes, middle, right, middleBlock, rightBlock);
        }

        private static void LocalBlockSort(List<int> list, int[] indexes)
        {
            IComparer<int> cmp = new NumericComparer();
            for (int i = 0; i < indexes.Count() - 1; i++)
            {
                int from = indexes[i];
                int count = indexes[i + 1] - from;
                list.Sort(from, count, cmp);
            }
        }

        private static void ParallelMergeSplit(List<int> list, int[] indexes)
        {
            for (int i = BLOCKS_COUNT; i > 1; i /= 2)
            {
                ParallelMergeSplitIth(list, i, indexes);
            }
        }

        private static void ParallelMergeSplitIth(List<int> list, 
            int intBlocksAmount, int[] indexes)
        {
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = THREADS_COUNT
            };
            for (int i = 0; i < BLOCKS_COUNT / intBlocksAmount; i++)
            {
                Parallel.For(0, intBlocksAmount / 2, options, 
                    (j, state) =>
                {
                    int from = j + i * intBlocksAmount;
                    int to = from + intBlocksAmount / 2;

                    MergeSplit(list, indexes, from, to);
                });
            }
        }

        private static void printList(List<int> list, string message)
        {
            Console.WriteLine(message);
            list.ForEach(n => Console.Write(n + ", "));
            Console.WriteLine();
        }

        private static void PolishByOddEvenSort(List<int> list, int[] indexes)
        {
            for (int even = 1; ; even = (even + 1) % 2)
            {
                bool changed = false;
                for (int i = 0; i + even + 1 < BLOCKS_COUNT; i += 2)
                {
                    int from = i + even;
                    int to = from + 1;

                    changed = MergeSplit(list, indexes, from, to);
                }
                if (even == 0 && !changed)
                {
                    return;
                }
            }
        }

        private static bool MergeSplit(List<int> list, int[] indexes, int from, int to)
        {
            int srcStart = indexes[from];
            int srcEnd = indexes[from + 1];
            int destStart = indexes[to];
            int destEnd = indexes[to + 1];
            //Console.WriteLine("srcStart={0}, srcEnd={1}, destStart={2}, destEnd={3}",
            //    srcStart, srcEnd, destStart, destEnd);

            List<int> merged = new List<int>();
            for (int s = srcStart, d = destStart; s < srcEnd || d < destEnd;)
            {
                if (s < srcEnd && (d == destEnd || list[s] < list[d]))
                {
                    merged.Add(list[s]);
                    s++;
                }
                else
                {
                    merged.Add(list[d]);
                    d++;
                }
            }
            bool changed = false;
            for (int s = srcStart, d = destStart, i = 0; i < merged.Count; i++)
            {
                if (s < srcEnd)
                {
                    if (list[s] != merged[i])
                    {
                        list[s] = merged[i];
                        changed = true;
                    }
                    s++;
                }
                else
                {
                    if(list[d] != merged[i])
                    {
                        list[d] = merged[i];
                        changed = true;
                    }
                    d++;
                }
            }
            return changed;
        }
    }

    class NumericComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            return x.CompareTo(y);
        }
    }
}
