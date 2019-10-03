using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonResourceAccess
{
    class Buffer
    {
        private string buffer;
        private bool isFull;
        private bool isComplete;
        public bool IsFull { get => isFull; set => isFull = value; }
        public bool IsComplete { get => isComplete; set => isComplete = value; }

        public void Complete()
        {
            isComplete = true;
        }

        public void Write(string str)
        {
            buffer = str;
            isFull = true;
        }

        public string Read()
        {
            isFull = false;
            return buffer;
        }
    }
}
