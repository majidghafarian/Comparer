using Domain.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Employe
    {
        [TrackChanges("نام")]
        public string Name { get; set; }

        [TrackChanges("سن")]
        public int Age { get; set; }

       
    }

}
