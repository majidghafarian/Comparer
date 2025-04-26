using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class CompareRequest<T>
    {
        public List<T> OldList { get; set; }
        public List<T> NewList { get; set; }
        public string KeyName { get; set; }
    }
}
