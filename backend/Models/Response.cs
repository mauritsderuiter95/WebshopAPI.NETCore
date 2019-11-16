using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Response<T>
    {
        public int Taken { get; set; }

        public int Skipped { get; set; }

        public int Total { get; set; }

        public List<T> Data { get; set; }
    }
}
