using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class CompareRequest<T>
    {
        public T OldObject { get; set; }
        public T NewObject { get; set; }
    }
}
