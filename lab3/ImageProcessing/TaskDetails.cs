using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    class TaskDetails
    {
        public int thr;
        public int task;
        public string idx;
        public double ms1;
        public double ms2;

        public TaskDetails(int thr, int task, string idx, double ms1, double ms2)
        {
            this.thr = thr;
            this.task = task;
            this.idx = idx;
            this.ms1 = ms1;
            this.ms2 = ms2;
        }
    }
}
