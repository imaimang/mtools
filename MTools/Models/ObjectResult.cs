using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTools.Models
{
    public class ObjectResult<T>
    {
        public T Datas { get; set; }
        public int Count { get; set; }
    }
}
