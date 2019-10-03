using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonResourceAccess
{
    class InterlockedKit : Kit
    {
        private int full = 0;
        private int empty = 1;

        public override void Reader(Buffer buffer, List<string> result)
        {
            while (!buffer.IsComplete)
            {
                // read full == 1 and set to 0
                if (Interlocked.CompareExchange(ref full, 0, 1) == 1)
                {
                    result.Add(buffer.Read());
                    empty = 1;
                }
                Thread.Sleep(0);
            }
        }

        public override void ReleaseReader()
        {
            Interlocked.Exchange(ref full, 1);
        }

        public override void Writer(Buffer buffer, Queue<string> source)
        {
            while (source.Count > 0)
            {
                // read empty == 1 and set to 0
                if (Interlocked.CompareExchange(ref empty, 0, 1) == 1)
                {
                    buffer.Write(source.Dequeue());
                    full = 1;
                }
                Thread.Sleep(0);
            }
        }
    }
}
