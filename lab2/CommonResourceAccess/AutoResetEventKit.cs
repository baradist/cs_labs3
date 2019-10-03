using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonResourceAccess
{
    class AutoResetEventKit : Kit
    {
        private AutoResetEvent full = new AutoResetEvent(false);
        private AutoResetEvent empty = new AutoResetEvent(true);

        public override void Writer(Buffer buffer, Queue<string> source)
        {
            while (source.Count > 0)
            {
                empty.WaitOne();
                buffer.Write(source.Dequeue());
                full.Set();
            }
        }

        public override void Reader(Buffer buffer, List<string> result)
        {
            while (true)
            {
                full.WaitOne();
                if (!buffer.IsComplete)
                {
                    result.Add(buffer.Read());
                    empty.Set();
                }
                else
                {
                    full.Set();
                    return;
                }
            }
        }

        public override void ReleaseReader()
        {
            full.Set();
        }
    }
}
