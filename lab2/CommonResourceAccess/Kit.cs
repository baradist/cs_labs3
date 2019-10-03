using System;
using System.Collections.Generic;

namespace CommonResourceAccess
{
    internal abstract class Kit
    {
        protected object[] syncers;

        public abstract void ReleaseReader();

        public abstract void Writer(Buffer buffer, Queue<string> source);

        public abstract void Reader(Buffer buffer, List<string> result);
    }
}