using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonResourceAccess
{
    class LockKit : Kit
    {
        public LockKit()
        {
        }

        public override void Reader(Buffer buffer, List<string> result)
        {
            while (!buffer.IsComplete)
            {
                lock ("read")
{
                    if (buffer.IsFull)
                    {
                        result.Add(buffer.Read());
                    }
                }
            }
        }

        public override void ReleaseReader()
        {
            
        }

        public override void Writer(Buffer buffer, Queue<string> source)
        {
            while (source.Count > 0)
            {
                lock("write")
                {
                    if (!buffer.IsFull)
                    {
                        buffer.Write(source.Dequeue());
                    }
                }
            }
        }
    }
}
