using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonResourceAccess
{
    class SemaphoreKit : Kit
    {
        private Semaphore full = new Semaphore(0, 1);
        private Semaphore empty = new Semaphore(1, 1);

        public override void Reader(Buffer buffer, List<string> result)
        {
            while (true)
            {
                full.WaitOne();
                if (!buffer.IsComplete)
                {
                    result.Add(buffer.Read());
                    empty.Release();
                }
                else
                {
                    full.Release();
                    return;
                }
            }
        }

        public override void ReleaseReader()
        {
            full.Release();
        }

        public override void Writer(Buffer buffer, Queue<string> source)
        {
            while (source.Count > 0)
            {
                empty.WaitOne();
                buffer.Write(source.Dequeue());
                full.Release();
            }
        }
    }
}
