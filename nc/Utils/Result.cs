using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nc.Utils
{
    public class Result
    {
        public bool Ok { get; set; }
        public long Change { get; set; }
        public object Data { get; set; }
    }
}
