using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonResourceAccess
{
    class SemaphoreSlimKit : Kit
    {
        private SemaphoreSlim full = new SemaphoreSlim(0, 1);
        private SemaphoreSlim empty = new SemaphoreSlim(1, 1);

        public override void Reader(Buffer buffer, List<string> result)
        {
            while (true)
            {
                full.Wait();
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
                empty.Wait();
                buffer.Write(source.Dequeue());
                full.Release();
            }
        }
    }
}
