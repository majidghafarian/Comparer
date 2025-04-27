using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class ProductSubDetail
    {
        public int Id { get; set; }
        [Display(Name = "زیر ویژگی")]
        public string SubFeature { get; set; }

        [Display(Name = "زیر مقدار")]
        public string SubValue { get; set; }
    }
}
