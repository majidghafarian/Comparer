using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    [Display(Name =  " زیر ویژگی محصول")]
    public class ProductSubDetail
    {
        [Key]
        public int? Id { get; set; }
        [Display(Name = "زیر ویژگی")]
        public string SubFeature { get; set; }
        [GetValueordinal("ردیف")]
        [Display(Name = "زیر مقدار")]
        public int? Ordinal { get; set; }
    }
}
